using OpenTK.Mathematics;
using KWEngine3.Helper;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using KWEngine3.Editor;
using KWEngine3.Model;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Common;
using System.Xml.Linq;

namespace KWEngine3
{
    /// <summary>
    /// Kernbibliothek der Engine
    /// </summary>
    public class KWEngine
    {
        internal const float SIMULATIONNIBBLESIZE = 1f / 120f;
        internal const float SIMULATIONMAXACCUMULATOR = 1 / 10f;
        internal const int MAXGAMEOBJECTID = 16777216;
        internal const int LIGHTINDEXDIVIDER = 17;
        internal static char _folderDivider = '\\';
        internal static string _folderDividerString = @"\\";

        /// <summary>
        /// Aktuelles Fenster
        /// </summary>
        public static GLWindow Window { get; internal set; } = null;
        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        public static World CurrentWorld { get; internal set; } = null;
        internal static Dictionary<World, Dictionary<string, int>> CustomTextures { get; set; } = new Dictionary<World, Dictionary<string, int>>();
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();

        internal static void DeleteCustomModelsAndTextures(World w)
        {
            // TEXTURES
            bool texturesFound = CustomTextures.TryGetValue(w, out Dictionary<string, int> textures);
            if(texturesFound)
            {
                for(int i = textures.Keys.Count - 1; i >= 0; i--)
                {
                    string currentTextureFilename = textures.Keys.ElementAt(i);
                    int currentTexture = textures[currentTextureFilename];
                    HelperTexture.DeleteTexture(currentTexture);
                }
                textures.Clear();
            }

            // MODELS
            for(int i = Models.Keys.Count - 1; i >= 3; i--)
            {
                string modelName = Models.Keys.ElementAt(i);
                GeoModel m = Models[modelName];
                m.Dispose();
                Models.Remove(modelName);
            }
        }

        internal static EngineMode Mode { get; set; } = EngineMode.Play;
        /// <summary>
        /// Gibt an, ob der Edit-Modus aktiv ist
        /// </summary>
        public static bool EditModeActive { get { return Mode == EngineMode.Edit; } }

        internal static bool _octreeVisible = false;
        /// <summary>
        /// Gibt an, ob der Octree sichtbar ist
        /// </summary>
        public static bool OctreeVisible { get { return _octreeVisible; } set { _octreeVisible = value; } }

        internal static float _octreeSafetyZone = 1f;
        /// <summary>
        /// Zusätzliches Padding für den Octree (Standard: 1)
        /// </summary>
        public static float OctreeSafetyZone { get { return _octreeSafetyZone; } set { _octreeSafetyZone = MathHelper.Max(0f, value); } }

        internal static void DeselectAll()
        {
            KWBuilderOverlay.SelectedGameObject = null;
            KWBuilderOverlay.SelectedLightObject = null;
            KWBuilderOverlay.SelectedTerrainObject = null;
        }

        internal static CursorState _stateCameraGameBeforeToggle = CursorState.Normal;
        internal static void ToggleEditMode()
        {
            Mode = Mode == EngineMode.Play ? EngineMode.Edit : EngineMode.Play;
            if(EditModeActive)
            {
                CurrentWorld._cameraEditor = CurrentWorld._cameraGame;
                CurrentWorld._cameraEditor.UpdatePitchYaw();
                _stateCameraGameBeforeToggle = Window.CursorState;
                CurrentWorld.MouseCursorReset();
            }
            else
            {
                Window.CursorState = _stateCameraGameBeforeToggle;
                DeselectAll();
            }
        }
        
        internal static float DeltaTimeAccumulator { get; set; } = 0.0f;
        internal static float DeltaTimeFactorForOverlay { get; set; } = 1f;
        //public static float DeltaTimeFactor { get; internal set; } = 1f;
        internal static float DeltaTimeCurrentNibbleSize { get; set; } = SIMULATIONNIBBLESIZE;
        
        /// <summary>
        /// Gibt an, wie lange die aktuelle Welt bereits aktiv ist
        /// </summary>
        public static float WorldTime { get; internal set; } = 0.0f;
        /// <summary>
        /// Gibt an, wie lange die Anwendung bereits aktiv ist
        /// </summary>
        public static float ApplicationTime { get; internal set; } = 0.0f;
        internal static float LastUpdateTime { get; set; } = 0.0f;
        internal static float LastFrameTime { get; set; } = 0.0f;
        internal static int LastSimulationUpdateCycleCount { get; set; } = 0;
        /// <summary>
        /// Schreibt eine Log-Zeile in das Ausgabefenster des Edit-Modus
        /// </summary>
        /// <param name="message"></param>
        public static void LogWriteLine(object message)
        {
            EngineLog.AddMessage(message.ToString());
        }

        /// <summary>
        /// Löscht alle Log-Nachrichten des Edit-Modus
        /// </summary>
        public static void LogClear()
        {
            EngineLog.Clear();
        }

        internal static float _glowRadius = .75f;
        internal const int BLOOMWIDTH = 720;//960;
        internal const int BLOOMHEIGHT = 405;//540;

        /// <summary>
        /// Anzahl der Renderschritte für den Glow-Effekt
        /// </summary>
        public const int MAX_BLOOM_BUFFERS = 7;
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
        private static Vector3 _worldUp = new Vector3(0, 1, 0);

        internal static int _uniformOffsetMultiplier = 1;

        /// <summary>
        /// Verhältnis zwischen innerem und äußerem Glühen (von 0 bis 1, Standard: 0.5)
        /// </summary>
        public static float GlowRadius
        {
            get
            {
                return _glowRadius;
            }
            set
            {
                _glowRadius = HelperGeneral.Clamp(value, 0, 1);
            }
        }

        internal static Matrix4 Matrix4Dummy = Matrix4.Identity;

        internal static int TextureDefault = -1;
        internal static int TextureBlack = -1;
        internal static int TextureWhite = -1;
        internal static int TextureAlpha = -1;
        internal static int TextureNormalEmpty = -1;
        internal static int TextureCubemapEmpty = -1;
        internal static int TextureDepthEmpty = -1;
        internal static int TextureDepthCubeMapEmpty = -1;
        
        internal static float TimeElapsed = 0;

        internal static Vector3 DefaultCameraPosition = new Vector3(0, 0, 10);

        /// <summary>
        /// Welt-Vektor, der angibt, wo 'oben' ist
        /// </summary>
        public static Vector3 WorldUp { get; } = Vector3.UnitY;

        internal static int[] FontTextureArray { get; set; } = new int[4];
        

        internal static void InitializeFontsAndDefaultTextures()
        {
            InitializeFont("anonymous.dds", 0);
            InitializeFont("anonymous2.dds", 1);
            InitializeFont("anonymous3.dds", 2);
            InitializeFont("anonymous4.dds", 3);

            TextureDefault = HelperTexture.LoadTextureInternal("checkerboard.png");
            TextureBlack = HelperTexture.LoadTextureInternal("black.png");
            TextureWhite = HelperTexture.LoadTextureInternal("white.png");
            TextureAlpha = HelperTexture.LoadTextureInternal("alpha.png");
            TextureNormalEmpty = HelperTexture.LoadTextureInternal("normalmap.png");

            TextureDepthEmpty = HelperTexture.CreateEmptyDepthTexture();
            TextureDepthCubeMapEmpty = HelperTexture.CreateEmptyCubemapDepthTexture();
            TextureCubemapEmpty = HelperTexture.CreateEmptyCubemapTexture();
        }
        internal static void InitializeFont(string filename, int index)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "KWEngine3.Assets.Fonts." + filename;
            int textureId = HelperFont.GenerateTexture(resourceName, assembly);
            FontTextureArray[index] = textureId;
        }

        /// <summary>
        /// Empfindlichkeit des Mauszeigers im First-Person-Modus (Standard: 0.05f)
        /// </summary>
        public static float MouseSensitivity { get; set; } = 0.05f;

        internal static GeoModel KWStar;
        internal static GeoModel KWHeart;
        internal static GeoModel KWSkull;
        internal static GeoModel KWDollar;
        internal static GeoModel KWLightBulb;
        internal static GeoModel KWTerrainDefault;

        internal static void InitializeModels()
        {
            Models.Add("KWCube", SceneImporter.LoadModel("kwcube.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWQuad", SceneImporter.LoadModel("kwquad.obj", true, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.fbx", true, SceneImporter.AssemblyMode.Internal));
            KWStar = SceneImporter.LoadModel("star.obj", false, SceneImporter.AssemblyMode.Internal);
            KWHeart = SceneImporter.LoadModel("heart.obj", false, SceneImporter.AssemblyMode.Internal);
            KWSkull = SceneImporter.LoadModel("skull.obj", false, SceneImporter.AssemblyMode.Internal);
            KWDollar = SceneImporter.LoadModel("dollar.obj", false, SceneImporter.AssemblyMode.Internal);
            KWLightBulb = SceneImporter.LoadModel("lightbulb.obj", false, SceneImporter.AssemblyMode.Internal);
            KWTerrainDefault = KWEngine.BuildDefaultTerrainModel("TerrainDefault", 10, 0, 10);

            for (int i = 0; i < ExplosionObject.Axes.Length; i++)
            {
                ExplosionObject.Axes[i] = Vector3.Normalize(ExplosionObject.Axes[i]);
            }
            
        }

        internal static Dictionary<ParticleType, ParticleInfo> ParticleDictionary = new Dictionary<ParticleType, ParticleInfo>();
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
        /// <param name="heightmap">Height Map Textur</param>
        /// <param name="texture">Textur der Oberfläche</param>
        /// <param name="width">Breite</param>
        /// <param name="height">Höhe</param>
        /// <param name="depth">Tiefe</param>
        public static void BuildTerrainModel(string name, string heightmap, string texture, float width, float height, float depth)
        {
            if (Models.ContainsKey(name))
            {
                KWEngine.LogWriteLine("[Terrain] Model name already used");
                return;
            }
            GeoModel terrainModel = new GeoModel();
            terrainModel.Name = name;
            terrainModel.Meshes = new Dictionary<string, GeoMesh>();
            terrainModel.IsValid = true;

            GeoMeshHitbox meshHitBox = new GeoMeshHitbox(0 + width / 2, 0 + height / 2, 0 + depth / 2, 0 - width / 2, 0 - height / 2, 0 - depth / 2, null);
            meshHitBox.Model = terrainModel;
            meshHitBox.Name = name;

            terrainModel.MeshHitboxes = new List<GeoMeshHitbox>();
            terrainModel.MeshHitboxes.Add(meshHitBox);

            GeoTerrain t = new GeoTerrain();
            GeoMesh terrainMesh = t.BuildTerrain2(heightmap, width, height, depth, out GeoMesh sideMeshes, 1, 1, true);
            terrainMesh.Terrain = t;
            GeoMaterial mat = new GeoMaterial();
            mat.BlendMode = BlendingFactor.OneMinusSrcAlpha;
            mat.ColorAlbedo = new Vector4(1, 1, 1, 1);
            mat.ColorEmissive = new Vector4(0, 0, 0, 0);
            mat.Metallic = 0;
            mat.Roughness = 1;

            GeoTexture texDiffuse = new GeoTexture();
            texDiffuse.Filename = texture;
            texDiffuse.Type = TextureType.Albedo;
            texDiffuse.UVMapIndex = 0;
            texDiffuse.UVTransform = new Vector4(1, 1, 0, 0);

            bool dictFound = CustomTextures.TryGetValue(CurrentWorld, out Dictionary<string, int> texDict);

            if (dictFound && texDict.ContainsKey(texture))
            {
                texDiffuse.OpenGLID = texDict[texture];
            }
            else
            {
                int texId = HelperTexture.LoadTextureForModelExternal(texture, out int mipMaps);
                texDiffuse.OpenGLID = texId > 0 ? texId : KWEngine.TextureDefault;

                if (dictFound && texId > 0)
                {
                    texDict.Add(texture, texDiffuse.OpenGLID);
                }
            }
            mat.TextureAlbedo = texDiffuse;


            terrainMesh.Material = mat;
            terrainModel.Meshes.Add("Terrain", terrainMesh);
            terrainModel.Meshes.Add("TerrainSides", sideMeshes);
            KWEngine.Models.Add(name, terrainModel);
        }

        internal static GeoModel BuildDefaultTerrainModel(string name, float width, float height, float depth)
        {
            GeoModel terrainModel = new GeoModel();
            terrainModel.Name = name;
            terrainModel.Meshes = new Dictionary<string, GeoMesh>();
            terrainModel.IsValid = true;

            GeoMeshHitbox meshHitBox = new GeoMeshHitbox(0 + width / 2, 0 + height / 2, 0 + depth / 2, 0 - width / 2, 0 - height / 2, 0 - depth / 2, null);
            meshHitBox.Model = terrainModel;
            meshHitBox.Name = name;

            terrainModel.MeshHitboxes = new List<GeoMeshHitbox>();
            terrainModel.MeshHitboxes.Add(meshHitBox);

            GeoTerrain t = new GeoTerrain();
            GeoMesh terrainMesh = t.BuildTerrain2(null, width, height, depth, out GeoMesh sideMeshes, 1f, 1f, false);
            terrainMesh.Terrain = t;
            GeoMaterial mat = new GeoMaterial();
            mat.BlendMode = BlendingFactor.OneMinusSrcAlpha;
            mat.ColorAlbedo = new Vector4(1, 1, 1, 1);
            mat.ColorEmissive = new Vector4(0, 0, 0, 0);
            mat.Metallic = 0;
            mat.Roughness = 1;

            GeoTexture texDiffuse = new GeoTexture();
            texDiffuse.Filename = "Default terrain texture";
            texDiffuse.Type = TextureType.Albedo;
            texDiffuse.UVMapIndex = 0;
            texDiffuse.UVTransform = new Vector4(1f, 1f, 0f, 0f);
            texDiffuse.OpenGLID = KWEngine.TextureWhite;
            mat.TextureAlbedo = texDiffuse;

            terrainMesh.Material = mat;
            terrainModel.Meshes.Add("Terrain", terrainMesh);
            terrainModel.Meshes.Add("TerrainSideMeshes", sideMeshes);

            return terrainModel;
        }

        /// <summary>
        /// Lädt ein Modell aus einer Datei
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="filename">Datei des Modells</param>
        /// <param name="callerName">(wird für interne Zwecke benötigt)</param>
        public static void LoadModel(string name, string filename, [CallerMemberName] string callerName = "")
        {
            if (callerName != "Prepare" && callerName != "BuildWorld")
            {
                KWEngine.LogWriteLine("[Import] Models must be imported in Prepare()");
                return; 
            }

            if (Models.ContainsKey(name.Trim()))
            {
                if (callerName != "BuildWorld")
                {
                    KWEngine.LogWriteLine("[Model] Model " + (name == null ? "" : name.Trim()) + " already exists");
                    return;
                }
            }
            GeoModel m = SceneImporter.LoadModel(filename, true, SceneImporter.AssemblyMode.File);
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

        /// <summary>
        /// Erstellt eine Liste der im 3D-Modell verfügbaren Knochennamen
        /// </summary>
        /// <param name="modelname">Name des Modells (muss zuvor importiert worden sein)</param>
        /// <returns>Liste der Knochennamen</returns>
        public static List<string> GetModelBoneNames(string modelname)
        {
            bool result = Models.TryGetValue(modelname, out GeoModel model);
            List<string> resultList = new List<string>();
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
    }
}
