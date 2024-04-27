using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldThirdPersonView;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldThirdPersonView : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("UBot", @".\Models\ThirdPersonView\ubot.fbx");

            SetColorAmbient(0.25f, 0.25f, 0.25f);
            SetBackgroundBrightnessMultiplier(4);
            SetBackgroundSkybox(@".\Textures\skybox.dds");

            Floor f01 = new Floor();
            f01.Name = "Floor";
            f01.SetModel("KWCube");
            f01.SetScale(15, 1f, 15);
            f01.SetPosition(0, -0.5f, 0);
            f01.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            f01.IsCollisionObject = true;
            AddGameObject(f01);

            PlayerThirdPerson p01 = new PlayerThirdPerson();
            p01.SetModel("UBot");
            p01.SetPosition(0, 0, 0);
            p01.SetScale(1);
            p01.IsShadowCaster = true;
            p01.IsCollisionObject = true;
            p01.SetRotation(0, 180, 0);
            p01.SetHitboxToCapsule(0.67f, 1f, 0.5f, new Vector3(0, 0.5f, 0));
            AddGameObject(p01);
            
            

            Immovable box01 = new Immovable();
            box01.SetScale(2);
            box01.SetPosition(4, 1, -4);
            box01.IsCollisionObject = true;
            box01.IsShadowCaster = true;
            AddGameObject(box01);
            /*
            HUDObject crosshair = new HUDObject(HUDObjectType.Image, Window.Width / 2, Window.Height / 2);
            crosshair.Name = "Crosshair";
            crosshair.SetTexture(@".\textures\crosshair.dds");
            AddHUDObject(crosshair);
            */
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetFOV(32);
            sun.SetNearFar(20, 100);
            sun.SetPosition(50, 15, 20);
            sun.SetColor(1, 0.9f, 0.8f, 2.5f);
            AddLightObject(sun);

            MouseCursorGrab();
        }
    }
}
