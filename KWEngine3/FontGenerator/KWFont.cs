namespace KWEngine3.FontGenerator
{
    internal struct KWFont
    {
        public bool IsValid { get; internal set; }
        public float Ascent { get; internal set; }
        public float Descent { get; internal set; }
        public KWFontGlyph[] Glyphs { get; internal set; }
        public Dictionary<char, KWFontGlyph> GlyphDict { get; internal set; }
        public int Texture { get; internal set; }

        public KWFontGlyph GetGlyphForCodepoint(char codepoint)
        {
            if(GlyphDict.TryGetValue(codepoint, out KWFontGlyph glyph))
            {
                return glyph;
            }
            else
            {
                return Glyphs[0];
            }
        }

        public KWFont()
        {
            IsValid = false;
            GlyphDict = new();
            Glyphs = new KWFontGlyph[0];
            Ascent = 0;
            Descent = 0;
            Texture = KWEngine.TextureAlpha;
        }

        public void Add(char codepoint, KWFontGlyph glyph)
        {
            GlyphDict[codepoint] = glyph;
        }
    }
}
