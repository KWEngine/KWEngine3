using KWEngine3.GameObjects;

namespace KWEngine3TestProject.Classes.WorldLightAndShadow
{
    public class SphereRotating : GameObject
    {
        public override void Act()
        {
            AddRotationY(0.25f);
        }
    }
}
