using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal abstract class FrustumShadowMap
    {
        public abstract void Update(LightObject l);
        public abstract bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, Vector3 aabbMin, Vector3 aabbMax, float diameter);
    }
}
