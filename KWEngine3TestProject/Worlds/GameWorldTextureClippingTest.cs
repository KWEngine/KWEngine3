using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldTextureClipping;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTextureClippingTest : World
    {
        
        public override void Act()
        {
           
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Robot", "./Models/PlatformerPack/Toon.glb");

            SetCameraFOV(10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            //SetBackgroundFillColor(1, 1, 1);

            Player p = new Player();
            p.SetModel("KWQuad");
            p.SetTexture("./Textures/spritesheet2.png");
            p.SetTextureRepeat(-1f / 7f, 1f / 3f);
            //p.SetTextureRepeat(-2f, 2f);
            p.SetTextureOffset(0, 0);
            p.SetTextureClip(0.33333f, 0.15f);
            p.IsShadowCaster = true;
            p.HasTransparencyTexture = true;
            AddGameObject(p);

            Immovable bg = new Immovable();
            bg.SetScale(20, 20, 1);
            bg.SetPosition(0, 0, -5);
            bg.IsShadowCaster = true;
            AddGameObject(bg);

            LightObject sun = new LightObjectSun(ShadowQuality.High);
            sun.SetPosition(0, 1, 5);
            sun.SetNearFar(1, 10);
            sun.SetFOV(35);
            sun.SetColor(1, 1, 1, 2);
            AddLightObject(sun);

            LightObject pointlight = new LightObjectPoint(ShadowQuality.High);
            pointlight.SetPosition(1, 0, 5);
            pointlight.SetNearFar(1, 10);
            pointlight.SetColor(1, 0, 0, 4);
            AddLightObject(pointlight);
        }
    }
}
