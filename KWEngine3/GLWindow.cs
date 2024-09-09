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
        internal List<Vector2> _mouseDeltas = new(MOUSEDELTAMAXSAMPLECOUNT);

        // quality related:
        internal PostProcessingQuality _ppQuality = PostProcessingQuality.Standard;
        internal int AnisotropicFilteringLevel { get; set; } = 4;

        // other:
        internal Matrix4 _viewProjectionMatrixHUD;
        internal Matrix4 _viewProjectionMatrixHUDNew;
        internal KWBuilderOverlay Overlay { get; set; }
        internal Stopwatch _stopwatch = new();


        internal Vector2 GatherWeightedMovingAvg(Vector2 mouseDelta, float dt_ms)
        {
            if (KWEngine.CurrentWorld._startingFrameActive)
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
        public GLWindow(bool vSync = true, PostProcessingQuality ppQuality = PostProcessingQuality.Standard)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Fullscreen,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     CurrentMonitor = Monitors.GetPrimaryMonitor().Handle
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
        public GLWindow(int width, int height, bool vSync = true)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     Size = new Vector2i(width, height),
                     WindowBorder = WindowBorder.Fixed,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = ""
                 }
        )
        {
            _ppQuality = PostProcessingQuality.Standard;
            KWEngine.InitializeFontsAndDefaultTextures();
        }

        /// <summary>
        /// Standardkonstruktor für den Fenstermodus
        /// </summary>
        /// <param name="width">Breite des Fensterinhalts in Pixeln</param>
        /// <param name="height">Höhe des Fenterinhalts in Pixeln</param>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        /// <param name="windowMode">Art des Fensters (Standard oder rahmenlos)</param>
        public GLWindow(int width, int height, bool vSync, PostProcessingQuality ppQuality, WindowMode windowMode)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     Size = new Vector2i(width, height),
                     WindowBorder = windowMode == WindowMode.Default ? WindowBorder.Fixed : WindowBorder.Hidden,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = ""
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
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        /// <param name="windowMode">Art des Fensters (Standard oder rahmenlos)</param>
        /// <param name="monitorHandle">Handle des Monitors, auf dem das Fenster geöffnet werden soll</param>
        public GLWindow(int width, int height, bool vSync, PostProcessingQuality ppQuality, WindowMode windowMode, IntPtr monitorHandle)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Normal,
                     Size = new Vector2i(width, height),
                     WindowBorder = windowMode == WindowMode.Default ? WindowBorder.Fixed : WindowBorder.Hidden,
                     Vsync = vSync ? VSyncMode.On : VSyncMode.Off,
                     Title = "",
                     CurrentMonitor = GetMonitorHandleForPointer(monitorHandle)
                 }
        )
        {
            _ppQuality = ppQuality;
            KWEngine.InitializeFontsAndDefaultTextures();
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger aktuell innerhalb des Fensters ist
        /// </summary>
        public bool IsMouseInWindow { get { return MouseState.X >= 0 && MouseState.X < ClientSize.X && MouseState.Y >= 0 && MouseState.Y < ClientSize.Y; } }

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

            RenderManager.InitializeFramebuffers();
            RenderManager.InitializeShaders();
            RenderManager.InitializeClearColor();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4); // terrain tesselation

            //GL.LineWidth(2f); // not available on core profile

            KWBuilderOverlay.InitFrameTimeQueue();
        }

        /// <summary>
        /// Wird für Änderungen der Tastatureingaben und Mauseingaben verwendet
        /// </summary>
        /// <param name="args">aktuelle Angaben</param>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

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
            _viewProjectionMatrixHUD = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0) * Matrix4.CreateOrthographic(ClientSize.X, ClientSize.Y, 0.1f, 100f);
            _viewProjectionMatrixHUDNew = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0) * Matrix4.CreateOrthographicOffCenter(0, ClientSize.X, ClientSize.Y, 0, 0.1f, 100f);
        }

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

            float alpha = UpdateScene();

            List<LightObject> pointLights = new();
            List<GameObject> gameObjectsForForwardRendering = new();
            List<RenderObject> renderObjectsForForwardRendering = new();

            KWEngine.LastFrameTime = (float)e.Time * 1000;



            // Start render process:
            if (KWEngine.CurrentWorld != null && !KWEngine.CurrentWorld._startingFrameActive)
            {
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

                if (KWEngine.Mode == EngineMode.Edit)
                {
                    GL.Disable(EnableCap.DepthTest);
                    RendererLightOverlay.Bind();
                    RendererLightOverlay.SetGlobals();
                    RendererLightOverlay.Draw(KWEngine.CurrentWorld._lightObjects);
                    GL.Enable(EnableCap.DepthTest);
                }

                // Shadow map pass:
                KWEngine.CurrentWorld.PrepareLightObjectsForRenderPass();
                if (Framebuffer._fbShadowMapCounter > 0)
                {
                    RendererShadowMap.Bind();
                    foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                    {
                        if (l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
                        {
                            l._fbShadowMap.Bind();
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
                            if (l.Type == LightType.Point)
                            {
                                continue;
                            }
                            l._fbShadowMap.Bind(false);
                            RendererShadowMapTerrain.RenderSceneForLight(l);
                        }
                    }

                    if (KWEngine.CurrentWorld._renderObjects.Count > 0)
                    {
                        RendererShadowMapInstanced.Bind();
                        foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                        {
                            if (l.Type == LightType.Point)
                            {
                                continue;
                            }
                            l._fbShadowMap.Bind(false);
                            RendererShadowMapInstanced.RenderSceneForLight(l);
                        }
                    }

                    if (pointLights.Count > 0)
                    {
                        RendererShadowMapCube.Bind();
                        foreach (LightObject l in pointLights)
                        {
                            l._fbShadowMap.Bind(false);
                            RendererShadowMapCube.RenderSceneForLight(l);
                        }

                        if (KWEngine.CurrentWorld._renderObjects.Count > 0)
                        {
                            RendererShadowMapCubeInstanced.Bind();
                            foreach (LightObject l in pointLights)
                            {
                                l._fbShadowMap.Bind(false);
                                RendererShadowMapCubeInstanced.RenderSceneForLight(l);
                            }
                        }
                    }
                }

                // clear inbetween:
                GL.UseProgram(0);

                // Lighting pass:
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
                RenderManager.FramebufferLightingPass.BindAndClearColor();
                RenderManager.FramebufferLightingPass.CopyDepthFrom(RenderManager.FramebufferDeferred);
                RenderManager.FramebufferLightingPass.Bind(false);

                GL.Disable(EnableCap.DepthTest);
                RendererLightingPass.Bind();
                RendererLightingPass.SetGlobals();
                RendererLightingPass.Draw(RenderManager.FramebufferDeferred);
                GL.Enable(EnableCap.DepthTest);

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
                // Forward rendering pass:
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
                        RendererForward.Draw(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject);
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
            }
            GL.Disable(EnableCap.DepthTest);

            // Terrain collision patch debug pass:
            RendererTerrainCollision.Bind();
            RendererTerrainCollision.SetGlobals();
            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                RendererTerrainCollision.Draw(t);
            }

            // HUD objects pass:
            RendererHUD.Bind();
            RendererHUD.SetGlobals();
            RendererHUD.RenderHUDObjects();

            // Bloom pass:
            RenderManager.DoBloomPass();

            // Final screen pass
            Vector4 fadeColor = new Vector4(
                Vector3.Lerp(KWEngine.CurrentWorld._fadeStatePrevious.Color, KWEngine.CurrentWorld._fadeStateCurrent.Color, alpha),
                KWEngine.CurrentWorld._fadeStatePrevious.Factor * alpha + KWEngine.CurrentWorld._fadeStatePrevious.Factor * (1f - alpha)
                );


            RenderManager.BindScreen();
            RendererCopy.Bind();
            RendererCopy.Draw(RenderManager.FramebufferLightingPass, RenderManager.FramebuffersBloomTemp[0], fadeColor);

            if (KWEngine.CurrentWorld._flowField != null && KWEngine.CurrentWorld._flowField.IsVisible)
            {
                if (KWEngine.CurrentWorld._flowField.Destination != null)
                {
                    GL.Enable(EnableCap.Blend);
                    RendererFlowFieldDirection.Bind();
                    RendererFlowFieldDirection.SetGlobals();
                    RendererFlowFieldDirection.Draw(KWEngine.CurrentWorld._flowField);
                    RendererFlowFieldDirection.UnsetGlobals();
                    GL.Disable(EnableCap.Blend);
                }
                RendererFlowField.Bind();
                Matrix4 vp = KWEngine.Mode != EngineMode.Edit ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
                RendererFlowField.Draw(KWEngine.CurrentWorld._flowField, ref vp);
            }
#if DEBUG
            HelperGeneral.CheckGLErrors();
#endif
            // unbind last render program:
            GL.UseProgram(0);


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
            if (KWEngine.Mode == EngineMode.Edit &&
                KWBuilderOverlay.IsButtonActive(MouseButton.Middle)
                && !KWBuilderOverlay.IsCursorOnAnyControl())
            {
                KWEngine.CurrentWorld._cameraEditor.MoveUpDown(-e.DeltaY * KWEngine.MouseSensitivity * 2f);
                KWEngine.CurrentWorld._cameraEditor.Strafe(e.DeltaX * KWEngine.MouseSensitivity * 2f);
            }

            if (KWEngine.Mode == EngineMode.Edit &&
                KWBuilderOverlay.IsButtonActive(MouseButton.Left)
                )
            {
                bool result1 = KWBuilderOverlay.IsCursorOnAnyControl();
                bool result2 = KWBuilderOverlay.IsCursorPressedOnAnyControl(MouseButton.Left);
                if (!result1 && !result2)
                {
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
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Mausrad betätigt wird
        /// </summary>
        /// <param name="e">Parameter</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (KWEngine.Mode == EngineMode.Edit && !KWBuilderOverlay.IsCursorOnAnyControl())
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
            HelperSweepAndPrune.StopThread();
            HelperFlowField.StopThread();
        }

        /// <summary>
        /// Event-Handler, der ausgelöst wird, wenn eine Taste im Fenster gedrückt wird
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Keyboard.IsKeyDown(Keys.LeftAlt) && Keyboard.IsKeyDown(Keys.F4))
            {
                HelperSweepAndPrune.StopThread();
                HelperFlowField.StopThread();
            }
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

        internal float UpdateScene()
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
            }


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
                HelperSimulation.BlendGameObjectStates(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject, alpha);
                foreach (GameObject att in postponedViewSpaceAttachments)
                {
                    HelperSimulation.BlendGameObjectStates(att, 1f);
                }
            }

            if (KWEngine.Mode == EngineMode.Edit)
            {
                KWEngine.CurrentWorld._cameraEditor._frustum.UpdateFrustum(
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ProjectionMatrix,
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewMatrix
                    );
            }

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

        internal int UpdateCurrentWorldAndObjects(out double elapsedUpdateTimeForCallInMS)
        {
            int n = 0;
            float tmpTimeAdd;
            float tmpTimeAddSum = 0.0f;
            elapsedUpdateTimeForCallInMS = 0.0;

            while (KWEngine.DeltaTimeAccumulator >= KWEngine.DeltaTimeCurrentNibbleSize)
            {
                _stopwatch.Restart();

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
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(l);
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

                    if (!KWEngine.EditModeActive)
                    {
                        KWEngine.CurrentWorld.Act();
                    }

                    KWEngine.CurrentWorld.ProcessWorldEventQueue();

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
            return n;
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
