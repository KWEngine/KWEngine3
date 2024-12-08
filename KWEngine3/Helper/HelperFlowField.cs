using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal static class HelperFlowField
    {
        internal static Thread FlowFieldThread;
        internal static bool DoRun = true;
        internal static float WorldTimeLast = 0;
        internal const float SLOTTIME = 1f / 30f;

        internal static void ThreadMethod()
        {
            while (DoRun)
            {
                if (KWEngine.WorldTime - WorldTimeLast > SLOTTIME)
                {
                    UpdateFlowField();
                    WorldTimeLast = KWEngine.WorldTime;
                }
                Thread.Sleep(33);
            }
        }

        internal static void StartThread()
        {
            FlowFieldThread = new Thread(ThreadMethod);
            DoRun = true;
            WorldTimeLast = 0;

            // Run once on start:
            UpdateFlowField();
            FlowFieldThread.Start();
        }

        internal static void StopThread()
        {
            DoRun = false;
            if (FlowFieldThread != null)
            {
                FlowFieldThread.Join();
            }
        }

        internal static void UpdateFlowField()
        {
            if(KWEngine.CurrentWorld._flowField != null)
            {
                KWEngine.CurrentWorld._flowField.UpdateCostField();
                KWEngine.CurrentWorld._flowField.UpdateFlowField();
            }
        }
    }
}
