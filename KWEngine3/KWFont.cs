namespace KWEngine3
{
    internal struct KWFont
    {
        public bool IsValid { get; internal set; }
        public KWFontGlyph[] Glyphs { get; internal set; }

        public KWFontGlyph GetGlyphForCodepoint(char codepoint)
        {
            return Glyphs[codepoint - 32];
        }

        public KWFont()
        {
            IsValid = false;
            Glyphs = new KWFontGlyph[0];
        }
    }
}
