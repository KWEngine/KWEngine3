using KWEngine3;
using KWEngine3.Audio;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldAudioStutterTest : World
    {
        private float _camX = 0f;
        private int _countDown = -1;
        private HUDObjectText _ho;

        public override void Act()
        {
            int newCountdown = 10 - ((int)WorldTime % 10);
            if(newCountdown != _countDown)
            {
                _countDown = newCountdown;
                if(_countDown < 0)
                {
                    _countDown = 10;
                }
                Audio.PlaySound("./SFX/" + _countDown.ToString().PadLeft(2, '0') + ".ogg");
                _ho.SetText(_countDown.ToString().PadLeft(2, '0'));
            }

            _camX = MathF.Cos(WorldTime) * 5;
            SetCameraPosition(_camX, 1, 10);
            SetCameraTarget(_camX, 1, 0);
        }

        public override void Prepare()
        {
            SetColorAmbient(1f, 1f, 1f);

            Audio.PreloadSound("./SFX/00.ogg");
            Audio.PreloadSound("./SFX/01.ogg");
            Audio.PreloadSound("./SFX/02.ogg");
            Audio.PreloadSound("./SFX/03.ogg");
            Audio.PreloadSound("./SFX/04.ogg");
            Audio.PreloadSound("./SFX/05.ogg");
            Audio.PreloadSound("./SFX/06.ogg");
            Audio.PreloadSound("./SFX/07.ogg");
            Audio.PreloadSound("./SFX/08.ogg");
            Audio.PreloadSound("./SFX/09.ogg");
            Audio.PreloadSound("./SFX/10.ogg");

            Immovable wall = new Immovable();
            wall.Name = "Wall";
            wall.SetScale(20, 20, 1);
            wall.SetPosition(0, 0, -0.5f);
            wall.SetTexture("./Textures/Brick_01_512.png");
            wall.SetTextureRepeat(5, 5);
            AddGameObject(wall);

            _ho = new HUDObjectText("10");
            _ho.SetFont(FontFace.NovaMono);
            _ho.SetScale(256);
            _ho.SetColorOutline(0.4f, 0.4f, 0.4f, 0.5f);
            _ho.SetTextAlignment(TextAlignMode.Center);
            _ho.CenterOnScreen();
            AddHUDObject(_ho);
        }
    }
}
