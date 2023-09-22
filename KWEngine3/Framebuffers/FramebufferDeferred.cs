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
            bool hq = KWEngine.Window._ppQuality == PostProcessingQuality.High;

            Bind(false);
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA32F, width, height, 0));  // Position and ID
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 1));     // Albedo
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16F, width, height, 2));  // Normal and CSDepth
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGB8, width, height, 3));     // Metallic, Roughness, MetallicType attachment
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.RGBA16F, width, height, 4));  // Emissive TODO: check if there is a R8G8B8A16 texture mode in OpenGL
            Attachments.Add(new FramebufferTexture(FramebufferTextureMode.DEPTH32F, width, height, 5)); // Depth
            
            DrawBuffersEnum[] dbe = new DrawBuffersEnum[Attachments.Count - 1];
            for(int i = 0; i < Attachments.Count - 1; i++)
            {
                dbe[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(Attachments.Count, dbe);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ClearColorValues.Add(0, new float[] { 0, 0, 0, 0 });
            ClearColorValues.Add(1, new float[] { 0, 0, 0 });
            ClearColorValues.Add(2, new float[] { 0, 0, 0, 1 });
            ClearColorValues.Add(3, new float[] { 0, 1, 0 });
            ClearColorValues.Add(4, new float[] { 0, 0, 0, 0 });

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
