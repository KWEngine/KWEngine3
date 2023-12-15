using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldShadowCasterTest
{
    internal class TerrainPlayer : GameObject
    {
        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, 0, +0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.01f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+0.01f, 0, 0);
            }
            if(HelperIntersection.GetPositionOnTerrainUnderneath(this.Center, 2, out Vector3 collision))
            {
                Console.WriteLine(collision);
            }
            else
            {
                Console.WriteLine("no collision");
            }
        }
    }
}
