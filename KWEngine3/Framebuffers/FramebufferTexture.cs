using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferTexture
    {
        public int ID { get; set; } = -1;
        public TextureTarget _target = TextureTarget.Texture2D;
        public FramebufferTextureMode Mode { get; set; } = FramebufferTextureMode.RGBA8;
        public FramebufferTexture(FramebufferTextureMode mode, int width, int height, int attachmentNumber, TextureMinFilter filter = TextureMinFilter.Nearest, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool borderColorWhite = false, bool cubeMap = false)
        {
            uint wrapmode = (uint)wrapMode;
            float[] borderColor = borderColorWhite ? new float[] { 1, 1, 1, 1 } : new float[] { 0, 0, 0, 0 };
            Mode = mode;
            _target = cubeMap ? TextureTarget.TextureCubeMap : TextureTarget.Texture2D;
            ID = GL.GenTexture();
            Bind();

            if (cubeMap)
            {
                for (int j = 0; j < 6; j++)
                {
                    HelperGeneral.CheckGLErrors();
                    GL.TexImage2D(
                        TextureTarget.TextureCubeMapPositiveX + j, 
                        0, 
                        mode == FramebufferTextureMode.RGBA16F ? PixelInternalFormat.Rgba16f
                        : mode == FramebufferTextureMode.RGBA32F ? PixelInternalFormat.Rgba32f
                        : mode == FramebufferTextureMode.RGBA16UI ? PixelInternalFormat.Rgba16
                        : mode == FramebufferTextureMode.DEPTH32F ? PixelInternalFormat.DepthComponent32f
                        : mode == FramebufferTextureMode.RGB8 ? PixelInternalFormat.Rgb8
                        : PixelInternalFormat.Rgba8,
                        width, 
                        height, 
                        0,
                        mode == FramebufferTextureMode.DEPTH32F ? PixelFormat.DepthComponent : mode == FramebufferTextureMode.RGB8 ? PixelFormat.Rgb : PixelFormat.Rgba,
                        mode == FramebufferTextureMode.RGBA16UI ? PixelType.UnsignedShort : mode == FramebufferTextureMode.RGBA8 ? PixelType.UnsignedByte : mode == FramebufferTextureMode.RGBA16F ? PixelType.HalfFloat : PixelType.Float,
                        IntPtr.Zero);
                    HelperGeneral.CheckGLErrors();
                }
                HelperGeneral.CheckGLErrors();
                GL.TexParameterI(_target, TextureParameterName.TextureCompareMode, new int[] { (int)TextureCompareMode.CompareRefToTexture });
                HelperGeneral.CheckGLErrors();
                GL.TexParameterI(_target, TextureParameterName.TextureCompareFunc, new int[] { (int)DepthFunction.Lequal });
                HelperGeneral.CheckGLErrors();
            }
            else
            {
                GL.TexImage2D(
                    _target,
                    0,
                    mode == FramebufferTextureMode.RGBA16F ? PixelInternalFormat.Rgba16f
                    : mode == FramebufferTextureMode.RGBA32F ? PixelInternalFormat.Rgba32f
                    : mode == FramebufferTextureMode.RGBA16UI ? PixelInternalFormat.Rgba16
                    : mode == FramebufferTextureMode.DEPTH32F ? PixelInternalFormat.DepthComponent32f
                    : mode == FramebufferTextureMode.RGB8 ? PixelInternalFormat.Rgb8
                    : PixelInternalFormat.Rgba8,
                    width,
                    height,
                    0,
                    mode == FramebufferTextureMode.DEPTH32F ? PixelFormat.DepthComponent : mode == FramebufferTextureMode.RGB8 ? PixelFormat.Rgb : PixelFormat.Rgba,
                    mode == FramebufferTextureMode.RGBA16UI ? PixelType.UnsignedShort : mode == FramebufferTextureMode.RGBA8 ? PixelType.UnsignedByte : PixelType.Float,
                    IntPtr.Zero);
                
            }
            HelperGeneral.CheckGLErrors();
            
            GL.TexParameterI(_target, TextureParameterName.TextureWrapS, ref wrapmode);
            GL.TexParameterI(_target, TextureParameterName.TextureWrapT, ref wrapmode);
            if (cubeMap)
                GL.TexParameterI(_target, TextureParameterName.TextureWrapR, ref wrapmode);

            GL.TexParameter(_target, TextureParameterName.TextureMinFilter, (float)filter);
            GL.TexParameter(_target, TextureParameterName.TextureMagFilter, filter == TextureMinFilter.Nearest ? (float)TextureMagFilter.Nearest : (float)TextureMagFilter.Linear);
            
            GL.TexParameter(_target, TextureParameterName.TextureBorderColor, borderColor);
            if(mode == FramebufferTextureMode.DEPTH32F)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, ID, 0);
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + attachmentNumber, ID, 0);
            }
            if (_target == TextureTarget.Texture2D && filter == TextureMinFilter.LinearMipmapLinear)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            HelperGeneral.CheckGLErrors();
        }

        public void Bind()
        {
            GL.BindTexture(_target, ID);
        }
        public void Unbind()
        {
            GL.BindTexture(_target, 0);
        }

        public void Dispose()
        {
            if(ID > 0)
            {
                GL.DeleteTextures(1, new int[] { ID });
            }
        }
    }
}
