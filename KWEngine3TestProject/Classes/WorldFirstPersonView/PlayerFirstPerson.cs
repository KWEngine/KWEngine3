using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Classes.WorldFirstPersonView
{
    public class PlayerFirstPerson : GameObject
    {
        public override void Act()
        {
            CurrentWorld.AddCameraRotation(MouseMovement);

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
                MoveAndStrafeAlongCameraXZ(move, strafe, 0.05f);
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0.5f);
        }
    }
}
