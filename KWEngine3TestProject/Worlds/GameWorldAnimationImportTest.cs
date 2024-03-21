using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldAnimationImportTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("ExoGray", "./Models/AnimationTest/NinjaSwimming.fbx");
            KWEngine.LoadAnimationIntoModel("ExoGray", "./Models/AnimationTest/NinjaAnimBoxing.fbx");

            SetCameraPosition(3, 3, 3);
            SetCameraTarget(0, 0.5f, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            PlayerAnimated test = new PlayerAnimated();
            test.SetModel("ExoGray");
            test.SetScale(0.01f);
            test.SetAnimationID(0);
            test.SetAnimationPercentage(0.0f);
            AddGameObject(test);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-50, 25, 50);
            sun.SetFOV(30);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);
        }
    }
}
