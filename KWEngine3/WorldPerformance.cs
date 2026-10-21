
using KWEngine3.GameObjects;
using KWEngine3.Helper;

namespace KWEngine3
{
    /// <summary>
    /// Stellt eine Erweiterung der Welt-Klasse zur Messung der Performance dar
    /// </summary>
    public abstract class WorldPerformance : World
    {
        internal HUDObjectText _performanceDeferred;
        internal HUDObjectText _performanceShadowMapping;
        internal HUDObjectText _performanceSSAO;
        internal HUDObjectText _performanceHUD;
        internal HUDObjectText _performancePostProcessing;
        internal HUDObjectText _performanceForward;
        internal HUDObjectText _performanceLighting;
        internal HUDObjectText _performanceFog;
        internal HUDObjectText _performanceFoliage;
        internal HUDObjectText _performanceAntiAliasing;
        internal HUDObjectText _performanceCPU;         // cpu: simulation steps of a frame
        internal HUDObjectText _performanceCPURender;   // cpu: preparing and submitting a frame
        internal HUDObjectText _performanceCPUStep;     // cpu: one simulation step
        internal HUDObjectText _performanceFPS;
        internal List<int> fps = new();
        private float _lastTextUpdate = -1f;

        /// <summary>
        /// Die Performance-Werte werden von der Engine automatisch aktualisiert, der Aufruf von base.Act() ist nicht mehr nötig
        /// </summary>
        public override void Act()
        {
        }

        /// <summary>
        /// Die Performance-Anzeige wird von der Engine automatisch vorbereitet, der Aufruf von base.Prepare() ist nicht mehr nötig
        /// </summary>
        public override void Prepare()
        {
        }

        // Called by the engine before Prepare() of the subclass. Not virtual: Prepare() is overridden by the subclass,
        // so a call of Prepare() would always run the subclass version.
        internal void PreparePerformance()
        {
            KWEngine.DebugPerformanceEnabled = true;
            InitHUDElements();
        }

        // called by the engine before Act() of the subclass (see PreparePerformance)
        internal void ActPerformance()
        {
            if (KWEngine.DebugPerformanceEnabled)
            {
                if (fps.Count == 240)
                    fps.RemoveAt(0);
                fps.Add(KWEngine.FPS);

                // averages change once per second only, so the texts (string allocations) are rebuilt only then
                if (HelperDebug._glQueryTimestampLastReset == _lastTextUpdate)
                    return;
                _lastTextUpdate = HelperDebug._glQueryTimestampLastReset;

                _performanceDeferred.SetText("Deferred:" + KWEngine.GetRenderTime(RenderType.Deferred) + "ms");
                _performanceLighting.SetText("Lighting:" + KWEngine.GetRenderTime(RenderType.Lighting) + "ms");
                _performanceShadowMapping.SetText("Shadows: " + KWEngine.GetRenderTime(RenderType.ShadowMapping) + "ms");
                _performanceSSAO.SetText("SSAO:    " + KWEngine.GetRenderTime(RenderType.SSAO) + "ms");
                _performanceFog.SetText("Fog:     " + KWEngine.GetRenderTime(RenderType.Fog) + "ms");
                _performanceFoliage.SetText("Foliage: " + KWEngine.GetRenderTime(RenderType.Foliage) + "ms");
                _performanceAntiAliasing.SetText("FXAA:    " + KWEngine.GetRenderTime(RenderType.AntiAliasing) + "ms");

                _performanceForward.SetText("Forward: " + KWEngine.GetRenderTime(RenderType.Forward) + "ms");
                _performanceHUD.SetText("HUD:     " + KWEngine.GetRenderTime(RenderType.HUD) + "ms");
                _performancePostProcessing.SetText("PostP.:  " + KWEngine.GetRenderTime(RenderType.PostProcessing) + "ms");

                _performanceCPU.SetText("CPU Upd: " + KWEngine.GetCPUTime() + "ms");
                _performanceCPURender.SetText("CPU Rnd: " + Math.Round(HelperDebug._renderTimeAvg, 3) + "ms");
                _performanceCPUStep.SetText("CPU Step:" + Math.Round(HelperDebug._stepTimeAvg, 3) + "ms");

                if (fps.Count == 240)
                    _performanceFPS.SetText("FPS:     " + Math.Round(fps.Average(), 0));
            }
        }

        internal void InitHUDElements()
        {
            _performanceDeferred = new HUDObjectText("0");
            _performanceDeferred.SetPosition(16, 16);
            _performanceDeferred.ForceMonospace = true;
            _performanceDeferred.SetScale(32);
            _performanceDeferred.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceDeferred);

            _performanceLighting = new HUDObjectText("0");
            _performanceLighting.SetPosition(16, 16 + 32);
            _performanceLighting.ForceMonospace = true;
            _performanceLighting.SetScale(32);
            _performanceLighting.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceLighting);

            _performanceSSAO = new HUDObjectText("0");
            _performanceSSAO.SetPosition(16, 16 + 32 + 32);
            _performanceSSAO.ForceMonospace = true;
            _performanceSSAO.SetScale(32);
            _performanceSSAO.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceSSAO);

            _performanceShadowMapping = new HUDObjectText("0");
            _performanceShadowMapping.SetPosition(16, 16 + 32 + 32 + 32);
            _performanceShadowMapping.ForceMonospace = true;
            _performanceShadowMapping.SetScale(32);
            _performanceShadowMapping.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceShadowMapping);

            _performanceHUD = new HUDObjectText("0");
            _performanceHUD.SetPosition(16, 16 + 32 + 32 + 32 + 32);
            _performanceHUD.ForceMonospace = true;
            _performanceHUD.SetScale(32);
            _performanceHUD.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceHUD);

            _performancePostProcessing = new HUDObjectText("0");
            _performancePostProcessing.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32);
            _performancePostProcessing.ForceMonospace = true;
            _performancePostProcessing.SetScale(32);
            _performancePostProcessing.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performancePostProcessing);

            _performanceForward = new HUDObjectText("0");
            _performanceForward.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceForward.ForceMonospace = true;
            _performanceForward.SetScale(32);
            _performanceForward.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceForward);

            _performanceCPU = new HUDObjectText("0");
            _performanceCPU.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceCPU.ForceMonospace = true;
            _performanceCPU.SetScale(32);
            _performanceCPU.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceCPU);

            _performanceFog = new HUDObjectText("0");
            _performanceFog.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceFog.ForceMonospace = true;
            _performanceFog.SetScale(32);
            _performanceFog.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceFog);

            _performanceFoliage = new HUDObjectText("0");
            _performanceFoliage.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceFoliage.ForceMonospace = true;
            _performanceFoliage.SetScale(32);
            _performanceFoliage.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceFoliage);

            _performanceAntiAliasing = new HUDObjectText("0");
            _performanceAntiAliasing.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceAntiAliasing.ForceMonospace = true;
            _performanceAntiAliasing.SetScale(32);
            _performanceAntiAliasing.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceAntiAliasing);

            _performanceCPURender = new HUDObjectText("0");
            _performanceCPURender.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceCPURender.ForceMonospace = true;
            _performanceCPURender.SetScale(32);
            _performanceCPURender.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceCPURender);

            _performanceCPUStep = new HUDObjectText("0");
            _performanceCPUStep.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceCPUStep.ForceMonospace = true;
            _performanceCPUStep.SetScale(32);
            _performanceCPUStep.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceCPUStep);

            _performanceFPS = new HUDObjectText("FPS:     0");
            _performanceFPS.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceFPS.ForceMonospace = true;
            _performanceFPS.SetScale(32);
            _performanceFPS.SetColorOutline(0.25f, 0.25f, 0.25f, 0.8f);
            AddHUDObject(_performanceFPS);
        }
    }
}
