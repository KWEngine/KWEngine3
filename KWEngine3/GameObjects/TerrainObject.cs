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
        /// Interne ID des Objekts
        /// </summary>
        public int ID { get; internal set; } = 0;
        /// <summary>
        /// Gibt an, ob das Objekt gerade auf dem Bildschirm zu sehen ist
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;
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
        /// Name des Objekts
        /// </summary>
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        internal EngineObjectModel _gModel;
        internal TerrainObjectState _statePrevious;
        internal TerrainObjectState _stateCurrent;
        internal TerrainObjectState _stateRender;
        internal List<TerrainObjectHitbox> _hitboxes = new List<TerrainObjectHitbox>();
        internal string _name = "(no name)";
        internal bool _isCollisionObject = false;
        internal bool _isShadowCaster = false;
        internal int _heightMap = -1;
        internal int _idFromImport = -1;
        internal int _width = 16;
        internal int _depth = 16;
        internal float _height = 0;

        internal TerrainObject()
        {

        }

        /// <summary>
        /// Standardkonstruktor für ein Terrain-Objekt
        /// </summary>
        /// <param name="name">Name des Terrain-Modells</param>
        /// <param name="heightmap">Dateiname der Height-Map-Textur (idealerweise im PNG-Dateiformat)</param>
        /// <param name="width">Breite des Terrains (x-Achse, Mindestwert: 16, Angaben werden auf eine durch 16 teilbare Zahl gerundet)</param>
        /// <param name="depth">Tiefe des Terrains (z-Achse, Mindestwert: 16, Angaben werden auf eine durch 16 teilbare Zahl gerundet)</param>
        /// <param name="height">Maximale Höhe des Terrains (muss ein positiver Wert sein zwischen 0 und 256 sein)</param>
        public TerrainObject(string name, string heightmap, int width = 16, int depth = 16, float height = 0)
        {
            if(heightmap == null || !File.Exists(heightmap))
            {
                KWEngine.LogWriteLine("[TerrainObject] Heightmap file is invalid");
                throw new EngineException("[TerrainObject] Heightmap file is invalid");
            }
            if(name == null || name.Trim().Length == 0)
            {
                KWEngine.LogWriteLine("[TerrainObject] Name is invalid");
                throw new EngineException("[TerrainObject] Name is invalid");
            }

            _width = Math.Max(width - (width % 16), 16);
            _depth = Math.Max(depth - (depth % 16), 16);
            _height = Math.Clamp(height, 0f, 256f);

            BuildTerrainModel(name, heightmap, width, height, depth);

            InitHitboxes();
            InitRenderStateMatrices();
            InitStates();
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
        /// <param name="r">Rotanteil (0 bis 2)</param>
        /// <param name="g">Grünanteil (0 bis 2)</param>
        /// <param name="b">Blauanteil (0 bis 2)</param>
        /// <param name="intensity">Intensität der Lichtabstrahlung (0 bis 10)</param>
        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(r, 0, 2),
                MathHelper.Clamp(g, 0, 2),
                MathHelper.Clamp(b, 0, 2),
                MathHelper.Clamp(intensity, 0, 10));
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
        /// Setzt die Textur des Objekts
        /// </summary>
        /// <param name="filename">Dateiname (inkl. relativem Pfad)</param>
        /// <param name="type">Texturtyp (Standard: Albedo)</param>
        /// <param name="meshId">interne Nummer des 3D-Modellteils (Standard: 0)</param>
        public void SetTexture(string filename, TextureType type = TextureType.Albedo, int meshId = 0)
        {
            if (filename == null)
                filename = "";
            _gModel.SetTexture(filename.Trim(), type, meshId);
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

        internal static void BuildTerrainModel(string name, string heightmap, int width, float height, int depth)
        {
            GeoModel terrainModel = new()
            {
                Name = name,
                Meshes = new(),
                IsValid = true
            };

            GeoMeshHitbox meshHitBox = new(0 + width / 2, 0 + height / 2, 0 + depth / 2, 0 - width / 2, 0 - height / 2, 0 - depth / 2, null)
            {
                Model = terrainModel,
                Name = name
            };

            terrainModel.MeshCollider.MeshHitboxes = new()
            {
                meshHitBox
            };

            GeoTerrain t = new();
            GeoMesh terrainMesh = t.BuildTerrain(heightmap, width, height, depth, true);
            terrainMesh.Terrain = t;
            terrainMesh.Material = GenerateDefaultMaterial();

            terrainModel.Meshes.Add("Terrain", terrainMesh);
            KWEngine.Models.Add(name, terrainModel);
        }

        internal static GeoMaterial GenerateDefaultMaterial()
        {
            return new GeoMaterial()
            {
                BlendMode = BlendingFactor.OneMinusSrcAlpha,
                ColorAlbedo = new Vector4(1, 1, 1, 1),
                ColorEmissive = new Vector4(0, 0, 0, 0),
                Metallic = 0,
                Roughness = 1,
                TextureAlbedo = new()
                {
                    Filename = "",
                    Height = 256,
                    Width = 256,
                    IsEmbedded = false,
                    IsKWEngineTexture = true,
                    MipMaps = 0,
                    UVMapIndex = 0,
                    Type = TextureType.Albedo,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    OpenGLID = KWEngine.TextureCheckerboard
                }
            };
        }

        internal GeoMaterial GenerateMaterial(string texture)
        {
            GeoMaterial mat = new GeoMaterial();

            GeoTexture texDiffuse = new()
            {
                Filename = texture,
                Type = TextureType.Albedo,
                UVMapIndex = 0,
                UVTransform = new Vector4(1, 1, 0, 0)
            };

            if (KWEngine.CurrentWorld._customTextures.ContainsKey(texture))
            {
                texDiffuse.OpenGLID = KWEngine.CurrentWorld._customTextures[texture];
            }
            else
            {
                int texId = HelperTexture.LoadTextureForModelExternal(texture, out int mipMaps);
                texDiffuse.OpenGLID = texId > 0 ? texId : KWEngine.TextureDefault;

                if (texId > 0)
                {
                    KWEngine.CurrentWorld._customTextures.Add(texture, texDiffuse.OpenGLID);
                }
            }
            mat.TextureAlbedo = texDiffuse;
            return mat;
        }
    }
}
