using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FlowFieldHitbox
    {
        public Vector3[] _vertices = new Vector3[8];
        public readonly Vector3[] _verticesOrg = new Vector3[8];
        public readonly Vector3[] _normals = new Vector3[3];

        public float _left = float.MaxValue;
        public float _right = float.MinValue;
        public float _high = float.MinValue;
        public float _low = float.MaxValue;
        public float _front = float.MinValue;
        public float _back = float.MaxValue;

        public FlowFieldHitbox(float cellRadius, float cellRadiusY)
        {
            _verticesOrg[0] = new Vector3(-cellRadius, +cellRadiusY, +cellRadius);
            _verticesOrg[1] = new Vector3(-cellRadius, -cellRadiusY, +cellRadius);
            _verticesOrg[2] = new Vector3(+cellRadius, +cellRadiusY, +cellRadius);
            _verticesOrg[3] = new Vector3(+cellRadius, -cellRadiusY, +cellRadius);
            _verticesOrg[4] = new Vector3(-cellRadius, +cellRadiusY, -cellRadius);
            _verticesOrg[5] = new Vector3(-cellRadius, -cellRadiusY, -cellRadius);
            _verticesOrg[6] = new Vector3(+cellRadius, +cellRadiusY, -cellRadius);
            _verticesOrg[7] = new Vector3(+cellRadius, -cellRadiusY, -cellRadius);

            _normals[0] = new Vector3(1f, 0f, 0f);
            _normals[1] = new Vector3(0f, 1f, 0f);
            _normals[2] = new Vector3(0f, 0f, 1f);
        }

        internal void Update(float offsetX, float offsetY, float offsetZ)
        {
            _left = float.MaxValue;
            _right = float.MinValue;
            _high = float.MinValue;
            _low = float.MaxValue;
            _front = float.MinValue;
            _back = float.MaxValue;

            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = _verticesOrg[i] + new Vector3(offsetX, offsetY, offsetZ);

                if (_left > _vertices[i].X)
                    _left = _vertices[i].X;
                if (_right < _vertices[i].X)
                    _right = _vertices[i].X;
                if (_low > _vertices[i].Y)
                    _low = _vertices[i].Y;
                if (_high < _vertices[i].Y)
                    _high = _vertices[i].Y;
                if (_back > _vertices[i].Z)
                    _back = _vertices[i].Z;
                if (_front < _vertices[i].Z)
                    _front = _vertices[i].Z;
            }
        }
    }
}
