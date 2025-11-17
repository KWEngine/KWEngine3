using glTFLoader.Schema;
using KWEngine3.Exceptions;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Terrain-Objekte
    /// </summary>
    public sealed class TerrainObject
    {
        /// <summary>
        /// Gibt an, ob sich das Objekt aktuell in der Welt befindet
        /// </summary>
        public bool IsInCurrentWorld
        {
            get
            {
                return _myWorld == KWEngine.CurrentWorld;
            }
        }

        /// <summary>
        /// Interne ID des Objekts
        /// </summary>
        public ushort ID { get; internal set; } = 0;
        /// <summary>
        /// Gibt an, ob das Objekt gerade auf dem Bildschirm zu sehen ist
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;
        internal bool IsInsideScreenSpaceForRenderPass { get; set; } = true;
        /// <summary>
        /// Gibt an, ob das Objekt Schatten werfen und empfangen kann
        /// </summary>
        public bool IsShadowCaster { get { return _isShadowCaster; } set { _isShadowCaster = value; } }
        /// <summary>
        /// Gibt an, ob das Objekt ein Kollisionsobjekt ist
        /// </summary>
        public bool IsCollisionObject { get { return _isCollisionObject; } set { _isCollisionObject = value; } }

        /// <summary>
        /// Gibt an oder setzt fest, ob die Instanz sichtbar ist (Standard: true)
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gibt an, ab welchem Steigungsfaktor (zwischen 0 und 1) die Slope-Textur statt der regulären Terrain-Textur verwendet wird
        /// </summary>
        /// <param name="factor">Steigungsfaktor (zwischen 0 und 1, Standardwert: 0.5f)</param>
        public void SetTextureSlopeBlendFactor(float factor)
        {
            _textureSlopeBlendFactor = Math.Clamp(Math.Abs(factor), 0.0f, 1.0f);
        }

        /// <summary>
        /// Gibt an, wie weich die Übergänge zwischen den beiden Texturen (reguläre und Slope-Textur) verlaufen sollen (Standard: 0.05f)
        /// </summary>
        /// <param name="factor">Glättungsfaktor (zwischen 0f und 1.0f)</param>
        public void SetTextureSlopeBlendSmoothingFactor(float factor)
        {
            _textureSlopeSmoothingFactor = Math.Clamp(Math.Abs(factor), 0f, 1f);
        }

        /// <summary>
        /// Name des Objekts
        /// </summary>
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        /// <summary>
        /// Gibt an, ab welcher Entfernung dieses Terrain weniger detailliert dargestellt werden soll (Standard: 64 Längeneinheiten)
        /// </summary>
        public TerrainThresholdValue TessellationThreshold { get; set; } = TerrainThresholdValue.T64;

        internal float _textureSlopeBlendFactor = 0.5f;
        internal float _textureSlopeSmoothingFactor = 0.05f;
        internal EngineObjectModel _gModel;
        internal TerrainObjectState _statePrevious;
        internal TerrainObjectState _stateCurrent;
        internal TerrainObjectState _stateRender;
        internal List<TerrainObjectHitbox> _hitboxes = new List<TerrainObjectHitbox>();
        internal string _name = "(no name)";
        internal bool _isCollisionObject = false;
        internal bool _isShadowCaster = false;
        internal int _idFromImport = -1;
        internal int _heightmap = -1;

        /// <summary>
        /// Breite des Terrain-Objekts
        /// </summary>
        public int Width
        {
            get; internal set;
        }
        /// <summary>
        /// Tiefe des Terrain-Objekts
        /// </summary>
        public int Depth
        {
            get; internal set;
        }

        /// <summary>
        /// Höhe des Terrain-Objekts
        /// </summary>
        public int Height
        {
            get; internal set;
        }

        internal TerrainObject()
        {

        }

        /// <summary>
        /// Standardkonstruktor für ein Terrain-Objekt
        /// </summary>
        /// <param name="name">Name des Terrain-Modells</param>
        public TerrainObject(string name)
        {
            bool modelSet = SetModel(name);
            if(!modelSet)
            {
                throw new EngineException("[TerrainObject] Terrain model name not found");
            }

        }

        /// <summary>
        /// Setzt die Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 1)</param>
        /// <param name="g">Grünanteil (0 bis 1)</param>
        /// <param name="b">Blauanteil (0 bis 1)</param>
        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._colorTint = new Vector3(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1));
        }

        /// <summary>
        /// Setzt die selbstleuchtende Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 1)</param>
        /// <param name="g">Grünanteil (0 bis 1)</param>
        /// <param name="b">Blauanteil (0 bis 1)</param>
        /// <param name="intensity">Intensität der Lichtabstrahlung (0 bis 2)</param>
        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1),
                MathHelper.Clamp(intensity, 0, 2));
        }

        /// <summary>
        /// Setzt fest, wie metallisch das Objekt ist
        /// </summary>
        /// <param name="m">Faktor (0 bis 1)</param>
        public void SetMetallic(float m)
        {
            _gModel._metallicTerrain = MathHelper.Clamp(m, 0f, 1f);
        }

        /// <summary>
        /// Setzt den Metalltyp des Objekts
        /// </summary>
        /// <param name="type">Typ</param>
        public void SetMetallicType(MetallicType type)
        {
            _gModel._metallicType = type;
        }

        internal void SetMetallicType(int typeIndex)
        {
            SetMetallicType((MetallicType)typeIndex);
        }

        /// <summary>
        /// Setzt die Rauheit der Oberfläche (0 bis 1, Standard: 1)
        /// </summary>
        /// <param name="r">Rauheit</param>
        public void SetRoughness(float r)
        {
            _gModel._roughnessTerrain = MathHelper.Clamp(r, 0.00001f, 1f);
        }

        /// <summary>
        /// Setzt die Haupttextur des Terrains
        /// </summary>
        /// <param name="filename">Dateiname (inkl. relativem Pfad)</param>
        /// <param name="type">Texturtyp (Standard: Albedo)</param>
        public void SetTexture(string filename, TextureType type = TextureType.Albedo)
        {
            if (KWEngine.CurrentWorld == null)
            {
                KWEngine.LogWriteLine("[TerrainObject] No current world found, cannot load texture");
                return;
            }

            if (filename == null)
                filename = "";
            if(type == TextureType.Height && KWEngine.Window._renderQuality < RenderQualityLevel.Default)
            {
                KWEngine.LogWriteLine("[TerrainObject] Height texture not available at your current render profile");
                return;
            }

            _gModel.SetTexture(filename.Trim(), type, 0);
        }

        /// <summary>
        /// Setzt die Textur des Terrains für Anstiege und Gefälle
        /// </summary>
        /// <param name="filename">Dateiname (inkl. relativem Pfad)</param>
        /// <param name="type">Texturtyp (Standard: Albedo)</param>
        public void SetTextureForSlope(string filename, TextureType type = TextureType.Albedo)
        {
            if(type == TextureType.Height)
            {
                KWEngine.LogWriteLine("[TerrainObject] Height texture feature not available for slopes");
                return;
            }
            _gModel.SetTexture(filename.Trim(), type, 1);
        }

        /// <summary>
        /// Setzt die Texturverschiebung
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureOffset(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(_stateCurrent._uvTransform.X, _stateCurrent._uvTransform.Y, x, y);
        }

        /// <summary>
        /// Setzt die Texturwiederholung in x-/y-Richtung (Standard: 1)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureRepeat(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(x, y, _stateCurrent._uvTransform.Z, _stateCurrent._uvTransform.W);
        }

        /// <summary>
        /// Setzt die Texturverschiebung für starke Steigungen / Gefälle
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureOffsetForSlope(float x, float y)
        {
            _stateCurrent._uvTransformSlope = new Vector4(_stateCurrent._uvTransformSlope.X, _stateCurrent._uvTransformSlope.Y, x, y);
        }

        /// <summary>
        /// Setzt die Texturwiederholung in x-/y-Richtung (Standard: 1) für starke Steigungen / Gefälle
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureRepeatForSlope(float x, float y)
        {
            _stateCurrent._uvTransformSlope = new Vector4(x, y, _stateCurrent._uvTransformSlope.Z, _stateCurrent._uvTransformSlope.W);
        }

        internal void InitHitboxes()
        {
            _hitboxes.Clear();
            foreach (GeoMeshHitbox gmh in _gModel.ModelOriginal.MeshCollider.MeshHitboxes)
            {
                _hitboxes.Add(new TerrainObjectHitbox(this, gmh));
            }
        }

        internal void InitStates()
        {
            _stateCurrent = new TerrainObjectState(this);
            _stateRender = new TerrainObjectState(this);
            UpdateModelMatrixAndHitboxes();
            _statePrevious = _stateCurrent;
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="pos">Position</param>
        public void SetPosition(Vector3 pos)
        {
            _stateCurrent._position = pos;
            UpdateModelMatrixAndHitboxes();
            
        }

        /// <summary>
        /// Bewegt das Objekt relativ zur ursprünglichen Position
        /// </summary>
        /// <param name="x">x-Bewegung</param>
        /// <param name="y">y-Bewegung</param>
        /// <param name="z">z-Bewegung</param>
        public void MoveOffset(float x, float y, float z)
        {
            MoveOffset(new Vector3(x, y, z));
        }

        /// <summary>
        /// Bewegt das Objekt relativ zur ursprünglichen Position
        /// </summary>
        /// <param name="offset">Verschiebung</param>
        public void MoveOffset(Vector3 offset)
        {
            SetPosition(_stateCurrent._position + offset);
        }

        /// <summary>
        /// Setzt die Skalierung für das Parallax-Occlusion-Mapping (falls eine Height-Textur für das Objekt verwendet wird)
        /// </summary>
        /// <remarks>RenderQuality muss auf 'Default' oder höher eingestellt sein, damit dieser Effekt eintritt</remarks>
        /// <param name="scale">Skalierungsfaktor (Standardwert: 0.0f, Wertebereich: 0.0f bis 1.0f)</param>
        public void SetParallaxOcclusionMappingScale(float scale)
        {
            if (KWEngine.Window._renderQuality < RenderQualityLevel.Default)
            {
                KWEngine.LogWriteLine("[TerrainObject] Parallax Mapping is disabled on your current rendering profile");
            }
            _pomScale = MathHelper.Clamp(scale, 0f, 1.0f) * 0.1f;
        }

        internal void UpdateModelMatrixAndHitboxes()
        {
            _stateCurrent._modelMatrix = HelperMatrix.CreateModelMatrix(ref _stateCurrent._position);
            _stateCurrent._center = Vector3.Zero;

            _hitboxes[0].Update(ref _stateCurrent._center);

            _stateCurrent._dimensions.X = _hitboxes[0]._dimensions.X;
            _stateCurrent._dimensions.Y = _hitboxes[0]._dimensions.Y;
            _stateCurrent._dimensions.Z = _hitboxes[0]._dimensions.Z;
        }

        internal void InitRenderStateMatrices()
        {
            _stateRender._modelMatrix = Matrix4.Identity;
        }

        internal bool SetModel(string modelname)
        {
            if (modelname == null || modelname.Length == 0)
            {
                return false;
            }

            bool modelFound = KWEngine.Models.TryGetValue(modelname, out GeoModel model);
            if (modelFound && model.IsTerrain)
            {
                _gModel = new EngineObjectModel(model);
                _gModel.Material[0] = model.Meshes.Values.ToArray()[0].Material;
                _gModel.Material[1] = model.Meshes.Values.ToArray()[0].Material;

                Width = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetWidth();
                Height = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetHeight();
                Depth = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetDepth();
                _heightmap = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain._texHeight;

                InitHitboxes();
                InitRenderStateMatrices();
                InitStates();
                GenerateCoarseSectors();
            }
            return modelFound;
        }

        internal void GenerateCoarseSectors()
        {
            GeoTerrain geoterrain = _gModel.ModelOriginal.Meshes.Values.ElementAt(0).Terrain;

            // define sector sizes:
            int startXUltra = -Width / 2;
            int startZUltra = -Depth / 2;
            int mSectorLengthCoarseUltra = 16;
            int mSectorLengthCoarse = 4;
            int sectorCountXUltra = Width / mSectorLengthCoarseUltra;
            int sectorCountZUltra = Depth / mSectorLengthCoarseUltra;

            _sectorMapCoarseUltra = new TerrainSectorCoarseUltra[sectorCountXUltra, sectorCountZUltra];
            for (int i = 0; i < sectorCountXUltra; i++)
            {
                for (int j = 0; j < sectorCountZUltra; j++)
                {
                    int sectorLeftUltra = startXUltra + i * mSectorLengthCoarseUltra;
                    int sectorRightUltra = startXUltra + (i + 1) * mSectorLengthCoarseUltra;
                    int sectorFrontUltra = startZUltra + (j + 1) * mSectorLengthCoarseUltra;
                    int sectorBackUltra = startZUltra + j * mSectorLengthCoarseUltra;

                    TerrainSectorCoarseUltra currentSectorUltraCoarse = new TerrainSectorCoarseUltra(
                        sectorLeftUltra,
                        sectorRightUltra,
                        sectorBackUltra,
                        sectorFrontUltra
                        );

                    // generate inner (less coarse but still coarse) sectors:
                    int sectorCountXInner = (sectorRightUltra - sectorLeftUltra) / mSectorLengthCoarse;
                    int sectorCountZInner = (sectorFrontUltra - sectorBackUltra) / mSectorLengthCoarse;
                    for (int k = 0; k < sectorCountXInner; k++)
                    {
                        for (int l = 0; l < sectorCountZInner; l++)
                        {
                            int sectorLeftInner = sectorLeftUltra + k * mSectorLengthCoarse;
                            int sectorRightInner = sectorLeftUltra + (k + 1) * mSectorLengthCoarse;
                            int sectorFrontInner = sectorBackUltra + (l + 1) * mSectorLengthCoarse;
                            int sectorBackInner = sectorBackUltra + l * mSectorLengthCoarse;

                            TerrainSectorCoarse currentSectorCoarse = new TerrainSectorCoarse(sectorLeftInner, sectorRightInner, sectorBackInner, sectorFrontInner);

                            // Find usual sectors for the coarse one:
                            for (int m = 0; m < geoterrain.mSectorMap.GetLength(0); m++)
                            {
                                for (int n = 0; n < geoterrain.mSectorMap.GetLength(1); n++)
                                {
                                    Sector s = geoterrain.mSectorMap[m, n];

                                    if (HelperIntersection.CheckAABBCollisionNoTouch(s.Left, s.Right, s.Back, s.Front, 
                                        currentSectorCoarse.Left, currentSectorCoarse.Right, currentSectorCoarse.Back, currentSectorCoarse.Front))
                                    {

                                        TerrainSector ts = new TerrainSector(s.Left, s.Right, s.Back, s.Front, this);
                                        ts.AddTriangles(s.Triangles);
                                        currentSectorCoarse.AddSector(ts);
                                    }
                                }
                            }
                            currentSectorUltraCoarse.AddSector(currentSectorCoarse);
                        }
                    }
                    _sectorMapCoarseUltra[i, j] = currentSectorUltraCoarse;
                }
            }
        }

        internal TerrainSectorCoarseUltra[,] _sectorMapCoarseUltra;
        internal World _myWorld = null;
        internal float _pomScale = 0.0f;
    }
}
