using KWEngine3.GameObjects;
using OpenTK.Core.Native;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace KWEngine3.Helper
{
    internal static class HelperSweepAndPrune
    {
        internal static Thread BroadphaseThread;
        internal static bool DoRun = true;
        internal static float WorldTimeLast = 0;
        internal static int _sweepTestAxisIndex = 0;
        internal static Dictionary<GameObject, List<GameObjectHitbox>> OwnersDict = new Dictionary<GameObject, List<GameObjectHitbox>>();
        internal const float SLOTTIME = 1f / 30f;

        internal static void ThreadMethod()
        {
            while (DoRun)
            {
                if (KWEngine.WorldTime - WorldTimeLast > SLOTTIME)
                {
                    SweepAndPrune();
                    WorldTimeLast = KWEngine.WorldTime;
                }
                Thread.Sleep(0);
            }
        }

        internal static void StartThread()
        {
            BroadphaseThread = new Thread(ThreadMethod);
            DoRun = true;
            lock (OwnersDict)
            {
                OwnersDict.Clear();
            }
            WorldTimeLast = 0;
            SweepAndPrune();
            BroadphaseThread.Start();
        }

        internal static void StopThread()
        {
            DoRun = false;
            if (BroadphaseThread != null)
            {
                BroadphaseThread.Join();
            }
        }

        internal static void SweepAndPrune()
        {
            bool vsgObject = false;
            if (KWEngine.CurrentWorld._viewSpaceGameObject != null && KWEngine.CurrentWorld._viewSpaceGameObject.IsCollisionObject)
            {
                vsgObject = true;
            }

            List<GameObjectHitbox> axisList = new List<GameObjectHitbox>();
            lock (KWEngine.CurrentWorld._gameObjectHitboxes)
            {
                axisList = new List<GameObjectHitbox>(KWEngine.CurrentWorld._gameObjectHitboxes);

                if (vsgObject)
                {
                    lock (KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._colliderModel._hitboxes)
                    {
                        foreach (GameObjectHitbox vsgHitbox in KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._colliderModel._hitboxes)
                        {
                            if (vsgHitbox.IsActive)
                            {
                                axisList.Add(vsgHitbox);
                            }
                        }
                    }
                }
            }

            try
            {
                axisList.Sort(
                    (x, y) =>
                    {
                        if (_sweepTestAxisIndex == 0)
                        {
                            return x._left < y._left ? -1 : 1;
                        }
                        else if (_sweepTestAxisIndex == 1)
                        {
                            return x._low < y._low ? -1 : 1;
                        }
                        else
                        {
                            return x._back < y._back ? -1 : 1;
                        }
                    }
                );
            }
            catch(Exception ex)
            {
                Debug.WriteLine("[Sweep&Prune] Sorting failed due to bad comparer value (" + ex.Message + ")");
                return;
            }
                
            Vector3 centerSum = new Vector3(0, 0, 0);
            Vector3 centerSqSum = new Vector3(0, 0, 0);
            lock (OwnersDict)
            {
                OwnersDict.Clear();

                for (int i = 0; i < axisList.Count(); i++)
                {
                    if (OwnersDict.ContainsKey(axisList[i].Owner) == false)
                    {
                        OwnersDict.Add(axisList[i].Owner, new List<GameObjectHitbox>());
                    }
                    
                    Vector3 currentCenter = axisList[i]._center;
                    centerSum += currentCenter;
                    centerSqSum += (currentCenter * currentCenter);
                    for (int j = i + 1; j < axisList.Count; j++)
                    {
                        if (axisList[j].Owner.IsCollisionObject == false)
                        {
                            continue;
                        }
                        float fromJExtendsX =
                            _sweepTestAxisIndex == 0 ? axisList[j]._left - KWEngine.SweepAndPruneTolerance :
                            _sweepTestAxisIndex == 1 ? axisList[j]._low - KWEngine.SweepAndPruneTolerance :
                                                       axisList[j]._back - KWEngine.SweepAndPruneTolerance;  // side of neighbor 
                        float fromIExtendsY =
                            _sweepTestAxisIndex == 0 ? axisList[i]._right + KWEngine.SweepAndPruneTolerance :
                            _sweepTestAxisIndex == 1 ? axisList[i]._high + KWEngine.SweepAndPruneTolerance :
                                                       axisList[i]._front + KWEngine.SweepAndPruneTolerance; // right side of object
                        if (fromJExtendsX > fromIExtendsY)
                        {
                            break;
                        }

                        // check for second main axis:
                        if (_sweepTestAxisIndex == 0) // wenn x main ist, dann prüfe y und z auch
                        {
                            if (axisList[j]._low - KWEngine.SweepAndPruneTolerance > axisList[i]._high + KWEngine.SweepAndPruneTolerance
                                || axisList[j]._high + KWEngine.SweepAndPruneTolerance < axisList[i]._low - KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                            if (axisList[j]._front + KWEngine.SweepAndPruneTolerance < axisList[i]._back - KWEngine.SweepAndPruneTolerance
                                || axisList[j]._back - KWEngine.SweepAndPruneTolerance > axisList[i]._front + KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                        }
                        else if (_sweepTestAxisIndex == 1) // y
                        {
                            if (axisList[j]._left - KWEngine.SweepAndPruneTolerance > axisList[i]._right + KWEngine.SweepAndPruneTolerance
                                || axisList[j]._right + KWEngine.SweepAndPruneTolerance < axisList[i]._left - KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                            if (axisList[j]._front + KWEngine.SweepAndPruneTolerance < axisList[i]._back - KWEngine.SweepAndPruneTolerance
                                || axisList[j]._back - KWEngine.SweepAndPruneTolerance > axisList[i]._front + KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (axisList[j]._low - KWEngine.SweepAndPruneTolerance > axisList[i]._high + KWEngine.SweepAndPruneTolerance
                                || axisList[j]._high + KWEngine.SweepAndPruneTolerance < axisList[i]._low - KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                            if (axisList[j]._left - KWEngine.SweepAndPruneTolerance > axisList[i]._right + KWEngine.SweepAndPruneTolerance
                                || axisList[j]._right + KWEngine.SweepAndPruneTolerance < axisList[i]._left - KWEngine.SweepAndPruneTolerance)
                            {
                                continue;
                            }
                        }


                        OwnersDict[axisList[i].Owner].Add(axisList[j]);
                        if (OwnersDict.ContainsKey(axisList[j].Owner))
                        {
                            OwnersDict[axisList[j].Owner].Add(axisList[i]);
                        }
                        else
                        {
                            OwnersDict.Add(axisList[j].Owner, new List<GameObjectHitbox>());
                            OwnersDict[axisList[j].Owner].Add(axisList[i]);
                        }

                    }
                }
            }


            centerSum /= axisList.Count;
            centerSqSum /= axisList.Count;
            Vector3 variance = centerSqSum - (centerSum * centerSum);
            float maxVar = Math.Abs(variance.X);
            _sweepTestAxisIndex = 0;
            if (Math.Abs(variance.Y) > maxVar)
            {
                maxVar = Math.Abs(variance.Y);
                _sweepTestAxisIndex = 1;
            }
            if (Math.Abs(variance.Z) > maxVar)
            {
                maxVar = Math.Abs(variance.Z);
                _sweepTestAxisIndex = 2;
            }
        }
    }
}
