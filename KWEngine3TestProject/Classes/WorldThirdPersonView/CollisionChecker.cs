using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3TestProject.Classes.WorldThirdPersonView
{
    public struct CollisionCheckerResult
    {
        public Vector3 MTV { get; set; }
        public bool IsColliding { get; set; }
    }
    public class CollisionChecker : GameObject
    {
        public override void Act()
        {
        }
    }
}
