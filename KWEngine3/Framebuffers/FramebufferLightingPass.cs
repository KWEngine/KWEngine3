using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferLighting : Framebuffer
    {
        public FramebufferLighting(int width, int height)
            :base(width, height, false, LightType.Point)
        {

        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 0));   // Color
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 1));   // Bloom
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH24STENCIL8, width, height, 2)); // Depth

            SizeInBytes =
                width * height * 3 * sizeof(byte) +
                width * height * 3 * sizeof(byte) +
                width * height * 1 * sizeof(float);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count - 1];
            for(int i = 0; i < Attachments.Count - 1; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0 });

            FramebufferErrorCode error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (error != FramebufferErrorCode.FramebufferComplete)
            {
                KWEngine.LogWriteLine("[Renderer] FramebufferLightingPass: incomplete.");
            }
        }

        public override void Clear(bool keepDepth = false)
        {
            for(int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            if (keepDepth == false)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilMask(0xFF);
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.StencilMask(0x00);
                GL.Disable(EnableCap.StencilTest);
            }
        }

        public void BindAndClearColor(bool keepDepth = false)
        {
            //Bind(false);
            Bind(true, keepDepth);
            /*for (int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }*/
        }
    }
}
