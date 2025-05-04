using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapPerspectiveCube : FrustumShadowMap
    {
        public override void Update(ref Matrix4 viewProjectionMatrix)
        {
            // nothing to do for now ;-)
        }

        public override bool IsBoxInFrustum(Vector3 lightCenter, float lightZFar, Vector3 center, float diameter)
        {
            float distance = (lightCenter - center).LengthFast;
            return (distance < lightZFar / 2 + diameter / 2);
        }
    }
}
