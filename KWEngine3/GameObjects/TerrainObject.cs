using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

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

        internal TerrainObject()
            : this(null)
        {

        }

        /// <summary>
        /// Standardkonstruktor für ein Terrain-Objekt
        /// </summary>
        /// <param name="modelname">Terrain-Modellname</param>
        public TerrainObject(string modelname)
        {
            bool modelSetSuccessfully = SetModel(modelname);
            if (!modelSetSuccessfully)
            {
                KWEngine.LogWriteLine("[TerrainObject] Cannot set " + (modelname == null ? "" : modelname.Trim()));
            }

            InitStates();
        }

        /// <summary>
        /// Setzt das 3D-Modell des Terrains
        /// </summary>
        /// <param name="modelname">Name des Modells</param>
        /// <returns>true, wenn das Setzen des Modells erfolgreich war</returns>
        public bool SetModel(string modelname)
        {
            if (modelname == null || modelname.Length == 0)
            {
                _gModel = new GameObjectModel(KWEngine.KWTerrainDefault);
                for (int i = 0; i < _gModel.Material.Length; i++)
                {
                    _gModel.Material[i] = KWEngine.KWTerrainDefault.Meshes.Values.ToArray()[i].Material;
                }
                InitHitboxes();
                InitRenderStateMatrices();
                InitStates();
                
                return true;
            }

            bool modelFound = KWEngine.Models.TryGetValue(modelname, out GeoModel model);
            if (modelFound)
            {
                _gModel = new GameObjectModel(model);
                for (int i = 0; i < _gModel.Material.Length; i++)
                {
                    _gModel.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                }
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
            _gModel._metallic = MathHelper.Clamp(m, 0f, 1f);
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
            _gModel._roughness = MathHelper.Clamp(r, 0.00001f, 1f);
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
            _gModel.SetTexture(filename.ToLower().Trim(), type, meshId);
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
        /// Name des Objekts
        /// </summary>
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        internal GameObjectModel _gModel;
        internal TerrainObjectState _statePrevious;
        internal TerrainObjectState _stateCurrent;
        internal TerrainObjectState _stateRender;
        internal List<TerrainObjectHitbox> _hitboxes = new List<TerrainObjectHitbox>();
        internal string _name = "(no name)";

        internal void InitHitboxes()
        {
            _hitboxes.Clear();
            foreach (GeoMeshHitbox gmh in _gModel.ModelOriginal.MeshHitboxes)
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

        internal bool _isCollisionObject = false;
        internal bool _isShadowCaster = false;

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
    }
}
