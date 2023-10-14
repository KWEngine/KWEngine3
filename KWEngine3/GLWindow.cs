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
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Basisklasse für das Programmfenster
    /// </summary>
    public abstract class GLWindow : GameWindow
    {
        internal PostProcessingQuality _ppQuality = PostProcessingQuality.High;
        internal ulong FrameTotalCount { get; set; } = 0;
        internal Matrix4 _viewProjectionMatrixHUD;
        internal Matrix4 _viewProjectionMatrixHUDNew;
        internal KWBuilderOverlay Overlay { get; set; }
        internal Stopwatch _stopwatch = new Stopwatch();
        internal int AnisotropicFiltering { get; set; } = 4;
        internal Vector2 _mouseDeltaToUse = Vector2.Zero;
        internal List<Vector2> _mouseDeltas = new List<Vector2>(MOUSEDELTAMAXSAMPLECOUNT);
        internal const int DELTASFORMOVINGAVG = 4;
        internal const int MOUSEDELTAMAXSAMPLECOUNT = 128;


        internal Vector2 GatherWeightedMovingAvg(Vector2 mouseDelta, float dt_ms)
        {
            if (KWEngine.CurrentWorld._startingFrameActive)
                return Vector2.Zero;

            _mouseDeltas.Add(mouseDelta * (1f / dt_ms));
            while (_mouseDeltas.Count > MOUSEDELTAMAXSAMPLECOUNT)
            {
                _mouseDeltas.RemoveAt(0);
            }
            int samplesToLookAt = (int)Math.Ceiling(DELTASFORMOVINGAVG * Math.Pow(1f / dt_ms, 4.0));
            samplesToLookAt = Math.Min(samplesToLookAt + (DELTASFORMOVINGAVG - samplesToLookAt % DELTASFORMOVINGAVG), MOUSEDELTAMAXSAMPLECOUNT - 1);

            
            float sampleWeightStep = 1.0f / (samplesToLookAt - 1);
            int thresholdForNextWeightStep = samplesToLookAt / DELTASFORMOVINGAVG;
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
        /// Standardkonstruktor für den Fullscreen-Modus
        /// </summary>
        /// <param name="vSync">Begrenzung der FPS an die Bildwiederholrate des Monitors?</param>
        /// <param name="ppQuality">Qualität der Post-Processing-Pipeline (Standard: hohe Qualität)</param>
        public GLWindow(bool vSync = true, PostProcessingQuality ppQuality = PostProcessingQuality.High)
            : this(
                 new GameWindowSettings() { UpdateFrequency = 0 },
                 new NativeWindowSettings()
                 {
                     API = ContextAPI.OpenGL,
                     APIVersion = Version.Parse("4.0"),
                     Flags = ContextFlags.ForwardCompatible,
                     WindowState = WindowState.Fullscreen,
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
        public GLWindow(int width, int height, bool vSync = true, PostProcessingQuality ppQuality = PostProcessingQuality.High) 
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
            _ppQuality = ppQuality;
            KWEngine.InitializeFontsAndDefaultTextures();
        }

        

        /// <summary>
        /// Prüft, ob der Mauszeiger aktuell innerhalb des Fensters ist
        /// </summary>
        public bool IsMouseInWindow { get { return MouseState.X >= 0 && MouseState.X < ClientSize.X && MouseState.Y >= 0 && MouseState.Y < ClientSize.Y; } }

        /// <summary>
        /// Standard-Initialisierungen
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            KWEngine._uniformOffsetMultiplier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 4 : 1;
            KWEngine._folderDivider = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
            KWEngine._folderDividerString = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\\" : "/";

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            string date = fileVersionInfo.ProductName.Substring(fileVersionInfo.ProductName.IndexOf('2'), 10);

            if(Title == null || Title == "")
                Title = "KWEngine " + version + " (" + date + ") | OpenGL Version: " + GL.GetString(StringName.Version);
            
            RenderManager.InitializeFramebuffers();
            RenderManager.InitializeShaders();
            RenderManager.InitializeClearColor();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.LineWidth(2f); // not available on core profile
            HelperGeneral.CheckGLErrors();
            
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
            UpdateDeltaTime(e.Time);
            _mouseDeltaToUse = GatherWeightedMovingAvg(MouseState.Delta, (float)(e.Time * 1000.0));

            UpdateScene();

            List<LightObject> pointLights = new List<LightObject>();
            List<GameObject> gameObjectsForForwardRendering = new List<GameObject>();

            KWEngine.LastFrameTime = (float)e.Time * 1000;

            if (!KWEngine.CurrentWorld._startingFrameActive)
            {

                // Render to G-Buffer:
                RenderManager.FramebufferDeferred.Bind();
                RendererGBuffer.Bind();
                gameObjectsForForwardRendering.AddRange(RendererGBuffer.RenderScene());

                // Render terrain objects to G-Buffer:
                if (KWEngine.CurrentWorld._terrainObjects.Count > 0)
                {
                    RendererTerrainGBuffer.Bind();
                    RendererTerrainGBuffer.RenderScene();
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
                        if (l.ShadowCasterType != ShadowQuality.NoShadow)
                        {
                            if (l.Type == LightType.Point)
                            {
                                pointLights.Add(l);
                                continue;
                            }
                            l._fbShadowMap.Bind();
                            RendererShadowMap.RenderSceneForLight(l);
                        }
                    }
                    if (pointLights.Count > 0)
                    {
                        RendererShadowMapCube.Bind();
                        foreach (LightObject l in pointLights)
                        {
                            l._fbShadowMap.Bind();
                            RendererShadowMapCube.RenderSceneForLight(l);
                        }
                    }

                    // Shadow map blur pass:
                    if (_ppQuality == PostProcessingQuality.High)
                    {
                        RendererShadowMapBlur.Bind();
                        foreach (LightObject shadowLight in KWEngine.CurrentWorld._currentShadowLights)
                        {
                            RendererShadowMapBlur.Draw(shadowLight);
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

                RendererLightingPass.Bind();
                RendererLightingPass.SetGlobals();
                RendererLightingPass.Draw(RenderManager.FramebufferDeferred);

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
                        RendererForward.Draw(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject);
                }

                if(KWEngine.CurrentWorld._textObjects.Count > 0)
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

                // HUD objects pass:
                RendererHUD.Bind();
                RendererHUD.SetGlobals();
                RendererHUD.RenderHUDObjects();

                // Bloom pass:
                if (_ppQuality != PostProcessingQuality.Disabled)
                {
                    RenderManager.DoBloomPass();
                }

                // Final screen pass
                RenderManager.BindScreen();
                RendererCopy.Bind();
                RendererCopy.Draw(RenderManager.FramebufferLightingPass, _ppQuality == PostProcessingQuality.Disabled ? null : RenderManager.FramebuffersBloomTemp[0]);

                //if (KWEngine.Mode == EngineMode.Edit && HelperOctree._rootNode != null)
                /*
                if (HelperOctree._rootNode != null)
                {
                    //GL.Disable(EnableCap.DepthTest);
                    RendererOctreeNodes.Bind();
                    Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
                    RendererOctreeNodes.Draw(HelperOctree._rootNode, ref vp);
                    //GL.Enable(EnableCap.DepthTest);
                }
                */

                // unbind last render program:
                GL.UseProgram(0);
            }

            KWBuilderOverlay.AddFrameTime(KWEngine.LastFrameTime);
            RenderOverlay((float)e.Time);

            double elapsedTimeForCallsInMS = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
            KWBuilderOverlay.UpdateLastRenderCallsTime(elapsedTimeForCallsInMS);

            // bring image to monitor screen:
            SwapBuffers();
            FrameTotalCount++;
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
            if(KWEngine.Mode == EngineMode.Edit && 
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
                    KWEngine.CurrentWorld._cameraEditor.ArcBall(e.Delta * KWEngine.MouseSensitivity * 20f);
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
        /// Setzt eine Welt für das Fenster zur Anzeige und initialisiert diese
        /// </summary>
        /// <param name="w">zu setzende Welt</param>
        public void SetWorld(World w)
        {
            if(KWEngine.CurrentWorld != null)
            {
                KWEngine.CurrentWorld.Dispose();
            }
            KWEngine.CurrentWorld = w;
            CursorState = CursorState.Normal;
            KWEngine.CurrentWorld.Init();
            KWEngine.CurrentWorld.SetCameraPosition(KWEngine.DefaultCameraPosition);
            KWEngine.CurrentWorld.SetCameraTarget(Vector3.Zero);
            KWEngine.WorldTime = 0;
            KWEngine.CurrentWorld.Prepare();
            _mouseDeltaToUse = Vector2.Zero;

            HelperGeneral.FlushAndFinish();
        }

        internal void UpdateDeltaTime(double t)
        {
            float tf = (float)t;
            KWEngine.ApplicationTime += tf;
            if (KWEngine.Mode == EngineMode.Play && KWEngine.CurrentWorld._startingFrameActive == false)
                KWEngine.WorldTime += tf;
            KWEngine.DeltaTimeAccumulator += Math.Min(tf, KWEngine.SIMULATIONMAXACCUMULATOR);
        }

        internal void UpdateScene()
        {
            
            List<GameObject> postponedViewSpaceAttachments = new List<GameObject>();
            if(KWEngine.CurrentWorld._startingFrameActive && MouseState.Delta.LengthSquared == 0)
            {
                KWEngine.CurrentWorld._startingFrameActive = false;
            }
            int n = 0;
            if(KWEngine.CurrentWorld._startingFrameActive == false)
            {
                World currentWorldForLoop = KWEngine.CurrentWorld;
                n = UpdateCurrentWorldAndObjects(currentWorldForLoop, out double elapsedTimeForCall);
                if (!KWEngine.EditModeActive)
                    KWBuilderOverlay.UpdateLastUpdateTime(elapsedTimeForCall);
            }

            // Start render calls:
            _stopwatch.Restart();
            KWEngine.LastSimulationUpdateCycleCount = n;
            float alpha = (float)(KWEngine.DeltaTimeAccumulator / KWEngine.DeltaTimeCurrentNibbleSize);
            if (alpha < 0 || alpha > 1)
                alpha = 0;

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if(g.IsAttachedToViewSpaceGameObject == false)
                {
                    HelperSimulation.BlendGameObjectStates(g, alpha);
                }
                else
                {
                    postponedViewSpaceAttachments.Add(g);
                }
            }

            foreach(LightObject l in KWEngine.CurrentWorld._lightObjects)
            {
                HelperSimulation.BlendLightObjectStates(l, alpha);
            }

            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                HelperSimulation.BlendTerrainObjectStates(t, alpha);
            }

            foreach(TextObject t in KWEngine.CurrentWorld._textObjects)
            {
                HelperSimulation.BlendTextObjectStates(t, alpha);
            }

            HelperSimulation.BlendCameraStates(ref KWEngine.CurrentWorld._cameraGame, ref KWEngine.CurrentWorld._cameraEditor, alpha);

            if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
            {
                HelperSimulation.BlendGameObjectStates(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject, alpha);
                foreach(GameObject att in postponedViewSpaceAttachments)
                {
                    HelperSimulation.BlendGameObjectStates(att, 1f);
                }
                KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._collisionCandidates.Clear();
            }
                
            if (KWEngine.Mode == EngineMode.Edit)
            {
                KWEngine.CurrentWorld._cameraEditor._frustum.UpdateFrustum(
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ProjectionMatrix,
                    KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewMatrix
                    );
            }
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

        internal int UpdateCurrentWorldAndObjects(World worldBeforeLoop, out double elapsedUpdateTimeForCallInMS)
        {
            int n = 0;
            elapsedUpdateTimeForCallInMS = 0.0;
            while (KWEngine.DeltaTimeAccumulator >= KWEngine.DeltaTimeCurrentNibbleSize)
            {
                _stopwatch.Restart();
                if (KWEngine.CurrentWorld != null && worldBeforeLoop == KWEngine.CurrentWorld)
                {
                    KWEngine.CurrentWorld.ResetWorldDimensions();
                    List<GameObject> postponedObjects = new List<GameObject>();
                    List<GameObject> postponedObjectsAttachments = new List<GameObject>();

                    KWEngine.CurrentWorld.AddRemoveGameObjects();
                    KWEngine.CurrentWorld.AddRemoveTerrainObjects();
                    KWEngine.CurrentWorld.AddRemoveLightObjects();
                    KWEngine.CurrentWorld.AddRemoveHUDObjects();
                    KWEngine.CurrentWorld.AddRemoveTextObjects();
                    HelperSweepAndPrune.SweepAndPrune();

                    /*
                    // Octree test:
                    float dimX = KWEngine.CurrentWorld._xMinMax.Y - KWEngine.CurrentWorld._xMinMax.X;
                    float dimY = KWEngine.CurrentWorld._yMinMax.Y - KWEngine.CurrentWorld._yMinMax.X;
                    float dimZ = KWEngine.CurrentWorld._zMinMax.Y - KWEngine.CurrentWorld._zMinMax.X;
                    float dimMax = Math.Max(Math.Max(dimX, dimY), dimZ);
                    HelperOctree.Init(KWEngine.CurrentWorld._worldCenter, dimMax);

                    foreach (GameObjectHitbox hb in KWEngine.CurrentWorld._gameObjectHitboxes)
                    {
                        HelperOctree.Add(hb);
                    }
                    */

                    n++;

                    KWEngine.CurrentWorld._cameraGame.BackupCameraState();
                    KWEngine.CurrentWorld._cameraEditor.BackupCameraState();

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

                    foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                    {
                        g._statePrevious = g._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
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
                            g.Act();
                        KWEngine.CurrentWorld.UpdateWorldDimensions(g._stateCurrent._center, g._stateCurrent._dimensions);
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(g);
                    }

                    for (int i = KWEngine.CurrentWorld._particleAndExplosionObjects.Count - 1; i >= 0; i--)
                    {
                        TimeBasedObject tbo = KWEngine.CurrentWorld._particleAndExplosionObjects[i];
                        if (tbo._done)
                            KWEngine.CurrentWorld._particleAndExplosionObjects.Remove(tbo);
                        else
                            tbo.Act();
                    }

                    foreach(TextObject t in KWEngine.CurrentWorld._textObjects)
                    {
                        t._statePrevious = t._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            t.Act();
                        }
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(t);
                    }

                    if (!KWEngine.EditModeActive)
                        KWEngine.CurrentWorld.Act();

                    KWEngine.CurrentWorld.ProcessWorldEventQueue();

                    if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached)
                    {
                        KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._statePrevious = KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._stateCurrent;
                        if (!KWEngine.EditModeActive)
                        {
                            KWEngine.CurrentWorld._viewSpaceGameObject.Act();
                            //HelperSimulation.UpdateBoneTransformsForViewSpaceGameObject(KWEngine.CurrentWorld._viewSpaceGameObject);
                        }
                        KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject);

                        foreach (GameObject attachment in KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._gameObjectsAttached.Values)
                        {
                            if (!KWEngine.EditModeActive)
                                attachment.Act();
                            KWEngine.CurrentWorld._cameraGame._frustum.UpdateScreenSpaceStatus(attachment);
                        }

                    }

                    double elapsedTimeForIterationInSeconds = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;// * 1000.0;
                    KWEngine.DeltaTimeAccumulator -= KWEngine.DeltaTimeCurrentNibbleSize;
                    elapsedUpdateTimeForCallInMS += elapsedTimeForIterationInSeconds * 1000.0;
                }
                else
                {
                    double elapsedTimeForIterationInSeconds = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;// * 1000.0;
                    KWEngine.DeltaTimeAccumulator -= KWEngine.DeltaTimeCurrentNibbleSize;
                    elapsedUpdateTimeForCallInMS += elapsedTimeForIterationInSeconds * 1000.0;
                    break;
                }
            }
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
 
    }
}
