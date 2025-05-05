using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapOrthographic : FrustumShadowMap
    {
        internal Matrix4 _viewProjectionMatrix;
        public override void Update(ref Matrix4 viewProjectionMatrix)
        {
            _viewProjectionMatrix = viewProjectionMatrix;
        }

        public override bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, float diameter)
        {
            float halfSize = diameter / 2.0f;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector4 corner = new Vector4(center + new Vector3(x * halfSize, y * halfSize, z * halfSize), 1.0f);
                        Vector4 cornerClip = Vector4.TransformRow(corner, _viewProjectionMatrix);
                        Vector3 cornerNDC = new Vector3(cornerClip.X, cornerClip.Y, cornerClip.Z);

                        if (cornerNDC.X >= -1.0f && cornerNDC.X <= 1.0f &&
                            cornerNDC.Y >= -1.0f && cornerNDC.Y <= 1.0f &&
                            cornerNDC.Z >= -1.0f && cornerNDC.Z <= 1.0f)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
