using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal static class HelperSweepAndPrune
    {
        internal static int _sweepTestAxisIndex = 0;
        internal static void SweepAndPrune()
        {
            if (KWEngine.CurrentWorld._gameObjects.Count < 2)
                return;

            List<GameObject> axisList = null;
            if (_sweepTestAxisIndex == 0)
                axisList = KWEngine.CurrentWorld._gameObjects.OrderBy(x => x.LeftRightMost.X).ToList();
            else if (_sweepTestAxisIndex == 1)
                axisList = KWEngine.CurrentWorld._gameObjects.OrderBy(x => x.BottomTopMost.X).ToList();
            else if (_sweepTestAxisIndex == 2)
                axisList = KWEngine.CurrentWorld._gameObjects.OrderBy(x => x.BackFrontMost.X).ToList();

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
