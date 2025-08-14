using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFadeTest : World
    {
        private float _fadeFactor = 1.0f;
        public override void Act()
        {
            if (_fadeFactor > 0)
            {
                _fadeFactor -= 0.005f;
                SetFadeFactor(_fadeFactor);
            }
            else
            {
                Window.SetWorld(new GameWorldFadeTest2());
            }

        }

        public override void Prepare()
        {
            SetFadeColor(1f, 0f, 1f);
            SetFadeFactor(1f);
            HUDObjectText test = new HUDObjectText("World #1");
            test.CenterOnScreen();
            AddHUDObject(test);
        }
    }
}
