using KWEngine3;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGLTFTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(25, 25, 25);
            SetColorAmbient(0.1f, 0.1f, 0.1f);
            SetBackgroundSkybox("./Textures/skybox.dds", 0);
            SetBackgroundBrightnessMultiplier(10f);


            KWEngine.LoadModel("Sword", "./Models/GLTFTest/Sword.gltf");
            KWEngine.LoadModel("Ninja", "./Models/GLTFTest/SwordNinjaAnimated.gltf");

            Immovable i01 = new Immovable();
            i01.Name = "Sword";
            i01.SetModel("Sword");
            i01.HasTransparencyTexture = true;
            i01.SetScale(10);
            AddGameObject(i01);
        }
    }
}
