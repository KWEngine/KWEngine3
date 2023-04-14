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

            List<GameObject> axisList = new List<GameObject>(KWEngine.CurrentWorld._gameObjects);
            if (vsgObject)
                axisList.Add(KWEngine.CurrentWorld._viewSpaceGameObject._gameObject);

            axisList.Sort(
                (x, y) =>
                {
                    x._collisionCandidates.Clear();
                    y._collisionCandidates.Clear();
                    if(_sweepTestAxisIndex == 0)
                    {
                        return x.LeftRightMost.X < y.LeftRightMost.X ? -1 : 1;
                    }
                    else if(_sweepTestAxisIndex == 1)
                    {
                        return x.BottomTopMost.X < y.BottomTopMost.X ? -1 : 1;
                    }
                    else
                    {
                        return x.BackFrontMost.X < y.BackFrontMost.X ? -1 : 1;
                    }
                }
            );

            /*if (_sweepTestAxisIndex == 0)
                axisList = axisList.OrderBy(x => x.LeftRightMost.X).ToList();
            else if (_sweepTestAxisIndex == 1)
                axisList = axisList.OrderBy(x => x.BottomTopMost.X).ToList();
            else if (_sweepTestAxisIndex == 2)
                axisList = axisList.OrderBy(x => x.BackFrontMost.X).ToList();
            */

            Vector3 centerSum = new Vector3(0, 0, 0);
            Vector3 centerSqSum = new Vector3(0, 0, 0);
            for (int i = 0; i < axisList.Count(); i++)
            {
                if (axisList[i].IsCollisionObject == false)
                {
                    continue;
                }

                Vector3 currentCenter = axisList[i].Center;
                centerSum += currentCenter;
                centerSqSum += (currentCenter * currentCenter);

                for (int j = i + 1; j < axisList.Count; j++)
                {
                    GameObject fromJ = axisList[j];
                    if (fromJ.IsCollisionObject == false)
                    {
                        continue;
                    }

                    GameObject fromI = axisList[i]; // i = leftmost, j = Center
                    float fromJExtendsX = fromJ.GetExtentsForAxis(_sweepTestAxisIndex).X; // left side of right neighbor 
                    float fromIExtendsY = fromI.GetExtentsForAxis(_sweepTestAxisIndex).Y; // right side of object
                    if (fromJExtendsX > fromIExtendsY)
                    {
                        break;
                    }
                    fromI._collisionCandidates.Add(fromJ);
                    fromJ._collisionCandidates.Add(fromI);
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
