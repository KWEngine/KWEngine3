using KWEngine3.GameObjects;

namespace KWEngine3TestProject.Classes.WorldShadowTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if(HasAnimations)
            {
                SetAnimationID(1);
                SetAnimationPercentageAdvance(0.001f);
            }
        }
    }
}
