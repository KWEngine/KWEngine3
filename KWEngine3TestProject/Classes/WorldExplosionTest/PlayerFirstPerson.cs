using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.Helper;
using KWEngine3.GameObjects;

namespace KWEngine3TestProject.Classes.WorldExplosionTest
{
    public class PlayerFirstPerson : GameObject
    {
        public override void Act()
        {
            CurrentWorld.AddCameraRotationFromMouseDelta();

            if (Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.01f, 0);

            int move = 0;
            int strafe = 0;
            if (Keyboard.IsKeyDown(Keys.W))
                move++;
            if (Keyboard.IsKeyDown(Keys.S))
                move--;
            if (Keyboard.IsKeyDown(Keys.A))
                strafe--;
            if (Keyboard.IsKeyDown(Keys.D))
                strafe++;
            

            if (move != 0 || strafe != 0)
            {
                MoveAndStrafeAlongCamera(move, strafe, 0.03f);
            }
            CurrentWorld.UpdateCameraPositionForFirstPersonView(Position);
        }
    }
}
