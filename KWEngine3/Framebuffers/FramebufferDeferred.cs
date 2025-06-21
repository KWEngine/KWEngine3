using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferDeferred : Framebuffer
    {
        public FramebufferDeferred(int width, int height)
            : base(width, height, false, LightType.Point)
        {

        }

        public override void Init(int width, int height)
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.R11G11B10f, width, height, 0, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));   // Albedo
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RG16F, width, height, 1, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));   // Normal
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 2, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));     // Metallic, Roughness, MetallicType attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 3, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));     // ID, ShadowCaster
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F, width, height, 4, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge)); // Depth
            SizeInBytes =
                width * height * 4 +
                width * height * 2 * sizeof(short) +
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
            /*
            int rbuf = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbuf);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, rbuf);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            */
            ClearColorValues.Add(0, new float[] { 0, 0, 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0 });
            ClearColorValues.Add(2, new float[] { 0, 1, 0 });
            ClearColorValues.Add(3, new float[] { 0, 0, 0 });

            //ClearDepthValues.Add(0, new float[] { 1 });
        }

        public override void Clear(bool keepDepth = false)
        {
            for(int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            if(keepDepth == false)
                GL.Clear(ClearBufferMask.DepthBufferBit);
        }
    }
}
