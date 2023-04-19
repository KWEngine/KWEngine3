using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3TestProject.Classes.WorldMotionTest
{
    public class MovingCube : GameObject
    {
        private Vector3 _pivot;
        private float _degrees;

        public MovingCube(Vector3 pivot, float degrees)
        {
            _pivot = pivot;
            _degrees = degrees;
        }

        public override void Act()
        {
            _degrees++;
            Vector3 newPos = HelperRotation.CalculatePositionAfterRotationAroundPointOnAxis(_pivot, 2, _degrees);
            SetPosition(newPos);
        }
    }
}
