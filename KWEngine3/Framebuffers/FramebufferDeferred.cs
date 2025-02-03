using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferDeferred : Framebuffer
    {
        public FramebufferDeferred(int width, int height)
            : base(width, height, false, LightType.Point)
        {

        }

        public override void Init()
        {
            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB16F, _size.X, _size.Y, 0, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));   // Albedo
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB16F, _size.X, _size.Y, 1, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));   // Normal
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, _size.X, _size.Y, 2, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));     // Metallic, Roughness, MetallicType attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RG32I, _size.X, _size.Y, 3, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge));     // ID, ShadowCaster
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F, _size.X, _size.Y, 4, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge)); // Depth
            SizeInBytes =
                _size.X * _size.Y * 3 * sizeof(float) +
                _size.X * _size.Y * 3 * sizeof(float) +
                _size.X * _size.Y * 3 * sizeof(byte) +
                _size.X * _size.Y * 2 * sizeof(int) +
                _size.X * _size.Y * 1 * sizeof(float);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count - 1];
            for(int i = 0; i < Attachments.Count - 1; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0 });
            ClearColorValues.Add(2, new float[] { 0, 1, 0 });
            ClearColorValues.Add(3, new float[] { 0, 0 });

            //ClearDepthValues.Add(0, new float[] { 0, 0, 0, 0 });
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
