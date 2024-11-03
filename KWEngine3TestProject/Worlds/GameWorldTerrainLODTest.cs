using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK;
using OpenTK.Mathematics;
using KWEngine3TestProject.Classes.WorldTerrainLOD;


namespace KWEngine3TestProject.Worlds
{
    class GameWorldTerrainLODTest : World
    {
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q))
            {
                Window.SetWorld(new GameWorldTerrainTest());
            }
        }

        public override void Prepare()
        {
            
            KWEngine.BuildTerrainModel("KarauTest", "./Textures/heightmap.png", 32, 32, 5);

            SetCameraFOV(90);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            Player p = new Player();
            p.Name = "Player #1";
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 10, 25);
            AddGameObject(p);

            Immovable i1 = new Immovable();
            i1.SetColor(1, 1, 0);
            i1.SetPosition(0, 2.5f, 0);
            i1.SetScale(5f);
            i1.IsShadowCaster = true;
            AddGameObject(i1);

            TerrainObject t = new TerrainObject("KarauTest");
            t.SetTexture("./Textures/uvpattern.png");
            t.SetTextureRepeat(1, 1);
            t.IsShadowCaster = true;
            t.IsCollisionObject = true;
            t.SetPosition(0, 0, 0);
            AddTerrainObject(t);

            /*
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-100, 100, -100);
            sun.SetFOV(20);
            sun.SetTarget(0, 1, 0);
            sun.SetColor(1, 1, 1, 3.5f);
            sun.SetNearFar(50, 500);
            sun.Name = "Sun";
            AddLightObject(sun);
            */

            LightObject point = new LightObject(LightType.Point, ShadowQuality.Medium);
            point.SetPosition(0, 10, 0);
            point.SetNearFar(0.01f, 30f);
            point.SetColor(1, 1, 1, 3);
            AddLightObject(point);

            //LoadJSON("./JSON/test.json");

            SetCameraToFirstPersonGameObject(GetGameObjectByName("Player #1"), 0.5f);

            MouseCursorGrab();

            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T32;
        }
    }
}
