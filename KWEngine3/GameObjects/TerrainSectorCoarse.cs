using KWEngine3.Helper;

namespace KWEngine3.GameObjects
{
    internal class TerrainSectorCoarse
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Back { get; set; }
        public int Front { get; set; }
        internal List<TerrainSector> Sectors { get; set; }

        public TerrainSectorCoarse(int l, int r, int b, int f)
        {
            Left = l;
            Right = r;
            Back = b;
            Front = f;

            Sectors = new List<TerrainSector>();
        }

        public void AddSector(TerrainSector s)
        {
            Sectors.Add(s);
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
