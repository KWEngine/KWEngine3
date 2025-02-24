using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFontGlyph
    {
        public float Width { get; internal set; }
        public float Height { get; internal set; }
        public Vector2 UCoordinate { get; internal set; }
        public float Advance { get; internal set; }
        public float Bearing { get; internal set; }
        public char CodePoint { get; internal set; }

        public KWFontGlyph(char codepoint, float width, float height, float bearing, float advanceWidth, Vector2 uv)
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
            return CodePoint.ToString();
        }
    }
}
