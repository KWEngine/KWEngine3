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
        private HUDObjectText _h4;
        private HUDObjectText _h5;
        private HUDObjectText _h6;
        private HUDObjectImage _hCrosshair;
        //private float _h1Distance = 1;
        //private bool _h1DistanceGrow = true;

        public override void Act()
        {
            /*
            if(_h1DistanceGrow)
            {
                _h1Distance += 0.0025f;
                _h1.SetCharacterDistanceFactor(_h1Distance);
                if(_h1Distance >= 2)
                {
                    _h1DistanceGrow = false;
                }
            }
            else
            {
                _h1Distance -= 0.0025f;
                _h1.SetCharacterDistanceFactor(_h1Distance);
                if (_h1Distance <= 0.1f)
                {
                    _h1DistanceGrow = true;
                }
            }
            */
            if(_h1.IsMouseCursorOnMe())
            {
                _h1.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h1.SetColorEmissiveIntensity(0.0f);
            }
            /*
            if (_h2.IsMouseCursorOnMe())
            {
                _h2.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h2.SetColorEmissiveIntensity(0.0f);
            }
            if (_h3.IsMouseCursorOnMe())
            {
                _h3.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h3.SetColorEmissiveIntensity(0.0f);
            }
            if (_h4.IsMouseCursorOnMe())
            {
                _h4.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h4.SetColorEmissiveIntensity(0.0f);
            }
            if (_h5.IsMouseCursorOnMe())
            {
                _h5.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h5.SetColorEmissiveIntensity(0.0f);
            }
            if (_h6.IsMouseCursorOnMe())
            {
                _h6.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _h6.SetColorEmissiveIntensity(0.0f);
            }

            */
            if (_hCrosshair.IsMouseCursorOnMe())
            {
                _hCrosshair.SetColorEmissiveIntensity(0.9f);
            }
            else
            {
                _hCrosshair.SetColorEmissiveIntensity(0.0f);
            }
        }

        public override void Prepare()
        {
            _h1 = new HUDObjectText("LeftPlus1");
            _h1.SetPosition(64, 256 + 48 * 0);
            _h1.SetCharacterDistanceFactor(1f);
            _h1.SetTextAlignment(TextAlignMode.Left);
            _h1.SetColorEmissive(1, 0, 1);
            _h1.SetColorEmissiveIntensity(0);
            _h1.SetScale(16, 16);
            AddHUDObject(_h1);
            /*
            _h2 = new HUDObjectText("LeftMinus2");
            _h2.SetPosition(768, 256 + 48 * 1);
            _h2.SetCharacterDistanceFactor(-2.0f);
            _h2.SetTextAlignment(TextAlignMode.Left);
            _h2.SetColorEmissive(1, 0, 1);
            _h2.SetColorEmissiveIntensity(0);
            _h2.SetScale(16, 24);
            AddHUDObject(_h2);

            _h3 = new HUDObjectText("CenterPlus2");
            _h3.SetPosition(Window.Center.X, 256 + 48 * 2);
            _h3.SetCharacterDistanceFactor(2f);
            _h3.SetTextAlignment(TextAlignMode.Center);
            _h3.SetColorEmissive(1, 0, 1);
            _h3.SetColorEmissiveIntensity(0);
            _h3.SetScale(24, 16);
            AddHUDObject(_h3);

            _h4 = new HUDObjectText("CenterMinus3");
            _h4.SetPosition(Window.Center.X, 256 + 48 *3);
            _h4.SetCharacterDistanceFactor(-3f);
            _h4.SetTextAlignment(TextAlignMode.Center);
            _h4.SetColorEmissive(1, 0, 1);
            _h4.SetColorEmissiveIntensity(0);
            _h4.SetScale(12, 12);
            AddHUDObject(_h4);

            _h5 = new HUDObjectText("RightPlus2");
            _h5.SetPosition(Window.Center.X, 256 + 48 * 4);
            _h5.SetCharacterDistanceFactor(2f);
            _h5.SetTextAlignment(TextAlignMode.Right);
            _h5.SetColorEmissive(1, 0, 1);
            _h5.SetColorEmissiveIntensity(0);
            _h5.SetScale(10, 10);
            AddHUDObject(_h5);

            _h6 = new HUDObjectText("RightMinus3");
            _h6.SetPosition(Window.Center.X - 256, 256 + 48 * 5);
            _h6.SetCharacterDistanceFactor(-3.0f);
            _h6.SetTextAlignment(TextAlignMode.Right);
            _h6.SetColorEmissive(1, 0, 1);
            _h6.SetColorEmissiveIntensity(0);
            _h6.SetScale(64, 64);
            AddHUDObject(_h6);
            */
            _hCrosshair = new HUDObjectImage();
            _hCrosshair.SetPosition(768, 196);
            _hCrosshair.SetTexture(@".\Textures\fx_boom.png");
            _hCrosshair.SetScale(64, 64);
            _hCrosshair.SetColorEmissive(1, 1, 0);
            _hCrosshair.SetColorEmissiveIntensity(0);
            AddHUDObject(_hCrosshair);

        }
    }
}
