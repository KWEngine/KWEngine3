using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3TestProject.Classes.WorldMouseMotionTest
{
    internal class MouseCube : GameObject
    {
        public override void Act()
        {
            Vector3 pos = HelperIntersection.GetMouseIntersectionPointOnPlane(KWEngine3.Plane.XZ, 0f);
            SetPosition(pos);
            ForceUpdate();
        }
    }
}
