using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class Sphere : GameObject
    {
        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.W))
                MoveOffset(0, 0, -0.01f);
            if (Keyboard.IsKeyDown(Keys.S))
                MoveOffset(0, 0, +0.01f);
            if (Keyboard.IsKeyDown(Keys.A))
                MoveOffset(-0.01f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.D))
                MoveOffset(+0.01f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.01f, 0);

            List<IntersectionTerrain> intersections = GetIntersectionsWithTerrain();
            foreach(IntersectionTerrain it in intersections)
            {
                MoveOffset(it.MTV);
            }

            CurrentWorld.SetCameraPosition(this.Center + new Vector3(0, 10, 10));
            CurrentWorld.SetCameraTarget(this.Center);
        }
    }
}
