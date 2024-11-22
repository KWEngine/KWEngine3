using KWEngine3.GameObjects;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldFlowFieldTest
{
    internal class Impassable : GameObject
    {
        private bool _right = HelperRandom.GetRandomNumber(0, 1) > 0;
        private bool _move = false;
        public override void Act()
        {
            if (_move)
            {


                if (_right)
                {
                    MoveOffset(0.005f, 0, 0);
                    if (Position.X > 2)
                    {
                        _right = false;
                    }
                }
                else
                {
                    MoveOffset(-0.005f, 0, 0);
                    if (Position.X < -2)
                    {
                        _right = true;
                    }
                }
                FlowField f = CurrentWorld.GetFlowField();
                if (f != null)
                {
                    f.Update();
                }
            }
        }
    }
}
