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
            KWEngine.LoadModel("ExoGray", "./Models/AnimationTest/ExoGray.fbx");
            KWEngine.LoadAnimationIntoModel("ExoGray", "./Models/AnimationTest/ExoGrayStandUp.fbx");

            SetCameraPosition(15, 15, 15);
            SetCameraPosition(0, 1, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            Immovable test = new Immovable();
            test.SetModel("ExoGray");
            test.SetScale(0.01f);
            AddGameObject(test);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-50, 25, 50);
            sun.SetFOV(30);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);
        }
    }
}
