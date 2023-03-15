using KWEngine3;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFramebufferDepthTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            Immovable cube1 = new Immovable();
            cube1.SetPosition(0, 0, 2);
            cube1.SetColor(1, 1, 0);
            AddGameObject(cube1);

            Immovable cube2 = new Immovable();
            cube2.SetPosition(1, 0, -2);
            cube2.SetColor(0, 1, 1);
            cube2.SetOpacity(0.75f);
            AddGameObject(cube2);

            Immovable cube3 = new Immovable();
            cube3.SetPosition(-1, 0, 2);
            cube3.SetColor(1, 0, 1);
            AddGameObject(cube3);

            Immovable cube4 = new Immovable();
            cube4.SetPosition(3, 0, -2);
            cube4.SetColor(1, 1, 1);
            AddGameObject(cube4);
        }
    }
}
