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
                for (int i = 0; i < _gModel.Material.Length; i++)
                {
                    _gModel.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                }

                Width = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetWidth();
                Height = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetHeight();
                Depth = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.GetDepth();
                _heightmap = _gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain._texHeight;

                InitHitboxes();
                InitRenderStateMatrices();
                InitStates();
            }
            return modelFound;
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
        public void SetTexture(string filename, TextureType type = TextureType.Albedo)
        {
            if (filename == null)
                filename = "";
            _gModel.SetTexture(filename.Trim(), type, 0);
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

        internal World _myWorld = null;

        /*
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
                texDiffuse.OpenGLID = KWEngine.CurrentWorld._customTextures[texture].ID;
            }
            else
            {
                int texId = HelperTexture.LoadTextureForModelExternal(texture, out int mipMaps);
                texDiffuse.OpenGLID = texId > 0 ? texId : KWEngine.TextureDefault;

                if (texId > 0)
                {
                    KWEngine.CurrentWorld._customTextures.Add(texture, new KWTexture(texDiffuse.OpenGLID, TextureTarget.Texture2D));
                }
            }
            mat.TextureAlbedo = texDiffuse;
            return mat;
        }
        */
    }
}
