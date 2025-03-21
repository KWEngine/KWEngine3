
using KWEngine3.GameObjects;

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
        internal HUDObjectText _performanceCPU;
        internal HUDObjectText _performanceFPS;
        internal List<int> fps = new();

        /// <summary>
        /// Erweitert die Act()-Methode um das Aktualisieren der gemessenen Performance-Werte und stellt diese Werte dar
        /// </summary>
        public override void Act()
        {
            if (KWEngine.DebugPerformanceEnabled)
            {
                if (fps.Count == 240)
                    fps.RemoveAt(0);
                fps.Add(KWEngine.FPS);

                _performanceDeferred.SetText("Deferred:" + KWEngine.GetRenderTime(RenderType.Deferred) + "ms");
                _performanceLighting.SetText("Lighting:" + KWEngine.GetRenderTime(RenderType.Lighting) + "ms");
                _performanceShadowMapping.SetText("Shadows: " + KWEngine.GetRenderTime(RenderType.ShadowMapping) + "ms");
                _performanceSSAO.SetText("SSAO:    " + KWEngine.GetRenderTime(RenderType.SSAO) + "ms");

                _performanceForward.SetText("Forward: " + KWEngine.GetRenderTime(RenderType.Forward) + "ms");
                _performanceHUD.SetText("HUD:     " + KWEngine.GetRenderTime(RenderType.HUD) + "ms");
                _performancePostProcessing.SetText("PostP.:  " + KWEngine.GetRenderTime(RenderType.PostProcessing) + "ms");

                _performanceCPU.SetText("CPU:     " + KWEngine.GetRenderTime(RenderType.PostProcessing) + "ms");

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
            AddHUDObject(_performanceDeferred);

            _performanceLighting = new HUDObjectText("0");
            _performanceLighting.SetPosition(16, 16 + 32);
            _performanceLighting.ForceMonospace = true;
            _performanceLighting.SetScale(32);
            AddHUDObject(_performanceLighting);

            _performanceSSAO = new HUDObjectText("0");
            _performanceSSAO.SetPosition(16, 16 + 32 + 32);
            _performanceSSAO.ForceMonospace = true;
            _performanceSSAO.SetScale(32);
            AddHUDObject(_performanceSSAO);

            _performanceShadowMapping = new HUDObjectText("0");
            _performanceShadowMapping.SetPosition(16, 16 + 32 + 32 + 32);
            _performanceShadowMapping.ForceMonospace = true;
            _performanceShadowMapping.SetScale(32);
            AddHUDObject(_performanceShadowMapping);

            _performanceHUD = new HUDObjectText("0");
            _performanceHUD.SetPosition(16, 16 + 32 + 32 + 32 + 32);
            _performanceHUD.ForceMonospace = true;
            _performanceHUD.SetScale(32);
            AddHUDObject(_performanceHUD);

            _performancePostProcessing = new HUDObjectText("0");
            _performancePostProcessing.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32);
            _performancePostProcessing.ForceMonospace = true;
            _performancePostProcessing.SetScale(32);
            AddHUDObject(_performancePostProcessing);

            _performanceForward = new HUDObjectText("0");
            _performanceForward.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceForward.ForceMonospace = true;
            _performanceForward.SetScale(32);
            AddHUDObject(_performanceForward);

            _performanceCPU = new HUDObjectText("0");
            _performanceCPU.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceCPU.ForceMonospace = true;
            _performanceCPU.SetScale(32);
            AddHUDObject(_performanceCPU);

            _performanceFPS = new HUDObjectText("FPS:     0");
            _performanceFPS.SetPosition(16, 16 + 32 + 32 + 32 + 32 + 32 + 32 + 32 + 32);
            _performanceFPS.ForceMonospace = true;
            _performanceFPS.SetScale(32);
            AddHUDObject(_performanceFPS);
        }

        /// <summary>
        /// Erweitert die Prepare()-Methode um das Vorbereiten der Darstellung von Performance-Werten
        /// </summary>
        public override void Prepare()
        {
            KWEngine.DebugPerformanceEnabled = true;
            InitHUDElements();
        }
    }
}
