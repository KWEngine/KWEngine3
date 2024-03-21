using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldCollisionTest;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldCollisionTest : World
    {
        private List<BroadphaseTest> bpts = new List<BroadphaseTest>();
        public override void Act()
        {
            /*
            int r = HelperRandom.GetRandomNumber(1, 1000);
            if (r <= 100)
            {
                RemoveGameObject(bpts[0]);
                bpts.RemoveAt(0);

                int howmany = HelperRandom.GetRandomNumber(1, 5);
                for(int i = 0; i < howmany; i++)
                    Spawn(10000 + HelperRandom.GetRandomNumber(0, 10000) + i);
            }
            */
        }

        private void Spawn(int i)
        {
            BroadphaseTest b = new BroadphaseTest();
            b.SetColliderType(ColliderType.ConvexHull);
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
            KWEngine.LoadModel("FHBTEST", "./Models/GLTFTest/fullhitboxtest.glb");
            KWEngine.LoadModel("Anim", "./Models/AnimationTest/Mannequin_StandUp.fbx");

            SetCameraFOV(90);
            SetCameraPosition(25, 25, 25);
            SetCameraTarget(0, 0, 0);

            Player p1 = new Player();
            p1.SetModel("Anim");
            p1.SetScale(0.01f);
            p1.Name = "Player #1";
            p1.SetColliderType(ColliderType.ConvexHull);
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 0.5f, 0);
            AddGameObject(p1);

            Immovable left = new Immovable();
            left.SetColliderType(ColliderType.ConvexHull);
            left.SetPosition(-5, 0.5f, 0);
            left.SetColor(0, 1, 1);
            AddGameObject(left);

            Immovable right = new Immovable();
            right.SetModel("FHBTEST");
            right.SetColliderType(ColliderType.ConvexHull);
            right.SetPosition(+5, 0, 0);
            right.SetColor(1, 0, 1);
            AddGameObject(right);

            /*
            for(int i = 0; i < 1000; i++)
            {
                Spawn(i);
            }
            */
        }
    }
}
