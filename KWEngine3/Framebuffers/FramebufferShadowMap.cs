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

        public FramebufferShadowMap(int width, int height, LightType lightType, SunShadowType shadowType = SunShadowType.Default)
            : base(width, height, true, lightType, shadowType)
        {
        }

        public override void Init(int width, int height)
        {
            SizeInBytes = width * height * 4 * sizeof(ushort) * (_shadowType == SunShadowType.CascadedShadowMap ? 2 : 1);
            SizeInBytes += width * height * sizeof(float) * (_shadowType == SunShadowType.CascadedShadowMap ? 2 : 1);

            if (_lightType == LightType.Point)
                SizeInBytes *= 6;

            Bind(false); 
            ClearColorValues.Add(0, new float[] { 1, 1, 1, 1 });

            Attachments.Add(new FramebufferTexture(width, 0, _lightType == LightType.Point, false, _shadowType == SunShadowType.CascadedShadowMap));
            Attachments.Add(new FramebufferTexture(width, 1, _lightType == LightType.Point, true, _shadowType == SunShadowType.CascadedShadowMap));
           
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
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