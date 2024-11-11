using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldMapTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0.02f, 0);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, -0.02f, 0);
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.02f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(0.02f, 0, 0);
            }
        }
    }
}
