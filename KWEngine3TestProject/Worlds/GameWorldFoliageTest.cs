using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFoliageTest;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFoliageTest : World
    {
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.R))
            {
                TerrainObject t = GetTerrainObjectByName("TestTerrain");
                if(t != null)
                {
                    RemoveTerrainObject(t);
                }
            }
        }

        public override void Prepare()
        {
            KWEngine.BuildTerrainModel("T1", "./Textures/heightmap2.png", "./Textures/Dirt_01_512.png", 40, 1, 80);

            TerrainObject t = new TerrainObject("T1");
            t.Name = "TestTerrain";
            t.SetPosition(0, 0, 0);
            AddTerrainObject(t);
            /*
            FoliageObject tf1 = new FoliageObject(FoliageType.Fern, 2000);
            tf1.SetPosition(0, 0, 0);
            tf1.SetPatchSize(40, 80);
            tf1.SetScale(1f, 1f, 1f);
            tf1.SetSwayFactor(0.01f);
            tf1.AttachToTerrain(t);
            tf1.IsShadowReceiver = true;
            tf1.IsSizeReducedAtCorners = true;
            AddFoliageObject(tf1);
            */

            FoliageObject tf2 = new FoliageObject(FoliageType.GrassFresh, 50000);
            tf2.SetPosition(0, 0, 0);
            tf2.SetPatchSize(20, 40);
            tf2.SetScale(3f, 0.75f, 3f);
            tf2.SetSwayFactor(0.1f);
            tf2.AttachToTerrain(t);
            tf2.IsShadowReceiver = true;
            tf2.IsSizeReducedAtCorners = true;
            AddFoliageObject(tf2);


            PlayerFoliageTest player = new PlayerFoliageTest();
            player.SetRotation(0, 180, 0);
            player.SetPosition(15, 2f, -20f);
            player.SetOpacity(0);
            SetCameraToFirstPersonGameObject(player, 0f);
            MouseCursorGrab();
            AddGameObject(player);
            /*
            Immovable center = new Immovable();
            center.SetColor(1, 1, 1);
            center.SetScale(5);
            center.IsShadowCaster = true;
            AddGameObject(center);
            */
            /*
            Immovable radius = new Immovable();
            radius.SetColor(1, 0, 0);
            radius.SetPosition(0, 0.5f, 0);
            radius.SetScale(4, 1, 8);
            radius.SetOpacity(0.25f);
            AddGameObject(radius);
            */
            /*
            Immovable floor = new Immovable();
            floor.SetTexture("./Textures/Grass_01_512.png");
            floor.SetScale(40, 1, 80);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetTextureRepeat(20, 40);
            floor.IsShadowCaster = true;
            AddGameObject(floor);
            */

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun.SetPosition(100, 100, 100);
            sun.SetNearFar(20, 400);
            sun.SetFOV(100);
            sun.SetColor(1, 0.75f, 0.5f, 3);
            AddLightObject(sun);

            /*
            LightObject sun2 = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun2.SetPosition(-25, 25, -25);
            sun2.SetColor(0, 0, 1.0f, 3);
            AddLightObject(sun2);
            
            */
            SetColorAmbient(0.5f, 0.5f, 0.5f);
            SetBackgroundBrightnessMultiplier(2);
            
            SetBackgroundSkybox("./Textures/skybox.dds");
        }
    }
}
