using KWEngine3;
using KWEngine3TestProject.Classes.WorldMouseMotionTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMouseMotionTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 25, 0);

            MouseCube c = new MouseCube();
            c.SetScale(5);
            AddGameObject(c);

        }
    }
}
