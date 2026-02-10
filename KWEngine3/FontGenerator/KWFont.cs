using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFont : IDisposable
    {
        public bool IsValid { get; internal set; }
        public bool IsSDF { get; internal set; }
        public Dictionary<int, KWFontGlyph> GlyphDict { get; internal set; }
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
            IsSDF = false;
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
            IsSDF = false;
        }

        public void Add(char codepoint, KWFontGlyph glyph)
        {
            GlyphDict[codepoint] = glyph;
        }

        public void ConvertSDFAtlas(AtlasRoot atlas)
        {
            IsSDF = true;
            IsValid = true;
            TextureSize = atlas.Atlas.Width * atlas.Atlas.Height * 3;
            Ascent = (float)atlas.Metrics.Ascender;
            Descent = (float)atlas.Metrics.Descender;

            foreach (Glyph sdfGlyph in atlas.Glyphs)
            {
                KWFontGlyph kwGlyph;
                if(sdfGlyph.AtlasBounds == null)
                {
                    kwGlyph = new KWFontGlyph(
                    sdfGlyph.Unicode,
                    0f,
                    0f,
                    0f,
                    (float)sdfGlyph.Advance,
                    new Vector4(0f, 0f, 0f, 0f));
                }
                else
                {
                    float uvleft = (float)(sdfGlyph.AtlasBounds.Left / atlas.Atlas.Width);
                    float uvright = (float)(sdfGlyph.AtlasBounds.Right / atlas.Atlas.Width);
                    float uvbottom = (float)(sdfGlyph.AtlasBounds.Bottom / atlas.Atlas.Height);
                    float uvtop = (float)(sdfGlyph.AtlasBounds.Top / atlas.Atlas.Height);

                    kwGlyph = new KWFontGlyph(
                        sdfGlyph.Unicode,
                        (float)(sdfGlyph.PlaneBounds.Right - sdfGlyph.PlaneBounds.Left),
                        (float)sdfGlyph.PlaneBounds.Top,
                        (float)sdfGlyph.PlaneBounds.Bottom,
                        (float)sdfGlyph.Advance,
                        new Vector4(uvleft, uvright, uvbottom, uvtop)
                    );
                }
                GlyphDict.Add(sdfGlyph.Unicode, kwGlyph);
            }
        }
    }
}
