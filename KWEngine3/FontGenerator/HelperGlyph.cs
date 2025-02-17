using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.FontGenerator
{
    internal static class HelperGlyph
    {
        internal static readonly string GLYPHS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~°€§²³ÜÖÄüöäß·±";
        internal const int GAP = 0;

        internal static void ReadGlyphs(Font f, float scale, ref KWFont kwfont)
        {
            List<KWFontGlyph> glyphlist = new List<KWFontGlyph>();

            for (int i = 0; i < GLYPHS.Length; i++)
            {
                char g = GLYPHS[i];
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

                    KWFontGlyph newGlyph = new KWFontGlyph(
                            GLYPHS[i],
                            vao1,
                            vao2,
                            vbo1,
                            vbo2,
                            vaoFontBasicSize,
                            vaoFontCurvesSize,
                            (ix1 - ix0) * scale,
                            (iy1 - iy0) * scale,
                            advanceWidth * scale,
                            advanceHeight * scale,
                            new Vector2(0, 0)
                        );
                    glyphlist.Add(newGlyph);
                    kwfont.Add(GLYPHS[i], newGlyph);
                }
                else
                {
                    glyphlist.Add(new KWFontGlyph());
                }
            }

            kwfont.Glyphs = glyphlist.ToArray();
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

            f.GetFontVMetrics(out int ascent, out int descent, out int lineGap);
            kwfont.Ascent = ascent * scale;
            kwfont.Descent = descent * scale;
            ReadGlyphs(f, scale, ref kwfont);
            if (kwfont.Glyphs.Length > 0) // TODO: length does not cut it ;-)
            {
                float advance = 0f;
                for (int i = 1; i < kwfont.Glyphs.Length; i++)
                {
                    kwfont.Glyphs[i].UpdateUV(advance, 0f);
                    advance += kwfont.Glyphs[i].Advance.X;
                }

                RenderFontToTexture(kwfont, 256, out int finalTextureId, out int finalTextureByteCount);
                kwfont.Texture = finalTextureId;
                kwfont.TextureSize = finalTextureByteCount;
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

            f.GetFontVMetrics(out int ascent, out int descent, out int lineGap);
            kwfont.Ascent = ascent * scale;
            kwfont.Descent = descent * scale;
            ReadGlyphs(f, scale, ref kwfont);
            if (kwfont.Glyphs.Length > 0) // TODO: length does not cut it ;-)
            {
                float advance = 0f;
                for (int i = 1; i < kwfont.Glyphs.Length; i++)
                {
                    kwfont.Glyphs[i].UpdateUV(advance, 0f);
                    advance += kwfont.Glyphs[i].Advance.X;
                }

                RenderFontToTexture(kwfont, 256, out int finalTextureId, out int finalTextureByteCount);
                kwfont.Texture = finalTextureId;
                kwfont.TextureSize = finalTextureByteCount;
                return kwfont;
            }
            else
            {
                return new KWFont() { IsValid = false };
            }
        }

        public static void RenderFontToTexture(KWFont f, float scale, out int texture, out int byteCount)
        {
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            byteCount = 0;
            float width = 0f;
            float height = scale;
            

            for(int i = 0; i < f.Glyphs.Length; i++)
            {
                width += f.Glyphs[i].Advance.X * scale + GAP;
            }
            width -= GAP;

            FramebuffersGlyphs.Init((int)width, (int)height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebuffersGlyphs.FBGlyphs);
            
            // outline pass:
            float currentPos = 0f;
            float currentHeight = -f.Descent;
            RendererGlyph1.Bind();
            RendererGlyph1.SetGlobals((int)width, (int)height);
            HelperGeneral.CheckGLErrors();
            for (int i = 0; i < f.Glyphs.Length; i++)
            {
                RendererGlyph1.Draw(f.Glyphs[i], new Vector2(currentPos, currentHeight * scale), scale);
                currentPos += f.Glyphs[i].Advance.X * scale + GAP;

                HelperGeneral.CheckGLErrors();
            }

            // arc pass:
            currentPos = 0f;
            HelperGeneral.CheckGLErrors();
            RendererGlyph2.Bind();
            RendererGlyph2.SetGlobals((int)width, (int)height);
            HelperGeneral.CheckGLErrors();
            for (int i = 0; i < f.Glyphs.Length; i++)
            {

                RendererGlyph2.Draw(f.Glyphs[i], new Vector2(currentPos, currentHeight * scale), scale);
                currentPos += f.Glyphs[i].Advance.X * scale + GAP;

                HelperGeneral.CheckGLErrors();
            }

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebuffersGlyphs.FBGlyphsBlend);
            RendererGlyph3.Bind();
            RendererGlyph3.SetGlobals((int)width, (int)height);
            RendererGlyph3.Draw();

            // NOW BLIT AND DOWNSAMPLE ONE STEP


            /*
            int[] mipmaps = new int[6]; // 0 = 256px
                                        // 1 = 128px
                                        // 2 = 64px
                                        // 3 = 32px
                                        // 4 = 16px
                                        // 5 = 8px
            */
            texture = CopyTextureIntoNewTexture(FramebuffersGlyphs.FBGlyphsBlendTexture, PixelType.UnsignedByte, PixelFormat.Red, PixelInternalFormat.R8);

            // downsample loop
            int fbDownsample;
            int texDownsample;
            int w = (int)width;
            int h = (int)height;
            byteCount += w * h;
            int w2 = w / 2;
            int h2 = h / 2;
            for (int i = h, j = 1; i > 8; i /= 2, j++)
            {
                byteCount += w2 * h2;

                int downsampledTexture = Downsample(w, h, w2, h2);

                w2 = w2 / 2;
                h2 = h2 / 2;
                HelperGeneral.CheckGLErrors();
                CopyTextureIntoMipmapLevel(texDownsample, PixelType.UnsignedByte, PixelFormat.Red, PixelInternalFormat.R8, texture, j);
                HelperGeneral.CheckGLErrors();

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                GL.DeleteTextures(1, new int[] { texDownsample });
                GL.DeleteFramebuffers(1, new int[] { fbDownsample });
            }
            


            GL.Viewport(0, 0, KWEngine.Window.Width, KWEngine.Window.Height);
        }

        public static int Downsample(int w, int h, int w2, int h2)
        {
            int fbDownsample = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbDownsample);
            int texDownsample = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texDownsample);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, w2, h2, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texDownsample, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FramebuffersGlyphs.FBGlyphsBlend);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbDownsample);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BlitFramebuffer(0, 0, w, h, 0, 0, w2, h2, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            return CopyTextureIntoNewTexture(texDownsample, PixelType.UnsignedByte, PixelFormat.Red, PixelInternalFormat.R8);
        }

        public static void CopyTextureIntoMipmapLevel(int src, PixelType pxType, PixelFormat pxFormat, PixelInternalFormat pxFormatInternal, int dst, int dstMipMapLevel)
        {
            GL.BindTexture(TextureTarget.Texture2D, src);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int width);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int height);
            byte[] imageData = new byte[width * height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, pxFormat, pxType, imageData);

            GL.BindTexture(TextureTarget.Texture2D, dst);
            GL.TexImage2D(TextureTarget.Texture2D, dstMipMapLevel, pxFormatInternal, width, height, 0, pxFormat, pxType, imageData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            imageData = null;
        }

        public static int CopyTextureIntoNewTexture(int src, PixelType pxType, PixelFormat pxFormat, PixelInternalFormat pxFormatInternal)
        {
            GL.BindTexture(TextureTarget.Texture2D, src);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int width);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int height);

            byte[] imageData = new byte[width * height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, pxFormat, pxType, imageData);

            int dst = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, dst);
            GL.TexImage2D(TextureTarget.Texture2D, 0, pxFormatInternal, width, height, 0, pxFormat, pxType, imageData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            imageData = null;

            return dst;
        }
    }
}
