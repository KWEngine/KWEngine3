using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferSSAOBlur : Framebuffer
    {
        public FramebufferSSAOBlur(int width, int height, bool isLight, LightType lightType) : base(width, height, isLight, lightType)
        {
        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.R8, width, height, 0, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));   // SSAO
            SizeInBytes = width * height * 1 * sizeof(byte);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count];
            for(int i = 0; i < Attachments.Count; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 1 });
        }

        public override void Clear(bool keepDepth = false)
        {
            for (int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ref ClearColorValues[0][0]);
            }
        }
    }
}
