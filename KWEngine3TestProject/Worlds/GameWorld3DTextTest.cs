using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorld3DTextTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraFOV(90);


            Immovable i01 = new Immovable();
            i01.Name = "Floor";
            i01.IsCollisionObject = true;
            i01.SetPosition(0, -1, 0);
            i01.SetScale(5, 2, 2);
            i01.SetRotation(0, 0, 0);
            i01.SetColor(1, 0, 1);
            AddGameObject(i01);

            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.IsCollisionObject = true;
            p1.SetModel("KWCube");
            p1.SetHitboxScale(1, 1, 1);
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 2, 0);
            AddGameObject(p1);

            TextObject t1 = new TextObject("Hallo Welt.");
            AddTextObject(t1);
        }
    }
}
