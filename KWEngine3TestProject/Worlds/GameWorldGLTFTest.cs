using KWEngine3;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldGLTFTest;
using KWEngine3.Helper;
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
            SetCameraPosition(-2.5f, 2.5f, 5);
            SetCameraTarget(0, 0.5f, 0);
            SetColorAmbient(0.75f, 0.75f, 0.75f);
            SetBackgroundSkybox("./Textures/skybox.dds", 0);
            SetBackgroundBrightnessMultiplier(1.25f);

            KWEngine.LoadModel("Scooter", "./Models/GLTFTest/bot.gltf");
            //KWEngine.LoadModel("Pete", "./Models/GLTFTest/Pete_The_Builder.glb");
            //KWEngine.LoadModel("Ninja", "./Models/GLTFTest/SwordNinjaAnimated.gltf");
            //KWEngine.LoadModel("Sword", "./Models/GLTFTest/Sword.glb");
            /*
            Immovable floor = new Immovable();
            floor.SetScale(10, 1, 10);
            floor.SetPosition(0, -0.5f, 0);
            AddGameObject(floor);
            */

            GLTFPlayer i02 = new GLTFPlayer();
            i02.SetModel("Scooter");
            i02.SetAnimationID(1);
            AddGameObject(i02);

            

            /*
            Immovable i01 = new Immovable();
            i01.Name = "Sword";
            i01.SetModel("Sword");
            i01.HasTransparencyTexture = true;
            i01.SetScale(10);
            AddGameObject(i01);

            GLTFPlayer p = new GLTFPlayer();
            p.Name = "Ninja";
            p.SetModel("Ninja");
            p.SetAnimationID(7);
            p.SetScale(10);
            AddGameObject(p);
            */

            /*
            p.AttachGameObjectToBone(i01, "mixamorig:RightHand");
            HelperGameObjectAttachment.SetScaleForAttachment(i01, 50, 50, 50);
            HelperGameObjectAttachment.SetPositionOffsetForAttachment(i01, 0, 0, 0);
            HelperGameObjectAttachment.SetRotationForAttachment(i01, 0, 0, 0);

            GLTFRotationObject rotO = new GLTFRotationObject();
            AddGameObject(rotO);

            foreach (string bone in KWEngine.GetModelBoneNames("Ninja"))
            {
                Console.WriteLine( bone);
            }
            */
        }
    }
}
