using OpenTK.Mathematics;
using KWEngine3.Helper;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using KWEngine3.Editor;
using KWEngine3.Model;
using KWEngine3.GameObjects;
using OpenTK.Windowing.Common;
using KWEngine3.Assets;
using System.Diagnostics;
using OpenTK.Windowing.Desktop;
using KWEngine3.Exceptions;
using KWEngine3.FontGenerator;


namespace KWEngine3
{
    /// <summary>
    /// Kernbibliothek der Engine
    /// </summary>
    public class KWEngine
    {
        internal const float SIMULATIONNIBBLESIZE = 1f / 240f;
        internal const float SIMULATIONMAXACCUMULATOR = 1 / 10f;
        internal const int LIGHTINDEXDIVIDER = 17;
        internal const float RAYTRACE_EPSILON = 0.000001f;
        internal const float RAYTRACE_SAFETY = 0.1f;
        internal const float RAYTRACE_SAFETY_SQ = RAYTRACE_SAFETY * RAYTRACE_SAFETY;
        internal const int TERRAIN_PATCH_SIZE = 16;
        
        
        /// <summary>
        /// Gibt an oder setzt den Faktor, der bei der Berechnung des Kameraschüttelns einer ViewSpaceGameObject-Instanz zugerechnet wird (Standard: 0.1f, gültiger Wertebereich liegt zwischen 0f und 1f)
        /// </summary>
        public static float ViewSpaceGameObjectShakeFactor
        {
            get
            {
                return _vsgShakeFactor;
            }
            set
            {
                _vsgShakeFactor = Math.Clamp(value, 0f, 1f);
            }
        }
       

        /// <summary>
        /// Gibt an, ab welcher Entfernung Terrain-Geometrie weniger detailliert dargestellt werden soll (Standard: 32 Einheiten)
        /// </summary>
        public static TerrainThresholdValue TerrainTessellationThreshold { get; set; } = TerrainThresholdValue.T32;

        /// <summary>
        /// Gibt die maximale Anzahl der Instanzen für RenderObjects an
        /// </summary>
        public const int MAXADDITIONALINSTANCECOUNT = 1024 - 1; // -1 because the main instance does not count here

        /// <summary>
        /// Erfragt oder setzt den aktuellen Debug-Modus für den Viewport
        /// </summary>
        public static DebugMode DebugMode { get; set; } = DebugMode.None;

        /// <summary>
        /// Aktuelles Fenster
        /// </summary>
        public static GLWindow Window { get; internal set; } = null;
        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        public static World CurrentWorld { get; internal set; } = null;
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();
        internal static Dictionary<string, GeoMeshCollider> CustomColliders { get; set; } = new Dictionary<string, GeoMeshCollider>();

        internal static void DeleteCustomModelsFontsAndTexturesFromCurrentWorld()
        {
            for(int i = KWEngine.CurrentWorld._customTextures.Count - 1; i >= 0; i--)
            {
                string currentTextureFilename = KWEngine.CurrentWorld._customTextures.Keys.ElementAt(i);
                int currentTexture = KWEngine.CurrentWorld._customTextures[currentTextureFilename].ID;
                HelperTexture.DeleteTexture(currentTexture);
            }
            KWEngine.CurrentWorld._customTextures.Clear();

            // MODELS
            for(int i = Models.Keys.Count - 1; i >= 5; i--)
            {
                string modelName = Models.Keys.ElementAt(i);
                GeoModel m = Models[modelName];
                m.Dispose();
                Models.Remove(modelName);
            }

            // FONTS
            for (int i = FontDictionary.Keys.Count - 1; i >= 5; i--)
            {
                string fontname = FontDictionary.Keys.ElementAt(i);
                KWFont kwfont = FontDictionary[fontname];
                if(kwfont != null)
                {
                    kwfont.Dispose();
                }
                FontDictionary.Remove(fontname);
            }

            // CUSTOM COLLIDERS
            CustomColliders.Clear();
        }

        internal static EngineMode Mode { get; set; } = EngineMode.Play;

        /// <summary>
        /// Gibt die Namen aller Hitboxen des angegebenen Modells auf der Editorkonsole aus
        /// </summary>
        /// <param name="modelname">Modellname</param>
        public static void PrintHitboxesForModel(string modelname)
        {
            if (Models.ContainsKey(modelname))
            {
                GeoModel model = Models[modelname];
                if (!model.IsPrimitive && !model.IsTerrain)
                {
                    if (model.MeshCollider.MeshHitboxes == null || model.MeshCollider.MeshHitboxes.Count == 0)
                    {
                        LogWriteLine("[Import] Model " + modelname + " has no hitboxes");
                    }
                    else
                    {
                        LogWriteLine("[Import] Listing hitboxes for model " + modelname + ":");
                        foreach (GeoMeshHitbox hb in model.MeshCollider.MeshHitboxes)
                        {
                            LogWriteLine("         -> " + hb.Name);
                        }
                    }
                }
                else
                {
                    LogWriteLine("[Import] Invalid model name - needs to be an imported model");
                }
            }
            else
            {
                LogWriteLine("[Import] Model " + modelname + " not found in database");
            }
        }

        /// <summary>
        /// Gibt die Namen aller Hitboxen des angegebenen Modells auf der Editorkonsole aus
        /// </summary>
        /// <param name="modelname">Modellname</param>
        /// <returns>Liste der Hitboxnamen</returns>
        public static List<string> GetHitboxesForModel(string modelname)
        {
            List<string> result = new();

            if (Models.ContainsKey(modelname))
            {
                GeoModel model = Models[modelname];
                if (!model.IsPrimitive && !model.IsTerrain)
                {
                    if (model.MeshCollider.MeshHitboxes == null || model.MeshCollider.MeshHitboxes.Count == 0)
                    {
                        LogWriteLine("[Import] Model " + modelname + " has no hitboxes");
                    }
                    else
                    {
                        foreach (GeoMeshHitbox hb in model.MeshCollider.MeshHitboxes)
                        {
                            result.Add(hb.Name);
                        }
                    }
                }
                else
                {
                    LogWriteLine("[Import] Invalid model name - needs to be an imported model");
                }
            }
            else
            {
                LogWriteLine("[Import] Model " + modelname + " not found in database");
            }
            return result;
        }

        /// <summary>
        /// Gibt an, ob der Edit-Modus aktiv ist
        /// </summary>
        public static bool EditModeActive { get { return Mode == EngineMode.Edit; } }

        internal static float _octreeSafetyZone = 1f;
        internal static float _swpruneTolerance = 2.0f;

        /// <summary>
        /// Zusätzliches Padding für die Kollisionsvorhersage (Standard: 2.0f)
        /// </summary>
        public static float SweepAndPruneTolerance { get { return _swpruneTolerance; } set { _swpruneTolerance = MathHelper.Max(0f, value); } }

        /// <summary>
        /// Aktiviert oder deaktiviert den Post-Processing-Effekt "Screen-Space Ambient Occlusion" (Standard: false)
        /// </summary>
        public static bool SSAO_Enabled { get { return _ssaoEnabled; } set { _ssaoEnabled = value; } }
        /// <summary>
        /// Weite des SSAO-Effekts (Standard: 0.4f, Bereich: 0.01f bis 1.0f)
        /// </summary>
        public static float SSAO_Radius { get { return _ssaoRadius; } set { _ssaoRadius = Math.Clamp(value, 0.01f, 1.0f); } }
        /// <summary>
        /// Intensität des SSAO-Effekts (Standard: 0.1f, Bereich: 0.00f bis 0.5f)
        /// </summary>
        public static float SSAO_Bias { get { return _ssaoBias; } set { _ssaoBias = Math.Clamp(value, 0.0f, 0.5f); } }

        internal static void DeselectAll()
        {
            KWBuilderOverlay.SelectedGameObject = null;
            KWBuilderOverlay.SelectedLightObject = null;
            KWBuilderOverlay.SelectedTerrainObject = null;
        }

        // SSAO settings
        internal static float _ssaoRadius = 0.40f;
        internal static float _ssaoBias = 0.10f;
        internal static bool _ssaoEnabled = false;

        internal static CursorState _stateCameraGameBeforeToggle = CursorState.Normal;
        internal static float _texMemUsed = 0f;
        internal static float _geometryMemUsed = 0f;
        internal static void ToggleEditMode()
        {
            if (HelperGeneral.IsAssemblyDebugBuild(Assembly.GetEntryAssembly()))
            {
                Mode = Mode == EngineMode.Play ? EngineMode.Edit : EngineMode.Play;
                if (EditModeActive)
                {
                    _texMemUsed = MathF.Round(HelperGeneral.GetTextureStorageSize() / 1000000f, 2);
                    _geometryMemUsed = MathF.Round(HelperGeneral.GetGeometryStorageSize() / 1000000f, 2);
                    CurrentWorld._cameraEditor = CurrentWorld._cameraGame;
                    CurrentWorld._cameraEditor.UpdatePitchYaw(true);
                    _stateCameraGameBeforeToggle = Window.CursorState;
                    CurrentWorld.MouseCursorReset();
                }
                else
                {
                    CurrentWorld._cameraGame._frustum.UpdateFrustum(CurrentWorld._cameraGame._stateCurrent.ProjectionMatrix, CurrentWorld._cameraGame._stateCurrent.ViewMatrix);
                    Window.CursorState = _stateCameraGameBeforeToggle;
                    DeselectAll();
                }
            }
        }

        internal static double DeltaTimeAccumulator { get; set; } = 0.0f;
        internal static float DeltaTimeFactorForOverlay { get; set; } = 1f;
        internal static double DeltaTimeCurrentNibbleSize { get; set; } = SIMULATIONNIBBLESIZE;

        /// <summary>
        /// Gibt an, wie lange die aktuelle Welt bereits aktiv ist
        /// </summary>
        public static float WorldTime { get; internal set; } = 0.0f;
        /// <summary>
        /// Gibt an, wie lange die Anwendung bereits aktiv ist
        /// </summary>
        public static float ApplicationTime { get; internal set; } = 0.0f;
        internal static float LastFrameTime { get; set; } = 0.0f;

        /// <summary>
        /// Gibt die aktuelle Anzahl der Frames-Per-Second (FPS) an
        /// </summary>
        public static int FPS { get { return (int)(1000f / LastFrameTime); } }

        internal static int LastSimulationUpdateCycleCount { get; set; } = 0;

        /// <summary>
        /// Schreibt eine Log-Zeile in das Ausgabefenster des Edit-Modus (nur Debug-Modus)
        /// </summary>
        /// <param name="message">Zu schreibende Nachricht</param>
        public static void LogWriteLine(object message)
        {
            if (HelperGeneral.IsAssemblyDebugBuild(Assembly.GetEntryAssembly()))
                EngineLog.AddMessage(message.ToString());
        }

        /// <summary>
        /// Löscht alle Log-Nachrichten des Edit-Modus
        /// </summary>
        public static void LogClear()
        {
            EngineLog.Clear();
        }



        internal const int BLOOMWIDTH = 960;//720;//960;
        internal const int BLOOMHEIGHT = 540; //405;//540;
        /// <summary>
        /// Anzahl der Renderschritte für den Glow-Effekt
        /// </summary>
        public const int MAX_BLOOM_BUFFERS = 8;
        /// <summary>
        /// Anzahl der Gewichte pro Knochen
        /// </summary>
        public const int MAX_BONE_WEIGHTS = 3;
        /// <summary>
        /// Anzahl der Knochen pro GameObject
        /// </summary>
        public const int MAX_BONES = 128;
        /// <summary>
        /// Anzahl der Lichter pro Welt
        /// </summary>
        public const int MAX_LIGHTS = 50;
        /// <summary>
        /// Anzahl der Schattenlichter pro Welt (anteilig an MAX_LIGHTS)
        /// </summary>
        public const int MAX_SHADOWMAPS = 3;

        internal static Matrix4 Identity = Matrix4.Identity;
        //private static Vector3 _worldUp = new Vector3(0, 1, 0);
        internal static int _uniformOffsetMultiplier = 1;
        internal static float _glowRadius = 0.75f;
        internal static float _glowUpsampleF1 = 0.15f;
        internal static float _glowUpsampleF2 = 0.70f;

        /// <summary>
        /// Steuert das Ausmaß des durch Überbelichtung erzeugten Glow-Effekts (von 0 bis 1, Standard: 0.75)
        /// </summary>
        public static float GlowRadius
        {
            get
            {
                return _glowRadius;
            }
            set
            {
                _glowRadius = HelperGeneral.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Steuert den Stil des Glühens (Faktor 1, erlaubte Werte zwischen 0 und 1, Standard: 0.15)
        /// </summary>
        public static float GlowStyleFactor1 { get { return _glowUpsampleF1; } set { _glowUpsampleF1 = Math.Clamp(value, 0.001f, 1.0f); } }
        /// <summary>
        /// Steuert den Stil des Glühens (Faktor 2, erlaubte Werte zwischen 0 und 1, Standard: 0.75)
        /// </summary>
        public static float GlowStyleFactor2 { get { return _glowUpsampleF2; } set { _glowUpsampleF2 = Math.Clamp(value, 0.001f, 1.0f); } }

        internal static Matrix4 Matrix4Dummy = Matrix4.Identity;

        internal static int TextureDefault = -1;
        internal static int TextureBlack = -1;
        internal static int TextureWhite = -1;
        internal static int TextureAlpha = -1;
        internal static int TextureNormalEmpty = -1;
        internal static int TextureNoise = -1;
        internal static int TextureCubemapEmpty = -1;
        internal static int TextureDepthEmpty = -1;
        internal static int TextureDepthCubeMapEmpty = -1;
        internal static int TextureCheckerboard = -1;

        internal static int TextureFlowFieldArrow = -1;
        internal static int TextureFlowFieldCross = -1;

        internal static int TextureFoliageGrass1 = -1;
        internal static int TextureFoliageGrass2 = -1;
        internal static int TextureFoliageGrass3 = -1;
        internal static int TextureFoliageGrassMinecraft = -1;
        internal static int TextureFoliageFern = -1;
        internal static int TextureFoliageGrassNormal = -1;

        internal static float TimeElapsed = 0;

        internal static Vector3 DefaultCameraPosition = new(0, 0, 10);

        /// <summary>
        /// Welt-Vektor, der angibt, wo 'oben' ist
        /// </summary>
        public static Vector3 WorldUp { get; } = Vector3.UnitY;

        //internal static int[] FontTextureArray { get; set; } = new int[4];

        internal static Dictionary<string, KWFont> FontDictionary = new();

        internal static void InitializeFontsAndDefaultTextures()
        {
            Window.AnisotropicFilteringLevel = (int)Window._ppQuality == 2 ? 8 : (int)Window._ppQuality == 1 ? 4 : 1;

            int mipMaps;

            KWQuad2D.Init();
            KWQuad2D_05.Init();
            FontDictionary.Add("Anonymous", HelperGlyph.LoadFont(HelperGlyph.LoadFontInternal("Anonymous.ttf")));
            FontDictionary.Add("MajorMonoDisplay", HelperGlyph.LoadFont(HelperGlyph.LoadFontInternal("MajorMonoDisplay.ttf")));
            FontDictionary.Add("NovaMono", HelperGlyph.LoadFont(HelperGlyph.LoadFontInternal("NovaMono.ttf")));
            FontDictionary.Add("XanhMono", HelperGlyph.LoadFont(HelperGlyph.LoadFontInternal("XanhMono.ttf")));
            FontDictionary.Add("OpenSans", HelperGlyph.LoadFont(HelperGlyph.LoadFontInternal("OpenSans.ttf")));

            TextureDefault = HelperTexture.LoadTextureForModelInternalExecutingAssembly("default.dds", out mipMaps);
            TextureBlack = HelperTexture.LoadTextureInternal("black.png");
            TextureWhite = HelperTexture.LoadTextureInternal("white.png");
            TextureAlpha = HelperTexture.LoadTextureInternal("alpha.png");
            TextureNormalEmpty = HelperTexture.LoadTextureInternal("normalmap.png");
            TextureNoise = HelperTexture.LoadTextureInternal("noise.png");
            TextureCheckerboard = HelperTexture.LoadTextureInternal("checkerboard.png");

            TextureFlowFieldArrow = HelperTexture.LoadTextureInternal("arrow.png");
            TextureFlowFieldCross = HelperTexture.LoadTextureInternal("cross.png");

            TextureFoliageGrass1 = HelperTexture.LoadTextureForModelInternalExecutingAssembly("foliage_grassblade_01.dds", out mipMaps);
            TextureFoliageGrass2 = HelperTexture.LoadTextureForModelInternalExecutingAssembly("foliage_grassblade_02.dds", out mipMaps);
            TextureFoliageGrass3 = HelperTexture.LoadTextureForModelInternalExecutingAssembly("foliage_grassblade_03.dds", out mipMaps);
            TextureFoliageGrassMinecraft = HelperTexture.LoadTextureForModelInternalExecutingAssembly("foliage_grassblade_minecraft.dds", out mipMaps);
            TextureFoliageFern = HelperTexture.LoadTextureForModelInternalExecutingAssembly("foliage_fern.dds", out mipMaps);
            TextureFoliageGrassNormal = HelperTexture.LoadTextureInternal("foliage_grassblade_normal.png");

            TextureDepthEmpty = HelperTexture.CreateEmptyDepthTexture();
            TextureDepthCubeMapEmpty = HelperTexture.CreateEmptyCubemapDepthTexture();
            TextureCubemapEmpty = HelperTexture.CreateEmptyCubemapTexture();
        }

        /// <summary>
        /// Empfindlichkeit des Mauszeigers im First-Person-Modus (Standard: 0.05f, negative Werte für die Invertierung der y-Achse)
        /// </summary>
        public static float MouseSensitivity { get; set; } = 0.05f;

        internal static GeoModel KWStar;
        internal static GeoModel KWHeart;
        internal static GeoModel KWSkull;
        internal static GeoModel KWDollar;
        internal static GeoModel KWLightBulb;
        internal static GeoMeshCollider KWCapsule;
        internal static GeoMeshCollider KWCapsule2;
        internal static GeoMeshCollider KWSphereCollider;
        internal static GeoModel KWFoliageMinecraft;
        internal static GeoModel KWFoliageFern;
        internal static GeoModel KWMapItemXZ;
        internal static GeoModel KWMapItemXY;

        internal static void InitializeModels()
        {
            Models.Add("KWCube", SceneImporter.LoadModel("kwcube.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWQuad", SceneImporter.LoadModel("kwquad.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWQuad2D", SceneImporter.LoadModel("kwquad2.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWPlatform", SceneImporter.LoadModel("kwplatform.obj", true, SceneImporter.AssemblyMode.Internal));
            KWStar = SceneImporter.LoadModel("star.obj", false, SceneImporter.AssemblyMode.Internal);
            KWHeart = SceneImporter.LoadModel("heart.obj", false, SceneImporter.AssemblyMode.Internal);
            KWSkull = SceneImporter.LoadModel("skull.obj", false, SceneImporter.AssemblyMode.Internal);
            KWDollar = SceneImporter.LoadModel("dollar.obj", false, SceneImporter.AssemblyMode.Internal);
            KWLightBulb = SceneImporter.LoadModel("lightbulb.obj", false, SceneImporter.AssemblyMode.Internal);
            KWSphereCollider = SceneImporter.LoadColliderInternal("kwsphere_collider.obj", ColliderType.ConvexHull);
            KWCapsule = SceneImporter.LoadColliderInternal("capsulehitbox.obj", ColliderType.ConvexHull);
            KWCapsule2 = SceneImporter.LoadColliderInternal("capsulehitbox2.obj", ColliderType.ConvexHull);
            KWFoliageMinecraft = SceneImporter.LoadModel("kwgrass_minecraft.obj", true, SceneImporter.AssemblyMode.Internal);
            KWFoliageFern = SceneImporter.LoadModel("kwgrass_fern.obj", true, SceneImporter.AssemblyMode.Internal);
            KWMapItemXZ = SceneImporter.LoadModel("kwmapquadxz.obj", false, SceneImporter.AssemblyMode.Internal);
            KWMapItemXY = SceneImporter.LoadModel("kwmapquadxy.obj", false, SceneImporter.AssemblyMode.Internal);

            KWFoliageGrass.Init();
            KWTerrainQuad.Init();


            for (int i = 0; i < ExplosionObject.Axes.Length; i++)
            {
                ExplosionObject.Axes[i] = Vector3.Normalize(ExplosionObject.Axes[i]);
            }

        }

        internal static Dictionary<ParticleType, ParticleInfo> ParticleDictionary = new();
        internal static void InitializeParticles()
        {
            int tex;

            // Bursts:
            tex = HelperTexture.LoadTextureCompressedNoMipMap("fire01.dds");
            ParticleDictionary.Add(ParticleType.BurstFire1, new ParticleInfo(tex, 8, 64));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("fire02.dds");
            ParticleDictionary.Add(ParticleType.BurstFire2, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("fire03.dds");
            ParticleDictionary.Add(ParticleType.BurstFire3, new ParticleInfo(tex, 9, 81));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("fire04.dds");
            ParticleDictionary.Add(ParticleType.BurstElectricity, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_bubbles.dds");
            ParticleDictionary.Add(ParticleType.BurstBubblesColored, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_bubbles_unicolor.dds");
            ParticleDictionary.Add(ParticleType.BurstBubblesMonochrome, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_explosioncolored.dds");
            ParticleDictionary.Add(ParticleType.BurstFirework1, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_firework.dds");
            ParticleDictionary.Add(ParticleType.BurstFirework2, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_hearts.dds");
            ParticleDictionary.Add(ParticleType.BurstHearts, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_plusplusplus.dds");
            ParticleDictionary.Add(ParticleType.BurstOneUps, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_shield.dds");
            ParticleDictionary.Add(ParticleType.BurstShield, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_teleport1.dds");
            ParticleDictionary.Add(ParticleType.BurstTeleport1, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_teleport2.dds");
            ParticleDictionary.Add(ParticleType.BurstTeleport2, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("particleburst_teleport3.dds");
            ParticleDictionary.Add(ParticleType.BurstTeleport3, new ParticleInfo(tex, 4, 16));

            // Loops:
            tex = HelperTexture.LoadTextureCompressedNoMipMap("smoke01.dds");
            ParticleDictionary.Add(ParticleType.LoopSmoke1, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("smoke02.dds");
            ParticleDictionary.Add(ParticleType.LoopSmoke2, new ParticleInfo(tex, 7, 46));

            tex = HelperTexture.LoadTextureCompressedNoMipMap("smoke03.dds");
            ParticleDictionary.Add(ParticleType.LoopSmoke3, new ParticleInfo(tex, 6, 32));

        }

        internal static GeoModel GetModel(string name)
        {
            bool modelFound = Models.TryGetValue(name, out GeoModel m);
            if (!modelFound)
            {
                KWEngine.LogWriteLine("[Model] Model " + (name == null ? "" : name.Trim()) + " not found");
            }
            return m;
        }

        internal static float RayIntersectionErrorMarginFactor { get; set; }= 1.00001f;

        /// <summary>
        /// Baut ein Terrain-Modell
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="heightmap">Heightmap Textur (maximal: 1024x1024px)</param>
        /// <param name="height">Höhe (y-Achse, gültige Werte von 0 bis 128)</param>
        public static void BuildTerrainModel(string name, string heightmap, int height)
        {
            if (name == null || name.Trim().Length == 0)
            {
                KWEngine.LogWriteLine("[KWEngine] Terrain name is invalid");
                return;
            }
            if (heightmap == null || !File.Exists(heightmap) || (!heightmap.Trim().ToLower().EndsWith(".jpg") && !heightmap.Trim().ToLower().EndsWith(".png") && !heightmap.Trim().ToLower().EndsWith(".jpeg")))
            {
                KWEngine.LogWriteLine("[KWEngine] Terrain height map file is invalid");
                return;
            }
            
            height = Math.Clamp(height, 0, 128);

            if (Models.ContainsKey(name))
            {
                KWEngine.LogWriteLine("[KWEngine] Model name already used");
                return;
            }
            GeoModel terrainModel = new()
            {
                Name = name,
                Meshes = new(),
                IsValid = true
            };

            GeoTerrain t = new();
            GeoMesh terrainMesh = t.BuildTerrain(name, heightmap, height, out int width, out int depth);
            if (terrainMesh != null)
            {
                terrainMesh.Terrain = t;
                GeoMaterial mat = new()
                {
                    BlendMode = BlendingFactor.OneMinusSrcAlpha,
                    ColorAlbedo = new Vector4(1, 1, 1, 1),
                    ColorEmissive = new Vector4(0, 0, 0, 0),
                    Metallic = 0,
                    Roughness = 1
                };
                terrainMesh.Material = mat;
                terrainModel.Meshes.Add("Terrain", terrainMesh);

                GeoMeshHitbox meshHitBox = new(0 + width / 2, 0 + height / 2, 0 + depth / 2, 0 - width / 2, 0 - height / 2, 0 - depth / 2, null)
                {
                    Model = terrainModel,
                    Name = name
                };

                terrainModel.MeshCollider.MeshHitboxes = new()
                {
                    meshHitBox
                };

                KWEngine.Models.Add(name, terrainModel);
            }
        }

        /// <summary>
        /// Lädt die unter dem Dateinamen zu findenden Animationen in das (zuvor unter dem Namen angegebenen) 3D-Modell
        /// </summary>
        /// <param name="modelname">Name des bereits importierten Modells</param>
        /// <param name="filename">Dateiname der zu importierenden Datei</param>
        public static void LoadAnimationIntoModel(string modelname, string filename)
        {
            MethodBase caller = new StackTrace().GetFrame(1).GetMethod();
            string callerName = caller.Name;

            if (callerName != "Prepare" && callerName != "BuildWorld" && callerName != "BuildAndAddViewSpaceGameObject")
            {
                LogWriteLine("[Import] Model " + filename + " must be imported in Prepare() - aborting import");
                return;
            }
            if (!Models.ContainsKey(modelname.Trim()))
            {
                LogWriteLine("[Import] Model " + modelname + " not found in model database - please import it first");
                return;
            }
            if(!File.Exists(filename))
            {
                LogWriteLine("[Import] File " + filename + " not found");
                return;
            }
            GeoModel model = Models[modelname];
            if(!model.HasBones)
            {
                LogWriteLine("[Import] Model " + modelname + " has no bones - aborting import");
                return;
            }
            if(!HelperGeneral.IsModelFile(filename))
            {
                LogWriteLine("[Import] File " + filename + " is not a valid model file - aborting import");
                return;
            }

            List<GeoAnimation> animations = null;
            if (filename.ToLower().EndsWith("glb") || filename.ToLower().EndsWith("gltf"))
            {
                animations = SceneImporterGLTF.LoadAnimations(filename);
            }
            else
            {
                animations = SceneImporter.LoadAnimations(filename);

            }

            if(animations != null && animations.Count > 0)
            {
                if (model.Animations == null)
                    model.Animations = new List<GeoAnimation>();
                model.Animations.AddRange(animations);
            }
        }

        /// <summary>
        /// Importiert ein 3D-Collider-Modell unter dem angegebenen Namen
        /// </summary>
        /// <param name="name">Name, unter dem das Collider-Modell gespeichert werden soll</param>
        /// <param name="filename">Dateiname des Collider-Modells</param>
        /// <param name="colliderType">Art des Collider-Modells</param>
        public static void LoadCollider(string name, string filename, ColliderType colliderType)
        {

            filename = filename == null ? "" : filename.Trim();
            MethodBase caller = new StackTrace().GetFrame(1).GetMethod();
            string callerName = caller.Name;

            if (callerName != "Prepare" && callerName != "BuildWorld" && callerName != "BuildAndAddViewSpaceGameObject")
            {
                LogWriteLine("[Import] Collider " + filename + " must be imported in Prepare() - aborting import");
                return;
            }

            if (name == null || name.Trim().Length == 0)
            {
                LogWriteLine("[Import] Collider " + filename + " must be imported with a proper database name");
                return;
            }
            name = name.Trim();
            if (CustomColliders.ContainsKey(name))
            {
                if (callerName != "BuildWorld")
                {
                    KWEngine.LogWriteLine("[Import] Collider " + (name == null ? "" : name) + " already exists");
                    return;
                }
            }
            else
            {
                GeoMeshCollider c = SceneImporter.LoadCollider(filename, colliderType);
                if (c != null)
                {
                    c.FileName = filename;
                    c.Name = name;
                    CustomColliders.Add(name, c);
                }
                else
                {
                    KWEngine.LogWriteLine("[Import] Collider " + (name == null ? "" : name) + " invalid or file not found");
                    return;
                }
            }
        }

        /// <summary>
        /// Lädt ein Modell aus einer Datei
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="filename">Datei des Modells</param>
        public static void LoadModel(string name, string filename)
        {
            MethodBase caller = new StackTrace().GetFrame(1).GetMethod();
            string callerName = caller.Name;

            if (callerName != "Prepare" && callerName != "BuildWorld" && callerName != "BuildAndAddViewSpaceGameObject")
            {
                LogWriteLine("[Import] Model " + filename + " must be imported in Prepare() - aborting import");
                return;
            }
            GeoModel m;
            if (Models.ContainsKey(name.Trim()))
            {
                if (callerName != "BuildWorld")
                {
                    KWEngine.LogWriteLine("[Model] Model " + (name == null ? "" : name.Trim()) + " already exists");
                    return;
                }
                else
                {
                    m = Models[name.Trim()];
                }
            }
            else
            {
                m = SceneImporter.LoadModel(filename, true, SceneImporter.AssemblyMode.File);
                if (m != null)
                {
                    name = name.Trim();
                    m.Name = name;
                    Models.Add(name, m);
                }
                else
                {
                    KWEngine.LogWriteLine("[Model] Model " + (name == null ? "" : name.Trim()) + " invalid");
                    return;
                }
            }
        }

        /// <summary>
        /// Erstellt eine Liste der im 3D-Modell verfügbaren Knochennamen
        /// </summary>
        /// <param name="modelname">Name des Modells (muss zuvor importiert worden sein)</param>
        /// <returns>Liste der Knochennamen</returns>
        public static List<string> GetModelBoneNames(string modelname)
        {
            bool result = Models.TryGetValue(modelname, out GeoModel model);
            List<string> resultList = new();
            if (result)
            {
                resultList.AddRange(model.BoneNames);
            }
            else
            {
                KWEngine.LogWriteLine("[Model] Model " + (modelname == null ? "" : modelname.Trim()) + " not found");
            }
            return resultList;
        }

        /// <summary>
        /// Erfragt Informationen über die aktuell angeschlossenen Monitore/Bildschirme
        /// </summary>
        /// <returns>Monitor-/Bildschirminformationen</returns>
        public static ScreenInfo ScreenInformation => _screenInfo;
        
        internal static ScreenInfo CollectScreenInfo()
        {
            MonitorInfo primaryMonitor = Monitors.GetPrimaryMonitor();
            List<MonitorInfo> monitors = Monitors.GetMonitors();
            ScreenInfo sInfo = new ScreenInfo()
            {
                _screens = new Screen[monitors.Count]
            };

            int index = 0;
            foreach (MonitorInfo mInfo in monitors)
            {
                Screen s;
                if (mInfo.Handle.Pointer == primaryMonitor.Handle.Pointer)
                {
                    s = new Screen()
                    {
                        IsPrimary = true,
                        Width = mInfo.HorizontalResolution,
                        Height = mInfo.VerticalResolution,
                        DPI = new Vector2(mInfo.HorizontalRawDpi, mInfo.VerticalRawDpi),
                        Name = mInfo.Name,
                        Handle = mInfo.Handle.Pointer
                    };
                    sInfo._primaryIndex = index;
                }
                else
                {
                    s = new Screen()
                    {
                        IsPrimary = false,
                        Width = mInfo.HorizontalResolution,
                        Height = mInfo.VerticalResolution,
                        DPI = new Vector2(mInfo.HorizontalRawDpi, mInfo.VerticalRawDpi),
                        Name = mInfo.Name,
                        Handle = mInfo.Handle.Pointer
                    };
                }
                sInfo._screens[index++] = s;
            }
            return sInfo;
        }

        internal static ScreenInfo _screenInfo = CollectScreenInfo();
        internal static float _vsgShakeFactor = 0.1f;
    }
}
