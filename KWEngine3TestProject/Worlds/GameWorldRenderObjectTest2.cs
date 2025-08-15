using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldRenderObjectTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldRenderObjectTest2 : World
    {
        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.F1))
                Window.SetWorld(new GameWorldRenderObjectTest());
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 10);
            SetCameraFOV(90);
            KWEngine.LoadModel("Bee", "./Models/PlatformerPack/Bee.gltf");
            SetColorAmbient(0.2f, 0.2f, 0.2f);
            SetBackgroundFillColor(1, 1, 0);

            PlayerFirstPerson player = new PlayerFirstPerson();
            player.SetPosition(0, 2.5f, 20);
            player.SetRotation(0, 180, 0);
            player.SkipRender = true;
            SetCameraToFirstPersonGameObject(player, 0f);
            MouseCursorGrab();
            AddGameObject(player);

            StaticObject floor = new StaticObject();
            floor.Name = "Floor";
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(50, 1, 50);
            floor.SetColor(0, 1, 0);
            floor.IsShadowCaster = true;
            floor.SetAdditionalInstanceCount(0);
            AddRenderObject(floor);

            StaticObject r1 = new StaticObject();
            r1.Name = "Würfelpack +3X";
            r1.SetPosition(3, 2.5f, 0);
            r1.IsShadowCaster = true;
            r1.SetAdditionalInstanceCount(2);
            r1.SetPositionRotationScaleForInstance(1, new Vector3(10, 5, 0), Quaternion.Identity, Vector3.One);
            r1.SetPositionRotationScaleForInstance(2, new Vector3(0, 5, 0), Quaternion.Identity, Vector3.One);
            AddRenderObject(r1);
            
            StaticObject r2 = new StaticObject();
            r2.Name = "Würfelpack -3X";
            r2.SetPosition(-3, 3.5f, 0);
            r2.SetRotation(45, 45, 0);
            r2.SetScale(1);
            r2.SetOpacity(0.5f);
            r2.IsShadowCaster = true;
            r2.SetAdditionalInstanceCount(2);
            r2.SetPositionRotationScaleForInstance(1, new Vector3(-3, 6, 0), Quaternion.Identity, Vector3.One);
            r2.SetPositionRotationScaleForInstance(2, new Vector3(-3, 0.5f, 0), Quaternion.Identity, Vector3.One);
            AddRenderObject(r2);
            

            /*
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-25, 25, 25);
            sun.SetColor(1, 1, 1, 1);
            sun.SetNearFar(1, 100);
            AddLightObject(sun);
            */

            LightObject pointlight = new LightObjectPoint(ShadowQuality.High);
            pointlight.SetPosition(0, 10, 5);
            pointlight.SetNearFar(1, 100);
            pointlight.SetColor(1, 1, 1, 2);
            AddLightObject(pointlight);


            //LoadJSON("./JSON/renderobjectstest.json");
        }
    }
}
