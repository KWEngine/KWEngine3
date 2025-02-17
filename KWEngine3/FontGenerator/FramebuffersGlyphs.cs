using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.FontGenerator
{
    internal static class FramebuffersGlyphs
    {
        internal static int FBGlyphs = -1;
        internal static int FBGlyphsBlend = -1;
        internal static int FBGlyphsTexture = -1;
        internal static int FBGlyphsBlendTexture = -1;

        internal static int Width = 0;
        internal static int Height = 0;

        public static void Dispose()
        {
            if (FBGlyphs > 0)
            {
                GL.DeleteTextures(2, new int[] { FBGlyphsTexture, FBGlyphsBlendTexture });
                GL.DeleteFramebuffers(2, new int[] { FBGlyphs, FBGlyphsBlend });
            }
        }

        public static void ReInit(int width, int height)
        {
            Width = width;
            Height = height;

            if(FBGlyphs > 0)
            {
                GL.DeleteTextures(2, new int[] { FBGlyphsTexture, FBGlyphsBlendTexture });
                GL.DeleteFramebuffers(2, new int[] { FBGlyphs, FBGlyphsBlend });
            }

            // WORKING
            FBGlyphs = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBGlyphs);

            // Color Attachment
            FBGlyphsTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FBGlyphsTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, FBGlyphsTexture, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            FramebufferErrorCode ecode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (ecode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer FBGlyphs incomplete");
            }
            HelperGeneral.CheckGLErrors();

            FBGlyphsBlend = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBGlyphsBlend);

            FBGlyphsBlendTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FBGlyphsBlendTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, FBGlyphsBlendTexture, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            

            ecode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (ecode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer FBGlyphsBlend incomplete");
            }
            HelperGeneral.CheckGLErrors();
        }
    }
}
