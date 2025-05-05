using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapPerspective : FrustumShadowMap
    {
        public override void Update(ref Matrix4 viewProjectionMatrix)
        {
            _frustumPlanes[0] = new Vector4(viewProjectionMatrix.M41 + viewProjectionMatrix.M11, viewProjectionMatrix.M42 + viewProjectionMatrix.M12, viewProjectionMatrix.M43 + viewProjectionMatrix.M13, viewProjectionMatrix.M44 + viewProjectionMatrix.M14); // Linke Ebene
            _frustumPlanes[1] = new Vector4(viewProjectionMatrix.M41 - viewProjectionMatrix.M11, viewProjectionMatrix.M42 - viewProjectionMatrix.M12, viewProjectionMatrix.M43 - viewProjectionMatrix.M13, viewProjectionMatrix.M44 - viewProjectionMatrix.M14); // Rechte Ebene
            _frustumPlanes[2] = new Vector4(viewProjectionMatrix.M41 + viewProjectionMatrix.M21, viewProjectionMatrix.M42 + viewProjectionMatrix.M22, viewProjectionMatrix.M43 + viewProjectionMatrix.M23, viewProjectionMatrix.M44 + viewProjectionMatrix.M24); // Untere Ebene
            _frustumPlanes[3] = new Vector4(viewProjectionMatrix.M41 - viewProjectionMatrix.M21, viewProjectionMatrix.M42 - viewProjectionMatrix.M22, viewProjectionMatrix.M43 - viewProjectionMatrix.M23, viewProjectionMatrix.M44 - viewProjectionMatrix.M24); // Obere Ebene
            _frustumPlanes[4] = new Vector4(viewProjectionMatrix.M41 + viewProjectionMatrix.M31, viewProjectionMatrix.M42 + viewProjectionMatrix.M32, viewProjectionMatrix.M43 + viewProjectionMatrix.M33, viewProjectionMatrix.M44 + viewProjectionMatrix.M34); // Nahe Ebene
            _frustumPlanes[5] = new Vector4(viewProjectionMatrix.M41 - viewProjectionMatrix.M31, viewProjectionMatrix.M42 - viewProjectionMatrix.M32, viewProjectionMatrix.M43 - viewProjectionMatrix.M33, viewProjectionMatrix.M44 - viewProjectionMatrix.M34); // Ferne Ebene
        }

        public override bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, float diameter)
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
    }
}
