using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldRayHitboxTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            int forward = 0;
            int strafe = 0;
            if (Keyboard.IsKeyDown(Keys.A))
                strafe -= 1;
            if (Keyboard.IsKeyDown(Keys.D))
                strafe += 1;
            if (Keyboard.IsKeyDown(Keys.W))
                forward += 1;
            if (Keyboard.IsKeyDown(Keys.S))
                forward -= 1;
            if(Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.02f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.02f, 0);

            CurrentWorld.AddCameraRotationFromMouseDelta();
            MoveAndStrafeAlongCameraXZ(forward, strafe, 0.02f);
            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0.5f);
            TurnTowardsXZ(CurrentWorld.CameraPosition + CurrentWorld.CameraLookAtVector);

            List<RayIntersectionExt> rayHits = HelperIntersection.RayTraceObjectsForViewVector(CurrentWorld.CameraPosition, CurrentWorld.CameraLookAtVector, 0, false, this, typeof(Obstacle));
            if(rayHits.Count > 0)
            {
                Console.WriteLine("HIT @ " + rayHits[0].IntersectionPoint);
            }
        }
    }
}
