using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldCollisionTest;
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
            SetCameraFOV(90);
            SetCameraPosition(100, 100, 100);
            SetCameraTarget(0, 0, 0);


            /*
            Immovable i01 = new Immovable();
            i01.Name = "Floor";
            i01.IsCollisionObject = true;
            i01.SetPosition(0, -2, 0);
            i01.SetScale(5, 2, 2);
            i01.SetRotation(0, 0, -15);
            i01.SetColor(1, 0, 0);
            AddGameObject(i01);
            */

            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.IsCollisionObject = true;
            //p1.SetHitboxScale(1, 1, 100);
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 2, 0);
            AddGameObject(p1);

            for(int i = 0; i < 1000; i++)
            {
                BroadphaseTest b = new BroadphaseTest();
                b.IsCollisionObject = true;
                b.Name = "BPT #" + i;
                b.SetPosition(
                    HelperRandom.GetRandomNumber(-25, 25),
                    HelperRandom.GetRandomNumber(-25, 25),
                    HelperRandom.GetRandomNumber(-25, 25)
                    );
                AddGameObject(b);
            }
            /*
            for (int i = 0; i < 10; i++)
            {
                BroadphaseTest b = new BroadphaseTest();
                b.IsCollisionObject = true;
                b.Name = "BPT #" + i;
                b.SetPosition(
                    HelperRandom.GetRandomNumber(-10, 10),
                    HelperRandom.GetRandomNumber(-10, 10),
                    HelperRandom.GetRandomNumber(-10, 10)
                    );
                AddGameObject(b);
            }
            */
            /*
            for (int i = -5; i < 20; i+=5)
            {
                BroadphaseTest b = new BroadphaseTest();
                b.IsCollisionObject = true;
                b.Name = "BPT #" + i;
                b.SetPosition(0, 0, i);  
                AddGameObject(b);
            }
            */
        }
    }
}
