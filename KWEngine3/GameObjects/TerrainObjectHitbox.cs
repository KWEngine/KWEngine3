using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal class TerrainObjectHitbox
    {
        public Vector3 _center = new Vector3(0, 0, 0);
        public Vector3 _dimensions = new Vector3(0, 0, 0);
        public float _low = 0;
        public float _high = 0;
        public float _left = 0;
        public float _right = 0;
        public float _back = 0;
        public float _front = 0;
        public float _averageDiameter = 0;
        public float _fullDiameter = 0;

        public bool IsActive { get { return _mesh.IsActive; } }

        internal TerrainObject Owner { get; private set; }
        internal GeoMeshHitbox _mesh;

        public TerrainObjectHitbox(TerrainObject owner, GeoMeshHitbox mesh)
        {
            Owner = owner;
            _mesh = mesh;
        }

        internal bool Update(ref Vector3 gCenter)
        {
            if (!IsActive)
                return false;

            _center = Owner._stateCurrent._position + new Vector3(0f, _mesh.height / 2f, 0f);
            gCenter += _center;

            _averageDiameter = (_mesh.width + _mesh.height + _mesh.depth) / 3f;
            _fullDiameter = _averageDiameter;
           
            _dimensions.X = _mesh.width;
            _dimensions.Y = _mesh.height;
            _dimensions.Z = _mesh.depth;

            return true;
        }
    }
}
