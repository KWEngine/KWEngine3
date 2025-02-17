using OpenTK.Graphics.OpenGL4;
using System.Net;
using System.Reflection;

namespace KWEngine3.FontGenerator
{
    internal static class HelperGlyph
    {
        internal static readonly string GLYPHS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~°€§²³ÜÖÄüöäß█«»·±";

        internal static void ReadGlyphs(Font f, float scale, ref KWFont kwfont)
        {
            List<KWFontGlyph> glyphs = new List<KWFontGlyph>();

            for (int i = 0; i < GLYPHS.Length; i++)
            {
                if(f.HasGlyph(GLYPHS[i]))
                {
                    ushort glyphindex = f.FindGlyphIndex(GLYPHS[i]);
                    List<Vertex> glyphVertices = f.GetGlyphShape(glyphindex);
                    List<float> verticesGL_Step1 = new List<float>();
                    List<float> verticesGL_Step2 = new List<float>();
                    if(glyphVertices == null)
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
                            verticesGL_Step1.Add(-1);
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
                            GLYPHS[i],
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
        }

        private static void DisposeFont(KWFont f)
        {
            foreach (KWFontGlyph g in f.Glyphs)
            {
                GL.DeleteBuffers(1, new int[] { g.VBO_Step1 });
                GL.DeleteBuffers(1, new int[] { g.VBO_Step2 });
                GL.DeleteVertexArrays(1, new int[] {g.VAO_Step1});
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
            catch(Exception)
            {
                // ... ;-)
            }
            if (f == null)
                return new KWFont() { IsValid = false };

            ReadGlyphs(f, scale, ref kwfont);
            if(kwfont.Glyphs.Length > 0) // TODO: length does not cut it ;-)
            {
                return kwfont;
            }
            else
            {
                return new KWFont() { IsValid = false };
            }
        }

        public static KWFont LoadFontInternal(string name)
        {
            KWFont kwfont = new KWFont();
            Font f = null;
            float scale = 1f;
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                using (Stream s = a.GetManifestResourceStream("KWEngine3.Assets.Fonts." + name))
                {
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, (int)s.Length);
                    f = new Font(buffer, null);
                    scale = f.ScaleInPixels(1);
                }

                    
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
    }
}
