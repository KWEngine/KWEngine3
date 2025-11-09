using KWEngine3.Helper;

namespace KWEngine3.GameObjects
{
    internal class TerrainSectorCoarseUltra
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Back { get; set; }
        public int Front { get; set; }
        internal List<TerrainSectorCoarse> SectorsCoarse { get; set; }

        public TerrainSectorCoarseUltra(int l, int r, int b, int f)
        {
            Left = l;
            Right = r;
            Back = b;
            Front = f;

            SectorsCoarse = new List<TerrainSectorCoarse>();
        }


        public void AddSector(TerrainSectorCoarse sector)
        {
            SectorsCoarse.Add(sector);
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
