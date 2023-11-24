using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldCollisionTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.R))
            {
                SetRotation(0, 0, 0);
            }

            if(Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0.01f, 0);
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.01f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(0.01f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, -0.01f, 0);
            }
            if (Keyboard.IsKeyDown(Keys.Q))
            {
                MoveOffset(0, 0, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.E))
            {
                MoveOffset(0, 0, +0.01f);
            }
            /*
            Intersection i = GetIntersection();
            if (i != null)
            {
                this.SetRotationToMatchSurfaceNormal(i.ColliderSurfaceNormal);
            }
            */
            Console.WriteLine(HelperIntersection.GetCollisionCandidateNamesFor(this));


            List<Intersection> intersections = GetIntersections();
            foreach (Intersection intersection in intersections)
            {
                Console.WriteLine("collision: " + intersection.Object.Name);
                MoveOffset(intersection.MTV);
            }

            CurrentWorld.SetCameraPosition(Position + new Vector3(5, 10, 10));
            CurrentWorld.SetCameraTarget(Center);
        }
    }
}
