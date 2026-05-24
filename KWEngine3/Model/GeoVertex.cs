using System;

namespace KWEngine3.Model
{
    internal struct GeoVertex
    {
        public override string ToString()
        {
            return X+"|"+Y+"|"+Z+" | " + Index + ": " + BoneID0 + " - " + Math.Round(Weight0, 3) + ", " + BoneID1 + " - " + Math.Round(Weight1, 3) + ", " + BoneID2 + " - " + Math.Round(Weight2, 3);
        }
        public int Index { get; internal set; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }
        public float Weight0;
        public float Weight1;
        public float Weight2;
        public uint BoneID0;
        public uint BoneID1;
        public uint BoneID2;
        internal int WeightSet;

        public GeoVertex(int i, float x, float y, float z)
        {
            Index = i;

            X = x;
            Y = y;
            Z = z;

            Weight0 = 0f;
            Weight1 = 0f;
            Weight2 = 0f;
            BoneID0 = 0;
            BoneID1 = 0;
            BoneID2 = 0;
            WeightSet = 0;
        }
        
    }
}
