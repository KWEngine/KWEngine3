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

        public override bool IsBoxInFrustum(Vector3 lightCenter, float lightZFar, Vector3 center, float diameter)
        {
            float halfSize = diameter / 2.0f;
            foreach (Vector4 plane in _frustumPlanes)
            {
                // Normale der Ebene
                Vector3 normal = new Vector3(plane.X, plane.Y, plane.Z);
                float distance = plane.W;

                // Prüfe, ob mindestens ein Eckpunkt innerhalb des Frustums liegt
                bool inside = false;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 point = center + new Vector3(x * halfSize, y * halfSize, z * halfSize);
                            if (Vector3.Dot(normal, point) + distance >= 0)
                            {
                                inside = true;
                                break;
                            }
                        }
                        if (inside) break;
                    }
                    if (inside) break;
                }

                // Falls kein Eckpunkt innerhalb der aktuellen Frustum-Ebene liegt, ist der Quader nicht sichtbar
                if (!inside)
                    return false;
            }

            return true;
        }
    }
}
