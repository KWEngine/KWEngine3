using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    internal class GeoTerrainTriangle
    {
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;

        internal Vector3 edge1;
        internal Vector3 edge2;
        private readonly float uu;
        private readonly float uv;
        private readonly float vv;
        private readonly bool isUpperTriangle;

        internal Vector3[] Vertices;
        internal Vector3 Center;
        internal Vector3 Normal;

        internal Vector3 crossEdges;

        internal float boundLeft;
        internal float boundRight;
        internal float boundFront;
        internal float boundBack;
        internal float heightMaxUntranslated;
        internal float heightMinUntranslated;

        internal Vector3[] Normals;

        private Vector3 CalculateSurfaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
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
            isUpperTriangle = a.Z > b.Z;
            v1 = a; v2 = b; v3 = c;
            if (!isUpperTriangle)
            {
                boundLeft = v1.X;
                boundRight = v3.X;
                boundBack = v1.Z;
                boundFront = v2.Z;
            }
            else
            {
                boundLeft = v3.X;
                boundRight = v2.X;
                boundBack = v2.Z;
                boundFront = v1.Z;
            }

            Vertices = new Vector3[3];
            Vertices[0] = v1; Vertices[1] = v2; Vertices[2] = v3;
            Center = new Vector3((v1.X + v2.X + v3.X) / 3f, (v1.Y + v2.Y + v3.Y) / 3f, (v1.Z + v2.Z + v3.Z) / 3f);

            edge1 = Vertices[1] - Vertices[0];
            edge2 = Vertices[2] - Vertices[0];
            Vector3.Cross(edge1, edge2, out crossEdges);

            uu = Vector3.Dot(edge1, edge1);
            uv = Vector3.Dot(edge1, edge2);
            vv = Vector3.Dot(edge2, edge2);

            heightMaxUntranslated = Math.Max(c.Y, Math.Max(a.Y, b.Y));
            heightMinUntranslated = Math.Min(c.Y, Math.Min(a.Y, b.Y));

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
        }

        public int Intersect3D_RayTriangle(Vector3 origin, ref Vector3 I, Vector3 offset, bool shootFromBelow = false)
        {
            Vector3 dir, w0, w;           // ray vectors
            float r, a, b;              // Vertices[0]rams to calc ray-plane intersect

            dir = shootFromBelow ? KWEngine.WorldUp : -KWEngine.WorldUp;              // ray direction vector
            w0 = origin - (v1 + offset); // R.P0 - T.V0;
            a = -Vector3.Dot(Normal, w0);
            b = Vector3.Dot(Normal, dir);
            if (Math.Abs(b) <= 0.00001f)
            {     // ray is  Vertices[0]rallel to triangle plane
                if (a == 0)                 // ray lies in triangle plane
                    return 2;
                else return 0;              // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                    // ray goes away from triangle
                return 0;                   // => no intersect
                                            // for a segment, also test if (r > 1.0) => no intersect
            I = origin + r * dir;
            //*I = R.P0 + r * dir;            // intersect point of ray and plane

            // is I inside T?
            float wu, wv, D;

            w = I - (v1 + offset);
            wu = Vector3.Dot(w, edge1);
            wv = Vector3.Dot(w, edge2);
            D = uv * uv - uu * vv;

            // get and test Vertices[0]rametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)         // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                       // I is in T
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

        public static bool IsPointInTriangle2(ref Vector3 p, ref Vector3 p0, ref Vector3 p1, ref Vector3 p2)
        {
            var s = (p0.X - p2.X) * (p.Z - p2.Z) - (p0.Z - p2.Z) * (p.X - p2.X);
            var t = (p1.X - p0.X) * (p.Z - p0.Z) - (p1.Z - p0.Z) * (p.X - p0.X);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = (p2.X - p1.X) * (p.Z - p1.Z) - (p2.Z - p1.Z) * (p.X - p1.X);
            return d == 0 || (d < 0) == (s + t <= 0);
        }

        public float InterpolateHeight(float x, float z)
        {
            // Plane equation ax+by+cz+d=0
            float a = (Vertices[1].Y - Vertices[0].Y) * (Vertices[2].Z - Vertices[0].Z) - (Vertices[2].Y - Vertices[0].Y) * (Vertices[1].Z - Vertices[0].Z);
            float b = (Vertices[1].Z - Vertices[0].Z) * (Vertices[2].X - Vertices[0].X) - (Vertices[2].Z - Vertices[0].Z) * (Vertices[1].X - Vertices[0].X);
            float c = (Vertices[1].X - Vertices[0].X) * (Vertices[2].Y - Vertices[0].Y) - (Vertices[2].X - Vertices[0].X) * (Vertices[1].Y - Vertices[0].Y);
            float d = -(a * Vertices[0].X + b * Vertices[0].Y + c * Vertices[0].Z);
            // y = (-d -ax -cz) / b
            float y = (-d - a * x - c * z) / b;
            return y;
        }
    }
}
