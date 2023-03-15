using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA32F, width, height, 0));   // Position and depth attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 1));   // Albedo
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA32F, width, height, 2));   // Normal and ID attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA32F, width, height, 3));   // CSDepth, Metallic, Roughness attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16F, width, height, 4));   // Emissive
            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count];
            for(int i = 0; i < Attachments.Count; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            Renderbuffers.Add(GL.GenRenderbuffer());
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Renderbuffers[0]);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0, 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0, 0 });
            ClearColorValues.Add(2, new float[] { 0, 0, 0, 0 });
            ClearColorValues.Add(3, new float[] { 1, 0, 1, 1 });
            ClearColorValues.Add(4, new float[] { 0, 0, 0, 0 });

            ClearDepthValues.Add(0, new float[] { 0, 0, 0, 0 });
        }

        public override void Clear()
        {
            for(int i = 0; i < ClearColorValues.Count; i++)
            {
                GL.ClearBuffer(ClearBuffer.Color, i, ClearColorValues[i]);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }
    }
}
