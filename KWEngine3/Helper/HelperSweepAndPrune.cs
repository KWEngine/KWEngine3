using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace KWEngine3.Helper
{
    internal static class HelperSweepAndPrune
    {
        internal static float WorldTimeLast = 0;
        internal static int _sweepTestAxisIndex = 0;
        internal const float BROADPHASECOOLDOWN = 1f / 60f;

        internal static void SweepAndPrune()
        {
            if (KWEngine.WorldTime - WorldTimeLast <= BROADPHASECOOLDOWN)
            {
                return;
            }

            bool vsgObject = false;
            if (KWEngine.CurrentWorld._viewSpaceGameObject != null && KWEngine.CurrentWorld._viewSpaceGameObject.IsCollisionObject)
            {
                vsgObject = true;
            }

            List<GameObjectHitbox> axisList = new List<GameObjectHitbox>();
            lock (KWEngine.CurrentWorld._gameObjectHitboxes)
            {
                 axisList = new List<GameObjectHitbox>(KWEngine.CurrentWorld._gameObjectHitboxes);
            }
            if (vsgObject)
            {
                foreach (GameObjectHitbox vsgHitbox in KWEngine.CurrentWorld._viewSpaceGameObject._gameObject._hitboxes)
                {
                    if (vsgHitbox.IsActive)
                    {
                        axisList.Add(vsgHitbox);
                    }
                }
            }

            axisList.Sort(
                (x, y) =>
                {
                    x.Owner._collisionCandidates.Clear();
                    y.Owner._collisionCandidates.Clear();
                    if (_sweepTestAxisIndex == 0)
                    {
                        return x._left < y._left ? -1 : 1;
                    }
                    else if(_sweepTestAxisIndex == 1)
                    {
                        return x._low < y._low ? -1 : 1;
                    }
                    else
                    {
                        return x._back < y._back ? -1 : 1;
                    }
                }
            );

            Vector3 centerSum = new Vector3(0, 0, 0);
            Vector3 centerSqSum = new Vector3(0, 0, 0);
            for (int i = 0; i < axisList.Count(); i++)
            {
                if (axisList[i].Owner.IsCollisionObject == false)
                {
                    continue;
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
                        _sweepTestAxisIndex == 1 ? axisList[j]._low  - KWEngine.SweepAndPruneTolerance : 
                                                   axisList[j]._back - KWEngine.SweepAndPruneTolerance;  // side of neighbor 
                    float fromIExtendsY = 
                        _sweepTestAxisIndex == 0 ? axisList[i]._right + KWEngine.SweepAndPruneTolerance : 
                        _sweepTestAxisIndex == 1 ? axisList[i]._high  + KWEngine.SweepAndPruneTolerance : 
                                                   axisList[i]._front + KWEngine.SweepAndPruneTolerance; // right side of object
                    if (fromJExtendsX > fromIExtendsY)
                    {
                        break;
                    }

                    // check radii:
                    float distance = (axisList[i]._center - axisList[j]._center).LengthFast - (KWEngine.SweepAndPruneTolerance * 2);
                    if(distance > axisList[i]._fullRadius + axisList[j]._fullRadius)
                    {
                        continue;
                    }
                    
                    axisList[i].Owner._collisionCandidates.Add(axisList[j]);
                    axisList[j].Owner._collisionCandidates.Add(axisList[i]);
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

            WorldTimeLast = KWEngine.WorldTime;
        }
    }
}
