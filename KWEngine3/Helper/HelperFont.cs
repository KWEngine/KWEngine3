using LunarLabs.Fonts;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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

            ReadGlyphs2(lunarFont, scale, ref f);
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
                GL.DeleteVertexArrays(1, new int[] { g.VAO_Step1 });
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

            ReadGlyphs2(f, scale, ref kwfont);
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

        internal static void ReadGlyphs2(Font f, float scale, ref KWFont kwfont)
        {
            float fontHeight = ReadMHeight(f, scale);

            List<KWFontGlyph> glyphs = new List<KWFontGlyph>();

            for (char i = (char)32; i < 256; i++)
            {
                if (f.HasGlyph(i))
                {
                    ushort glyphindex = f.FindGlyphIndex(i);
                    List<Vertex> glyphVertices = f.GetGlyphShape(glyphindex);
                    if (glyphVertices == null)
                    {
                        glyphVertices = new List<Vertex>();
                    }

                    List<List<Vector2>> shapes = new();

                    // vertex types:
                    // 1 = begin new shape and draw anew
                    // 2 = draw line
                    // 3 = draw curve
                    List<Vector2> currentVertices = new();
                    for (int j = 0; j < glyphVertices.Count; j++)
                    {
                        Vertex vertex1 = glyphVertices[j];
                        

                        if (j > 0 && vertex1.vertexType == 1)
                        {
                            shapes.Add(new List<Vector2>(currentVertices));
                            currentVertices = new();
                            currentVertices.Add(new Vector2(vertex1.x * scale, vertex1.y * scale));
                        }
                        else
                        {
                            currentVertices.Add(new Vector2(vertex1.x * scale, vertex1.y * scale));
                        }
                    }
                    if (currentVertices.Count > 2)
                    {
                        shapes.Add(new List<Vector2>(currentVertices));
                    }

                    f.GetGlyphVMetrics(glyphindex, out int advanceHeight, out int topSideBearing);
                    f.GetGlyphHMetrics(glyphindex, out int advanceWidth, out int leftSideBearing);
                    f.GetGlyphBoundingBox(glyphindex, 1, 1, 1, 1, out int ix0, out int iy0, out int ix1, out int iy1);

                    float[] glVertices = GenerateTriangleMeshFrom(shapes);

                    int vertexCount = glVertices.Length / 2;
                    int vao1 = GL.GenVertexArray();
                    GL.BindVertexArray(vao1);
                    int vbo1 = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo1);
                    GL.BufferData(BufferTarget.ArrayBuffer, glVertices.Length * sizeof(float), glVertices, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.BindVertexArray(0);

                    glyphs.Add(
                        new KWFontGlyph(
                            i,
                            vao1,
                            vbo1,
                            vertexCount,
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

        internal static float[] GenerateTriangleMeshFrom(List<List<Vector2>> vertices)
        {
            List<Vector2> overallList = new();
            foreach (List <Vector2> vs in vertices)
            {
                List<Vector2> mesh = Triangulate(vs);
                overallList.AddRange(mesh);
            }
            float[] result = new float[overallList.Count * 2];
            for(int i = 0, j = 0; i < overallList.Count; i++, j+=2)
            {
                result[j + 0] = overallList[i].X;
                result[j + 1] = overallList[i].Y;
            }
            return result;
        }

        internal static List<Vector2> Triangulate(List<Vector2> outline)
        {
            List<Vector2> triangles = new List<Vector2>();

            if (outline.Count < 3)
                return triangles; // Eine Form muss mindestens 3 Punkte haben, um trianguliert zu werden

            List<int> indices = new List<int>();
            for (int i = 0; i < outline.Count; i++)
            {
                indices.Add(i);
            }

            int iterations = 0;
            while (indices.Count > 3)
            {
                bool earFound = false;
                iterations++;

                for (int i = 0; i < indices.Count; i++)
                {
                    int prevIndex = (i - 1 + indices.Count) % indices.Count;
                    int currIndex = i;
                    int nextIndex = (i + 1) % indices.Count;

                    Vector2 prevVertex = outline[indices[prevIndex]];
                    Vector2 currVertex = outline[indices[currIndex]];
                    Vector2 nextVertex = outline[indices[nextIndex]];

                    if (IsEar(prevVertex, currVertex, nextVertex, outline, indices))
                    {
                        triangles.Add(prevVertex);
                        triangles.Add(currVertex);
                        triangles.Add(nextVertex);

                        indices.RemoveAt(currIndex);
                        earFound = true;
                        break;
                    }
                }

                if (!earFound || iterations > outline.Count * 2)
                {
                    // Falls wir keine Ohren mehr finden können oder es zu viele Iterationen gab
                    break;
                }
            }

            if (indices.Count == 3)
            {
                // Das letzte verbleibende Dreieck hinzufügen
                triangles.Add(outline[indices[0]]);
                triangles.Add(outline[indices[1]]);
                triangles.Add(outline[indices[2]]);
            }

            return triangles;
        }

        internal static float Cross(Vector2 value1, Vector2 value2)
        {
            return value1.X * value2.Y
                   - value1.Y * value2.X;
        }

        internal static bool IsEar(Vector2 v1, Vector2 v2, Vector2 v3, List<Vector2> outline, List<int> indices)
        {
            if (Cross(v2 - v1, v3 - v2) >= 0)
                return false; // Das Dreieck muss im Uhrzeigersinn orientiert sein

            for (int i = 0; i < outline.Count; i++)
            {
                if (!indices.Contains(i))
                {
                    if (PointInTriangle(outline[i], v1, v2, v3))
                        return false; // Ein Punkt liegt innerhalb des Dreiecks
                }
            }
            return true;
        }

        internal static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1 = Sign(pt, v1, v2);
            float d2 = Sign(pt, v2, v3);
            float d3 = Sign(pt, v3, v1);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        internal static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
        /*
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
                    int vao1 = GL.GenVertexArray();
                    GL.BindVertexArray(vao1);
                    int vbo1 = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo1);
                    GL.BufferData(BufferTarget.ArrayBuffer, verticesGL.Length * sizeof(float), verticesGL, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.BindVertexArray(0);

                    int vaoFontCurvesSize = verticesGLCurves.Length / 2;
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
        */
    }
}
