using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Framebuffers
{
    internal class FramebufferShadowMap : Framebuffer
    {
        public FramebufferShadowMapBlur _blurBuffer1;
        public FramebufferShadowMapBlur _blurBuffer2;

        public override void Clear()
        {
            for (int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public FramebufferShadowMap(int width, int height, LightType lightType)
            : base(width, height, true, lightType)
        {
        }

        public override void Init(int width, int height)
        {
            bool hq = KWEngine.Window._ppQuality == PostProcessingQuality.High;
            Bind(false);
            ClearColorValues.Add(0, new float[] { 1, 1, 1, 1 });
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16UI, width, height, 0, TextureMinFilter.Nearest, TextureWrapMode.ClampToBorder, true, _lightType == LightType.Point));
            FramebufferErrorCode status;

            if(_lightType == LightType.Point)
            {
                Attachments.Add(new FramebufferTexture(hq ? FramebufferTextureMode.DEPTH32F : FramebufferTextureMode.DEPTH16F, width, height, 1, TextureMinFilter.Nearest, TextureWrapMode.ClampToBorder, true, true));
            }
            else
            {
                // TODO: Check whether this can go or not :-)
                /*
                Renderbuffers.Add(GL.GenRenderbuffer());
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, hq ? RenderbufferStorage.DepthComponent32f : RenderbufferStorage.DepthComponent16, width, height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
                */
            }

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer invalid.");
            }
            Unbind();

            if (_lightType != LightType.Point)
            {
                _blurBuffer1 = new FramebufferShadowMapBlur(width, height, _lightType);
                _blurBuffer2 = new FramebufferShadowMapBlur(width, height, _lightType);
            }
        }
    }
}