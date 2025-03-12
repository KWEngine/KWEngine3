using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using KWEngine3.Audio;
using KWEngine3.Editor;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Basisklasse für das Programmfenster
    /// </summary>
    public abstract class GLWindow : GameWindow
    {
        // input related:
        internal KeyboardExt _keyboard = new();
        internal MouseExt _mouse = new();
        internal const int DELTASFORMOVINGAVG = 4;
        internal const int MOUSEDELTAMAXSAMPLECOUNT = 128;
        internal Vector2 _mouseDeltaToUse = Vector2.Zero;
        internal int _mouseScrollPosition = 0;
        internal int _mouseScrollDelta = 0;
        internal List<Vector2> _mouseDeltas = new(MOUSEDELTAMAXSAMPLECOUNT);

        internal void ResetMouseDeltas()
        {
            _mouseDeltas = new(MOUSEDELTAMAXSAMPLECOUNT);
            _mouseDeltaToUse = Vector2.Zero;
        }

        // quality related:
        internal PostProcessingQuality _ppQuality = PostProcessingQuality.Standard;
        internal int AnisotropicFilteringLevel { get; set; } = 4;

        // other:
        internal Matrix4 _viewProjectionMatrixHUD;
        internal Matrix4 _viewMatrixHUD;
        internal Matrix4 _projectionMatrixHUD;
        internal KWBuilderOverlay Overlay { get; set; }
        internal Stopwatch _stopwatch = new();

        internal Vector2 GatherWeightedMovingAvg(Vector2 mouseDelta, float dt_ms)
        {
            if (KWEngine.CurrentWorld._mouseCursorJustGrabbed)
                return Vector2.Zero;

            _mouseDeltas.Add(mouseDelta * (1f / dt_ms));
            while (_mouseDeltas.Count > MOUSEDELTAMAXSAMPLECOUNT)
            {
                _mouseDeltas.RemoveAt(0);
            }
            int samplesToLookAt = Math.Clamp((int)Math.Ceiling((1f / dt_ms) * 10f), 1, MOUSEDELTAMAXSAMPLECOUNT - 1);
            float sampleWeightStep = 1.0f / Math.Max(samplesToLookAt - 1, 1);
            int thresholdForNextWeightStep = Math.Max(samplesToLookAt / DELTASFORMOVINGAVG, DELTASFORMOVINGAVG);
            float currentSampleWeight = sampleWeightStep;
            Vector2 movingAvg = Vector2.Zero;

            for (int i = Math.Max(0, _mouseDeltas.Count - 1 - samplesToLookAt), j = 1; i < _mouseDeltas.Count; i++, j++)
            {
                movingAvg += _mouseDeltas[i] * currentSampleWeight;
                if (j == thresholdForNextWeightStep)
                {
                    currentSampleWeight += sampleWeightStep;
                    j = 1;
                }
            }
            return movingAvg;
        }

        internal GLWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            Overlay = new KWBuilderOverlay(ClientSize.X, ClientSize.Y);
            CenterWindow();
            KWEngine.Window = this;
            KWEngine.InitializeModels();
            KWEngine.InitializeParticles();
            GLAudioEngine.InitAudioEngine();
        }

        /// <summary>
        /// Standardkonstruktor für den exklusiven Fullscreen-Modus
        /// </summary>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        /// <param name="icon">Fenstersymbol</param>
        public GLWindow(
            bool vSync = true, 
            PostProcessingQuality ppQuality = PostProcessingQuality.Standard,
            WindowIcon icon = null)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Fullscreen,
                     WindowBorder = WindowBorder.Hidden, 
                     ClientSize = new Vector2i(KWEngine.ScreenInformation.PrimaryScreenWidth, KWEngine.ScreenInformation.PrimaryScreenHeight),
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     Icon = icon,
                     CurrentMonitor = Monitors.GetPrimaryMonitor().Handle,
                     StartVisible = false
                 }
                 )
        {
            _ppQuality = ppQuality;
            KWEngine.InitializeFontsAndDefaultTextures();
        }

        /// <summary>
        /// Standardkonstruktor für den Fenstermodus
        /// </summary>
        /// <param name="width">Breite des Fensterinhalts in Pixeln</param>
        /// <param name="height">Höhe des Fenterinhalts in Pixeln</param>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="icon">Fenstersymbol</param>
        public GLWindow(int width, int height, bool vSync = true, WindowIcon icon = null)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     ClientSize = new Vector2i(width, height),
                     WindowBorder = WindowBorder.Fixed,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     Icon = icon,
                     Location = new Vector2i(KWEngine.ScreenInformation.PrimaryScreen.Width / 2 - width / 2, KWEngine.ScreenInformation.PrimaryScreen.Height / 2 - height / 2),
                     StartVisible = false
                 }
        )
        {
            _ppQuality = PostProcessingQuality.Standard;
            KWEngine.InitializeFontsAndDefaultTextures();
            CenterWindow();
        }

        /// <summary>
        /// Standardkonstruktor für den Fenstermodus
        /// </summary>
        /// <param name="width">Breite des Fensterinhalts in Pixeln</param>
        /// <param name="height">Höhe des Fenterinhalts in Pixeln</param>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        /// <param name="windowMode">Art des Fensters (Standard oder rahmenlos)</param>
        /// <param name="icon">Fenstersymbol</param>
        public GLWindow(int width, int height, bool vSync, PostProcessingQuality ppQuality, WindowMode windowMode, WindowIcon icon = null)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     ClientSize = new Vector2i(width, height),
                     WindowBorder = windowMode == WindowMode.Default ? WindowBorder.Fixed : WindowBorder.Hidden,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     Icon = icon,
                     Location = new Vector2i(KWEngine.ScreenInformation.PrimaryScreen.Width / 2 - width / 2, KWEngine.ScreenInformation.PrimaryScreen.Height / 2 - height / 2),
                     StartVisible = false
                 }
        )
        {
            _ppQuality = ppQuality;
            KWEngine.InitializeFontsAndDefaultTextures();
            CenterWindow();
        }

        /// <summary>
        /// Standardkonstruktor für den Fenstermodus
        /// </summary>
        /// <param name="width">Breite des Fensterinhalts in Pixeln</param>
        /// <param name="height">Höhe des Fenterinhalts in Pixeln</param>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        /// <param name="windowMode">Art des Fensters (Standard oder rahmenlos)</param>
        /// <param name="monitorHandle">Handle des Monitors, auf dem das Fenster geöffnet werden soll</param>
        /// <param name="icon">Fenstersymbol</param>
        public GLWindow(int width, int height, bool vSync, PostProcessingQuality ppQuality, WindowMode windowMode, IntPtr monitorHandle, WindowIcon icon = null)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     ClientSize = new Vector2i(width, height),
                     WindowBorder = windowMode == WindowMode.Default ? WindowBorder.Fixed : WindowBorder.Hidden,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     CurrentMonitor = GetMonitorHandleForPointer(monitorHandle),
                     Icon = icon,
                     Location = new Vector2i(KWEngine.ScreenInformation.PrimaryScreen.Width / 2 - width / 2, KWEngine.ScreenInformation.PrimaryScreen.Height / 2 - height / 2),
                     StartVisible = false
                 }
        )
        {
            _ppQuality = ppQuality;
            KWEngine.InitializeFontsAndDefaultTextures();
            CenterWindow();
        }

        /// <summary>
        /// Gibt die aktuelle Anzahl der Frames-Per-Second (FPS) an
        /// </summary>
        public int FPS { get { return KWEngine.FPS; } }

        /// <summary>
        /// Prüft, ob der Mauszeiger aktuell innerhalb des Fensters ist
        /// </summary>
        public bool IsMouseInWindow { get { return IsFocused && MouseState.X >= 0 && MouseState.X < ClientSize.X && MouseState.Y >= 0 && MouseState.Y < ClientSize.Y; } }

        /// <summary>
        /// Verweis auf Keyboardeingaben
        /// </summary>
        public KeyboardExt Keyboard { get { return _keyboard; } }
        /// <summary>
        /// Verweis auf Mauseingaben
        /// </summary>
        public MouseExt Mouse { get { return _mouse; } }

        /// <summary>
        /// Standard-Initialisierungen
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductMajorPart + "." + fileVersionInfo.ProductMinorPart + "." + fileVersionInfo.ProductBuildPart + "." + fileVersionInfo.ProductPrivatePart;
            string date = fileVersionInfo.ProductName.Substring(fileVersionInfo.ProductName.IndexOf('2'), 10);

            if (Title == null || Title == "")
                Title = "KWEngine " + version + " (" + date + ") | OpenGL Version: " + GL.GetString(StringName.Version);

            // Unter MacOS ist die OpenGL-Bibliothek so implementiert, dass Uniform Arrays mit Mehrkomponenten-Inhalten
            // im Shader als einzelne Komponenten angesprochen werden:
            // Windows: Vector4(0, 1, 2, 3) wird im Uniform Array als ein vec4-Eintrag [0] gesehen.
            // MacOS:   Vector4(0, 1, 2, 3) wird im Uniform Array als vier separate floats gesehen [0][1][2][3].
            KWEngine._uniformOffsetMultiplier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 4 : 1;

            unsafe
            {
                GLFW.GetCursorPos(this.WindowPtr, out var xPos, out var yPos);
                Mouse._mousePositionFromGLFW = new Vector2((float)xPos, (float)yPos);
            }
            
            RenderManager.InitializeFramebuffers();
            RenderManager.InitializeShaders();
            RenderManager.InitializeClearColor();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4); // terrain tesselation

            //GL.LineWidth(2f); // not available on core profile

            KWBuilderOverlay.InitFrameTimeQueue();

            IsVisible = true;

            
        }

        /// <summary>
        /// Wird ausgeführt, wenn sich das Fenster vergrößert/verkleinert
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            Overlay.WindowResized(ClientSize.X, ClientSize.Y);
            _viewMatrixHUD = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);
            _projectionMatrixHUD = Matrix4.CreateOrthographicOffCenter(0f, ClientSize.X, ClientSize.Y, 0f, 0.01f, 10f);
            _viewProjectionMatrixHUD = _viewMatrixHUD * _projectionMatrixHUD;
        }

        /// <summary>
        /// Enthält die aktuelle Projektionsmatrix für HUD-Objekte
        /// </summary>
        public Matrix4 ProjectionMatrixHUD { get { return _projectionMatrixHUD; } }

        /// <summary>
        /// Enthält die aktuelle Viewmatrix für HUD-Objekte
        /// </summary>
        public Matrix4 ViewMatrixHUD { get { return _viewMatrixHUD; } }

        /// <summary>
        /// Enthält die aktuelle View-Projektionsmatrix für HUD-Objekte
        /// </summary>
        public Matrix4 ViewProjectionMatrixHUD { get { return _viewProjectionMatrixHUD; } }

        /// <summary>
        /// Enthält die aktuelle View-Projektionsmatrix für die In-Game-Kamera
        /// </summary>
        public Matrix4 ViewProjectionMatrix { get { return KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix; } }

        internal int lastCycleCount = 0;
        /// <summary>
        /// Wird ausgeführt, wenn ein neues Bild gezeichnet werden soll
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (_worldNew != null)
            {
                SetWorldInternal(_worldNew);
                return;
            }

            UpdateDeltaTime(e.Time);
            _mouseDeltaToUse = GatherWeightedMovingAvg(MouseState.Delta, (float)(e.Time * 1000.0));
           
            if (KWEngine.CurrentWorld._mouseCursorJustGrabbed)
                KWEngine.CurrentWorld._mouseCursorJustGrabbed = false;

            
            float alpha = UpdateScene(out lastCycleCount);
            List<LightObject> pointLights = new();
            List<GameObject> gameObjectsForForwardRendering = new();
            List<RenderObject> renderObjectsForForwardRendering = new();

            KWEngine.LastFrameTime = (float)e.Time * 1000;

            // Start render process:
            if (KWEngine.CurrentWorld != null && !KWEngine.CurrentWorld._startingFrameActive)
            {
                HelperDebug.StartTimeQuery(RenderType.Deferred);
                #region [DEFERRED PASS]
                GL.Disable(EnableCap.Blend);
                RenderManager.FramebufferDeferred.Bind();

                // Render GameObject instances to G-Buffer:
                RendererGBuffer.Bind();
                gameObjectsForForwardRendering.AddRange(RendererGBuffer.RenderScene());
                // Render RenderObject instances to G-Buffer:
                if (KWEngine.CurrentWorld._renderObjects.Count > 0)
                {
                    RendererGBufferInstanced.Bind();
                    renderObjectsForForwardRendering.AddRange(RendererGBufferInstanced.RenderScene());
                }

                if (KWEngine.CurrentWorld._foliageObjects.Count > 0)
                {
                    RendererGBufferFoliage.Bind();
                    RendererGBufferFoliage.RenderScene();
                }

                // Render terrain objects to G-Buffer:
                if (KWEngine.CurrentWorld._terrainObjects.Count > 0)
                {
                    RendererTerrainGBufferNew.Bind();
                    RendererTerrainGBufferNew.RenderScene();
                }

                if (KWEngine.CurrentWorld._particleAndExplosionObjects.Count > 0)
                {
                    RendererExplosion.Bind();
                    RendererExplosion.SetGlobals();
                    RendererExplosion.RenderExplosions(KWEngine.CurrentWorld._particleAndExplosionObjects);
                }
                #endregion
                HelperDebug.StopTimeQuery(RenderType.Deferred);
                HelperGeneral.CheckGLErrors();

                if (KWEngine.Mode == EngineMode.Edit)
                {
                    GL.Disable(EnableCap.DepthTest);
                    RendererLightOverlay.Bind();
                    RendererLightOverlay.SetGlobals();
                    RendererLightOverlay.Draw(KWEngine.CurrentWorld._lightObjects);
                    GL.Enable(EnableCap.DepthTest);
                }
                HelperGeneral.CheckGLErrors();

                // Prepare lights
                HelperDebug.StartTimeQuery(RenderType.ShadowMapping);
                KWEngine.CurrentWorld.PrepareLightObjectsForRenderPass();

                // Shadow map pass:
                if (Framebuffer._fbShadowMapCounter > 0)
                {
                    RendererShadowMap.Bind();
                    foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                    {
                        if (l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
                        {
                            l._fbShadowMap.Bind(true);
                            if (l.Type == LightType.Point)
                            {
                                pointLights.Add(l);
                                continue;
                            }

                            RendererShadowMap.RenderSceneForLight(l);
                        }
                    }

                    if (KWEngine.CurrentWorld._terrainObjects.Count > 0)
                    {
                        RendererShadowMapTerrain.Bind();
                        foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                        {
                            if (l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
                            {
                                if (l.Type == LightType.Point)
                                {
                                    continue;
                                }
                                l._fbShadowMap.Bind(false);
                                RendererShadowMapTerrain.RenderSceneForLight(l);
                            }
                        }
                    }

                    if (KWEngine.CurrentWorld._renderObjects.Count > 0)
                    {
                        RendererShadowMapInstanced.Bind();
                        foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                        {
                            if (l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
                            {
                                if (l.Type == LightType.Point)
                                {
                                    continue;
                                }
                                l._fbShadowMap.Bind(false);
                                RendererShadowMapInstanced.RenderSceneForLight(l);
                            }
                        }
                    }

                    if (pointLights.Count > 0)
                    {
                        foreach(LightObject l in pointLights)
                        {
                            l._fbShadowMap.Bind(true);
                        }

                        RendererShadowMapCube.Bind();
                        foreach (LightObject l in pointLights)
                        {
                            l._fbShadowMap.Bind(false);
                            RendererShadowMapCube.RenderSceneForLight(l); // Renders GameObject and VSGO instances only
                        }

                        if (KWEngine.CurrentWorld._renderObjects.Count > 0)
                        {
                            RendererShadowMapCubeInstanced.Bind(); // Renders RenderObject instances only
                            foreach (LightObject l in pointLights)
                            {
                                l._fbShadowMap.Bind(false);
                                RendererShadowMapCubeInstanced.RenderSceneForLight(l);
                            }
                        }
                    }
                }
                HelperDebug.StopTimeQuery(RenderType.ShadowMapping);

                // clear inbetween:
                GL.UseProgram(0);
                GL.Disable(EnableCap.DepthTest);
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

                // SSAO pass
                HelperDebug.StartTimeQuery(RenderType.SSAO);
                if (KWEngine.SSAO_Enabled)
                {
                    RenderManager.FramebufferSSAO.Bind(true);
                    RendererSSAO.Bind();
                    RendererSSAO.Draw(RenderManager.FramebufferDeferred);

                    RenderManager.FramebufferSSAOBlur.Bind(true);
                    RendererSSAOBlur.Bind();
                    RendererSSAOBlur.Draw(RenderManager.FramebufferSSAO);
                }
                HelperDebug.StopTimeQuery(RenderType.SSAO);

                // Lighting pass:
                HelperGeneral.CheckGLErrors();
                HelperDebug.StartTimeQuery(RenderType.Lighting);
                RenderManager.FramebufferLightingPass.BindAndClearColor();
                RenderManager.FramebufferLightingPass.CopyDepthFrom(RenderManager.FramebufferDeferred);
                RenderManager.FramebufferLightingPass.Bind(false);
                HelperGeneral.CheckGLErrors();
                RendererLightingPass.Bind();
                RendererLightingPass.SetGlobals();
                HelperGeneral.CheckGLErrors();
                RendererLightingPass.Draw(RenderManager.FramebufferDeferred);
                HelperDebug.StopTimeQuery(RenderType.Lighting);
                HelperGeneral.CheckGLErrors();

                GL.Enable(EnableCap.DepthTest);
                GL.Viewport(0, 0, Width, Height);
                if (KWEngine.CurrentWorld._background.Type != BackgroundType.None)
                {
                    if (KWEngine.CurrentWorld._background.Type == BackgroundType.Skybox)
                    {
                        RendererBackgroundSkybox.Bind();
                        RendererBackgroundSkybox.SetGlobals();
                        RendererBackgroundSkybox.Draw();
                    }
                    else
                    {
                        RendererBackgroundStandard.Bind();
                        RendererBackgroundStandard.SetGlobals();
                        RendererBackgroundStandard.Draw();
                    }
                }

                GL.Enable(EnableCap.Blend);

                // Forward rendering pass:
                HelperDebug.StartTimeQuery(RenderType.Forward);
                if (gameObjectsForForwardRendering.Count > 0 || KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
                {
                    RendererForward.Bind();
                    RendererForward.SetGlobals();
                    if (gameObjectsForForwardRendering.Count > 0)
                        RendererForward.RenderScene(gameObjectsForForwardRendering);
                    if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
                    {
                        if (KWEngine.CurrentWorld._viewSpaceGameObject.DepthTestingEnabled == false)
                        {
                            GL.Disable(EnableCap.DepthTest);
                        }
                        RendererForward.Draw(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject, true);
                        if (KWEngine.CurrentWorld._viewSpaceGameObject.DepthTestingEnabled == false)
                        {
                            GL.Enable(EnableCap.DepthTest);
                        }
                    }
                }

                if (renderObjectsForForwardRendering.Count > 0)
                {
                    RendererForwardInstanced.Bind();
                    RendererForwardInstanced.SetGlobals();
                    RendererForwardInstanced.RenderScene(renderObjectsForForwardRendering);
                }

                if (KWEngine.CurrentWorld._textObjects.Count > 0)
                {
                    RendererForwardText.Bind();
                    RendererForwardText.SetGlobals();
                    RendererForwardText.RenderScene();
                }
                if (KWEngine.CurrentWorld._particleAndExplosionObjects.Count > 0)
                {
                    RendererParticle.Bind();
                    RendererParticle.RenderParticles(KWEngine.CurrentWorld._particleAndExplosionObjects);
                }
                HelperDebug.StopTimeQuery(RenderType.Forward);
            }

            if (KWEngine.DebugMode == DebugMode.TerrainCollisionModel && KWEngine.CurrentWorld._terrainObjects.Count > 0)
            {
                // Terrain collision patch debug pass:
                RendererTerrainCollision.Bind();
                RendererTerrainCollision.SetGlobals();
                foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    RendererTerrainCollision.Draw(t);
                }
            }

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);

            // HUD objects:
            HelperDebug.StartTimeQuery(RenderType.HUD);
            RendererHUD.Bind();

            // HUD objects first pass:
            int hudrenderindex = RendererHUD.RenderHUDObjects(0, true);
            bool noReset = false;
            // Map pass:
            if (KWEngine.CurrentWorld.Map.Enabled && KWEngine.Mode == EngineMode.Play)
            {
                RendererHUD.Bind();
                RendererHUD.SetGlobals();
                RendererHUD.DrawMap(out noReset);
            }

            // HUD objects second pass:
            RendererHUD.RenderHUDObjects(hudrenderindex, false);
            HelperDebug.StopTimeQuery(RenderType.HUD);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Bloom pass:
            HelperDebug.StartTimeQuery(RenderType.PostProcessing);
            RenderManager.DoBloomPass();

            // Final screen pass:
            Vector4 fadeColor = new Vector4(
                Vector3.Lerp(KWEngine.CurrentWorld._fadeStatePrevious.Color, KWEngine.CurrentWorld._fadeStateCurrent.Color, alpha),
                KWEngine.CurrentWorld._fadeStatePrevious.Factor * alpha + KWEngine.CurrentWorld._fadeStatePrevious.Factor * (1f - alpha)
                );

            RenderManager.BindScreen();
            RendererCopy.Bind();
            RendererCopy.Draw(RenderManager.FramebufferLightingPass, RenderManager.FramebuffersBloomTemp[0], fadeColor);
            HelperDebug.StopTimeQuery(RenderType.PostProcessing);

            if ((int)KWEngine.DebugMode > 0 && (int)KWEngine.DebugMode < 10)
            {
                RenderManager.BindScreen();

                List<FramebufferShadowMap> maps = new();
                bool isCubeMap = false;
                if ((int)KWEngine.DebugMode >= 7 && (int)KWEngine.DebugMode <= 9)
                {
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
                }
                if(isCubeMap)
                {
                    RendererDebugCube.Bind();
                    RendererDebugCube.Draw(maps);
                }
                else
                {
                    RendererDebug.Bind();
                    RendererDebug.Draw(maps);
                }
            }
            else if (KWEngine.CurrentWorld._flowFields.Count > 0)
            {
                GL.Enable(EnableCap.Blend);
                Matrix4 vp = KWEngine.Mode != EngineMode.Edit ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
                lock (KWEngine.CurrentWorld._flowFields)
                {
                    foreach (FlowField f in KWEngine.CurrentWorld._flowFields)
                    {
                        if (f != null && f.IsVisible)
                        {
                            if (f.Destination != null)
                            {
                                RendererFlowFieldDirection.Bind();
                                RendererFlowFieldDirection.SetGlobals();
                                RendererFlowFieldDirection.Draw(f);
                                RendererFlowFieldDirection.UnsetGlobals();
                            }
                            RendererFlowField.Bind();
                            RendererFlowField.Draw(f, ref vp);
                        }
                    }
                }
                GL.Disable(EnableCap.Blend);
            }
#if DEBUG
            HelperGeneral.CheckGLErrors();
#endif
            // unbind last render program:
            GL.UseProgram(0);
            HelperDebug.UpdateTimesAVG();

            KWBuilderOverlay.AddFrameTime(KWEngine.LastFrameTime);
            RenderOverlay((float)e.Time);

            double elapsedTimeForCallsInMS = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
            KWBuilderOverlay.UpdateLastRenderCallsTime(elapsedTimeForCallsInMS);

            // bring image to monitor screen:
            SwapBuffers();

            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Wird aufgerufen, wenn Text eingegeben wird
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            Overlay.PressChar((char)e.Unicode);
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine Maustaste gedrückt wird
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (KWEngine.EditModeActive)
            {
                KWBuilderOverlay.HandleMouseButtonStatus(e.Button, true);

                if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button2)
                {
                    KWBuilderOverlay.HandleMouseSelection(MouseState.Position);
                }
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich die Maus bewegt
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            bool shadowMapDebugMode = (int)KWEngine.DebugMode >= 7 && (int)KWEngine.DebugMode <= 9;

            if (KWEngine.Mode == EngineMode.Edit &&
                KWBuilderOverlay.IsButtonActive(MouseButton.Middle) &&
                !KWBuilderOverlay.IsCursorOnAnyControl() && 
                !shadowMapDebugMode)
            {
                KWEngine.CurrentWorld._cameraEditor.MoveUpDown(-e.DeltaY * KWEngine.MouseSensitivity * 2f);
                KWEngine.CurrentWorld._cameraEditor.Strafe(e.DeltaX * KWEngine.MouseSensitivity * 2f);
            }

            if (KWEngine.Mode == EngineMode.Edit && KWBuilderOverlay.IsButtonActive(MouseButton.Left))
            {
                bool result1 = KWBuilderOverlay.IsCursorOnAnyControl();
                bool result2 = KWBuilderOverlay.IsCursorPressedOnAnyControl(MouseButton.Left);
                if (!result1 && !result2)
                {
                    if (shadowMapDebugMode)
                    {
                        if (!RenderManager.IsCurrentDebugMapACubeMap())
                            return;
                    }
                    KWEngine.CurrentWorld._cameraEditor.ArcBallEditor(e.Delta * KWEngine.MouseSensitivity * 20f, KWBuilderOverlay.SelectedGameObject);
                }
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine Maustaste losgelassen wird
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            KWBuilderOverlay.HandleMouseButtonStatus(e.Button, false);

            if (_mouse._buttonsPressed.TryGetValue(e.Button, out MouseExtState m))
            {
                _mouse._buttonsPressed.Remove(e.Button);
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Mausrad betätigt wird
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            bool shadowMapDebugMode = (int)KWEngine.DebugMode >= 7 && (int)KWEngine.DebugMode <= 9;
            base.OnMouseWheel(e);
            if (KWEngine.Mode == EngineMode.Edit && 
                !KWBuilderOverlay.IsCursorOnAnyControl() &&
                shadowMapDebugMode == false)
            {
                KWEngine.CurrentWorld._cameraEditor.Move(e.OffsetY * 2f);
            }
            Overlay.MouseScroll(e.Offset);
        }

        /// <summary>
        /// Wird ausgelöst, wenn das aktuelle Fenster geschlossen wird
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var item in HelperDebug._renderTimesIDDict.Keys)
            {
                GL.DeleteQuery(HelperDebug._renderTimesIDDict[item]);
            }
            HelperSweepAndPrune.StopThread();
            HelperFlowField.StopThread();
        }

        /// <summary>
        /// Event-Handler, der ausgelöst wird, wenn eine Taste im Fenster gedrückt wird
        /// </summary>
        /// <param name="e">Event-Infos</param>
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Keyboard.IsKeyDown(Keys.LeftAlt) && Keyboard.IsKeyDown(Keys.F4))
            {
                HelperSweepAndPrune.StopThread();
                HelperFlowField.StopThread();
            }
        }

        /// <summary>
        /// Event-Handler, der ausgelöst wird, wenn eine Taste im Fenster losgelassen wird
        /// </summary>
        /// <param name="e">Event-Infos</param>
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            _keyboard._keysPressed.Remove(e.Key);
        }

        internal World _worldNew = null;
        internal void SetWorldInternal(World w)
        {
            foreach (MouseButton b in _mouse._buttonsPressed.Keys)
            {
                if (_mouse._buttonsPressed.ContainsKey(b))
                {
                    _mouse.ChangeToOldWorld(b);
                }
            }

            foreach (Keys k in _keyboard._keysPressed.Keys)
            {
                if (_keyboard._keysPressed.ContainsKey(k))
                {
                    _keyboard.ChangeToOldWorld(k);
                }
            }

            if (KWEngine.CurrentWorld != null)
            {
                HelperSweepAndPrune.StopThread();
                HelperFlowField.StopThread();
                KWEngine.CurrentWorld.Dispose();
            }

            KWEngine.CurrentWorld = w;
            CursorState = CursorState.Normal;
            KWEngine.CurrentWorld.Init();
            KWEngine.CurrentWorld.SetCameraPosition(KWEngine.DefaultCameraPosition);
            KWEngine.CurrentWorld.SetCameraTarget(Vector3.Zero);
            KWEngine.WorldTime = 0;
            KWEngine.CurrentWorld.Prepare();
            KWEngine.CurrentWorld.IsPrepared = true;
            _mouseDeltaToUse = Vector2.Zero;

            HelperGeneral.FlushAndFinish(); 

            HelperSweepAndPrune.StartThread();
            HelperFlowField.StartThread();
            _worldNew = null;  

            HelperDebug.ClearTimeDicts();
        }

        /// <summary>
        /// Setzt eine Welt für das Fenster zur Anzeige und initialisiert diese
        /// </summary>
        /// <param name="w">zu setzende Welt</param>
        public void SetWorld(World w)
        {
            if (w != null)
            {
                _worldNew = w;
            }
        }

        internal static void UpdateDeltaTime(double t)
        {
            float tf = (float)t;
            KWEngine.ApplicationTime += tf;
            if (KWEngine.Mode == EngineMode.Play && KWEngine.CurrentWorld._startingFrameActive == false)
                KWEngine.WorldTime += tf;
            KWEngine.DeltaTimeAccumulator += Math.Min(tf, KWEngine.SIMULATIONMAXACCUMULATOR);
        }

        internal float UpdateScene(out int cycleCount)
        {
            List<GameObject> postponedViewSpaceAttachments = new();
            if (KWEngine.CurrentWorld._startingFrameActive && MouseState.Delta.LengthSquared == 0)
            {
                KWEngine.CurrentWorld._startingFrameActive = false;
            }
            int n = 0;
            if (KWEngine.CurrentWorld._startingFrameActive == false)
            {
                n = UpdateCurrentWorldAndObjects(out double elapsedTimeForCall);
                if (!KWEngine.EditModeActive)
                    KWBuilderOverlay.UpdateLastUpdateTime(elapsedTimeForCall);
                if (KWEngine.DebugPerformanceEnabled)
                    HelperDebug._cpuTimes.Add((float)elapsedTimeForCall);
            }
            cycleCount = n;

            // Start render preparation:
            _stopwatch.Restart();
            KWEngine.LastSimulationUpdateCycleCount = n;
            float alpha = (float)(KWEngine.DeltaTimeAccumulator / KWEngine.DeltaTimeCurrentNibbleSize);
            if (alpha < 0 || alpha > 1)
                alpha = 0;

            HelperSimulation.BlendWorldBackgroundStates(alpha);

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (g.IsAttachedToViewSpaceGameObject == false)
                {
                    HelperSimulation.BlendGameObjectStates(g, alpha);
                }
                else
                {
                    postponedViewSpaceAttachments.Add(g);
                }
            }

            foreach (RenderObject r in KWEngine.CurrentWorld._renderObjects)
            {
                HelperSimulation.BlendRenderObjectStates(r, alpha);
            }

            foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
            {
                HelperSimulation.BlendLightObjectStates(l, alpha);
            }

            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                HelperSimulation.BlendTerrainObjectStates(t, alpha);
            }

            foreach (TextObject t in KWEngine.CurrentWorld._textObjects)
            {
                HelperSimulation.BlendTextObjectStates(t, alpha);
            }

            HelperSimulation.BlendCameraStates(ref KWEngine.CurrentWorld._cameraGame, ref KWEngine.CurrentWorld._cameraEditor, alpha);

            if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
            {
                HelperSimulation.BlendGameObjectStates(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject, alpha, true);
                foreach (GameObject att in postponedViewSpaceAttachments)
                {
                    HelperSimulation.BlendGameObjectStates(att, 1f, true);
                }
            }

            if (KWEngine.Mode == EngineMode.Edit)
            {
                KWEngine.CurrentWorld._cameraEditor._frustum.UpdateFrustum(
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ProjectionMatrix,
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewMatrix
                    );
            }

            // Sort HUDObjects by their z indices:
            KWEngine.CurrentWorld._hudObjects.Sort((x, y) => x._zIndex.CompareTo(y._zIndex));

            return alpha;
        }

        /// <summary>
        /// Fensterbreite (Inhalt) in Pixeln
        /// </summary>
        public int Width { get { return ClientSize.X; } }
        /// <summary>
        /// Fensterhöhe (Inhalt) in Pixeln
        /// </summary>
        public int Height { get { return ClientSize.Y; } }

        /// <summary>
        /// Gibt die (relative) Mitte des Fensters in Pixeln an
        /// </summary>
        public Vector2 Center { get { return ClientRectangle.HalfSize; } }

        /// <summary>
        /// Erfragt die aktuelle Mauscursorposition (relativ zum Fenster)
        /// </summary>
        public new Vector2 MousePosition { get { return Mouse.Position; } }

        internal int UpdateCurrentWorldAndObjects(out double elapsedUpdateTimeForCallInMS)
        {
            int n = 0;
            float tmpTimeAdd;
            float tmpTimeAddSum = 0.0f;
            elapsedUpdateTimeForCallInMS = 0.0;

            while (KWEngine.DeltaTimeAccumulator >= KWEngine.DeltaTimeCurrentNibbleSize)
            {
                _stopwatch.Restart();
                _mouseScrollDelta = (int)MouseState.Scroll.Y - _mouseScrollPosition;
                _mouseScrollPosition = (int)MouseState.Scroll.Y;

                unsafe
                {
                    GLFW.PollEvents();
                    GLFW.GetCursorPos(this.WindowPtr, out var xPos, out var yPos);
                    Mouse._mousePositionFromGLFW = new Vector2((float)xPos, (float)yPos);
                }

                if (KWEngine.CurrentWorld != null && _worldNew == null)
                {
                    KWEngine.CurrentWorld.ResetWorldDimensions();
                    List<GameObject> postponedObjects = new();
                    List<GameObject> postponedObjectsAttachments = new();

                    KWEngine.CurrentWorld.AddRemoveGameObjects();
                    KWEngine.CurrentWorld.AddRemoveRenderObjects();
                    KWEngine.CurrentWorld.AddRemoveFoliageObjects();
                    KWEngine.CurrentWorld.AddRemoveTerrainObjects();
                    KWEngine.CurrentWorld.AddRemoveLightObjects();
                    KWEngine.CurrentWorld.AddRemoveHUDObjects();
                    KWEngine.CurrentWorld.AddRemoveTextObjects();

                    n++;

                    KWEngine.CurrentWorld._cameraGame.BackupCameraState();
                    KWEngine.CurrentWorld._cameraEditor.BackupCameraState();

                    KWEngine.CurrentWorld._background._statePrevious = KWEngine.CurrentWorld._background._stateCurrent;
                    KWEngine.CurrentWorld._fadeStatePrevious = KWEngine.CurrentWorld._fadeStateCurrent;

                    foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                    {
                        l._statePrevious = l._stateCurrent;
                        //KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(l);
                    }

                    foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                    {
                        t._statePrevious = t._stateCurrent;
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(t);
                    }

                    foreach (FoliageObject f in KWEngine.CurrentWorld._foliageObjects)
                    {
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(f);
                    }

                    foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                    {
                        g._statePrevious = g._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            lock (HelperSweepAndPrune.OwnersDict)
                            {
                                bool gHasList = HelperSweepAndPrune.OwnersDict.TryGetValue(g, out List<GameObjectHitbox> collisions);
                                if (gHasList)
                                {
                                    g._collisionCandidates = new List<GameObjectHitbox>(collisions);
                                }
                                else
                                {
                                    g._collisionCandidates = new List<GameObjectHitbox>();
                                }
                            }
                            if (g.UpdateLast)
                            {
                                postponedObjects.Add(g);
                                continue;
                            }
                            else if (g.IsAttachedToGameObject)
                            {
                                // is g's parent the viewspace-gameobject?
                                // if true, the object gets updated in a later state and does not need to get updated here
                                if (KWEngine.CurrentWorld._viewSpaceGameObject != null && g.GetGameObjectThatIAmAttachedTo() == KWEngine.CurrentWorld._viewSpaceGameObject._gameObject)
                                {
                                    continue;
                                }
                                else
                                {
                                    postponedObjectsAttachments.Add(g);
                                    continue;
                                }
                            }
                            g.Act();
                            KWEngine.CurrentWorld.UpdateWorldDimensions(g._stateCurrent._center, g._stateCurrent._dimensions);
                            KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(g);
                        }
                    }

                    foreach (GameObject g in postponedObjects)
                    {
                        g.Act();
                        KWEngine.CurrentWorld.UpdateWorldDimensions(g._stateCurrent._center, g._stateCurrent._dimensions);
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(g);
                    }

                    foreach (GameObject g in postponedObjectsAttachments)
                    {
                        if (!KWEngine.EditModeActive)
                        {
                            g.Act();
                        }
                        KWEngine.CurrentWorld.UpdateWorldDimensions(g._stateCurrent._center, g._stateCurrent._dimensions);
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(g);
                    }

                    for (int i = KWEngine.CurrentWorld._particleAndExplosionObjects.Count - 1; i >= 0; i--)
                    {
                        TimeBasedObject tbo = KWEngine.CurrentWorld._particleAndExplosionObjects[i];
                        if (tbo._done)
                            KWEngine.CurrentWorld._particleAndExplosionObjects.Remove(tbo);
                        else
                        {
                            tbo.Act();
                        }
                    }

                    foreach (TextObject t in KWEngine.CurrentWorld._textObjects)
                    {
                        t._statePrevious = t._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            t.Act();
                        }
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(t);
                    }

                    foreach (RenderObject r in KWEngine.CurrentWorld._renderObjects)
                    {
                        r._statePrevious = r._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            r.Act();
                        }
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(r);
                    }

                    bool hudobjectlostfocus = KWEngine.CurrentWorld.ProcessWorldEventQueue();
                    if (hudobjectlostfocus)
                    {
                        KWEngine.CurrentWorld._textinputLostFocusTimout = KWEngine.CurrentWorld.WorldTime + HUDObjectTextInput.TimeoutDuration;
                    }

                    if (!KWEngine.EditModeActive)
                    {
                        KWEngine.CurrentWorld.Act();
                    }

                    if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
                    {
                        KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._statePrevious = KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            lock (HelperSweepAndPrune.OwnersDict)
                            {
                                bool hasCCs = HelperSweepAndPrune.OwnersDict.TryGetValue(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject, out List<GameObjectHitbox> hitBoxes);
                                if (hasCCs)
                                    KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._collisionCandidates = new List<GameObjectHitbox>(hitBoxes);
                                else
                                    KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._collisionCandidates.Clear();
                            }
                            KWEngine.CurrentWorld._viewSpaceGameObject.Act();
                        }
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject);

                        foreach (GameObject attachment in KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._gameObjectsAttached.Values)
                        {
                            if (!KWEngine.EditModeActive)
                            {
                                lock (HelperSweepAndPrune.OwnersDict)
                                {
                                    bool hasCCs = HelperSweepAndPrune.OwnersDict.TryGetValue(attachment, out List<GameObjectHitbox> hitBoxes);
                                    if (hasCCs)
                                    {
                                        attachment._collisionCandidates = new List<GameObjectHitbox>(hitBoxes);
                                    }
                                }
                                attachment.Act();
                            }
                            KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(attachment);
                        }
                    }

                    
                    double elapsedTimeForIterationInSeconds = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                    KWEngine.DeltaTimeAccumulator -= KWEngine.DeltaTimeCurrentNibbleSize;
                    elapsedUpdateTimeForCallInMS += elapsedTimeForIterationInSeconds * 1000.0;
                    tmpTimeAdd = (float)elapsedTimeForIterationInSeconds;
                    tmpTimeAddSum += tmpTimeAdd;
                    KWEngine.WorldTime += tmpTimeAdd;
                }
                else
                {
                    double elapsedTimeForIterationInSeconds = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                    KWEngine.DeltaTimeAccumulator -= KWEngine.DeltaTimeCurrentNibbleSize;
                    elapsedUpdateTimeForCallInMS += elapsedTimeForIterationInSeconds * 1000.0;
                    tmpTimeAdd = (float)elapsedTimeForIterationInSeconds;
                    tmpTimeAddSum += tmpTimeAdd;
                    KWEngine.WorldTime += tmpTimeAdd;

                    break;
                }
            }
            KWEngine.WorldTime -= tmpTimeAddSum;

            if (KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
            {
                ProcessKeysForHUDObjectTextInput(KWEngine.CurrentWorld._hudObjectInputWithFocus);
            }

            return n;
        }

        internal void ProcessKeysForHUDObjectTextInput(HUDObjectTextInput h)
        {
            if(h != null && h.HasFocus)
            {
                string result = HelperGeneral.ProcessInputs(out Keys specialKey);
                if(specialKey == Keys.Enter)
                {
                    h.ConfirmAndRaiseWorldEvent();
                }
                else if (specialKey == Keys.Escape)
                {
                    h.AbortAndRaiseWorldEvent();
                }
                else if (specialKey == Keys.Backspace)
                {
                    h.Backspace();
                }
                else if (specialKey == Keys.Delete)
                {
                    h.Delete();
                }
                else if (specialKey == Keys.Home)
                {
                    h.MoveCursorToStart();
                }
                else if (specialKey == Keys.End)
                {
                    h.MoveCursorToEnd();
                }
                else if (specialKey == Keys.Left)
                {
                    if (h._cursorPos > 0)
                    {
                        h.MoveCursor(-1);
                    }
                }
                else if (specialKey == Keys.Right)
                {
                    if (h._cursorPos < h.Text.Length)
                    {
                        h.MoveCursor(+1);
                    }
                }
                else
                {
                    if(result.Length > 0)
                        h.AddCharacters(result);
                }
            }
        }

        internal void RenderOverlay(float t)
        {
            if (KeyboardState.IsKeyPressed(Keys.F11))
            {
                KWEngine.ToggleEditMode();
            }
            if (KWEngine.EditModeActive)
            {
                Overlay.Update(this, t);

                KWEngine.DeltaTimeFactorForOverlay = t / (1f / 60f);
                KWBuilderOverlay.Draw();
                Overlay.Render();
            }
        }

        /// <summary>
        /// Ersetzt den aktuellen Mauscursor durch den angegebenen Cursor
        /// </summary>
        /// <param name="cursorName">Name, unter dem der Cursor importiert wurde (wenn dieser Wert null ist, wird der Standardcursor geladen)</param>
        public void SetCursor(string cursorName)
        {
            MouseCursor c = HelperCursor.GetCursor(cursorName);
            if (c != null)
            {
                this.Cursor = c;
            }
            else
            {
                this.Cursor = MouseCursor.Default;
            }
        }

        /// <summary>
        /// Importiert eine eigene Mauscursorgrafik und legt die Grafik unter dem angegebenen Namen in der Engine-Datenbank ab (gültige Dateiformate: PNG, CUR)
        /// </summary>
        /// <param name="name">Name der Grafik</param>
        /// <param name="filename">Relativer Pfad zu der zu importierenden Datei</param>
        /// <param name="tipX">Relative X-Position der Cursorspitze (von 0 bis 1, Standardwert: 0.5f)</param>
        /// <param name="tipY">Relative Y-Position der Cursorspitze (von 0 bis 1, Standardwert: 0.5f)</param>
        public void LoadCursorImage(string name, string filename, float tipX = 0.5f, float tipY = 0.5f)
        {
            if (!HelperCursor.Import(name, filename, tipX, tipY))
            {
                KWEngine.LogWriteLine("[Cursor] Import of " + filename + " failed: File missing or not of type PNG/CUR");
            }
        }

        /// <summary>
        /// Setzt bzw. ändert das Anwendungsfenstersymbol
        /// </summary>
        /// <param name="icon">WindowIcon-Instanz</param>
        public void SetWindowIcon(WindowIcon icon)
        {
            Icon = icon;
        }

        /// <summary>
        /// Konvertiert eine Grafikdatei in das WindowIcon-Format
        /// </summary>
        /// <remarks>Method output may be null if file is invalid</remarks>
        /// <param name="iconFile">Dateiname</param>
        /// <returns>WindowIcon-Instanz</returns>
        public static WindowIcon CreateWindowIconFromFile(string iconFile)
        {
            if(HelperTexture.LoadBitmapForWindowIcon(iconFile, out int width, out int height, out byte[] data))
            {
                return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(width, height, data));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gibt die bei Programmstart gewählte Qualität für Post-Processing-Effekte zurück
        /// </summary>
        public PostProcessingQuality Quality { get { return _ppQuality; } }

        internal static MonitorHandle GetMonitorHandleForPointer(IntPtr ptr)
        {
            foreach (MonitorInfo mInfo in Monitors.GetMonitors())
            {
                if (mInfo.Handle.Pointer == ptr)
                {
                    return mInfo.Handle;
                }
            }
            return Monitors.GetPrimaryMonitor().Handle;
        }
    }
}
