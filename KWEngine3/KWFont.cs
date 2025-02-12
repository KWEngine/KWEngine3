namespace KWEngine3
{
    internal struct KWFont
    {
        public bool IsValid { get; internal set; }
        public string Name { get; internal set; } = "undefined font name";
        internal KWFontGlyph[] Glyphs { get; set; }

        internal KWFontGlyph GetGlyphForCodepoint(char codepoint)
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
