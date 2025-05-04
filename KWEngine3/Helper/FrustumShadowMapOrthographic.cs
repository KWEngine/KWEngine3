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

        public override bool IsBoxInFrustum(Vector3 lightCenter, float lightZFar, Vector3 center, float diameter)
        {
            // Berechnung der halben Größe des Quaders
            float halfSize = diameter / 2.0f;

            // Definiere die 8 Eckpunkte des Quaders im World-Space
            Vector3[] corners = new Vector3[8];
            int index = 0;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        corners[index++] = center + new Vector3(x * halfSize, y * halfSize, z * halfSize);
                    }
                }
            }

            // Transformiere die Eckpunkte in den Clip-Space und überprüfe, ob mindestens einer innerhalb der Grenzen liegt
            foreach (Vector3 corner in corners)
            {
                Vector4 cornerClip = Vector4.TransformRow(new Vector4(corner, 1.0f), _viewProjectionMatrix);

                // Normalisiere die Koordinaten (bei orthografischer Projektion keine perspektivische Division)
                Vector3 cornerNDC = new Vector3(cornerClip.X, cornerClip.Y, cornerClip.Z);

                // Grenzwerte des Frustums in NDC (normalized device coordinates)
                if (cornerNDC.X >= -1.0f && cornerNDC.X <= 1.0f &&
                    cornerNDC.Y >= -1.0f && cornerNDC.Y <= 1.0f &&
                    cornerNDC.Z >= -1.0f && cornerNDC.Z <= 1.0f)
                {
                    return true; // Mindestens ein Punkt liegt im Frustum
                }
            }

            return false; // Kein Punkt liegt im Frustum
        }
    }
}
