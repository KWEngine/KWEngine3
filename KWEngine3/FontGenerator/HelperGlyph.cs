using KWEngine3.Helper;
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

        private static void DisposeFont(KWFont f)
        {
            GL.DeleteTexture(f.Texture);
            f.GlyphDict.Clear();
        }

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

            kwfont.GlyphDict = GenerateGlyphsAndDict(f, 256);

            byte[] tx256 = GenerateTextureForRes(f, 256, out int tw256, out int th256);
            byte[] tx128 = GenerateTextureForRes(f, 128, out int tw128, out int th128);
            byte[] tx64 = GenerateTextureForRes(f, 64, out int tw64, out int th64);
            byte[] tx32 = GenerateTextureForRes(f, 32, out int tw32, out int th32);
            byte[] tx16 = GenerateTextureForRes(f, 16, out int tw16, out int th16);
            byte[] tx8 = GenerateTextureForRes(f, 8, out int tw8, out int th8);

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, tw256, th256, 0, PixelFormat.Bgra, PixelType.UnsignedByte, tx256);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            /*
            GL.TexImage2D(TextureTarget.Texture2D, 1, PixelInternalFormat.R8, tw128, th128, 0, PixelFormat.Red, PixelType.UnsignedByte, tx128);
            GL.TexImage2D(TextureTarget.Texture2D, 2, PixelInternalFormat.R8, tw64, th64, 0, PixelFormat.Red, PixelType.UnsignedByte, tx64);
            GL.TexImage2D(TextureTarget.Texture2D, 3, PixelInternalFormat.R8, tw32, th32, 0, PixelFormat.Red, PixelType.UnsignedByte, tx32);
            GL.TexImage2D(TextureTarget.Texture2D, 4, PixelInternalFormat.R8, tw16, th16, 0, PixelFormat.Red, PixelType.UnsignedByte, tx16);
            GL.TexImage2D(TextureTarget.Texture2D, 5, PixelInternalFormat.R8, tw8, th8, 0, PixelFormat.Red, PixelType.UnsignedByte, tx8);
            */
            
            GL.BindTexture(TextureTarget.Texture2D, 0);

            kwfont.Texture = texture;
            kwfont.TextureSize = tx256.Length + tx128.Length + tx64.Length + tx32.Length + tx16.Length + tx8.Length;

            return kwfont;
        }

        internal static Dictionary<char, KWFontGlyph> GenerateGlyphsAndDict(Font f, int res)
        {
            int pixelGap = res / 8;
            float renderScale = f.ScaleInPixels(res);
            float normalisedScale = f.ScaleInPixels(1f);
            // default width and advance for M:
            int glyphIndexM = f.FindGlyphIndex('M');
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
                f.GetGlyphBitmapBox(n, 1, 1, 0f, 0f, out int x0, out int y0, out int x1, out int y1);
                float pixelWidthForGlyph = x1 * renderScale - x0 * renderScale;
                if (pixelWidthForGlyph == 0)
                {
                    pixelWidthForGlyph = MWidthF;
                }
                pixelWidthSum += (int)pixelWidthForGlyph + pixelGap;
            }
            pixelWidthSum -= pixelGap;

            Dictionary<char, KWFontGlyph> glyphDict = new();

            int offsetX = 0;
            for (int x = 0; x < GLYPHS.Length; x++)
            {
                char theChar = GLYPHS[x];
                

                int glyphIndex = f.FindGlyphIndex(theChar);
                f.GetGlyphHMetrics(glyphIndex, out int advanceWInt, out int leftBearingInt);
                f.GetGlyphVMetrics(glyphIndex, out int advanceHInt, out int topBearingInt);
                f.GetGlyphBoundingBox(glyphIndex, 1, 1, 0, 0, out int x0, out int y0, out int x1, out int y1);
                f.GetGlyphBitmapBox(theChar, renderScale, renderScale, 0f, 0f, out int x0bmp, out int y0bmp, out int x1bmp, out int y1bmp);

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


                KWFontGlyph kwGlyph = new KWFontGlyph(theChar, widthNormalised, heightNormalised, bearingLeftNormalised, advanceNormalised, new Vector2(offsetX / pixelWidthSum, Math.Min(1f, (offsetX + (int)width + pixelGap) / pixelWidthSum)));
                glyphDict.Add(theChar, kwGlyph);
                offsetX += (int)width + pixelGap;
            }
            return glyphDict;
        }

        internal static byte[] GenerateTextureForRes(Font f, int res, out int textureWidth, out int textureHeight)
        {
            int dummy = 0;
            int pixelGap = res / 8;

            float renderScale = f.ScaleInPixels(res);

            // default width and advance for M:
            int glyphIndexM = f.FindGlyphIndex('M');
            f.GetGlyphHMetrics(glyphIndexM, out int MAdvance, out dummy);
            float MAdvanceF = MAdvance * renderScale;
            f.GetGlyphBoundingBox(glyphIndexM, 1, 1, 0, 0, out int Mx0, out int My0, out int Mx1, out int My1);
            float MWidthF = Mx1 * renderScale - Mx0 * renderScale;
            float MHeightF = My1 * renderScale - My0 * renderScale;

            // general metrics for font:
            f.GetFontVMetrics(out int ascentInt, out int descentInt, out int lineGap);
            float ascent = ascentInt * renderScale;
            float descent = descentInt * renderScale;

            int pixelWidthSum = 0;
            for (int i = 0; i < GLYPHS.Length; i++)
            {
                char n = GLYPHS[i];
                int glyphIndex = f.FindGlyphIndex(n);
                f.GetGlyphBitmapBox(n, 1, 1, 0f, 0f, out int x0, out int y0, out int x1, out int y1);
                float pixelWidthForGlyph = x1 * renderScale - x0 * renderScale;
                if (pixelWidthForGlyph == 0)
                {
                    pixelWidthForGlyph = MWidthF;
                }
                pixelWidthSum += (int)pixelWidthForGlyph + pixelGap;
            }
            pixelWidthSum -= pixelGap;

            SKBitmap bigTex = new SKBitmap(pixelWidthSum, res, SKColorType.Rgba8888, SKAlphaType.Premul);
            textureWidth = pixelWidthSum;
            textureHeight = res;
            int offsetX = 0;

            for(int x = 0; x < GLYPHS.Length; x++)
            {
                char theChar = GLYPHS[x];
                int glyphIndex = f.FindGlyphIndex(theChar);
                f.GetGlyphHMetrics(glyphIndex, out int advanceWInt, out int leftBearingInt);
                f.GetGlyphVMetrics(glyphIndex, out int advanceHInt, out int topBearingInt);
                f.GetGlyphBoundingBox(glyphIndex, 1, 1, 0, 0, out int x0, out int y0, out int x1, out int y1);
                f.GetGlyphBitmapBox(theChar, renderScale, renderScale, 0f, 0f, out int x0bmp, out int y0bmp, out int x1bmp, out int y1bmp);

                float advance = advanceWInt * renderScale;
                float bearingLeft = leftBearingInt * renderScale;
                float width = x1 * renderScale - x0 * renderScale;
                float height = y1 * renderScale - y0 * renderScale;
                if (width == 0)
                {
                    width = MWidthF;
                    height = MHeightF;
                }

                // y0 = oben
                // y1 = unten
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
                SKCanvas canvas = new SKCanvas(bigTex);
                SKPoint target = new SKPoint(offsetX, downshift);
                canvas.DrawBitmap(bm8, target);
                canvas.Flush();

                canvas.Dispose();
                bm8.Dispose();
                offsetX += (int)width + pixelGap;
            }

            
            using (SKWStream stream = SKFileWStream.OpenStream("" + res + ".jpg"))
            {
                bigTex.Encode(stream, SKEncodedImageFormat.Jpeg, 100);
            }
            bigTex.Dispose();

            SKBitmap tmp = SKBitmap.Decode("" + res + ".jpg");
            byte[] imageData = new byte[tmp.ByteCount];
            Array.Copy(tmp.Bytes, imageData, tmp.ByteCount);
            tmp.Dispose();
            
            return imageData;
        }
    }
}
