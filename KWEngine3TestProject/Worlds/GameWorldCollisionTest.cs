using KWEngine3;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldCollisionTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            Immovable i01 = new Immovable();
            i01.Name = "LeftMost";
            i01.IsCollisionObject = true;
            i01.SetPosition(-3, 0, 0);
            i01.SetScale(2);
            i01.SetColor(1, 0, 0);
            AddGameObject(i01);

            Immovable i02 = new Immovable();
            i02.Name = "Center";
            i02.IsCollisionObject = true;
            i02.SetPosition(-1.5f, 0, 0);
            i02.SetScale(2);
            i02.SetColor(0, 1, 0);
            AddGameObject(i02);

            Immovable i03 = new Immovable();
            i03.Name = "RightMost";
            i03.IsCollisionObject = true;
            i03.SetPosition(1, 0, 0);
            i03.SetScale(2);
            i03.SetColor(0, 0, 1);
            AddGameObject(i03);
        }
    }
}
