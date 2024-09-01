using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTerrainLOD
{
    internal class Player : GameObject
    {
        public override void Act()
        {

            CurrentWorld.AddCameraRotationFromMouseDelta();

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
                MoveAndStrafeAlongCameraXZ(move, strafe, 0.025f);
            }

            if(Keyboard.IsKeyDown(Keys.Q))
            {
                MoveOffset(0, -0.025f, 0);
            }
            if (Keyboard.IsKeyDown(Keys.E))
            {
                MoveOffset(0, +0.025f, 0);
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0.5f);
        }
    }
}
