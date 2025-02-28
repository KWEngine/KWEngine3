using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldCustomFontTest : World
    {
        private TextObject t1;
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.Escape))
            {
                Window.SetWorld(new GameWorldEmpty());
                return;
            }

            if(Keyboard.IsKeyDown(Keys.Left))
            {
                SetCameraPosition(CameraPosition.X - 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X - 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if(Keyboard.IsKeyDown(Keys.Right))
            {
                SetCameraPosition(CameraPosition.X + 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X + 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Up))
            {
                SetCameraPosition(CameraPosition.X, CameraPosition.Y + 0.05f, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X, CameraTarget.Y + 0.05f, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Down))
            {
                SetCameraPosition(CameraPosition.X, CameraPosition.Y - 0.05f, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X, CameraTarget.Y - 0.05f, CameraTarget.Z);
            }
        }

        public override void Prepare()
        {
            SetCameraFOV(90);
            SetCameraPosition(5.0f, 5.0f, 2.5f);
            KWEngine.LoadFont("Playwrite", "./Fonts/Playwrite.ttf");
            
            t1 = new TextObject("The QUICK brown fox jumped over the lazy dog.");
            t1.Name = "Test";
            t1.SetFont("Playwrite");
            t1.SetScale(1.0f);
            t1.SetCharacterDistanceFactor(1f);
            AddTextObject(t1);
           
        }
    }
}
