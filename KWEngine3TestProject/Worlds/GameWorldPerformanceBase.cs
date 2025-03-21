using KWEngine3;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    public abstract class GameWorldPerformanceBase : World
    {
        private HUDObjectText _performanceDeferred;
        private HUDObjectText _performanceShadowMapping;
        private HUDObjectText _performanceSSAO;
        private HUDObjectText _performanceHUD;
        private HUDObjectText _performancePostProcessing;
        private HUDObjectText _performanceForward;
        private HUDObjectText _performanceLighting;
        private HUDObjectText _performanceCPU;
        private HUDObjectText _performanceFPS;
        private List<int> fps = new();

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

                if(fps.Count == 240)
                    _performanceFPS.SetText("FPS:     " + Math.Round(fps.Average(), 0));
            }
        }

        public override void Prepare()
        {
            KWEngine.DebugPerformanceEnabled = true;
            InitHUD();
        }

        protected void InitHUD()
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
    }
}
