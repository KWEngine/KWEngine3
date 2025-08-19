using OpenTK.Mathematics;

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
                if (KWEngine.Window._disposed > GLWindow.DisposeStatus.None)
                {
                    DoRun = false;
                    break;
                }

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

        internal static CardinalDirection GetCardinalDirectionForVector3(Vector3 dir)
        {
            if (dir.X > 0.9f)
            {
                return CardinalDirection.East;
            }
            else if (dir.X < -0.9f)
            {
                return CardinalDirection.West;
            }
            else if (dir.Z > 0.9f)
            {
                return CardinalDirection.South;
            }
            else if (dir.Z < -0.9f)
            {
                return CardinalDirection.North;

            }
            else if (dir.X > 0.7f && dir.X < 0.8f)
            {
                // northeast or southeast?
                if (dir.Z > 0)
                    return CardinalDirection.SouthEast;
                else
                    return CardinalDirection.NorthEast;
            }
            else if (dir.X < -0.7f && dir.X > -0.8f)
            {
                // northwest or southwest?
                if (dir.Z > 0)
                    return CardinalDirection.SouthWest;
                else
                    return CardinalDirection.NorthWest;
            }
            else
            {
                return CardinalDirection.None;
            }
        }
    }
}
