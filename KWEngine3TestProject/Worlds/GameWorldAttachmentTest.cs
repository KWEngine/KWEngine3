using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldAttachments;
using OpenTK.Graphics.ES20;
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
            /*
            KWEngine.LoadModel("Bot", "./Models/GLTFTest/bot.gltf");
            Console.WriteLine("Knochen des Modells 'Bot':");
            foreach(string bone in KWEngine.GetModelBoneNames("Bot"))
            {
                Console.WriteLine(bone);
            }
            KWEngine.AddBonePresetToModel("Bot", "Hüfte Aufwärts");
            KWEngine.AddBonePresetToModel("Bot", "Beide Beine");

            foreach (string bone in KWEngine.GetModelBoneNames("Bot"))
            {
                if (bone == "mixamorig:LeftUpLeg" || bone == "mixamorig:RightUpLeg")
                    break;

                KWEngine.AddBoneToPreset("Bot", "Hüfte Aufwärts", bone, false);
                Console.WriteLine("adding " + bone + " to 'Hüfte Aufwärts'");
                
            }

            KWEngine.AddBoneToPreset("Bot", "Beide Beine", "mixamorig:Hips", false);
            KWEngine.AddBoneToPreset("Bot", "Beide Beine", "mixamorig:LeftUpLeg", true);
            KWEngine.AddBoneToPreset("Bot", "Beide Beine", "mixamorig:RightUpLeg", true);


            // 4 = laufen, 1 = schlagen
            PlayerAnimated test = new PlayerAnimated();
            test.SetModel("Bot");
            test.SetAnimationID(1, 0, 1f, "Hüfte Aufwärts");
            test.SetAnimationID(4, 1, 1f, "Beide Beine");
            test.UpdateLast = true;
            AddGameObject(test);


            Attachment a = new Attachment();
            a.SetColor(0, 1, 0);
            a.SetOpacity(0.5f);
            test.AttachGameObjectToBone(a, "mixamorig:RightHand");
            HelperGameObjectAttachment.SetScaleForAttachment(a, 10);
            AddGameObject(a);
            */

            KWEngine.LoadModel("Brute", "./Models/Brute/brute.fbx");
            PlayerAnimated test = new PlayerAnimated();
            test.SetModel("Brute");
            test.UpdateLast = true;
            AddGameObject(test);

            SetCameraPosition(0, 0.75f, 3.5f);
            SetCameraTarget(0, 0.75f, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);
            LightObject sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(-50, 25, 50);
            sun.SetFOV(30);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);


        }
    }
}
