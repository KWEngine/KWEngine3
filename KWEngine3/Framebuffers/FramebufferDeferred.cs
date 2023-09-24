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
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB16F, width, height, 0));   // Albedo
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB16F, width, height, 1));   // Normal
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 2));     // Metallic, Roughness, MetallicType attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.R32F, width, height, 3));     // ID
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F, width, height, 4)); // Depth
            
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
            ClearColorValues.Add(3, new float[] { 0 });

            ClearDepthValues.Add(0, new float[] { 0, 0, 0, 0 });
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
