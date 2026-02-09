using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFontGlyph
    {
        public float Width { get; internal set; }
        public float Height { get; internal set; }
        public Vector4 UCoordinate { get; internal set; }
        public float Advance { get; internal set; }
        public float Bearing { get; internal set; }
        public int CodePoint { get; internal set; }

        public KWFontGlyph(int codepoint, float width, float height, float bearing, float advanceWidth, Vector4 uv)
        {
            CodePoint = codepoint;
            Width = width;
            Height = height;
            UCoordinate = uv;
            Advance = advanceWidth;
            Bearing = bearing;
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(CodePoint);
        }
    }
}
