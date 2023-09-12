using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldHUDTest : World
    {
        private HUDObjectText _h1;
        private HUDObjectText _h2;
        private HUDObjectText _h3;

        public override void Act()
        {
            if(_h1.IsMouseCursorOnMe())
            {
                _h1.SetColorGlowIntensity(0.9f);
            }
            else
            {
                _h1.SetColorGlowIntensity(0.0f);
            }
        }

        public override void Prepare()
        {
            _h1 = new HUDObjectText("Hello World");
            _h1.SetPositionToWindowCenter();
            _h1.SetCharacterDistanceFactor(1f);
            _h1.SetTextAlignment(TextAlignMode.Center);
            _h1.SetColorGlow(1, 0, 1);
            _h1.SetScale(24, 24);
            AddHUDObject(_h1);

        }
    }
}
