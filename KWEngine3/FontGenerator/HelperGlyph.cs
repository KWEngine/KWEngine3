using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KWEngine3.FontGenerator
{
    internal static class HelperGlyph
    {
        internal static readonly string[] GLYPHS = { " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ", "[\\]_abcdefghijklmnopqrstuvwxyz{|}~°€ÜÖÄüöäß·" };

        public static Font LoadFontInternal(string assemblyname)
        {
            Font f = null;
            try
            {
                byte[] buffer;
                Assembly a = Assembly.GetExecutingAssembly();
                using (Stream s = a.GetManifestResourceStream("KWEngine3.Assets.Fonts." + assemblyname))
                {
                    buffer = new byte[s.Length];
                    s.Read(buffer, 0, (int)s.Length);
                    f = new Font(buffer, null);
                }
                buffer = null;
            }
            catch (Exception)
            {

            }
            return f;
        }

        public static Font LoadFont(string filename)
        {
            Font f = null;
            try
            {
                f = new Font(File.ReadAllBytes(filename), null);
            }
            catch (Exception)
            {
                // ... ;-)
            }
            return f;
        }

        public static void SetAscentDescent(Font f, KWFont kwfont)
        {
            float scaleNormalised = f.ScaleInPixels(1f);
            f.GetFontVMetrics(out int ascent, out int descent, out int lineGap);
            kwfont.Ascent = ascent * scaleNormalised;
            kwfont.Descent = descent * scaleNormalised;
        }

        public static KWFont LoadFont(Font f, string fontName, bool isBitmap = false, string bitmapName = "")
        {
            if (f == null || fontName == null)
                return new KWFont() { IsValid = false };

            KWFont kwfont = new KWFont();
            kwfont.Name = fontName;
            int res = 256;
            Dictionary<char, KWFontGlyph> tmpDict = GenerateGlyphsAndDict(f, res, out int[] mipmap0Width);
            
            int maxTexSize = GL.GetInteger(GetPName.MaxTextureSize);
            if (Math.Max(mipmap0Width[0], mipmap0Width[1]) >= maxTexSize)
            {
                KWEngine.LogWriteLine("[Font] Generated texture size for '" + fontName + "' exceeds your gpu's texture size limits");
                kwfont.IsValid = false;
                kwfont.GlyphDict = new();
                return kwfont;
            }
            else
            {
                kwfont.GlyphDict = tmpDict;
                kwfont.IsValid = true;
            }
            
            kwfont.GlyphDict = tmpDict;
            kwfont.IsValid = true;

            byte[] txPixels;
            if (isBitmap)
            {
                Assembly a = Assembly.GetExecutingAssembly();
                using (Stream s = a.GetManifestResourceStream("KWEngine3.Assets.Fonts." + bitmapName))
                {
                    SKBitmap bm = SKBitmap.Decode(s);

                    int texture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Math.Max(mipmap0Width[0], mipmap0Width[1]), res * 2, 0, PixelFormat.Red, PixelType.UnsignedByte, bm.Bytes);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    kwfont.Texture = texture;
                    kwfont.TextureSize = (int)(bm.Bytes.Length / 4 * 1.333333f);

                    bm.Dispose();
                }
            }
            else
            {
                txPixels = GenerateTextureForRes(f, Math.Max(mipmap0Width[0], mipmap0Width[1]), res * 2, ref kwfont);
                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Math.Max(mipmap0Width[0], mipmap0Width[1]), res * 2, 0, PixelFormat.Bgra, PixelType.UnsignedByte, txPixels);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                kwfont.Texture = texture;
                kwfont.TextureSize = (int)(txPixels.Length / 4 * 1.333333f);
            }
                
            SetAscentDescent(f, kwfont);
            return kwfont;
        }

        internal static Dictionary<char, KWFontGlyph> GenerateGlyphsAndDict(Font f, int res, out int[] pixelWidthSumEnlarged)
        {
            pixelWidthSumEnlarged = new int[2];
            int pixelGap = res / 8;
            float renderScale = f.ScaleInPixels(res);
            float normalisedScale = f.ScaleInPixels(1f);
            // default width and advance for UNDERSCORE:
            int glyphIndexM = f.FindGlyphIndex('_');
            f.GetGlyphHMetrics(glyphIndexM, out int MAdvance, out int dummy);
            float MAdvanceF = MAdvance * renderScale;
            f.GetGlyphBoundingBox(glyphIndexM, 1, 1, 0, 0, out int Mx0, out int My0, out int Mx1, out int My1);
            float MWidthF = Mx1 * renderScale - Mx0 * renderScale;
            float MHeightF = My1 * renderScale - My0 * renderScale;
            float MWidthFNormalised = Mx1 * normalisedScale - Mx0 * normalisedScale;
            float MHeightFNormalised = My1 * normalisedScale - My0 * normalisedScale;

            for (int row = 0; row < GLYPHS.Length; row++)
            {
                float pixelWidthSum = 0;
                for (int i = 0; i < GLYPHS[row].Length; i++)
                {
                    char n = GLYPHS[row][i];
                    int glyphIndex = f.FindGlyphIndex(n);
                    f.GetGlyphBoundingBox(n, 1, 1, 0f, 0f, out int x0, out int y0, out int x1, out int y1);
                    float pixelWidthForGlyph = x1 * renderScale - x0 * renderScale;
                    if (pixelWidthForGlyph == 0)
                    {
                        pixelWidthForGlyph = MWidthF;
                    }
                    pixelWidthSum += (int)pixelWidthForGlyph + pixelGap;
                }
                pixelWidthSum -= pixelGap;

                pixelWidthSumEnlarged[row] = FindNextDivisibleNumber((int)pixelWidthSum);
            }

            Dictionary<char, KWFontGlyph> glyphDict = new();

            for (int row = 0; row < GLYPHS.Length; row++)
            {
                float offsetX = 0;
                for (int x = 0; x < GLYPHS[row].Length; x++)
                {
                    char theChar = GLYPHS[row][x];

                    int glyphIndex = f.FindGlyphIndex(theChar);
                    f.GetGlyphHMetrics(glyphIndex, out int advanceWInt, out int leftBearingInt);
                    f.GetGlyphVMetrics(glyphIndex, out int advanceHInt, out int topBearingInt);
                    f.GetGlyphBoundingBox(glyphIndex, 1, 1, 0, 0, out int x0, out int y0, out int x1, out int y1);

                    float advance = advanceWInt * renderScale;
                    float bearingLeft = leftBearingInt * renderScale;
                    float width = x1 * renderScale - x0 * renderScale;
                    float height = y1 * renderScale - y0 * renderScale;
                    if (width == 0)
                    {
                        width = MWidthF;
                        height = MHeightF;
                    }

                    float advanceNormalised = advanceWInt * normalisedScale;
                    float bearingLeftNormalised = leftBearingInt * normalisedScale;
                    float widthNormalised = x1 * normalisedScale - x0 * normalisedScale;
                    float heightNormalised = y1 * normalisedScale - y0 * normalisedScale;
                    if (widthNormalised == 0)
                    {
                        widthNormalised = MWidthFNormalised;
                        widthNormalised = MHeightFNormalised;
                    }

                    int sum = Math.Max(pixelWidthSumEnlarged[0], pixelWidthSumEnlarged[1]);
                    KWFontGlyph kwGlyph = new KWFontGlyph(theChar, widthNormalised, heightNormalised, bearingLeftNormalised, advanceNormalised, new Vector3(offsetX / sum, Math.Min(1f, (offsetX + (int)width) / sum), row == 0 ? 0f : 0.5f));
                    glyphDict.Add(theChar, kwGlyph);
                    offsetX += (int)width + pixelGap;
                }
            }
            return glyphDict;
        }

        internal static byte[] GenerateTextureForRes(Font f, int width, int height, ref KWFont kwfont)
        {
            float renderScale = f.ScaleInPixels(height / 2 - height / 2 * 0.025f);
            f.GetFontVMetrics(out int ascentInt, out int descentInt, out int lineGap);
            float ascent = ascentInt * renderScale;
            float descent = descentInt * renderScale;
            SKBitmap bigTex = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKCanvas canvas = new SKCanvas(bigTex);

            for (int row = 0; row < GLYPHS.Length; row++)
            {
                for (int x = 0; x < GLYPHS[row].Length; x++)
                {
                    char theChar = GLYPHS[row][x];
                    Vector3 glyphPositionInTexture = kwfont.GlyphDict[theChar].UCoordinate;
                    SKBitmap currentBitmap = new SKBitmap();

                    int glyphIndex = f.FindGlyphIndex(theChar);
                    f.GetGlyphBoundingBox(glyphIndex, 1, 1, 0, 0, out int x0, out int y0, out int x1, out int y1);
                    float downshift = ascent + y0 * renderScale;


                    GlyphBitmap bm = f.RenderGlyph(theChar, renderScale, System.Drawing.Color.White, System.Drawing.Color.Transparent).Result;
                    SKBitmap bm8 = new SKBitmap(bm.Width, bm.Height, SKColorType.R8Unorm, SKAlphaType.Opaque);
                    byte[] bm8array = new byte[bm8.ByteCount];
                    for(int i = 3, j = 0; i < bm.Pixels.Length; i += 4, j++)
                    {
                        //bm8array[bm8array.Length - j - 1] = bm.Pixels[i];
                        bm8array[j] = bm.Pixels[i];
                    }

                    GCHandle gch = GCHandle.Alloc(bm8array, GCHandleType.Pinned);
                    IntPtr allocatedBytes = gch.AddrOfPinnedObject();
                    bm8.SetPixels(allocatedBytes);
                    gch.Free();

                    // copy to big texture:
                    float offsetX = glyphPositionInTexture.X * width;
                    if(bm8 != null && bm8.DrawsNothing == false)
                    {
                        SKPoint target = new SKPoint(offsetX, downshift + (kwfont.GlyphDict[theChar].UCoordinate.Z == 0f ? 0 : height / 2));
                        canvas.DrawBitmap(bm8, target);
                        canvas.Flush();
                    }
                    bm8.Dispose();
                }
            }
            canvas.Dispose();

            string tmpFilename = "" + kwfont.Name + ".jpg";
            using (SKWStream stream = SKFileWStream.OpenStream(tmpFilename))
            {

                bigTex.Encode(stream, SKEncodedImageFormat.Jpeg, 100);
            }
            bigTex.Dispose();

            
            SKBitmap tmp = SKBitmap.Decode(tmpFilename);
            byte[] imageData = new byte[tmp.ByteCount];
            Array.Copy(tmp.Bytes, imageData, tmp.ByteCount);
            tmp.Dispose();
            File.Delete(tmpFilename);
            
            return imageData;
        }

        internal static bool IsValid(int n)
        {
            for (int i = 0; i < 5; i++)
            {
                if (n % 2 != 0)
                {
                    return false;
                }
                n /= 2;
            }
            return true;
        }

        internal static int FindNextDivisibleNumber(int n)
        {
            n += 1;
            while (!IsValid(n))
            {
                n += 1;
            }
            return n;
        }
    }
}
