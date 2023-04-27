using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlatformerPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            f.SetScale(66, 1, 66);
            f.SetPosition(0, -0.5f, 0);
            f.IsCollisionObject = true;
            f.IsShadowCaster = true;
            f.SetTexture("./Textures/Grass_02_512.png");
            f.SetTextureRepeat(4, 4);
            AddGameObject(f);

            for(float x = -24; x < 40; x += 16)
            {
                Obstacle fence = new Obstacle();
                fence.Name = "Fence";
                fence.SetModel("Fence_Middle16");
                fence.SetPosition(x, 0, 32);
                fence.IsCollisionObject = true;
                fence.IsShadowCaster = true;
                AddGameObject(fence);
            }

            for (float x = -24; x < 40; x += 16)
            {
                Obstacle fence = new Obstacle();
                fence.Name = "Fence";
                fence.SetModel("Fence_Middle16");
                fence.SetPosition(x, 0, -32);
                fence.IsCollisionObject = true;
                fence.IsShadowCaster = true;
                AddGameObject(fence);
            }

            for (float z = -24; z < 40; z += 16)
            {
                Obstacle fence = new Obstacle();
                fence.Name = "Fence";
                fence.SetModel("Fence_Middle16");
                fence.SetPosition(-32, 0, z);
                fence.SetRotation(0, 90, 0);
                fence.IsCollisionObject = true;
                fence.IsShadowCaster = true;
                AddGameObject(fence);
            }

            for (float z = -24; z < 40; z += 16)
            {
                Obstacle fence = new Obstacle();
                fence.Name = "Fence";
                fence.SetModel("Fence_Middle16");
                fence.SetPosition(32, 0, z);
                fence.SetRotation(0, 90, 0);
                fence.IsCollisionObject = true;
                fence.IsShadowCaster = true;
                AddGameObject(fence);
            }

            Player p = new Player();
            p.SetModel("Toon");
            p.IsShadowCaster = true;
            p.IsCollisionObject = true;
            p.SetHitboxToCapsuleForMesh(0);
            p.SetAnimationID(0);
            AddGameObject(p);

            Weapon w = new Weapon();
            w.SetModel("Gun");
            w.Name = "Gun";
            w.SetScale(2);
            w.IsCollisionObject = true;
            w.SetPosition(15, 0.25f, 10);
            w.SetPivot(15, 0.25f, 10);
            AddGameObject(w);

            SetBackgroundSkybox("./Textures/skybox.png");
            SetBackgroundBrightnessMultiplier(1.25f);
        }
    }
}
