using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using KWEngine3.Helper;

namespace KWEngine3TestProject.Classes.WorldMultipleTerrainTest
{
    internal class Player : GameObject
    {
        private int mode = 1; // 0 = stand, 1 = fall
        private float velocityY = 0;

        public override void Act()
        {
            CurrentWorld.AddCameraRotationFromMouseDelta();


            int forward = 0;
            int strafe = 0;

            if (Keyboard.IsKeyDown(Keys.W))
                forward += 1;
            if (Keyboard.IsKeyDown(Keys.S))
                forward -= 1;
            if (Keyboard.IsKeyDown(Keys.A))
                strafe -= 1;
            if (Keyboard.IsKeyDown(Keys.D))
                strafe += 1;


            MoveAndStrafeAlongCameraXZ(forward, strafe, 0.01f);
            

            if (Keyboard.IsKeyPressed(Keys.Space) && mode == 0)
            {
                mode = 1;
                velocityY = 0.1f;
            }

            if (mode == 1)
            {
                velocityY -= 0.001f;
                velocityY = Math.Max(-0.2f, velocityY);
                MoveOffset(0, velocityY, 0);
            }
            else
            {
                
            }



            RayTerrainIntersection rti = RaytraceTerrainBelowPosition(new Vector3(Position.X, AABBLow, Position.Z));
            if (rti.IsValid)
            {
                if (mode == 1)
                {
                    if (rti.Distance < 0.05f)
                    {
                        mode = 0;
                        SetPositionY(rti.IntersectionPoint.Y, KWEngine3.PositionMode.BottomOfAABBHitbox);
                    }
                }
                else
                {
                    SetPositionY(rti.IntersectionPoint.Y, KWEngine3.PositionMode.BottomOfAABBHitbox);
                }
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Position, 0.25f);
            TurnTowardsXZ(CurrentWorld.CameraPosition + CurrentWorld.CameraLookAtVector);
        }
    }
}
