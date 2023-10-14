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
                    GL.TexImage2D(
                        TextureTarget.TextureCubeMapPositiveX + j, 
                        0, 
                        mode == FramebufferTextureMode.RGBA16F ? PixelInternalFormat.Rgba16f
                        : mode == FramebufferTextureMode.RGBA32F ? PixelInternalFormat.Rgba32f
                        : mode == FramebufferTextureMode.RGBA16UI ? PixelInternalFormat.Rgba16
                        : mode == FramebufferTextureMode.DEPTH32F ? PixelInternalFormat.DepthComponent32f
                        : mode == FramebufferTextureMode.DEPTH16F ? PixelInternalFormat.DepthComponent16
                        : mode == FramebufferTextureMode.RGB16F ? PixelInternalFormat.Rgb16f
                        : mode == FramebufferTextureMode.RGB8 ? PixelInternalFormat.Rgb8
                        : mode == FramebufferTextureMode.RG8 ? PixelInternalFormat.Rg8
                        : mode == FramebufferTextureMode.R32F ? PixelInternalFormat.R32f
                        : PixelInternalFormat.Rgba8,
                        width, 
                        height, 
                        0,
                        mode == FramebufferTextureMode.DEPTH32F || mode == FramebufferTextureMode.DEPTH16F ? PixelFormat.DepthComponent : (mode == FramebufferTextureMode.RGB8 || mode == FramebufferTextureMode.RGB16F )? PixelFormat.Rgb : mode == FramebufferTextureMode.RG8 ? PixelFormat.Rg : mode == FramebufferTextureMode.R32F ?  PixelFormat.Red : PixelFormat.Rgba,
                        mode == FramebufferTextureMode.RGBA16UI ? PixelType.UnsignedShort : (mode == FramebufferTextureMode.RGBA8 || mode == FramebufferTextureMode.RG8) ? PixelType.UnsignedByte : (mode == FramebufferTextureMode.RGBA16F || mode == FramebufferTextureMode.RGB16F) ? PixelType.HalfFloat : PixelType.Float,
                        IntPtr.Zero);
                }
                GL.TexParameterI(_target, TextureParameterName.TextureCompareMode, new int[] { (int)TextureCompareMode.CompareRefToTexture });
                GL.TexParameterI(_target, TextureParameterName.TextureCompareFunc, new int[] { (int)DepthFunction.Lequal });
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
                    : mode == FramebufferTextureMode.DEPTH16F ? PixelInternalFormat.DepthComponent16
                    : mode == FramebufferTextureMode.RGB16F ? PixelInternalFormat.Rgb16f
                    : mode == FramebufferTextureMode.RGB8 ? PixelInternalFormat.Rgb8
                    : mode == FramebufferTextureMode.RG8 ? PixelInternalFormat.Rg8
                    : mode == FramebufferTextureMode.R32F ? PixelInternalFormat.R32f
                    : PixelInternalFormat.Rgba8,
                    width,
                    height,
                    0,
                    mode == FramebufferTextureMode.DEPTH32F || mode == FramebufferTextureMode.DEPTH16F ? PixelFormat.DepthComponent : (mode == FramebufferTextureMode.RGB8 || mode == FramebufferTextureMode.RGB16F) ? PixelFormat.Rgb : mode == FramebufferTextureMode.RG8 ? PixelFormat.Rg : mode == FramebufferTextureMode.R32F ? PixelFormat.Red : PixelFormat.Rgba,
                    mode == FramebufferTextureMode.RGBA16UI ? PixelType.UnsignedShort : (mode == FramebufferTextureMode.RGBA8 || mode == FramebufferTextureMode.RG8) ? PixelType.UnsignedByte : (mode == FramebufferTextureMode.RGBA16F || mode == FramebufferTextureMode.RGB16F) ? PixelType.HalfFloat : PixelType.Float,
                    IntPtr.Zero);
                
            }
            
            GL.TexParameterI(_target, TextureParameterName.TextureWrapS, ref wrapmode);
            GL.TexParameterI(_target, TextureParameterName.TextureWrapT, ref wrapmode);
            if (cubeMap)
                GL.TexParameterI(_target, TextureParameterName.TextureWrapR, ref wrapmode);

            GL.TexParameter(_target, TextureParameterName.TextureMinFilter, (float)filter);
            GL.TexParameter(_target, TextureParameterName.TextureMagFilter, filter == TextureMinFilter.Nearest ? (float)TextureMagFilter.Nearest : (float)TextureMagFilter.Linear);
            
            GL.TexParameter(_target, TextureParameterName.TextureBorderColor, borderColor);
            if(mode == FramebufferTextureMode.DEPTH32F || mode == FramebufferTextureMode.DEPTH16F)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, ID, 0);
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + attachmentNumber, ID, 0);
            }
            if (_target == TextureTarget.Texture2D && filter == TextureMinFilter.LinearMipmapLinear)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
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
