using KWEngine3.Editor;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer.LowQuality;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
using System.Text.RegularExpressions;

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
        public static ScreenGrid _screenGrid;

        // Bloom chain: the same level structure for all render qualities, so the glow parameters look the same everywhere.
        // Level k has the size (BLOOMWIDTH x BLOOMHEIGHT) >> k. High uses all 8 levels (960x540 -> 7x4).
        // Default/Low skip the finest level and start at 480x270 (7 levels); the missing level is folded into the last upsample pass.
        private static bool _bloomFullChain = true;
        private static int _bloomLevelCount = 0;
        private const float BLOOM_DOWNSAMPLE_ENERGY = 0.875f; // sum of the weights in the 9-tap downsample shader

        public static int LoadCompileAttachShader(Stream stream, ShaderType type, int program)
        {
            int address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(stream))
            {
                GL.ShaderSource(address, ResolveShaderIncludes(sr.ReadToEnd()));
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            return address;
        }

        private const string SHADER_INCLUDE_RESOURCEPREFIX = "KWEngine3.Shaders.Include.";
        private const int SHADER_INCLUDE_MAXDEPTH = 8;
        private static readonly Regex _shaderIncludeRegex = new Regex(@"^[ \t]*#include[ \t]+""([^""]+)""[ \t]*(?://[^\r\n]*)?\r?$", RegexOptions.Multiline | RegexOptions.Compiled);

        internal static string ResolveShaderIncludes(string source, int depth = 0)
        {
            if (!source.Contains("#include"))
                return source;

            if (depth >= SHADER_INCLUDE_MAXDEPTH)
            {
                KWEngine.LogWriteLine("[Shader] #include: maximum depth reached (circular include?)");
                return source;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            return _shaderIncludeRegex.Replace(source, match =>
            {
                string includeName = match.Groups[1].Value;
                using (Stream s = assembly.GetManifestResourceStream(SHADER_INCLUDE_RESOURCEPREFIX + includeName))
                {
                    if (s == null)
                    {
                        KWEngine.LogWriteLine("[Shader] #include: resource '" + includeName + "' not found");
                        return "// #include \"" + includeName + "\" not found";
                    }
                    using (StreamReader sr = new StreamReader(s))
                    {
                        return ResolveShaderIncludes(sr.ReadToEnd(), depth + 1);
                    }
                }
            });
        }


        public static void UpdateFramebufferClearColor(Vector3 newFillColor)
        {
            FramebufferDeferred.ClearColorValues[0][0] = newFillColor.X;
            FramebufferDeferred.ClearColorValues[0][1] = newFillColor.Y;
            FramebufferDeferred.ClearColorValues[0][2] = newFillColor.Z;

            FramebufferLightingPass.ClearColorValues[0][0] = newFillColor.X;
            FramebufferLightingPass.ClearColorValues[0][1] = newFillColor.Y;
            FramebufferLightingPass.ClearColorValues[0][2] = newFillColor.Z;
        }

        public static void BindScreen(bool clear = true, bool scaled = false)
        {
            if (!scaled)
                KWEngine.Window.SetGLViewportToClientSize();
            else
                KWEngine.Window.SetGLViewportToScaledClientSize();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (clear)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        public static void InitializeFramebuffers()
        {
            Vector2i fbSize = KWEngine.Window.GetWindowFramebufferSize();
            _screenGrid = new ScreenGrid(fbSize.X, fbSize.Y);

            FramebufferQuad.Init();
            FramebufferDeferred = new FramebufferDeferred(fbSize.X, fbSize.Y);
            FramebufferLightingPass = new FramebufferLighting(fbSize.X, fbSize.Y);
            // SSAO at half resolution (bilinearly upsampled in the lighting pass)
            FramebufferSSAO = new FramebufferSSAO(Math.Max(1, fbSize.X / 2), Math.Max(1, fbSize.Y / 2), false, LightType.Point);
            FramebufferSSAOBlur = new FramebufferSSAOBlur(Math.Max(1, fbSize.X / 2), Math.Max(1, fbSize.Y / 2), false, LightType.Point);

            InitializeBloomFramebuffers();
        }

        // (re)creates the bloom chain for the current render quality (also called when the quality changes at runtime)
        internal static void InitializeBloomFramebuffers()
        {
            DisposeBloomFramebuffers();

            _bloomFullChain = KWEngine.Window._renderQuality == RenderQualityLevel.High;
            int firstLevel = _bloomFullChain ? 0 : 1;
            _bloomLevelCount = KWEngine.MAX_BLOOM_BUFFERS - firstLevel;
            for (int i = 0; i < _bloomLevelCount; i++)
            {
                int w = Math.Max(1, KWEngine.BLOOMWIDTH >> (i + firstLevel));
                int h = Math.Max(1, KWEngine.BLOOMHEIGHT >> (i + firstLevel));
                FramebuffersBloom[i] = new FramebufferBloom(w, h);
                FramebuffersBloomTemp[i] = new FramebufferBloom(w, h);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        internal static void DisposeBloomFramebuffers()
        {
            for (int i = 0; i < KWEngine.MAX_BLOOM_BUFFERS; i++)
            {
                if (FramebuffersBloom[i] != null)
                {
                    FramebuffersBloom[i].Dispose();
                    FramebuffersBloom[i] = null;
                }
                if (FramebuffersBloomTemp[i] != null)
                {
                    FramebuffersBloomTemp[i].Dispose();
                    FramebuffersBloomTemp[i] = null;
                }
            }
        }

        public static void InitializeClearColor()
        {
            GL.ClearColor(0, 0, 0, 0);
        }

        // uniform buffer binding points (always bind to these, never to a block index: block indices are assigned by the driver)
        internal const int UBO_BINDINGPOINT_INSTANCES = 0;   // uInstanceBlock (all instanced renderers)
        internal const int UBO_BINDINGPOINT_LIGHTING1 = 0;   // uBlockIndex1 (lighting pass)
        internal const int UBO_BINDINGPOINT_LIGHTING2 = 1;   // uBlockIndex2 (lighting pass)
        internal const int UBO_BINDINGPOINT_LIGHTING3 = 2;   // uBlockIndex3 (lighting pass)

        public static void UnbindUBOFromShader(int program, int bindingPoint, int ubo)
        {
            if (GL.IsBuffer(ubo))
            {
                GL.UseProgram(program);
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, 0);
                GL.UseProgram(0);
            }
        }

        public static void UnbindUBOFromAllInstanceShaders(int ubo)
        {
            RendererGBufferInstanced.Bind();
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBO_BINDINGPOINT_INSTANCES, 0);

            // general shaders:
            IRendererForwardInstanced.UnbindUBO(ubo);

            // shadow map stuff:
            IRendererShadowMapInstanced.UnbindUBO(ubo);
            IRendererShadowMapCubeInstanced.UnbindUBO(ubo);
            IRendererShadowMapInstancedCSM.UnbindUBO(ubo);
        }

        public static void InitializeShaders()
        {
            PrimitiveQuad.Init();
            PrimitivePoint.Init();
            RendererGBuffer.Init();
            RendererGBufferInstanced.Init(); // ok
            RendererTerrainGBufferNew.Init();
            RendererGBufferFoliage.Init();
            RendererForwardSimple.Init();


            RendererCopy.Init();
            RendererFog.Init();
            RendererBackgroundSkybox.Init();
            RendererBackgroundStandard.Init();
            RendererExplosion.Init();

            RendererEditorHitboxes.Init();
            RendererTerrainCollision.Init();
            RendererSSAO.Init();
            RendererSSAOBlur.Init();
            RendererDebug.Init();
            RendererDebugCube.Init();

            RendererFlowField.Init();
            RendererFlowFieldDirection.Init();


            RendererEditor.Init();
            RendererGrid.Init();
            RendererLightOverlay.Init();
            RendererLightFrustum.Init();
            RendererOctreeNodes.Init();
            RendererBloomDownsample.Init();
            RendererBloomUpsample.Init();
            RendererHUD.Init();
            RendererHUDText.Init();
            RendererLoadingScreenProgress.Init();

            RendererFrustum.Init();

            // Renderers that are dependent on RenderQuality enum:
            SelectQualityDependentRenderers();
        }

        // One renderer set for Low and one for Default/High. A set is created on first use and then kept,
        // so switching the render quality back and forth at runtime does not recompile any shaders.
        private sealed class QualityRendererSet
        {
            public IRenderer LightingPass;
            public IRenderer LightingPassMultiDraw;
            public IRenderer Forward;
            public IRenderer ForwardInstanced;
            public IRenderer ForwardText;
            public IRenderer ShadowMap;
            public IRenderer ShadowMapCSM;
            public IRenderer ShadowMapInstanced;
            public IRenderer ShadowMapInstancedCSM;
            public IRenderer ShadowMapCube;
            public IRenderer ShadowMapCubeInstanced;
            public IRenderer ShadowMapTerrain;
            public IRenderer ShadowMapTerrainCSM;
            public IRenderer ShadowMapTerrainCube;
        }
        private static QualityRendererSet _renderersLowQuality = null;
        private static QualityRendererSet _renderersDefaultQuality = null;

        private static QualityRendererSet CreateQualityRendererSet(bool lowQuality)
        {
            QualityRendererSet s = new QualityRendererSet();
            if (lowQuality)
            {
                s.LightingPass = new RendererLightingPassLQ(); s.LightingPass.Init();
                s.LightingPassMultiDraw = new RendererLightingPassMultiDrawLQ(); s.LightingPassMultiDraw.Init();
                s.Forward = new RendererForwardLQ(); s.Forward.Init();
                s.ForwardInstanced = new RendererForwardInstancedLQ(); s.ForwardInstanced.Init();
                s.ForwardText = new RendererForwardTextLQ(); s.ForwardText.Init();
                s.ShadowMap = new RendererShadowMapLQ(); s.ShadowMap.Init();
                s.ShadowMapCSM = new RendererShadowMapCSMLQ(); s.ShadowMapCSM.Init();
                s.ShadowMapInstanced = new RendererShadowMapInstancedLQ(); s.ShadowMapInstanced.Init();
                s.ShadowMapInstancedCSM = new RendererShadowMapInstancedCSMLQ(); s.ShadowMapInstancedCSM.Init();
                s.ShadowMapCube = new RendererShadowMapCubeLQ(); s.ShadowMapCube.Init();
                s.ShadowMapCubeInstanced = new RendererShadowMapCubeInstancedLQ(); s.ShadowMapCubeInstanced.Init();
                s.ShadowMapTerrain = new RendererShadowMapTerrainLQ(); s.ShadowMapTerrain.Init();
                s.ShadowMapTerrainCSM = new RendererShadowMapTerrainCSMLQ(); s.ShadowMapTerrainCSM.Init();
                s.ShadowMapTerrainCube = new RendererShadowMapTerrainCubeLQ(); s.ShadowMapTerrainCube.Init();
            }
            else
            {
                s.LightingPass = new RendererLightingPass(); s.LightingPass.Init();
                s.LightingPassMultiDraw = new RendererLightingPassMultiDraw(); s.LightingPassMultiDraw.Init();
                s.Forward = new RendererForward(); s.Forward.Init();
                s.ForwardInstanced = new RendererForwardInstanced(); s.ForwardInstanced.Init();
                s.ForwardText = new RendererForwardText(); s.ForwardText.Init();
                s.ShadowMap = new RendererShadowMap(); s.ShadowMap.Init();
                s.ShadowMapCSM = new RendererShadowMapCSM(); s.ShadowMapCSM.Init();
                s.ShadowMapInstanced = new RendererShadowMapInstanced(); s.ShadowMapInstanced.Init();
                s.ShadowMapInstancedCSM = new RendererShadowMapInstancedCSM(); s.ShadowMapInstancedCSM.Init();
                s.ShadowMapCube = new RendererShadowMapCube(); s.ShadowMapCube.Init();
                s.ShadowMapCubeInstanced = new RendererShadowMapCubeInstanced(); s.ShadowMapCubeInstanced.Init();
                s.ShadowMapTerrain = new RendererShadowMapTerrain(); s.ShadowMapTerrain.Init();
                s.ShadowMapTerrainCSM = new RendererShadowMapTerrainCSM(); s.ShadowMapTerrainCSM.Init();
                s.ShadowMapTerrainCube = new RendererShadowMapTerrainCube(); s.ShadowMapTerrainCube.Init();
            }
            GL.UseProgram(0);
            return s;
        }

        // activates the renderer set that matches the current render quality (also called when the quality changes at runtime)
        internal static void SelectQualityDependentRenderers()
        {
            QualityRendererSet s;
            if (KWEngine.Window._renderQuality == RenderQualityLevel.Low)
            {
                if (_renderersLowQuality == null)
                    _renderersLowQuality = CreateQualityRendererSet(true);
                s = _renderersLowQuality;
            }
            else
            {
                if (_renderersDefaultQuality == null)
                    _renderersDefaultQuality = CreateQualityRendererSet(false);
                s = _renderersDefaultQuality;
            }

            IRendererLightingPass = s.LightingPass;
            IRendererLightingPassMultiDraw = s.LightingPassMultiDraw;
            IRendererForward = s.Forward;
            IRendererForwardInstanced = s.ForwardInstanced;
            IRendererForwardText = s.ForwardText;
            IRendererShadowMap = s.ShadowMap;
            IRendererShadowMapCSM = s.ShadowMapCSM;
            IRendererShadowMapInstanced = s.ShadowMapInstanced;
            IRendererShadowMapInstancedCSM = s.ShadowMapInstancedCSM;
            IRendererShadowMapCube = s.ShadowMapCube;
            IRendererShadowMapCubeInstanced = s.ShadowMapCubeInstanced;
            IRendererShadowMapTerrain = s.ShadowMapTerrain;
            IRendererShadowMapTerrainCSM = s.ShadowMapTerrainCSM;
            IRendererShadowMapTerrainCube = s.ShadowMapTerrainCube;
        }

        public static IRenderer IRendererLightingPass;
        public static IRenderer IRendererLightingPassMultiDraw;
        public static IRenderer IRendererForward;
        public static IRenderer IRendererForwardInstanced;
        public static IRenderer IRendererForwardText;
        public static IRenderer IRendererShadowMap;
        public static IRenderer IRendererShadowMapCSM;
        public static IRenderer IRendererShadowMapInstanced;
        public static IRenderer IRendererShadowMapInstancedCSM;
        public static IRenderer IRendererShadowMapCube;
        public static IRenderer IRendererShadowMapCubeInstanced;
        public static IRenderer IRendererShadowMapTerrain;
        public static IRenderer IRendererShadowMapTerrainCSM;
        public static IRenderer IRendererShadowMapTerrainCube;

        public static void CheckShaderStatus(int programId, int vertexShaderId, int fragmentShaderId, int geometryShaderId = -1, int tessControlShaderId = -1, int tessEvalShaderId = -1)
        {
            GL.GetProgram(programId, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus != 1)
            {
                GL.GetProgram(programId, GetProgramParameterName.InfoLogLength, out int logLength);
                if (logLength > 0)
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
                if (vertexStatus == 0)
                {
                    vMsg = GL.GetShaderInfoLog(vertexShaderId);
                    KWEngine.LogWriteLine("[ShaderVertex] " + vMsg);
                }
                if (fragmentStatus == 0)
                {
                    fMsg = GL.GetShaderInfoLog(fragmentShaderId);
                    KWEngine.LogWriteLine("[ShaderFragment] " + fMsg);
                }
            }

            if (geometryShaderId > 0)
            {
                string gMsg = "";
                GL.GetShader(geometryShaderId, ShaderParameter.CompileStatus, out int geometryStatus);
                if (geometryStatus == 0)
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

        // clearTargets: only needed while blending is enabled (loading screen), because the downsample shader writes alpha < 1
        public static void DoBloomPass(bool clearTargets = false)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(FramebufferQuad.GetVAOId());

            // Every pass overwrites its whole target, so the targets do not need to be cleared.
            // DOWNSAMPLE STEPS:
            RendererBloomDownsample.Bind();
            RendererBloomDownsample.SetGlobals();
            for (int i = 0; i < _bloomLevelCount; i++)
            {
                FramebufferBloom target = FramebuffersBloom[i];
                GL.Viewport(0, 0, target.Width, target.Height);
                target.Bind(clearTargets);
                if (i == 0 && !_bloomFullChain)
                {
                    // half chain: 480x270 directly from the lighting pass, energy like two regular downsample steps
                    RendererBloomDownsample.DrawBox(FramebufferLightingPass, target, BLOOM_DOWNSAMPLE_ENERGY * BLOOM_DOWNSAMPLE_ENERGY);
                }
                else
                {
                    RendererBloomDownsample.Draw(i == 0 ? FramebufferLightingPass : FramebuffersBloom[i - 1]);
                }
            }

            // UPSAMPLE STEPS:
            // U(k) = A * tent(U(k + 1)) + B * tent(D(k)), with tap distance s = GlowRadius,
            // A = 2 * GlowStyleFactor1 + s and B = 2 * GlowStyleFactor2 + 1 - s
            float s = KWEngine._glowRadius;
            float a = 2f * KWEngine._glowUpsampleF1 + s;
            float b = 2f * KWEngine._glowUpsampleF2 + 1f - s;
            RendererBloomUpsample.Bind();
            RendererBloomUpsample.SetGlobals();
            for (int i = _bloomLevelCount - 1; i > 0; i--)
            {
                FramebufferBloom target = FramebuffersBloomTemp[i - 1];
                GL.Viewport(0, 0, target.Width, target.Height);
                target.Bind(clearTargets);

                float weightSmaller = a;
                float weightBigger = b;
                if (i == 1 && !_bloomFullChain)
                {
                    // half chain: fold the missing 960x540 level into the last pass
                    // U(0) = A * U(1) + B * D(0) with D(0) ~ D(1) / 0.875 and U(1) = A * tent(U(2)) + B * tent(D(1))
                    weightSmaller = a * a;
                    weightBigger = a * b + b / BLOOM_DOWNSAMPLE_ENERGY;
                }
                RendererBloomUpsample.Draw(i == _bloomLevelCount - 1 ? FramebuffersBloom[i] : FramebuffersBloomTemp[i], FramebuffersBloom[i - 1], s, weightSmaller, weightBigger);
            }

            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
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
