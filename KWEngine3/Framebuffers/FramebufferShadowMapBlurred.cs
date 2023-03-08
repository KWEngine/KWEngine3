using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferShadowMapBlur : Framebuffer
    {
        

        public FramebufferShadowMapBlur(int width, int height, LightType lightType)
            : base(width, height, true, lightType)
        {
        }

        public override void Clear()
        {
            GL.ClearColor(1, 1, 1, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16UI, width, height, 0, TextureMinFilter.LinearMipmapLinear, TextureWrapMode.ClampToBorder, true, _lightType == LightType.Point));
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer invalid.");
            }
        }
    }
}