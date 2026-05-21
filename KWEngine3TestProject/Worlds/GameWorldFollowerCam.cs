using KWEngine3;
using KWEngine3TestProject.Classes.WorldFollowerCam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFollowerCam : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Robot", "./Models/GLTFTest/bot.gltf");

            SetCameraPosition(0.0f, 5.0f, 10.0f);
            SetCameraTarget(0.0f, 2.0f, 0.0f);

            Camera c = new Camera();
            AddGameObject(c);

            Player p = new Player(c);
            AddGameObject(p);

            Floor f = new Floor();
            f.SetModel("KWPlatform");
            f.IsCollisionObject = true;
            f.SetScale(50, 1, 50);
            f.SetPosition(0, -0.5f, 0);
            f.SetTexture("./Textures/Grass_01_512.png", TextureType.Albedo, 0);
            f.SetTextureRepeat(10, 10, 0);
            f.SetTexture("./Textures/Dirt_01_512.png", TextureType.Albedo, 1);
            f.SetTextureRepeat(10, 1, 1);
            f.SetTexture("./Textures/Dirt_01_512.png", TextureType.Albedo, 2);
            f.SetTextureRepeat(10, 1, 2);
            AddGameObject(f);
        }
    }
}
