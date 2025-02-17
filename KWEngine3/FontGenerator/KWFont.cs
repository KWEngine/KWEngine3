namespace KWEngine3.FontGenerator
{
    internal struct KWFont
    {
        public bool IsValid { get; internal set; }
        public KWFontGlyph[] Glyphs { get; internal set; }

        public KWFontGlyph GetGlyphForCodepoint(char codepoint)
        {
            return Glyphs[0];
        }

        public KWFont()
        {
            IsValid = false;
            Glyphs = new KWFontGlyph[0];
        }
    }
}
