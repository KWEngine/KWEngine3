using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFontGlyph
    {
        //public float Width { get; internal set; }
        public float Left { get; internal set; }
        public float Right { get; internal set; }
        public float Top { get; internal set; }
        public float Bottom { get; internal set; }
        public Vector4 UCoordinate { get; internal set; }
        public float Advance { get; internal set; }
        public int CodePoint { get; internal set; }
        public Dictionary<int, float> Kerning { get; internal set; }

        public KWFontGlyph(int codepoint, float left, float right, float top, float bottom, float advanceWidth, Vector4 uv)
        {
            CodePoint = codepoint;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            UCoordinate = uv;
            Advance = advanceWidth;
            Kerning = new();
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(CodePoint);
        }
    }
}
