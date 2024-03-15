using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldPlaneColliderTest
{
    public class Player : GameObject
    {
        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.01f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+0.01f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, 0, +0.01f);
            }

            List<Intersection> intersections = GetIntersections();
            foreach (Intersection intersection in intersections)
            {
                MoveOffset(intersection.MTV);
            }
        }
    }
}
