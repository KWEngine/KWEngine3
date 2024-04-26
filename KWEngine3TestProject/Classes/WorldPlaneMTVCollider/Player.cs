using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPlaneMTVCollider
{
    public class Player : GameObject
    {
        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.01f, 0f, 0f);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+0.01f, 0f, 0f);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0f, 0f, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0f, 0f, +0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.Q))
            {
                MoveOffset(0f, -0.01f, 0f);
            }
            if (Keyboard.IsKeyDown(Keys.E))
            {
                MoveOffset(0f, +0.01f, 0f);
            }
            
            
            List<Intersection> it = GetIntersections(IntersectionTestMode.CheckAllHitboxTypes);
            foreach(Intersection i in it)
            {
                MoveOffset(i.MTV);
            }
            
            
        }
    }
}
