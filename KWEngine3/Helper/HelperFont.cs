using LunarLabs.Fonts;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using System.Reflection;


namespace KWEngine3.Helper
{
    internal static class HelperFont
    {
        public static KWFont LoadFontFromAssembly(string font)
        {
            KWFont f = new KWFont();
            Font lunarFont = null;
            float scale = 1f;
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                using (Stream stream = a.GetManifestResourceStream("KWEngine3.Assets.Fonts." + font))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    lunarFont = new Font(buffer, null);
                    scale = lunarFont.ScaleInPixels(1);
                }
            }
            catch (Exception)
            {
                // TODO
            }

            if (lunarFont == null)
                return new KWFont() { IsValid = false };

            ReadGlyphs(lunarFont, scale, ref f);
            if (f.Glyphs.Length > 0) // TODO: length does not cut it ;-)
            {
                return f;
            }
            else
            {
                return new KWFont() { IsValid = false };
            }
        }

        public static Dictionary<char, int> TextToOffsetDict = new()
        {
            {'Ä', 95 },
            {'Ö', 96 },
            {'Ü', 97 },
            {'ß', 98 },
            {'ä', 99 },
            {'ö', 100 },
            {'ü', 101 },
        };

        public static int GenerateTexture(string resourceName, Assembly a)
        {
            Stream resourceStream = a.GetManifestResourceStream(resourceName);
            return HelperTexture.LoadFontTextureCompressedNoMipMap(resourceStream);
        }

        public static void DisposeFont(KWFont f)
        {
            foreach (KWFontGlyph g in f.Glyphs)
            {
                GL.DeleteBuffers(1, new int[] { g.VBO_Step1 });
                GL.DeleteBuffers(1, new int[] { g.VBO_Step2 });
                GL.DeleteVertexArrays(1, new int[] { g.VAO_Step1 });
                GL.DeleteVertexArrays(1, new int[] { g.VAO_Step2 });
            }
            f.Glyphs = null;
        }

        public static KWFont LoadFont(string address)
        {
            KWFont kwfont = new KWFont();
            Font f = null;
            float scale = 1f;
            try
            {
                f = new Font(File.ReadAllBytes(address), null);
                scale = f.ScaleInPixels(1);
            }
            catch (Exception)
            {
                // ... ;-)
            }
            if (f == null)
                return new KWFont() { IsValid = false };

            ReadGlyphs(f, scale, ref kwfont);
            if (kwfont.Glyphs.Length > 0) // TODO: length does not cut it ;-)
            {
                return kwfont;
            }
            else
            {
                return new KWFont() { IsValid = false };
            }
        }

        internal static float ReadMHeight(Font f, float scale)
        {
            ushort glyphindex = f.FindGlyphIndex('M');
            List<Vertex> glyphVertices = f.GetGlyphShape(glyphindex);
            float max = float.MinValue;
            float min = float.MaxValue;
            foreach (Vertex v in glyphVertices)
            {
                float s = v.y * scale;
                if (s > max) max = s;
                if (s < min) min = s;
            }
            return max - min;
        }

        internal static void ReadGlyphs(Font f, float scale, ref KWFont kwfont)
        {
            float fontHeight = ReadMHeight(f, scale);

            List<KWFontGlyph> glyphs = new List<KWFontGlyph>();

            for (char i = (char)32; i < 8192 * 2; i++)
            {
                if (f.HasGlyph(i))
                {
                    ushort glyphindex = f.FindGlyphIndex(i);
                    List<Vertex> glyphVertices = f.GetGlyphShape(glyphindex);
                    List<float> verticesGL_Step1 = new List<float>();
                    List<float> verticesGL_Step2 = new List<float>();
                    if (glyphVertices == null)
                    {
                        glyphVertices = new List<Vertex>();
                    }


                    // vertex types:
                    // 1 = begin new shape and draw anew
                    // 2 = draw line
                    // 3 = draw curve
                    for (int j = 0; j < glyphVertices.Count - 1; j++)
                    {
                        Vertex vertex1 = glyphVertices[j];
                        Vertex vertex2 = glyphVertices[j + 1];

                        if (vertex2.vertexType == 1)
                        {
                            // skip this iteration
                        }
                        else
                        {
                            verticesGL_Step1.Add(0);
                            verticesGL_Step1.Add(0);
                            verticesGL_Step1.Add(vertex1.x * scale);
                            verticesGL_Step1.Add(vertex1.y * scale);
                            verticesGL_Step1.Add(vertex2.x * scale);
                            verticesGL_Step1.Add(vertex2.y * scale);
                        }
                    }

                    for (int j = 0; j < glyphVertices.Count - 1; j++)
                    {
                        if (glyphVertices[j].vertexType == 1)
                        {
                            continue;
                        }
                        Vertex vertex1 = glyphVertices[j + 0];
                        Vertex vertex2 = glyphVertices[j + 1];

                        // ist es eine Kurve?
                        if (vertex1.vertexType == 2 && vertex2.vertexType == 3)
                        {
                            verticesGL_Step2.Add(vertex1.x * scale);
                            verticesGL_Step2.Add(vertex1.y * scale);
                            verticesGL_Step2.Add(vertex2.cx * scale);
                            verticesGL_Step2.Add(vertex2.cy * scale);
                            verticesGL_Step2.Add(vertex2.x * scale);
                            verticesGL_Step2.Add(vertex2.y * scale);
                        }
                        else if (vertex1.vertexType == 3 && vertex2.vertexType == 3)
                        {
                            verticesGL_Step2.Add(vertex1.x * scale);
                            verticesGL_Step2.Add(vertex1.y * scale);
                            verticesGL_Step2.Add(vertex2.cx * scale);
                            verticesGL_Step2.Add(vertex2.cy * scale);
                            verticesGL_Step2.Add(vertex2.x * scale);
                            verticesGL_Step2.Add(vertex2.y * scale);
                        }
                    }
                    float[] verticesGL = verticesGL_Step1.ToArray();
                    float[] verticesGLCurves = verticesGL_Step2.ToArray();
                    int vaoFontBasicSize = verticesGL.Length / 2;
                    int vaoFontCurvesSize = verticesGLCurves.Length / 2;

                    int vao1 = GL.GenVertexArray();
                    GL.BindVertexArray(vao1);
                    int vbo1 = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo1);
                    GL.BufferData(BufferTarget.ArrayBuffer, verticesGL.Length * sizeof(float), verticesGL, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.BindVertexArray(0);

                    int vao2 = GL.GenVertexArray();
                    GL.BindVertexArray(vao2);
                    int vbo2 = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo2);
                    GL.BufferData(BufferTarget.ArrayBuffer, verticesGLCurves.Length * sizeof(float), verticesGLCurves, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.BindVertexArray(0);

                    f.GetGlyphVMetrics(glyphindex, out int advanceHeight, out int topSideBearing);
                    f.GetGlyphHMetrics(glyphindex, out int advanceWidth, out int leftSideBearing);
                    f.GetGlyphBoundingBox(glyphindex, 1, 1, 1, 1, out int ix0, out int iy0, out int ix1, out int iy1);

                    glyphs.Add(
                        new KWFontGlyph(
                            i,
                            vao1,
                            vao2,
                            vbo1,
                            vbo2,
                            vaoFontBasicSize,
                            vaoFontCurvesSize,
                            (ix1 - ix0) * scale,
                            (iy1 - iy0) * scale,
                            -1f,
                            -1f,
                            advanceWidth * scale,
                            advanceHeight * scale
                        )
                    );
                }
                else
                {
                    glyphs.Add(new KWFontGlyph());
                }
            }

            kwfont.Glyphs = glyphs.ToArray();
            kwfont.Height = fontHeight;
        }
    }
}
