using KWEngine3.GameObjects;
using OpenTK;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldLightAndShadow
{
    public class PlayerNightshade : GameObject
    {
        public override void Act()
        {
            if (IsAnimated)
            {
                SetAnimationPercentageAdvance(0.001f);
            }
        }
    }
}
