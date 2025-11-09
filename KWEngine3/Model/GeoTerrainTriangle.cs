using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    /// <summary>
    /// Klasse, die Dreiecke eines Terrain-Objekts repräsentiert
    /// </summary>
    internal class GeoTerrainTriangle
    {
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;

        internal Vector3[] Vertices;
        internal GeoTerrainTrianglePrismFace[] Faces;

        internal Vector3 Center;
        internal Vector3 Normal;

        const float EPSILON = 1e-5f;
        const float EPSILON_TRI_HEIGHT = 0.0001f;

        internal static Vector3 CalculateSurfaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 x = Vector3.NormalizeFast(v2 - v1);
            Vector3 z = Vector3.NormalizeFast(v3 - v1);
            Vector3 y = Vector3.NormalizeFast(Vector3.Cross(x, z));
            y = y.Y < 0 ? -y : y;
            return y;
        }

        /// <summary>
        /// Einfache Darstellung der Dreieckskoordinaten
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return v1.ToString() + "][" + v2.ToString() + "][" + v3.ToString();
        }
        public GeoTerrainTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Normal = Vector3.Zero;
            v1 = a; v2 = b; v3 = c;

            Vertices = new Vector3[3];
            Vertices[0] = v1; Vertices[1] = v2; Vertices[2] = v3;
            Center = new Vector3((v1.X + v2.X + v3.X) / 3f, (v1.Y + v2.Y + v3.Y) / 3f, (v1.Z + v2.Z + v3.Z) / 3f);

            Normal = CalculateSurfaceNormal(v1, v2, v3);

            Faces = new GeoTerrainTrianglePrismFace[5];

            Faces[0] = new GeoTerrainTrianglePrismFace(v1, v2, v3, Normal, true); // top

            Faces[1] = new GeoTerrainTrianglePrismFace(
                v2, 
                v1, 
                v1 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                v2 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                false
                );

            Faces[2] = new GeoTerrainTrianglePrismFace(
                v3,
                v2,
                v2 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                v3 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                false
                );

            Faces[3] = new GeoTerrainTrianglePrismFace(
                v1,
                v3,
                v3 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                v1 - new Vector3(0, -EPSILON_TRI_HEIGHT, 0),
                false
                );

            Faces[4] = new GeoTerrainTrianglePrismFace(v3, v2, v1, -Normal, true); // bottom
        }
        private static float Sign(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            return (p1.X - p3.X) * (p2.Z - p3.Z) - (p2.X - p3.X) * (p1.Z - p3.Z);
        }

        /// <summary>
        /// Prüft, ob ein Punkt innerhalb eines durch drei Punkte aufgespanntes Dreieck liegt (die Punkte müssen gegen den Uhrzeigersinn angegeben werden)
        /// </summary>
        /// <remarks>Die Methode prüft NICHT, ob die Punkte exakt auf der Dreiecksfläche liegen; nur, ob sie entlang des Ebenenvektors irgendwo innerhalb der äußeren Ränder liegen</remarks>
        /// <param name="pt">zu prüfender Punkt</param>
        /// <param name="v1">Dreieckspunkt 1</param>
        /// <param name="v2">Dreieckspunkt 2</param>
        /// <param name="v3">Dreieckspunkt 3</param>
        /// <returns>true, wenn der Punkt innerhalb der Dreiecksränder liegt</returns>
        public static bool IsPointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(ref pt, ref v1, ref v2);
            d2 = Sign(ref pt, ref v2, ref v3);
            d3 = Sign(ref pt, ref v3, ref v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        /// <summary>
        /// Prüft, ob ein Punkt exakt auf der durch drei Punkte aufgespannten Dreiecksfläche liegt (die Punkte müssen gegen den Uhrzeigersinn angegeben werden)
        /// </summary>
        /// <param name="p">zu prüfender Punkt</param>
        /// <param name="a">Dreieckspunkt 1</param>
        /// <param name="b">Dreieckspunkt 2</param>
        /// <param name="c">Dreieckspunkt 3</param>
        /// <param name="n">Dreiecksebenenvektor</param>
        /// <returns>true, wenn der Punkt exakt auf der Dreiecksfläche liegt</returns>
        public static bool IsPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 n)
        {
            float dist = Vector3.Dot(p - a, n);
            if (MathF.Abs(dist) > EPSILON)
                return false;

            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            return (u >= -EPSILON) && (v >= -EPSILON) && (w >= -EPSILON)
                && (u <= 1.0f + EPSILON) && (v <= 1.0f + EPSILON) && (w <= 1.0f + EPSILON);
        }
    }
}
