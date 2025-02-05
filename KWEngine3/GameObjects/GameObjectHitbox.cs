using Assimp;
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
        public Vector3 _dimensionsAABB = new Vector3(1, 1, 1);
        public Vector3 _dimensionsOBB = new Vector3(1, 1, 1);
        public float _low = 0;
        public float _high = 0;
        public float _left = 0;
        public float _right = 0;
        public float _back = 0;
        public float _front = 0;
        public float _averageDiameter = 0;
        public float _fullDiameterAABB = 0;

        public Matrix4 _modelMatrixFinal = Matrix4.Identity;
        public Matrix4 _modelMatrixFinalInv = Matrix4.Identity;

        internal GameObject Owner { get; private set; }
        internal GeoMeshHitbox _mesh;
        internal OctreeNode _currentOctreeNode = null;
        internal Matrix4 _capsulePreTransform = Matrix4.Identity;
        internal Matrix4 _meshPreTransform = Matrix4.Identity;
        internal bool _isCapsule = false;
        internal ColliderType _colliderType = ColliderType.ConvexHull;

        internal Vector2i indexLeftRightMostVertex = new Vector2i(0);
        internal Vector2i indexBackFrontMostVertex = new Vector2i(0);
        internal Vector2i indexBottomTopMostVertex = new Vector2i(0);

        public override string ToString()
        {
            return _mesh.Name + "(" + _mesh.Model.Name + ")";
        }

        public GameObjectHitbox(GameObject owner, GeoMeshHitbox mesh, Vector3 offset, Vector3 center, Vector3 frontbottomleft, Vector3 backtopright)
        {
            Owner = owner;
            _mesh = mesh;
            _colliderType = mesh._colliderType;
            _isCapsule = true;
            _capsulePreTransform = Matrix4.CreateScale(backtopright.X - frontbottomleft.X, backtopright.Y - frontbottomleft.Y, frontbottomleft.Z - backtopright.Z) * Matrix4.CreateTranslation(center + offset);

            if (_mesh.IsExtended)
            {
                _vertices = new Vector3[mesh.Vertices.Length];
                _normals = new Vector3[mesh.Normals.Length];
            }

            indexBackFrontMostVertex = _mesh.indexBackFrontMostVertex;
            indexBottomTopMostVertex = _mesh.indexBottomTopMostVertex;
            indexLeftRightMostVertex = _mesh.indexLeftRightMostVertex;
        }

        public GameObjectHitbox(GameObject owner, GeoMeshHitbox mesh)
        {
            Owner = owner;
            _mesh = mesh;
            _colliderType = mesh._colliderType;
            if (_mesh.IsExtended)
            {
                _vertices = new Vector3[mesh.Vertices.Length];
                _normals = new Vector3[mesh.Normals.Length];
            }

            indexBackFrontMostVertex = _mesh.indexBackFrontMostVertex;
            indexBottomTopMostVertex = _mesh.indexBottomTopMostVertex;
            indexLeftRightMostVertex = _mesh.indexLeftRightMostVertex;
        }

        internal void Update(ref Vector3 gCenter)
        {
            if (_isCapsule)
            {
                _meshPreTransform = _capsulePreTransform;
            }
            else
            {
                _meshPreTransform = this._mesh.Transform;
            }

            Matrix4.Mult(_meshPreTransform, Owner._stateCurrent._scaleHitboxMat, out Matrix4 tempMatrix);
            Matrix4.Mult(tempMatrix, Owner._stateCurrent._modelMatrix, out _modelMatrixFinal);
            _modelMatrixFinalInv = Matrix4.Invert(_modelMatrixFinal);

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
                    _normals[i].Normalize();
                }
                Vector3.TransformPosition(_mesh.Vertices[i], _modelMatrixFinal, out _vertices[i]);

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
            Vector3.TransformPosition(_mesh.Center, _modelMatrixFinal, out _center);
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
            _fullDiameterAABB = -1;
            if (xWidth > _fullDiameterAABB)
                _fullDiameterAABB = xWidth;
            if (yWidth > _fullDiameterAABB)
                _fullDiameterAABB = yWidth;
            if (zWidth > _fullDiameterAABB)
                _fullDiameterAABB = zWidth;

            _dimensionsAABB.X = xWidth;
            _dimensionsAABB.Y = yWidth;
            _dimensionsAABB.Z = zWidth;

            _dimensionsOBB = new Vector3(
                (_vertices[indexLeftRightMostVertex.Y] - _vertices[indexLeftRightMostVertex.X]).LengthFast,
                (_vertices[indexBottomTopMostVertex.Y] - _vertices[indexBottomTopMostVertex.X]).LengthFast,
                (_vertices[indexBackFrontMostVertex.Y] - _vertices[indexBackFrontMostVertex.X]).LengthFast
                );
        }

        public bool IsExtended
        {
            get
            {
                return _mesh != null && _mesh.IsExtended;
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

        internal void GetVerticesFromFace(int faceIndex, ref Span<Vector3> vertices, out Vector3 normal)
        {
            GeoMeshFace face = _mesh.Faces[faceIndex];
            for(int i  = 0; i < face.VertexCount; i++)
            {
                vertices[i] = _vertices[face.Vertices[i]];
            }
            normal = _normals[_mesh.Faces[faceIndex].Normal] * (face.Flip ? -1f : 1f);
        }

        internal bool GetVerticesFromFaceAndCheckAngle(int faceIndex, Vector3 dir, ref Span<Vector3> vertices, out HitboxFace hitboxface)
        {
            GeoMeshFace face = _mesh.Faces[faceIndex];
            Vector3 n = _normals[_mesh.Faces[faceIndex].Normal] * (face.Flip ? -1f : 1f);
            float dot = Vector3.Dot(dir, n);
            if (dot >= 0)
            {
                hitboxface = new HitboxFace();
                return false;
            }

            for (int i = 0; i < face.VertexCount; i++)
            {
                vertices[i] = _vertices[face.Vertices[i]];
            }
            hitboxface = new HitboxFace()
            {
                Normal = n,
                NormalFlip = face.Flip,
                Vertices = vertices.ToArray()
            };
            return true;
        }

        internal const float ONETHIRD = 1f / 3f;
    }
}
