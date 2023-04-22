using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal class GameObjectHitbox
    {
        public Vector3[] _vertices = new Vector3[8];
        public Vector3[] _normals = new Vector3[3];
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
        public Matrix4 _modelMatrixFinal = Matrix4.Identity;

        internal GameObject Owner { get; private set; }
        internal GeoMeshHitbox _mesh;
        internal OctreeNode _currentOctreeNode = null;
        internal Vector3 _offset;

        public GameObjectHitbox(GameObject owner, GeoMeshHitbox mesh, Vector3 offset)
        {
            Owner = owner;
            _mesh = mesh;
            _offset = offset;

            if (_mesh.IsExtended)
            {
                _vertices = new Vector3[mesh.Vertices.Length];
                _normals = new Vector3[mesh.Normals.Length];
            }
        }

        internal bool Update(ref Vector3 gCenter)
        {
            if (!IsActive)
                return false;

            if (Owner._stateCurrent._scaleHitbox != Vector3.One)
            {
                Matrix4.Mult(_mesh.Transform, Matrix4.CreateScale(Owner._stateCurrent._scaleHitbox), out Matrix4 tempMatrix);
                Matrix4.Mult(tempMatrix, HelperMatrix.CreateModelMatrix(Owner._stateCurrent), out _modelMatrixFinal);
            }
            else
            {
                Matrix4.Mult(_mesh.Transform, HelperMatrix.CreateModelMatrix(Owner._stateCurrent), out _modelMatrixFinal);
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < _vertices.Length; i++)
            {
                if (i < _normals.Length)
                {
                    Vector3.TransformNormal(_mesh.Normals[i], _modelMatrixFinal, out _normals[i]);
                    _normals[i].NormalizeFast();
                }
                Vector3.TransformPosition(_mesh.Vertices[i] + _offset, _modelMatrixFinal, out _vertices[i]);

                if (_vertices[i].X > maxX)
                    maxX = _vertices[i].X;
                if (_vertices[i].X < minX)
                    minX = _vertices[i].X;
                if (_vertices[i].Y > maxY)
                    maxY = _vertices[i].Y;
                if (_vertices[i].Y < minY)
                    minY = _vertices[i].Y;
                if (_vertices[i].Z > maxZ)
                    maxZ = _vertices[i].Z;
                if (_vertices[i].Z < minZ)
                    minZ = _vertices[i].Z;
            }
            Vector3.TransformPosition(_mesh.Center + _offset, _modelMatrixFinal, out _center);
            gCenter += _center;

            float xWidth = maxX - minX;
            float yWidth = maxY - minY;
            float zWidth = maxZ - minZ;
            
            _low = minY;
            _high = maxY;
            _left = minX;
            _right = maxX;
            _back = minZ;
            _front = maxZ;

            _averageDiameter = (xWidth + yWidth + zWidth) / 3;
            _fullDiameter = -1;
            if (xWidth > _fullDiameter)
                _fullDiameter = xWidth;
            if (yWidth > _fullDiameter)
                _fullDiameter = yWidth;
            if (zWidth > _fullDiameter)
                _fullDiameter = zWidth;

            _dimensions.X = xWidth;
            _dimensions.Y = yWidth;
            _dimensions.Z = zWidth;

            return true;
        }

        public bool IsExtended
        {
            get
            {
                return _mesh != null ? _mesh.IsExtended : false;
            }
        }

        public float[] GetVerticesAsFloatArray()
        {
            float[] vertices = new float[_vertices.Length * 3];
            for (int i = 0, j = 0; i < _vertices.Length; i++)
            {
                vertices[j + 0] = _vertices[i].X;
                vertices[j + 1] = _vertices[i].Y;
                vertices[j + 2] = _vertices[i].Z;
                j += 3;
            }
            return vertices;
        }

        internal void GetVerticesForTriangleFace(int faceIndex, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 normal)
        {
            v1 = _vertices[_mesh.Faces[faceIndex].Vertices[0]];
            v2 = _vertices[_mesh.Faces[faceIndex].Vertices[1]];
            v3 = _vertices[_mesh.Faces[faceIndex].Vertices[2]];
            normal = _normals[_mesh.Faces[faceIndex].Normal];
        }

        internal void GetVerticesForCubeFace(int faceIndex, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 normal)
        {
            Vector3 qv1 = _vertices[_mesh.Faces[faceIndex].Vertices[0]];
            Vector3 qv2 = _vertices[_mesh.Faces[faceIndex].Vertices[1]];
            Vector3 qv3 = _vertices[_mesh.Faces[faceIndex].Vertices[2]];
            Vector3 qv4 = _vertices[_mesh.Faces[faceIndex].Vertices[3]];

            v1 = qv1;
            v2 = qv3;
            v3 = qv4;

            v4 = qv1;
            v5 = qv2;
            v6 = qv3;

            normal = _mesh.Faces[faceIndex].Flip ? -_normals[_mesh.Faces[faceIndex].Normal] : _normals[_mesh.Faces[faceIndex].Normal];
        }
    }
}
