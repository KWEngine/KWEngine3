using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldHitboxConcurrencyTest;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldHitboxConcurrencyTest : World
    {
        public override void Act()
        {
            int r = HelperRandom.GetRandomNumber(1, 30);
            if(r == 1)
            {
                Enemy e = new Enemy();
                e.SetCollisionType(ColliderType.ConvexHull);
                //e.SetModel("EnemyShip");
                e.SetColor(1, 0, 0);
                e.SetPosition(HelperRandom.GetRandomNumber(-10f, +10f), 0, -8.5f);
                AddGameObject(e);
            }
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("PlayerShip", "./Models/Tutorial/PowerUp.gltf");
            KWEngine.LoadModel("EnemyShip", "./Models/ThirdPersonView/uBot.fbx");

            SetCameraFOV(10);
            SetCameraPosition(0, 100, 0.0001f);
            SetCameraTarget(0, 0, 0);

            Player p = new Player();
            p.SetModel("PlayerShip");
            p.SetCollisionType(ColliderType.ConvexHull);
            p.SetPosition(0, 0, 6);
            p.SetColor(0, 1, 1);
            p.SetRotation(0, 180, 0);
            AddGameObject(p);
        }
    }
}
