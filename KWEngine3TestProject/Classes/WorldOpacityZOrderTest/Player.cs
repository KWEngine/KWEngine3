using KWEngine3.GameObjects;
using KWEngine3;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldOpacityZOrderTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.01f, 0.0f, 0.0f);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+0.01f, 0.0f, 0.0f);
            }
            if(Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0.0f, 0.0f, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0.0f, 0.0f, +0.01f);
            }

            CurrentWorld.SetCameraPosition(Center + new Vector3(0.0f, 0.0f, 10.0f));
            CurrentWorld.SetCameraTarget(Center);
        }
    }
}
