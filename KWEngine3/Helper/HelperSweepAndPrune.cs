using ImGuiNET;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal static class HelperSweepAndPrune
    {
        internal static int _sweepTestAxisIndex = 0;
        internal static void SweepAndPrune()
        {
            int objectCount = KWEngine.CurrentWorld._gameObjects.Count;
            bool vsgObject = false;
            if (KWEngine.CurrentWorld._viewSpaceGameObject != null)
            {
                objectCount++;
                vsgObject = true;
            }

            if (objectCount < 2)
                return;

            List<GameObjectHitbox> axisList = new List<GameObjectHitbox>(KWEngine.CurrentWorld._gameObjectHitboxes);
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
                    //y.Owner._collisionCandidates.Clear();
                    if(_sweepTestAxisIndex == 0)
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
                    /*
                     if (a == 1)
                        return BottomTopMost;
                    else if (a == 2)
                        return BackFrontMost;
                    else
                        return LeftRightMost;
                     */
                    float fromJExtendsX = _sweepTestAxisIndex == 0 ? axisList[j]._left : _sweepTestAxisIndex == 1 ? axisList[j]._low : axisList[j]._back;  // side of neighbor 
                    float fromIExtendsY = _sweepTestAxisIndex == 0 ? axisList[i]._right : _sweepTestAxisIndex == 1 ? axisList[i]._high : axisList[i]._front; // right side of object
                    if (fromJExtendsX > fromIExtendsY)
                    {
                        break;
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
        }
    }
}
