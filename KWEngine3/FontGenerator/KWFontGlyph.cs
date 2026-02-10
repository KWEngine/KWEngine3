using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFontGlyph
    {
        public float Width { get; internal set; }
        public float Top { get; internal set; }
        public float Bottom { get; internal set; }
        public Vector4 UCoordinate { get; internal set; }
        public float Advance { get; internal set; }
        public int CodePoint { get; internal set; }

        public KWFontGlyph(int codepoint, float width, float top, float bottom, float advanceWidth, Vector4 uv)
        {
            CodePoint = codepoint;
            Width = width;
            Top = top;
            Bottom = bottom;
            UCoordinate = uv;
            Advance = advanceWidth;
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(CodePoint);
        }
    }
}
