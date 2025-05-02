using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
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
            KWEngine.LoadModel("ExoGray", "./Models/AnimationTest/ExoGray_Idle_Uniform24.fbx");
            KWEngine.LoadAnimationIntoModel("ExoGray", "./Models/AnimationTest/ExoGrayAnim_GetHit_Uniform24.fbx");
            KWEngine.LoadCollider("Plane2m", "./Models/OBJTest/UprightPlaneCollider2m.obj", ColliderType.ConvexHull);

            //SetCameraPosition(3, 3, 3);
            SetCameraPosition(0, 0.75f, 5);
            SetCameraTarget(0, 0.75f, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            PlayerAnimated test = new PlayerAnimated();
            test.SetModel("ExoGray");
            
            test.SetAnimationID(0);
            test.SetAnimationPercentage(0.0f);
            test.SetHitboxToCapsule(Vector3.Zero);
            test.SetScale(0.01f);
            AddGameObject(test);

            LightObject sun = new LightObjectSun(ShadowQuality.High, SunShadowType.Default);
            sun.SetPosition(-50, 25, 50);
            sun.SetFOV(30);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);
        }
    }
}
