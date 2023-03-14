using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldThirdPersonView;
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

            Immovable f01 = new Immovable();
            f01.Name = "Floor";
            f01.SetModel("KWCube");
            f01.SetTexture(@"./textures/tiles_albedo.jpg", TextureType.Albedo);       // regular texture file
            f01.SetTexture(@"./textures/tiles_normal.jpg", TextureType.Normal);       // (optional) normal map for custom light reflections
            f01.SetTexture(@"./textures/tiles_roughness.png", TextureType.Roughness); // (optional) roughness map for specular highlights
            f01.SetTextureRepeat(3, 3);                                               // how many times the texture is tiled across the object?
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
            AddGameObject(p01);

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
