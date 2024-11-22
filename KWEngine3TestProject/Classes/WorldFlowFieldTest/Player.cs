using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldFlowFieldTest
{
    internal class Player : GameObject
    {
        private float _speed = 0.02f;
        private bool _autoUpdate = false;

        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-_speed, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+_speed, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0, -_speed);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, 0, +_speed);
            }

            if (Keyboard.IsKeyDown(Keys.Q))
            {
                MoveOffset(0, -_speed, 0);
            }
            if (Keyboard.IsKeyDown(Keys.E))
            {
                MoveOffset(0, +_speed, 0);
            }

            if (_autoUpdate)
            {
                FlowField f = CurrentWorld.GetFlowField();
                if (f != null)
                {
                    

                    if (f.Contains(this))
                    {
                        f.SetTarget(this.Position);
                    }
                    else
                    {
                        f.UnsetTarget();
                    }
                }
            }
        }
    }
}
