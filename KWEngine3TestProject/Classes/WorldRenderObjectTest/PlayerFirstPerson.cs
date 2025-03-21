﻿using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Classes.WorldRenderObjectTest
{
    public class PlayerFirstPerson : GameObject
    {
        private float _speed = 0.02f;

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

            if (Keyboard.IsKeyDown(Keys.E))
                MoveAlongVector(CurrentWorld.CameraLookAtVectorLocalUp, _speed);
            if (Keyboard.IsKeyDown(Keys.Q))
                MoveAlongVector(-CurrentWorld.CameraLookAtVectorLocalUp, _speed);

            if (move != 0 || strafe != 0)
            {
                MoveAndStrafeAlongCamera(move, strafe, _speed);
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0f);
        }
    }
}
