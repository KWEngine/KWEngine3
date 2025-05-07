using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FrustumShadowMapOrthographic : FrustumShadowMap
    {
        internal Vector3 lightAABBMin;
        internal Vector3 lightAABBMax;

        public override void Update(LightObject l)
        {
            float factor = 1.0f;
            if (l is LightObjectSun && (l as LightObjectSun).ShadowType == SunShadowType.CascadedShadowMap)
                factor = (float)(l as LightObjectSun)._csmFactor;

            ComputeOrthographicAABB(
                l._stateRender._position, 
                l._stateRender._lookAtVector, 
                l._stateRender._nearFarFOVType.X, 
                l._stateRender._nearFarFOVType.Y, 
                l._stateRender._nearFarFOVType.Z * factor, 
                out lightAABBMin, 
                out lightAABBMax);
        }

        internal static void ComputeOrthographicAABB(
            Vector3 position, Vector3 direction,
            float nearPlane, float farPlane,
            float orthoSize, out Vector3 min, out Vector3 max)
        {
            // Berechnung der Kamera-Orientierung
            Vector3 right = Vector3.NormalizeFast(Vector3.Cross(direction, KWEngine.WorldUp));
            Vector3 up = Vector3.NormalizeFast(Vector3.Cross(right, direction));

            // Berechnung der Frustum-Eckpunkte
            Vector3 nearCenter = position + direction * nearPlane;
            Vector3 farCenter = position + direction * farPlane;

            Vector3 nearTopLeft = nearCenter + up * (orthoSize / 2f) - right * (orthoSize / 2f);
            Vector3 nearBottomRight = nearCenter - up * (orthoSize / 2f) + right * (orthoSize / 2f);

            Vector3 farTopLeft = farCenter + up * (orthoSize / 2f) - right * (orthoSize / 2f);
            Vector3 farBottomRight = farCenter - up * (orthoSize / 2f) + right * (orthoSize / 2f);

            // Bestimmung der minimalen und maximalen Werte für die AABB
            float minX = Math.Min(nearTopLeft.X, Math.Min(nearBottomRight.X, Math.Min(farTopLeft.X, farBottomRight.X)));
            float minY = Math.Min(nearTopLeft.Y, Math.Min(nearBottomRight.Y, Math.Min(farTopLeft.Y, farBottomRight.Y)));
            float minZ = Math.Min(nearTopLeft.Z, Math.Min(nearBottomRight.Z, Math.Min(farTopLeft.Z, farBottomRight.Z)));

            float maxX = Math.Max(nearTopLeft.X, Math.Max(nearBottomRight.X, Math.Max(farTopLeft.X, farBottomRight.X)));
            float maxY = Math.Max(nearTopLeft.Y, Math.Max(nearBottomRight.Y, Math.Max(farTopLeft.Y, farBottomRight.Y)));
            float maxZ = Math.Max(nearTopLeft.Z, Math.Max(nearBottomRight.Z, Math.Max(farTopLeft.Z, farBottomRight.Z)));

            min = new Vector3(minX, minY, minZ);
            max = new Vector3(maxX, maxY, maxZ);
        }

        public override bool IsBoxInFrustum(Vector3 lightCenter, Vector3 lightDirection, float lightZFar, Vector3 center, Vector3 aabbMin, Vector3 aabbMax, float diameter)
        {
            return CheckCollisionOrContainment(lightAABBMin, lightAABBMax, aabbMin, aabbMax);

            /*
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
            */
        }

        public static bool CheckCollisionOrContainment(
            Vector3 minA, Vector3 maxA,
            Vector3 minB, Vector3 maxB)
        {
            // Überlappungsprüfung
            bool overlapX = minA.X <= maxB.X && minB.X <= maxA.X;
            bool overlapY = minA.Y <= maxB.Y && minB.Y <= maxA.Y;
            bool overlapZ = minA.Z <= maxB.Z && minB.Z <= maxA.Z;

            bool isOverlapping = overlapX && overlapY && overlapZ;
            if (isOverlapping) return true;

            bool aContainsB = minA.X <= minB.X && maxA.X >= maxB.X &&
                              minA.Y <= minB.Y && maxA.Y >= maxB.Y &&
                              minA.Z <= minB.Z && maxA.Z >= maxB.Z;
            if(aContainsB) return true;

            bool bContainsA = minB.X <= minA.X && maxB.X >= maxA.X &&
                              minB.Y <= minA.Y && maxB.Y >= maxA.Y &&
                              minB.Z <= minA.Z && maxB.Z >= maxA.Z;
            if(bContainsA) return true;

            return false;
        }
    }
}
