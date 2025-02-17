namespace KWEngine3.FontGenerator
{
    internal class KWFont
    {
        public bool IsValid { get; internal set; }
        public float Ascent { get; internal set; }
        public float Descent { get; internal set; }
        public Dictionary<char, KWFontGlyph> GlyphDict { get; internal set; }
        public int Texture { get; internal set; }
        public int TextureSize { get; internal set; }

        public KWFontGlyph GetGlyphForCodepoint(char codepoint)
        {
            if(GlyphDict.TryGetValue(codepoint, out KWFontGlyph glyph))
            {
                return glyph;
            }
            else
            {
                return GlyphDict[' '];
            }
        }

        public KWFont()
        {
            IsValid = false;
            GlyphDict = new();
            Ascent = 0;
            Descent = 0;
            Texture = KWEngine.TextureAlpha;
            TextureSize = 4;
        }

        public void Add(char codepoint, KWFontGlyph glyph)
        {
            GlyphDict[codepoint] = glyph;
        }
    }
}
