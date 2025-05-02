using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferShadowMap : Framebuffer
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

        public FramebufferShadowMap(int width, int height, LightType lightType)
            : base(width, height, true, lightType)
        {
        }

        public override void Init(int width, int height)
        {
            SizeInBytes = width * height * 4 * sizeof(ushort);
            if (_lightType == LightType.Point)
                SizeInBytes *= 6;

            Bind(false); 
            ClearColorValues.Add(0, new float[] { 1, 1, 1, 1 });
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16UI, width, height, 0, TextureMinFilter.Linear, TextureMagFilter.Linear, _lightType == LightType.Point ? TextureWrapMode.ClampToEdge : TextureWrapMode.ClampToBorder, true, _lightType == LightType.Point));
            
                
            FramebufferErrorCode status;

            if(_lightType == LightType.Point)
            {
                Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F, width, height, 1, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.ClampToEdge, true, true));
            }
            else if(_lightType == LightType.Sun && _shadowType == ShadowType.CascadedShadowMap)
            {

            }
            else
            {
                // TODO: Check why this has to be included for directional lights!
                Renderbuffers.Add(GL.GenRenderbuffer());
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, width, height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[0]);

            }
            SizeInBytes += width * height * sizeof(float);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer invalid.");
            }
            Unbind();
        }
    }
}