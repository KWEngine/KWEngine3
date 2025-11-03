using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal struct Sector
    {
        //internal bool IsValid { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Back { get;  set; }
        public float Front { get;  set; }

        public Vector2 Center { get;  set; }

        internal GeoTerrainTriangle[] Triangles { get; set; }
        internal int tcounter = 0;

        public Sector()
        {
            Left = 0;
            Right = 0;
            Back = 0;
            Front = 0;

            Center = new Vector2(0, 0);

            //IsValid = true; // for overhaul!
            Triangles = new GeoTerrainTriangle[0];
        }

        public Sector(float l, float r, float b, float f, int tCount)
        {
            Left = l;
            Right = r;
            Back = b;
            Front = f;

            Center = new Vector2((l + r) / 2f, (b + f) / 2f);

            //IsValid = true;
            Triangles = new GeoTerrainTriangle[tCount]; // !!!!!!
        }
        

        public void AddTriangle(GeoTerrainTriangle t)
        {
            Triangles[tcounter++] = t;
        }


        public GeoTerrainTriangle? GetTriangle(Vector3 untranslatedPosition)
        {

            foreach (GeoTerrainTriangle t in Triangles)
            {
                if (GeoTerrainTriangle.IsPointInTriangle(ref untranslatedPosition, ref t.Vertices[0], ref t.Vertices[1], ref t.Vertices[2]))
                {
                    return t;
                }
            }
            return null;
        }

        public string GetInfo()
        {
            string s = "L: " + Math.Round(Left, 2) + " | R: " + Math.Round(Right, 2) + " | F: " + Math.Round(Back, 2) + " | B: " + Math.Round(Front, 2);
            return s;
        }
    }
}
