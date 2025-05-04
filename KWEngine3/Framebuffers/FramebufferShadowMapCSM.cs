using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferShadowMapCSM : FramebufferShadowMap
    {
        public override void Clear(bool keepDepth = false)
        {
            for (int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            if(keepDepth == false)
                GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public FramebufferShadowMapCSM(int width, int height)
            : base(width, height, LightType.Sun)
        {
            _shadowType = ShadowType.CascadedShadowMap;
        }

        public override void Init(int width, int height)
        {
            SizeInBytes = width * height * 4 * sizeof(ushort) + width * height * sizeof(float);

            Bind(false); 
            ClearColorValues.Add(0, new float[] { 1, 1, 1, 1 });
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16UI_3D, width, height, 0, 2));
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F_3D, width, height, 1, 2));

            FramebufferErrorCode status;
            /*
            Renderbuffers.Add(GL.GenRenderbuffer());
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[0]);

            Renderbuffers.Add(GL.GenRenderbuffer());
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[1]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[1]);
            */
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ReadBuffer(ReadBufferMode.None);

            status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer invalid.");
            }
            Unbind();
        }
    }
}