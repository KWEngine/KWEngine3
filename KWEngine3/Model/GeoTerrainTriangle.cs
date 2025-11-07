using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal class GeoTerrainTriangle
    {
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;

        internal Vector3 edge1;
        internal Vector3 edge2;

        internal Vector3[] Vertices;
        internal GeoTerrainTrianglePrismFace[] Faces;

        internal Vector3 Center;
        internal Vector3 Normal;

        internal Vector3[] Normals;

        internal static Vector3 CalculateSurfaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 x = Vector3.NormalizeFast(v2 - v1);
            Vector3 z = Vector3.NormalizeFast(v3 - v1);
            Vector3 y = Vector3.NormalizeFast(Vector3.Cross(x, z));
            y = y.Y < 0 ? -y : y;
            return y;
        }

        public override string ToString()
        {
            return v1.ToString() + "][" + v2.ToString() + "][" + v3.ToString();
        }
        public GeoTerrainTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Normals = new Vector3[3];
            Normal = Vector3.Zero;
            v1 = a; v2 = b; v3 = c;

            Vertices = new Vector3[3];
            Vertices[0] = v1; Vertices[1] = v2; Vertices[2] = v3;
            Center = new Vector3((v1.X + v2.X + v3.X) / 3f, (v1.Y + v2.Y + v3.Y) / 3f, (v1.Z + v2.Z + v3.Z) / 3f);

            // Find Normal that's pointing upward:
            Normal = CalculateSurfaceNormal(v1, v2, v3);

            // Find other 2 normals:
            Normals[2] = Vector3.NormalizeFast(Vector3.Cross(Normal, Vector3.UnitZ));
            if (Normals[2].X < 0)
                Normals[2] = -Normals[2];
            Normals[1] = Normal;
            Normals[0] = Vector3.NormalizeFast(Vector3.Cross(Normal, Vector3.UnitX));
            if (Normals[0].Z < 0)
                Normals[0] = -Normals[0];


            edge1 = Vertices[1] - Vertices[0];
            edge2 = Vertices[2] - Vertices[0];

            Faces = new GeoTerrainTrianglePrismFace[5];
            Faces[0] = new GeoTerrainTrianglePrismFace(v1, v2, v3, Normal, true); // top

            
            Faces[1] = new GeoTerrainTrianglePrismFace(
                v2, 
                v1, 
                v1 - new Vector3(0, -1, 0),
                v2 - new Vector3(0, -1, 0),
                false
                );

            Faces[2] = new GeoTerrainTrianglePrismFace(
                v3,
                v2,
                v2 - new Vector3(0, -1, 0),
                v3 - new Vector3(0, -1, 0),
                false
                );

            Faces[3] = new GeoTerrainTrianglePrismFace(
                v1,
                v3,
                v3 - new Vector3(0, -1, 0),
                v1 - new Vector3(0, -1, 0),
                false
                );

            Faces[4] = new GeoTerrainTrianglePrismFace(v3, v2, v1, -Normal, true); // bottom
        }
        private static float Sign(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            return (p1.X - p3.X) * (p2.Z - p3.Z) - (p2.X - p3.X) * (p1.Z - p3.Z);
        }

        public static bool IsPointInTriangle(ref Vector3 pt, ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
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
    }
}
