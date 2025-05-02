using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldAttachments;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldAttachmentTest : World
    {
        public override void Act()
        {
           
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Bot", "./Models/GLTFTest/bot.gltf");
            foreach(string bone in KWEngine.GetModelBoneNames("Bot"))
            {
                Console.WriteLine(bone);
            }

            SetCameraPosition(0, 0.75f, 5);
            SetCameraTarget(0, 0.75f, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            PlayerAnimated test = new PlayerAnimated();
            test.SetModel("Bot");
            test.SetAnimationID(0);
            test.SetAnimationPercentage(0.0f);
            //test.SetScale(0.01f);
            AddGameObject(test);

            Attachment a = new Attachment();
            a.SetColor(0, 1, 0);
            a.SetOpacity(0.5f);
            test.AttachGameObjectToBone(a, "mixamorig:LeftArm");
            HelperGameObjectAttachment.SetScaleForAttachment(a, 10);
            AddGameObject(a);

            LightObject sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(-50, 25, 50);
            sun.SetFOV(30);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);
        }
    }
}
