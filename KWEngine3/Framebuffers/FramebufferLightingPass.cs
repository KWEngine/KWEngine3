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
            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count];
            for(int i = 0; i < Attachments.Count; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            
            Renderbuffers.Add(GL.GenRenderbuffer());
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
            
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0, 1 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0, 1 });
        }

        public override void Clear()
        {
            for(int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }
    }
}
