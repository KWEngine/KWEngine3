using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldAttachments
{
    public class PlayerAnimated : GameObject
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
