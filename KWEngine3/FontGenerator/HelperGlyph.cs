using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KWEngine3.FontGenerator
{
    internal static class HelperGlyph
    {
        internal static readonly string GLYPHS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~°€§²³ÜÖÄüöäß·±";

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

        public static KWFont LoadFont(Font f)
        {
            if (f == null)
                return new KWFont() { IsValid = false };

            KWFont kwfont = new KWFont();

            kwfont.GlyphDict = GenerateGlyphsAndDict(f, 256, out int mipmap0Width);

            byte[] tx256 = GenerateTextureForRes(f, mipmap0Width, 256, ref kwfont);
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, mipmap0Width, 256, 0, PixelFormat.Bgra, PixelType.UnsignedByte, tx256);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            kwfont.Texture = texture;
            kwfont.TextureSize = (int)(tx256.Length / 4 * 1.333333f);
            return kwfont;
        }

        internal static Dictionary<char, KWFontGlyph> GenerateGlyphsAndDict(Font f, int res, out int pixelWidthSumEnlarged)
        {
            int pixelGap = res / 4;
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

            float pixelWidthSum = 0;
            for (int i = 0; i < GLYPHS.Length; i++)
            {
                char n = GLYPHS[i];
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

            pixelWidthSumEnlarged = FindNextDivisibleNumber((int)pixelWidthSum);

            Dictionary<char, KWFontGlyph> glyphDict = new();

            float offsetX = 0;
            for (int x = 0; x < GLYPHS.Length; x++)
            {
                char theChar = GLYPHS[x];
                

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


                KWFontGlyph kwGlyph = new KWFontGlyph(theChar, widthNormalised, heightNormalised, bearingLeftNormalised, advanceNormalised, new Vector2(offsetX / pixelWidthSumEnlarged, Math.Min(1f, (offsetX + (int)width) / pixelWidthSumEnlarged)));
                glyphDict.Add(theChar, kwGlyph);
                offsetX += (int)width + pixelGap;
            }
            return glyphDict;
        }

        internal static byte[] GenerateTextureForRes(Font f, int width, int height, ref KWFont kwfont)
        {
            float renderScale = f.ScaleInPixels(height);
            f.GetFontVMetrics(out int ascentInt, out int descentInt, out int lineGap);
            float ascent = ascentInt * renderScale;
            float descent = descentInt * renderScale;
            SKBitmap bigTex = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKCanvas canvas = new SKCanvas(bigTex);

            for (int x = 0; x < GLYPHS.Length; x++)
            {
                char theChar = GLYPHS[x];
                Vector2 glyphPositionInTexture = kwfont.GlyphDict[theChar].UCoordinate;
                SKBitmap currentBitmap = new SKBitmap();

                int glyphIndex = f.FindGlyphIndex(theChar);
                f.GetGlyphBoundingBox(glyphIndex, 1, 1, 0, 0, out int x0, out int y0, out int x1, out int y1);
                float downshift = ascent + y0 * renderScale;


                GlyphBitmap bm = f.RenderGlyph(theChar, renderScale, System.Drawing.Color.White, System.Drawing.Color.Transparent).Result;
                SKBitmap bm8 = new SKBitmap(bm.Width, bm.Height, SKColorType.R8Unorm, SKAlphaType.Opaque);
                byte[] bm8array = new byte[bm8.ByteCount];
                for(int i = 3, j = 0; i < bm.Pixels.Length; i+=4, j++)
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
                SKPoint target = new SKPoint(offsetX, downshift);
                canvas.DrawBitmap(bm8, target);
                canvas.Flush();
                
                bm8.Dispose();
            }
            canvas.Dispose();

            using (SKWStream stream = SKFileWStream.OpenStream("" + height + ".jpg"))
            {
                bigTex.Encode(stream, SKEncodedImageFormat.Jpeg, 100);
            }
            bigTex.Dispose();

            SKBitmap tmp = SKBitmap.Decode("" + height + ".jpg");
            byte[] imageData = new byte[tmp.ByteCount];
            Array.Copy(tmp.Bytes, imageData, tmp.ByteCount);
            tmp.Dispose();
            File.Delete("" + height + ".jpg");
            
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
