using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferBloom : Framebuffer
    {
        public FramebufferBloom(int width, int height)
            :base(width, height, false, LightType.Point)
        {

        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA8, width, height, 0, TextureMinFilter.Linear, TextureWrapMode.ClampToEdge, false, false)); // Color1
            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count];
            for(int i = 0; i < Attachments.Count; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            ClearColorValues.Add(0, new float[] { 0, 0, 0, 1 });
        }

        public override void Clear(bool keepDepth = false)
        {
            for(int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
        }
    }
}
