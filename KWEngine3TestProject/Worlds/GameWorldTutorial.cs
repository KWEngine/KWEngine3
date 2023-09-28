using KWEngine3;
using KWEngine3TestProject.Classes.WorldTutorial;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTutorial : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Hazard_Saw", @"./Models/Tutorial/Hazard_Saw.gltf");
            KWEngine.LoadModel("Thunder", @"./Models/Tutorial/Thunder.gltf");
            KWEngine.LoadModel("Toon", @"./Models/Tutorial/Toon.glb");

            SetCameraPosition(0, 10, 110);
            SetCameraTarget(0, 0, 95);

            Floor f = new Floor();
            f.SetModel("KWPlatform");
            f.SetTexture(@"./Textures/Rock_01_512.png", TextureType.Albedo, 0);
            f.SetTextureRepeat(2, 10, 0);
            f.SetTexture(@"./Textures/Brick_01_512.png", TextureType.Albedo, 1);
            f.SetTextureRepeat(20, 0.25f, 1);
            f.SetTexture(@"./Textures/Brick_01_512.png", TextureType.Albedo, 2);
            f.SetTextureRepeat(4, 0.25f, 2);
            f.SetScale(20, 2, 200);
            f.SetPosition(0, -1, 0);
            f.IsCollisionObject = true;
            AddGameObject(f);

            Player p = new Player();
            AddGameObject(p);

            Saw testSaw = new Saw();
            testSaw.SetPosition(-5, 1.25f, 90);
            AddGameObject(testSaw);
        }
    }
}
