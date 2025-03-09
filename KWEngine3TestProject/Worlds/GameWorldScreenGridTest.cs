using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldScreenGridTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    class GameWorldScreenGridTest : World
    {
        private LightObject pLight1;
        private LightObject dLight3;

        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.Left))
            {
                pLight1.SetPosition(pLight1.Position.X - 0.05f, pLight1.Position.Y, pLight1.Position.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Right))
            {
                pLight1.SetPosition(pLight1.Position.X + 0.05f, pLight1.Position.Y, pLight1.Position.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Up))
            {
                pLight1.SetPosition(pLight1.Position.X, pLight1.Position.Y + 0.05f, pLight1.Position.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Down))
            {
                pLight1.SetPosition(pLight1.Position.X, pLight1.Position.Y - 0.05f, pLight1.Position.Z);
            }
        }

        public override void Prepare()
        {
            SetColorAmbient(0.1f, 0.1f, 0.1f);
            SetBackgroundSkybox("./Textures/skybox.dds", 0);
            SetBackgroundBrightnessMultiplier(10);
            
            SetCameraFOV(90);
            SetCameraPosition(0, 1, 10);
            SetCameraTarget(0, 1, 0);

            Immovable floor = new Immovable();
            floor.SetScale(50, 1, 50);
            floor.SetPosition(0, -0.5f, 0);
            floor.IsShadowCaster = true;
            AddGameObject(floor);

            Immovable wallLeft = new Immovable();
            wallLeft.SetScale(1, 5, 50);
            wallLeft.SetPosition(-24.5f, 2.5f, 0);
            wallLeft.IsShadowCaster = true;
            AddGameObject(wallLeft);

            Immovable wallRight = new Immovable();
            wallRight.SetScale(1, 5, 50);
            wallRight.SetPosition(24.5f, 2.5f, 0);
            wallRight.IsShadowCaster = true;
            AddGameObject(wallRight);

            Immovable wallBack = new Immovable();
            wallBack.SetScale(48, 5, 1);
            wallBack.SetPosition(0, 2.5f, -24.5f);
            wallBack.IsShadowCaster = true;
            AddGameObject(wallBack);

            Immovable wallFront = new Immovable();
            wallFront.SetScale(48, 5, 1);
            wallFront.SetPosition(0, 2.5f, 24.5f);
            wallFront.IsShadowCaster = true;
            AddGameObject(wallFront);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 25);
            sun.SetColor(1, 1, 1, 2.5f);
            sun.SetFOV(75);
            AddLightObject(sun);
            
            PlayerFreeFloat p = new PlayerFreeFloat();
            p.SetPosition(0, 1, 20);
            AddGameObject(p);
            MouseCursorGrab();
            

            LightObject dLight1 = new LightObject(LightType.Directional, ShadowQuality.NoShadow);
            dLight1.SetColor(1, 0, 0, 7.5f);
            dLight1.SetPosition(-20, 1, -20);
            dLight1.SetTarget(-10, 0, -15);
            dLight1.SetNearFar(1, 20);
            AddLightObject(dLight1);

            LightObject dLight2 = new LightObject(LightType.Directional, ShadowQuality.NoShadow);
            dLight2.SetColor(0, 1, 1, 7.5f);
            dLight2.SetPosition(-20, 1, 20);
            dLight2.SetTarget(-10, 0, 10);
            dLight2.SetNearFar(1, 20);
            AddLightObject(dLight2);

            dLight3 = new LightObject(LightType.Directional, ShadowQuality.NoShadow);
            dLight3.SetColor(1, 1, 0, 2.5f);
            dLight3.SetPosition(0, 5, 0);
            dLight3.SetTarget(0, 0, 0);
            dLight3.SetNearFar(1, 10);
            AddLightObject(dLight3);

            pLight1 = new LightObject(LightType.Point, ShadowQuality.NoShadow);
            pLight1.SetColor(1, 0, 1, 3f);
            pLight1.SetPosition(20, 5, 0);
            pLight1.SetNearFar(1, 15);
            AddLightObject(pLight1);
        }
    }
}
