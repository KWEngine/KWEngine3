using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    public sealed class TerrainObject
    {
        public int ID { get; internal set; } = 0;
        public bool IsInsideScreenSpace { get; internal set; } = true;
        public bool IsShadowCaster { get { return _isShadowCaster; } set { _isShadowCaster = value; } }
        public bool IsCollisionObject { get { return _isCollisionObject; } set { _isCollisionObject = value; } }

        internal TerrainObject()
            : this(null)
        {

        }

        public TerrainObject(string modelname)
        {
            bool modelSetSuccessfully = SetModel(modelname);
            if (!modelSetSuccessfully)
            {
                KWEngine.LogWriteLine("[TerrainObject] Cannot set " + (modelname == null ? "" : modelname.Trim()));
            }

            InitStates();
        }

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

        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._colorTint = new Vector3(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1));
        }

        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1),
                MathHelper.Clamp(intensity, 0, 2));
        }

        public void SetMetallic(float m)
        {
            _gModel._metallic = MathHelper.Clamp(m, 0f, 1f);
        }

        public void SetMetallicType(MetallicType type)
        {
            _gModel._metallicType = type;
        }

        internal void SetMetallicType(int typeIndex)
        {
            SetMetallicType((MetallicType)typeIndex);
        }

        public void SetRoughness(float r)
        {
            _gModel._roughness = MathHelper.Clamp(r, 0.00001f, 1f);
        }

        public void SetTexture(string filename, TextureType type = TextureType.Albedo, int meshId = 0)
        {
            if (filename == null)
                filename = "";
            _gModel.SetTexture(filename.ToLower().Trim(), type, meshId);
        }

        public void SetTextureOffset(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(_stateCurrent._uvTransform.X, _stateCurrent._uvTransform.Y, x, y);
        }

        public void SetTextureRepeat(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(x, y, _stateCurrent._uvTransform.Z, _stateCurrent._uvTransform.W);
        }

        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        internal GameObjectModel _gModel;
        internal TerrainObjectState _statePrevious;
        internal TerrainObjectState _stateCurrent;
        internal TerrainObjectState _stateRender;
        internal List<TerrainObjectHitbox> _hitboxes = new List<TerrainObjectHitbox>();
        internal string _name = "undefined terrain object name";

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

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetPosition(Vector3 pos)
        {
            _stateCurrent._position = pos;
            UpdateModelMatrixAndHitboxes();
            
        }

        public void MoveOffset(float x, float y, float z)
        {
            MoveOffset(new Vector3(x, y, z));
        }

        public void MoveOffset(Vector3 offset)
        {
            //SetPosition(_stateCurrent._position + offset * KWEngine.DeltaTimeCurrentNibbleSize);
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
