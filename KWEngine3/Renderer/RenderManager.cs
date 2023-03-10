using KWEngine2.Renderer;
using KWEngine3.Editor;
using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Renderer
{
    internal static class RenderManager
    {
        public static FramebufferDeferred FramebufferDeferred { get; set; }
        public static FramebufferLighting FramebufferLightingPass { get; set; }
        public static FramebufferBloom[] FramebuffersBloom { get; set; } = new FramebufferBloom[KWEngine.MAX_BLOOM_BUFFERS];
        public static FramebufferBloom[] FramebuffersBloomTemp { get; set; } = new FramebufferBloom[KWEngine.MAX_BLOOM_BUFFERS];

        //internal static int[] FramebufferBloomDownsampleSteps = new int[KWEngine.MAX_BLOOM_BUFFERS];

        public static void BindScreen(bool clear = true)
        {
            GL.Viewport(0, 0, KWEngine.Window.ClientSize.X, KWEngine.Window.ClientSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (clear)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        public static void InitializeFramebuffers()
        {
            FramebufferQuad.Init();
            FramebufferDeferred = new FramebufferDeferred(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            FramebufferLightingPass = new FramebufferLighting(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);

            // Bloom
            for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS; i++)
            {
                FramebuffersBloom[i] = new FramebufferBloom(KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
                FramebuffersBloomTemp[i] = new FramebufferBloom(KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
            }
        }

        public static void InitializeClearColor()
        {
            GL.ClearColor(0, 0, 0, 0);
        }

        public static void InitializeShaders()
        {
            PrimitiveQuad.Init();
            PrimitivePoint.Init();

            RendererGBuffer.Init();
            RendererTerrainGBuffer.Init();
            RendererLightingPass.Init();
            RendererForward.Init();
            RendererCopy.Init();
            RendererBackgroundSkybox.Init();
            RendererBackgroundStandard.Init();
            RendererExplosion.Init();
            RendererParticle.Init();

            RendererShadowMap.Init();
            RendererShadowMapCube.Init();
            RendererShadowMapBlur.Init();

            RendererEditor.Init();
            RendererGrid.Init();
            RendererLightOverlay.Init();
            RendererLightFrustum.Init();
            RendererOctreeNodes.Init();
            

            RendererBloomDownsample.Init();
            RendererBloomUpsample.Init();

            RendererHUD.Init();
        }
        

        public static void CheckShaderStatus(int programId, int vertexShaderId, int fragmentShaderId, int geometryShaderId = -1)
        {
            string vMsg = "";
            string fMsg = "";
            GL.GetShader(vertexShaderId, ShaderParameter.CompileStatus, out int vertexStatus);
            GL.GetShader(fragmentShaderId, ShaderParameter.CompileStatus, out int fragmentStatus);
            if (vertexStatus == 0 || fragmentStatus == 0)
            {
                if(vertexStatus == 0)
                {
                    vMsg = GL.GetShaderInfoLog(vertexShaderId);
                    KWEngine.LogWriteLine("[ShaderVertex] " + vMsg);
                }
                if(fragmentStatus == 0)
                {
                    fMsg = GL.GetShaderInfoLog(fragmentShaderId);
                    KWEngine.LogWriteLine("[ShaderFragment] " + fMsg);
                }
            }

            if(geometryShaderId > 0)
            {
                string gMsg = "";
                GL.GetShader(geometryShaderId, ShaderParameter.CompileStatus, out int geometryStatus);
                if(geometryStatus == 0)
                {
                    gMsg = GL.GetShaderInfoLog(geometryShaderId);
                    KWEngine.LogWriteLine("[ShaderGeometry] " + gMsg);
                }
            }
        }

        public static void DoBloomPass()
        {
            GL.Disable(EnableCap.DepthTest);
            RendererBloomDownsample.Bind();
            for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS; i++)
            {
                GL.Viewport(0, 0, KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
                FramebuffersBloom[i].Bind(true);
                RendererBloomDownsample.Draw(i == 0 ? FramebufferLightingPass : FramebuffersBloom[i - 1]);
            }

            RendererBloomUpsample.Bind();
            for (int i = KWEngine.MAX_BLOOM_BUFFERS - 1; i > 0 ; i--)
            {
                GL.Viewport(0, 0, KWEngine.BLOOMWIDTH >> (i - 1), KWEngine.BLOOMHEIGHT >> (i - 1));
                FramebuffersBloomTemp[i - 1].Bind(true);
                RendererBloomUpsample.Draw(i == KWEngine.MAX_BLOOM_BUFFERS - 1 ? FramebuffersBloom[i] : FramebuffersBloomTemp[i], FramebuffersBloom[i - 1]);
            }

            GL.Enable(EnableCap.DepthTest);
        }
    }
}
