using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlatformerPack;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    public class GameWorldPlatformerPack : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            DirectoryInfo di = new DirectoryInfo("./Models/PlatformerPack");
            foreach(FileInfo fi in di.GetFiles())
            {
                if(HelperGeneral.IsModelFile(fi.Name))
                    KWEngine.LoadModel(fi.Name.Substring(0, fi.Name.LastIndexOf('.')), "./Models/PlatformerPack/" + fi.Name);
            }


            SetCameraPosition(0, 10, 25);
            SetCameraTarget(0, 0, 0);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sunlight";
            sun.SetPosition(-50, 50, 50);
            sun.SetFOV(100);
            sun.SetNearFar(20, 200);
            sun.SetColor(1, 0.9f, 0.8f, 2.5f);
            AddLightObject(sun);

            Floor f = new Floor();
            f.Name = "Floor";
            f.SetModel("KWPlatform");
            f.SetScale(66, 8, 66);
            f.SetPosition(0, -4f, 0);
            f.IsCollisionObject = true;
            f.IsShadowCaster = true;
            f.SetTexture("./Textures/Grass_02_512.png", TextureType.Albedo, 0);
            f.SetTexture("./Textures/Grass_01_512.png", TextureType.Albedo, 1);
            f.SetTexture("./Textures/Grass_01_512.png", TextureType.Albedo, 2);
            f.SetTextureRepeat(4, 4, 0);
            f.SetTextureRepeat(8, 1, 1);
            f.SetTextureRepeat(8, 1, 2);
            AddGameObject(f);

            Obstacle fenceFront = new Obstacle();
            fenceFront.Name = "FenceFront";
            fenceFront.SetModel("Fence_Middle64");
            fenceFront.SetPosition(0, 0, 32);
            fenceFront.IsCollisionObject = true;
            fenceFront.IsShadowCaster = true;
            AddGameObject(fenceFront);

            Obstacle fenceBack = new Obstacle();
            fenceBack.Name = "FenceBack";
            fenceBack.SetModel("Fence_Middle64");
            fenceBack.SetPosition(0, 0, -32);
            fenceBack.IsCollisionObject = true;
            fenceBack.IsShadowCaster = true;
            AddGameObject(fenceBack);


            Obstacle fenceLeft = new Obstacle();
            fenceLeft.Name = "FenceLeft";
            fenceLeft.SetModel("Fence_Middle64");
            fenceLeft.SetPosition(-32, 0, 0);
            fenceLeft.SetRotation(0, 90, 0);
            fenceLeft.SetHitboxScale(1, 10, 1);
            fenceLeft.IsCollisionObject = true;
            fenceLeft.IsShadowCaster = true;
            AddGameObject(fenceLeft);


            Obstacle fenceRight = new Obstacle();
            fenceRight.Name = "FenceRight";
            fenceRight.SetModel("Fence_Middle64");
            fenceRight.SetPosition(32, 0, 0);
            fenceRight.SetRotation(0, 90, 0);
            fenceRight.SetHitboxScale(1, 10, 1);
            fenceRight.IsCollisionObject = true;
            fenceRight.IsShadowCaster = true;
            AddGameObject(fenceRight);

            Obstacle ramp01 = new Obstacle();
            ramp01.SetModel("Ramp01");
            ramp01.SetPosition(10, 0, -10);
            ramp01.SetScale(4);
            ramp01.IsCollisionObject = true;
            ramp01.IsShadowCaster = true;
            AddGameObject(ramp01);

            Obstacle plateau01 = new Obstacle();
            plateau01.SetPosition(0, 2, -19);
            plateau01.SetScale(30, 4, 10);
            plateau01.IsCollisionObject = true;
            plateau01.IsShadowCaster = true;
            plateau01.SetColor(0.7f, 0.5f, 0.275f);
            AddGameObject(plateau01);

            PlayerPlatformerPack p = new PlayerPlatformerPack();
            p.Name = "Player";
            p.SetModel("Toon");
            p.IsShadowCaster = true;
            p.IsCollisionObject = true;
            p.SetHitboxToCapsuleForMesh(0);
            p.SetAnimationID(0);
            AddGameObject(p);

            Weapon w = new Weapon();
            w.SetModel("Gun");
            w.Name = "Gun";
            w.SetScale(3);
            w.IsCollisionObject = true;
            w.SetPosition(15, 0.25f, 10);
            w.SetPivot(15, 0.25f, 10);
            AddGameObject(w);

            SetBackgroundSkybox("./Textures/skybox.png");
            SetBackgroundBrightnessMultiplier(1.25f);

            WorldEvent testEvent = new WorldEvent(4, "test", "blabla");
            AddWorldEvent(testEvent);
            
        }
    }
}
