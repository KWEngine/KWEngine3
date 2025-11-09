using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal class TerrainSector
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Back { get; set; }
        public int Front { get; set; }
        public TerrainObject Terrain { get; set; }

        internal List<GeoTerrainTriangle> Triangles { get; set; }

        public TerrainSector(int l, int r, int b, int f, TerrainObject parent)
        {
            Left = l;
            Right = r;
            Back = b;
            Front = f;
            Terrain = parent;
            Triangles = new();
        }


        public void AddTriangles(GeoTerrainTriangle[] tris)
        {
            Triangles.AddRange(tris);
        }

        public string GetInfo()
        {
            string s = "L: " + Left + " | R: " + Right + " | F: " + Back + " | B: " + Front;
            return s;
        }

        public override string ToString()
        {
            return GetInfo();
        }
    }
}
