using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3TestProject.Classes;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldCSMTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 15, 15);
            SetCameraTarget(0, 0, 0);
            SetCameraFOV(90);
            SetColorAmbient(0.25f, 0.25f, 0.25f);


            Immovable box1 = new Immovable();
            box1.SetPosition(-4.5f, 0.5f, -4f);
            box1.SetColor(1, 1, 0);
            box1.IsShadowCaster = true;
            box1.Name = "YELLOW";
            AddGameObject(box1);

            Immovable box2 = new Immovable();
            box2.SetPosition(-3.25f, 0.5f, 5.55f);
            box2.SetColor(1, 0, 0);
            box2.IsShadowCaster = true;
            box2.SetOpacity(0.5f);
            box2.Name = "RED";
            AddGameObject(box2);

            RenderObjectDefault ro1 = new RenderObjectDefault();
            ro1.SetColor(0, 1, 1);
            ro1.SetPosition(5.5f, 0.5f, 8.0f);
            ro1.IsShadowCaster = true;
            ro1.SetOpacity(0.5f);
            ro1.Name = "AQUAMARINE";
            AddRenderObject(ro1);

            RenderObjectDefault ro2 = new RenderObjectDefault();
            ro2.SetColor(0, 0, 1);
            ro2.SetPosition(3.25f, 0.5f, -9.25f);
            ro2.IsShadowCaster = true;
            ro2.Name = "BLUE";
            AddRenderObject(ro2);

            /*
            Immovable floor = new Immovable();
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(10, 1, 10);
            floor.SetColor(1, 0, 1);
            floor.IsShadowCaster = true;
            AddGameObject(floor);
            */


            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.CascadedShadowMap);
            sun.SetCSMFactor(CSMFactor.Two);
            sun.SetPosition(25, 25, 25);
            sun.SetTarget(0, 0, 0);
            sun.SetFOV(10);
            sun.Name = "SUN";
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);
            
            
            LightObjectPoint pointlight = new LightObjectPoint(ShadowQuality.High);
            pointlight.SetPosition(0, 5, 0);
            pointlight.SetColor(1, 1, 1, 1.25f);
            pointlight.SetNearFar(0, 10);
            pointlight.Name = "POINTLIGHT";
            AddLightObject(pointlight);

            LightObjectDirectional flashlight = new LightObjectDirectional(ShadowQuality.High);
            flashlight.SetPosition(4, 1, 4);
            flashlight.SetTarget(6, 0.5f, 7);
            flashlight.SetNearFar(0.1f, 10f);
            flashlight.SetColor(0, 1, 0, 4);
            flashlight.Name = "FLASHLIGHT";
            AddLightObject(flashlight);

            TextObject text = new TextObject("Wahnsinnsloses Wort");
            text.SetPosition(-4.25f, 0.5f, -3.25f);
            text.SetScale(0.5f);
            text.SetColor(1, 1, 1);
            text.SetRotation(0, 45, 0);
            AddTextObject(text);

            KWEngine.BuildTerrainModel("TestTerrain", "./Textures/heightmap.png", 1);
            TerrainObject floor = new TerrainObject("TestTerrain");
            floor.IsShadowCaster = true;
            floor.Name = "TestTerrain";
            //floor.SetTexture("./Textures/sand_diffuse.dds");
            //floor.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            AddTerrainObject(floor);

            //KWEngine.DebugMode = DebugMode.TerrainCollisionModel;
        }
    }
}
