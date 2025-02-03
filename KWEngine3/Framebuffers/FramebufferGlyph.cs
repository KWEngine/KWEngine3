using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferGlyph : Framebuffer
    {


        public FramebufferGlyph(int width, int height)
            : base(width, height, false, LightType.Point)
        {

        }

        public override void Init()
        {
            _size.X = _size.X * 2;
            _size.Y = _size.Y * 2;
            

            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.R8, _size.X, _size.Y, 0, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.ClampToEdge));   // Glyphs
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA8, _size.X, _size.Y, 1, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.ClampToEdge));   // Color/Alpha

            SizeInBytes =
                _size.X * _size.Y * 1 * sizeof(byte) +
                _size.X * _size.Y * 4 * sizeof(byte);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count - 1];
            for(int i = 0; i < Attachments.Count - 1; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0, 0 });
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
