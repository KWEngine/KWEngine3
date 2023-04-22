using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3TestProject.Classes.WorldGLTFTest
{
    public class GLTFRotationObject : GameObject
    {
        private float _degrees = 0;
        public override void Act()
        {
            SetPosition(HelperRotation.CalculatePositionAfterRotationAroundPointOnAxis(Vector3.Zero, 5, _degrees, new Vector3(-0.707f, 0f, -0.707f)));
            _degrees = (_degrees + 1) % 360f;

        }
    }
}
