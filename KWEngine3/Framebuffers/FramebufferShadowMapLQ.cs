using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferShadowMapLQ : FramebufferShadowMap
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

        public FramebufferShadowMapLQ(int width, int height, LightType lightType, SunShadowType shadowType = SunShadowType.Default)
            : base(width, height, lightType, shadowType)
        {
        }

        public override void Init(int width, int height)
        {
            SizeInBytes = width * height  * sizeof(float) * (_shadowType == SunShadowType.CascadedShadowMap ? 2 : 1);

            if (_lightType == LightType.Point)
                SizeInBytes *= 6;

            Bind(false); 

            Attachments.Add(new FramebufferTexture(width, 0, _lightType == LightType.Point, true, _shadowType == SunShadowType.CascadedShadowMap));
           
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer invalid.");
            }
            Unbind();
        }
    }
}