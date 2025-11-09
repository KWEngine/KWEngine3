using KWEngine3.GameObjects;
using KWEngine3.Model;
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
        internal static Dictionary<GameObject, List<TerrainSector>> OwnersDictTerrainSector = new Dictionary<GameObject, List<TerrainSector>>();
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
                    SweepAndPrune();
                    WorldTimeLast = KWEngine.WorldTime;
                }
                Thread.Sleep(1);
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
            lock (OwnersDictTerrainSector)
            {
                OwnersDictTerrainSector.Clear();
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
                            axisList.Add(vsgHitbox);
                        }
                    }
                }
            }

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

            // Look for triangles:
            lock (OwnersDictTerrainSector)
            {
                OwnersDictTerrainSector.Clear();

                lock (KWEngine.CurrentWorld._terrainObjects)
                {
                    foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                    {
                        if (t.IsCollisionObject == false)
                            continue;

                        lock (t)
                        {
                            TerrainObjectHitbox thb = t._hitboxes[0];
                            foreach (GameObjectHitbox hb in axisList)
                            {
                                if (hb._colliderType == ColliderType.PlaneCollider || hb.Owner.ID <= 0)
                                    continue;

                                float hbleftT = hb._left - KWEngine.SweepAndPruneTolerance;
                                float hbRightT = hb._right + KWEngine.SweepAndPruneTolerance;
                                float hbLowT = hb._low - KWEngine.SweepAndPruneTolerance;
                                float hbHighT = hb._high + KWEngine.SweepAndPruneTolerance;
                                float hbBackT = hb._back - KWEngine.SweepAndPruneTolerance;
                                float hbFrontT = hb._front + KWEngine.SweepAndPruneTolerance;
                                if (HelperIntersection.CheckAABBCollision(
                                        hbleftT, hbRightT, hbLowT, hbHighT, hbBackT, hbFrontT,
                                        thb._left, thb._right, thb._low, thb._high, thb._back, thb._front
                                    )
                                )
                                {
                                    // current game object hitbox is inside terrain hitbox
                                    if (!OwnersDictTerrainSector.ContainsKey(hb.Owner))
                                    {
                                        OwnersDictTerrainSector.Add(hb.Owner, new List<TerrainSector>());
                                    }

                                    foreach (TerrainSectorCoarseUltra sectorUltra in t._sectorMapCoarseUltra)
                                    {
                                        if (HelperIntersection.CheckAABBCollision(
                                            sectorUltra.Left  + t._stateCurrent._position.X, 
                                            sectorUltra.Right + t._stateCurrent._position.X, 
                                            sectorUltra.Back  + t._stateCurrent._position.Z, 
                                            sectorUltra.Front + t._stateCurrent._position.Z,
                                            hbleftT, hbRightT, hbBackT, hbFrontT)
                                            )
                                        {
                                            foreach (TerrainSectorCoarse sectorCoarse in sectorUltra.SectorsCoarse)
                                            {
                                                if (HelperIntersection.CheckAABBCollision(
                                                    sectorCoarse.Left, sectorCoarse.Right, sectorCoarse.Back, sectorCoarse.Front,
                                                    hbleftT, hbRightT, hbBackT, hbFrontT)
                                                    )
                                                {
                                                    foreach (TerrainSector sector in sectorCoarse.Sectors)
                                                    {
                                                        if (HelperIntersection.CheckAABBCollision(
                                                            sector.Left  + t._stateCurrent._position.X, 
                                                            sector.Right + t._stateCurrent._position.X, 
                                                            sector.Back  + t._stateCurrent._position.Z, 
                                                            sector.Front + t._stateCurrent._position.Z,
                                                            hbleftT, hbRightT, hbBackT, hbFrontT)
                                                        )
                                                        {
                                                            if (!OwnersDictTerrainSector[hb.Owner].Contains(sector))
                                                            {
                                                                OwnersDictTerrainSector[hb.Owner].Add(sector);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
