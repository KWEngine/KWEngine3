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
            lock (KWEngine.CurrentWorld._flowFields)
            {
                foreach (FlowField f in KWEngine.CurrentWorld._flowFields)
                {
                    f.UpdateCostField();
                    f.UpdateFlowField();
                }
            }
        }

        internal static FlowFieldCell CopyCell(FlowFieldCell ffc)
        {
            FlowFieldCell copy = new FlowFieldCell(ffc.Position, ffc._gridIndex, ffc.Parent)
            {
                BestCost = ffc.BestCost,
                BestDirection = ffc.BestDirection,
                Cost = ffc.Cost
            };
            return copy;
        }
    }
}
