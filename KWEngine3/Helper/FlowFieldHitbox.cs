using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FlowFieldHitbox
    {
        public Vector3[] _vertices = new Vector3[8];
        public readonly Vector3[] _verticesOrg = new Vector3[8];
        public readonly Vector3[] _normals = new Vector3[3];

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

        internal void Update(float offsetX, float offsetZ)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = _verticesOrg[i] + new Vector3(offsetX, 0f, offsetZ);
            }
        }
    }
}
