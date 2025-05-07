using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapPerspectiveCube : FrustumShadowMap
    {
        public override bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, Vector3 aabbMin, Vector3 aabbMax, float diameter)
        {
            float distance = (lightCenter - center).LengthFast;
            return (distance < lightZFar + diameter / 2);
        }

        public override void Update(LightObject l)
        {
            // nothing to do here ;-)
        }
    }
}
