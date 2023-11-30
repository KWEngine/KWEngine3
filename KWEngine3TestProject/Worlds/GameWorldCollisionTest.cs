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
        private List<BroadphaseTest> bpts = new List<BroadphaseTest>();
        public override void Act()
        {
            int r = HelperRandom.GetRandomNumber(1, 1000);
            if (r <= 100)
            {
                //Console.WriteLine("Removing and creating object...");
                RemoveGameObject(bpts[0]);
                bpts.RemoveAt(0);

                Spawn(10000 + HelperRandom.GetRandomNumber(0, 10000));
            }

        }

        private void Spawn(int i)
        {
            BroadphaseTest b = new BroadphaseTest();
            b.IsCollisionObject = true;
            b.Name = "BPT #" + i;
            b.SetPosition(
                HelperRandom.GetRandomNumber(-25, 25),
                HelperRandom.GetRandomNumber(-25, 25),
                HelperRandom.GetRandomNumber(-25, 25)
                );
            bpts.Add(b);
            AddGameObject(b);
        }

        public override void Prepare()
        {
            SetCameraFOV(90);
            SetCameraPosition(100, 100, 100);
            SetCameraTarget(0, 0, 0);

            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.IsCollisionObject = true;
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 2, 0);
            AddGameObject(p1);

            for(int i = 0; i < 1000; i++)
            {
                Spawn(i);
            }
        }
    }
}
