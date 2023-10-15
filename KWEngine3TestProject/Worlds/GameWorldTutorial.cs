using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldTutorial;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTutorial : World
    {
        private Player _p;
        
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Hazard_Saw", @"./Models/Tutorial/Hazard_Saw.gltf");
            KWEngine.LoadModel("Thunder", @"./Models/Tutorial/Thunder.gltf");
            KWEngine.LoadModel("Toon", @"./Models/Tutorial/Toon.glb");
            KWEngine.LoadModel("PowerUp", @"./Models/Tutorial/PowerUp.gltf");

            SetCameraPosition(0, 10, 110);
            SetCameraTarget(0, 0, 95);

            Floor f = new Floor();
            f.SetModel("KWPlatform");
            f.SetTexture(@"./Textures/Rock_02_512.png", TextureType.Albedo, 0);
            f.SetTextureRepeat(2, 10, 0);
            f.SetTexture(@"./Textures/Brick_01_512.png", TextureType.Albedo, 1);
            f.SetTextureRepeat(20, 0.25f, 1);
            f.SetTexture(@"./Textures/Brick_01_512.png", TextureType.Albedo, 2);
            f.SetTextureRepeat(4, 0.25f, 2);
            f.SetScale(20, 2, 200);
            f.SetPosition(0, -1, 0);
            f.IsCollisionObject = true;
            AddGameObject(f);

            _p = new Player();
            AddGameObject(_p);
            /*
            Saw testSaw = new Saw();
            testSaw.SetPosition(-5, 1.25f, 90);
            AddGameObject(testSaw);

            PowerUp testPowerUp = new PowerUp("x5");
            testPowerUp.SetPosition(5, 0, 90);
            AddGameObject(testPowerUp);
            */

            AddWorldEvent(new WorldEvent(HelperRandom.GetRandomNumber(1f, 2f), "SpawnSaw", new SawSpawnInfo(35, "GunFaster", -6)));
            AddWorldEvent(new WorldEvent(HelperRandom.GetRandomNumber(1f, 1.1f), "SpawnSaw", new SawSpawnInfo(2, "GunSpread", 6)));
            AddWorldEvent(new WorldEvent(HelperRandom.GetRandomNumber(3f, 5f), "SpawnSaw", new SawSpawnInfo(2, "GunSpread", 6)));
            AddWorldEvent(new WorldEvent(HelperRandom.GetRandomNumber(5f, 7f), "SpawnSaw", new SawSpawnInfo(2, "GunSpread", 6)));
        }

        protected override void OnWorldEvent(WorldEvent e)
        {
            if(e.Description == "SpawnSaw")
            {
                SawSpawnInfo info = e.Tag as SawSpawnInfo;
                Saw testSaw = new Saw(info.Health, info.UpgradeType);
                testSaw.SetPosition(info.X, 1.25f, _p.Position.Z - 50);
                AddGameObject(testSaw);
            }
        }

        public Player GetPlayer()
        {
            return _p;
        }
    }
}
