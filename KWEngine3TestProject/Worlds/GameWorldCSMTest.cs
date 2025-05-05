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
            SetCameraPosition(0, 5, 5);
            SetCameraTarget(0, 0, 0);
            SetCameraFOV(90);
            SetColorAmbient(0.25f, 0.25f, 0.25f);


            Immovable box = new Immovable();
            box.SetPosition(-3.25f, 0.5f, -3.25f);
            box.SetColor(1, 1, 0);
            box.IsShadowCaster = true;
            AddGameObject(box);

            RenderObjectDefault ro1 = new RenderObjectDefault();
            ro1.SetColor(0, 1, 1);
            ro1.SetPosition(2.0f, 0.5f, 2.0f);
            ro1.IsShadowCaster = true;
            AddRenderObject(ro1);

            /*
            Immovable floor = new Immovable();
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(10, 1, 10);
            floor.SetColor(1, 0, 1);
            floor.IsShadowCaster = true;
            AddGameObject(floor);
            */

            
            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.CascadedShadowMap);
            sun.SetCSMFactor(CSMFactor.Eight);
            sun.SetPosition(25, 25, -25);
            sun.SetTarget(0, 0, 0);
            sun.SetFOV(10);
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);
            
            
            LightObjectPoint pointlight = new LightObjectPoint(ShadowQuality.High);
            pointlight.SetPosition(0, 2, 0);
            pointlight.SetColor(1, 1, 1, 2.25f);
            pointlight.SetNearFar(0, 10);
            AddLightObject(pointlight);

            LightObjectDirectional flashlight = new LightObjectDirectional(ShadowQuality.High);
            flashlight.SetPosition(4, 1, 4);
            flashlight.SetTarget(0, 1, 0);
            flashlight.SetNearFar(0.1f, 10f);
            flashlight.SetColor(1, 0, 1, 4);
            AddLightObject(flashlight);

            KWEngine.BuildTerrainModel("TestTerrain", "./Textures/heightmap.png", 1);
            TerrainObject floor = new TerrainObject("TestTerrain");
            floor.IsShadowCaster = true;
            floor.SetTexture("./Textures/sand_diffuse.dds");
            floor.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            AddTerrainObject(floor);

            //KWEngine.DebugMode = DebugMode.TerrainCollisionModel;
        }
    }
}
