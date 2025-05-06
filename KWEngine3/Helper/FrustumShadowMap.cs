using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal abstract class FrustumShadowMap
    {
        public abstract bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, float diameter);
    }
}
