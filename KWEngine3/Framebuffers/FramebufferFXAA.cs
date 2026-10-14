using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    // Target of the FXAA pass. Its color texture is swapped with the color attachment of the lighting pass
    // after every FXAA pass (RenderManager.SwapLightingPassColorWithFXAATarget), so both textures must have
    // exactly the same format and sampling state as FramebufferLighting's attachment 0.
    internal class FramebufferFXAA : Framebuffer
    {
        public FramebufferFXAA(int width, int height)
            : base(width, height, false, LightType.Point)
        {

        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 0));   // Color (same as FramebufferLighting)

            SizeInBytes = width * height * 3 * sizeof(byte);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 };
            GL.DrawBuffers(dbe.Length, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0 });

            FramebufferErrorCode error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (error != FramebufferErrorCode.FramebufferComplete)
            {
                KWEngine.LogWriteLine("[Renderer] FramebufferFXAA: incomplete.");
            }
        }

        public override void Clear(bool keepDepth = false)
        {
            GL.ClearBuffer(ClearBuffer.Color, 0, ClearColorValues[0]);
        }
    }
}
