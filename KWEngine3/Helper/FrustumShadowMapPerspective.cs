using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapPerspective : FrustumShadowMap
    {
        public override bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, Vector3 aabbMin, Vector3 aabbMax, float diameter)
        {
            Vector3 lightToObject = center - lightCenter;
            float distance = lightToObject.LengthFast;
            if (distance < lightZFar + diameter / 2)
            {
                if (distance > diameter)
                {
                    float dot = Vector3.Dot(lightToObject, lightDirection);
                    return dot >= 0;
                }
                else
                    return true;
            }
            return false;
        }

        public override void Update(LightObject l)
        {
            // nothing to do here ;-)
        }
    }
}
