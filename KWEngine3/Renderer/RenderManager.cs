using KWEngine3.Editor;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.Renderer
{
    internal static class RenderManager
    {
        public static FramebufferDeferred FramebufferDeferred { get; set; }
        public static FramebufferLighting FramebufferLightingPass { get; set; }
        public static FramebufferSSAO FramebufferSSAO { get; set; }
        public static FramebufferSSAOBlur FramebufferSSAOBlur { get; set; }
        public static FramebufferBloom[] FramebuffersBloom { get; set; } = new FramebufferBloom[KWEngine.MAX_BLOOM_BUFFERS];
        public static FramebufferBloom[] FramebuffersBloomTemp { get; set; } = new FramebufferBloom[KWEngine.MAX_BLOOM_BUFFERS];
        public const int LQPENALTY_W = 192;
        public const int LQPENALTY_H = 112;
        public static ScreenGrid _screenGrid;

        public static void UpdateFramebufferClearColor(Vector3 newFillColor)
        {
            FramebufferDeferred.ClearColorValues[0][0] = newFillColor.X;
            FramebufferDeferred.ClearColorValues[0][1] = newFillColor.Y;
            FramebufferDeferred.ClearColorValues[0][2] = newFillColor.Z;

            FramebufferLightingPass.ClearColorValues[0][0] = newFillColor.X;
            FramebufferLightingPass.ClearColorValues[0][1] = newFillColor.Y;
            FramebufferLightingPass.ClearColorValues[0][2] = newFillColor.Z;
        }

        public static void BindScreen(bool clear = true)
        {
            GL.Viewport(0, 0, KWEngine.Window.ClientSize.X, KWEngine.Window.ClientSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (clear)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }
        
        public static void InitializeFramebuffers()
        {
            _screenGrid = new ScreenGrid(KWEngine.Window.Width, KWEngine.Window.Height);

            FramebufferQuad.Init();
            FramebufferDeferred = new FramebufferDeferred(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            FramebufferLightingPass = new FramebufferLighting(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            FramebufferSSAO = new FramebufferSSAO(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y, false, LightType.Point);
            FramebufferSSAOBlur = new FramebufferSSAOBlur(KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y, false, LightType.Point);

            // Bloom
            if ((int)KWEngine.Window._ppQuality > 1) // high only
            {
                for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS; i++)
                {
                    FramebuffersBloom[i] = new FramebufferBloom(KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
                    FramebuffersBloomTemp[i] = new FramebufferBloom(KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
                }
            }
            else
            {
                for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS / 2; i++)
                {
                    Vector2i bloomSize = GetBloomSize(i);
                    FramebuffersBloom[i] = new FramebufferBloom(bloomSize.X, bloomSize.Y);
                    FramebuffersBloomTemp[i] = new FramebufferBloom(bloomSize.X, bloomSize.Y);
                }
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
            RendererGBufferInstanced.Init();
            RendererLightingPass.Init();
            RendererForward.Init();
            RendererForwardInstanced.Init();
            RendererCopy.Init();
            RendererBackgroundSkybox.Init();
            RendererBackgroundStandard.Init();
            RendererExplosion.Init();
            RendererParticle.Init();
            RendererForwardText.Init();
            RendererEditorHitboxes.Init();
            RendererTerrainCollision.Init();
            RendererSSAO.Init();
            RendererSSAOBlur.Init();

            RendererDebug.Init();
            RendererDebugCube.Init();

            RendererFlowField.Init();
            RendererFlowFieldDirection.Init();

            RendererGBufferFoliage.Init();

            RendererShadowMap.Init();
            RendererShadowMapInstanced.Init();
            RendererShadowMapCube.Init();
            RendererShadowMapCubeInstanced.Init();
            RendererShadowMapTerrain.Init();
            RendererShadowMapTerrainCube.Init();

            RendererEditor.Init();
            RendererGrid.Init();
            RendererLightOverlay.Init();
            RendererLightFrustum.Init();
            RendererOctreeNodes.Init();
            
            RendererBloomDownsample.Init();
            RendererBloomUpsample.Init();

            RendererHUD.Init();
            RendererHUDText.Init();

            RendererTerrainGBufferNew.Init();
        }

        public static void CheckShaderStatus(int programId, int vertexShaderId, int fragmentShaderId, int geometryShaderId = -1, int tessControlShaderId = -1, int tessEvalShaderId = -1)
        {
            GL.GetProgram(programId, GetProgramParameterName.LinkStatus, out int linkStatus);
            if(linkStatus != 1)
            {
                GL.GetProgram(programId, GetProgramParameterName.InfoLogLength, out int logLength);
                if(logLength > 0)
                {
                    string msg = GL.GetProgramInfoLog(programId);
                    KWEngine.LogWriteLine("[ProgramLog] " + msg);
                }
            }


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

            if (tessControlShaderId > 0)
            {
                string gMsg = "";
                GL.GetShader(tessControlShaderId, ShaderParameter.CompileStatus, out int tcStatus);
                if (tcStatus == 0)
                {
                    gMsg = GL.GetShaderInfoLog(tessControlShaderId);
                    KWEngine.LogWriteLine("[ShaderTessC] " + gMsg);
                }
            }

            if (tessEvalShaderId > 0)
            {
                string gMsg = "";
                GL.GetShader(tessControlShaderId, ShaderParameter.CompileStatus, out int teStatus);
                if (teStatus == 0)
                {
                    gMsg = GL.GetShaderInfoLog(tessEvalShaderId);
                    KWEngine.LogWriteLine("[ShaderTessE] " + gMsg);
                }
            }
        }

        public static void DoBloomPass()
        {
            GL.Disable(EnableCap.DepthTest);
            RendererBloomDownsample.Bind();

            if((int)KWEngine.Window._ppQuality > 1) // high only
            {
                for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS; i++)
                {
                    GL.Viewport(0, 0, KWEngine.BLOOMWIDTH >> i, KWEngine.BLOOMHEIGHT >> i);
                    FramebuffersBloom[i].Bind(true);
                    RendererBloomDownsample.Draw(i == 0 ? FramebufferLightingPass : FramebuffersBloom[i - 1]);
                }

                RendererBloomUpsample.Bind();
                for (int i = KWEngine.MAX_BLOOM_BUFFERS - 1; i > 0; i--)
                {
                    GL.Viewport(0, 0, KWEngine.BLOOMWIDTH >> (i - 1), KWEngine.BLOOMHEIGHT >> (i - 1));
                    FramebuffersBloomTemp[i - 1].Bind(true);
                    RendererBloomUpsample.Draw(i == KWEngine.MAX_BLOOM_BUFFERS - 1 ? FramebuffersBloom[i] : FramebuffersBloomTemp[i], FramebuffersBloom[i - 1]);
                }
            }
            else
            {
                // DOWNSAMPLE STEPS:
                for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS / 2; i++)
                {
                    Vector2i bloomSize = GetBloomSize(i, true);
                    GL.Viewport(0, 0, bloomSize.X, bloomSize.Y);
                    FramebuffersBloom[i].Bind(true);
                    RendererBloomDownsample.Draw(i == 0 ? FramebufferLightingPass : FramebuffersBloom[i - 1]);
                }

                RendererBloomUpsample.Bind();
                for (int i = KWEngine.MAX_BLOOM_BUFFERS / 2 - 1; i > 0; i--)
                {
                    Vector2i bloomSize = GetBloomSize(i, false);
                    GL.Viewport(0, 0, bloomSize.X, bloomSize.Y);
                    FramebuffersBloomTemp[i - 1].Bind(true);
                    if(i == KWEngine.MAX_BLOOM_BUFFERS / 2 - 1)
                    {
                        RendererBloomUpsample.Draw(FramebuffersBloom[i], FramebuffersBloom[i - 1]);
                    }
                    else
                    {
                        RendererBloomUpsample.Draw(FramebuffersBloomTemp[i], FramebuffersBloom[i - 1]);
                    }

                    
                }
            }
        }

        public static Vector2i GetBloomSize(int i = 0, bool downsample = true)
        {
            if (downsample)
            {
                Vector2i result = new Vector2i(
                     (KWEngine.BLOOMWIDTH - LQPENALTY_W * (i + 1)) >> i,
                     (KWEngine.BLOOMHEIGHT - LQPENALTY_H * (i + 1)) >> i
                    );
                //Console.WriteLine(result + " (down)");
                return result;
            }
            else
            {
                Vector2i result = new Vector2i(
                     (KWEngine.BLOOMWIDTH - LQPENALTY_W * (i - 0)) >> (i - 1),
                     (KWEngine.BLOOMHEIGHT - LQPENALTY_H * (i - 0)) >> (i - 1)
                    );
                //Console.WriteLine(result + " (up)");
                return result;
            }
        }

        public static bool IsCurrentDebugMapACubeMap()
        {
            if ((int)KWEngine.DebugMode < 7 || (int)KWEngine.DebugMode > 9)
                return false;

            List<FramebufferShadowMap> maps = new();
            bool isCubeMap = false;
            foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
            {
                if (l._fbShadowMap != null)
                {
                    maps.Add(l._fbShadowMap);
                }
            }

            if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap1)
            {
                if (maps.Count >= 1)
                {
                    if (maps[0]._lightType == LightType.Point)
                    {
                        isCubeMap = true;
                    }
                }
            }
            else if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap2)
            {
                if (maps.Count >= 2)
                {
                    if (maps[1]._lightType == LightType.Point)
                    {
                        isCubeMap = true;
                    }
                }
            }
            else if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap2)
            {
                if (maps.Count >= 3)
                {
                    if (maps[2]._lightType == LightType.Point)
                    {
                        isCubeMap = true;
                    }
                }
            }
            return isCubeMap;
        }
    }
}
