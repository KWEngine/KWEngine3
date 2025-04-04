﻿using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.FontGenerator
{
    internal class KWFont
    {
        public bool IsValid { get; internal set; }
        public Dictionary<char, KWFontGlyph> GlyphDict { get; internal set; }
        public int Texture { get; internal set; }
        public int TextureSize { get; internal set; }
        public float Ascent { get; internal set; }
        public float Descent { get; internal set; }
        public string Name { get; internal set; }
        public string FontFilename { get; internal set; }

        public void Dispose()
        {
            GL.DeleteTexture(Texture);
            GlyphDict.Clear();
            TextureSize = 0;
            FontFilename = "";
            IsValid = false;
        }

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
            Texture = KWEngine.TextureAlpha;
            TextureSize = 1;
            FontFilename = "";
        }

        public void Add(char codepoint, KWFontGlyph glyph)
        {
            GlyphDict[codepoint] = glyph;
        }
    }
}
