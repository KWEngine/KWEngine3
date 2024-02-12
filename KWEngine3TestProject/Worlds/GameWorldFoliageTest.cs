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
            
        }

        public override void Prepare()
        {
            
            FoliageObject tf1 = new FoliageObject(FoliageType.Grass1, 100000);
            tf1.SetPosition(0, 0, 0);
            tf1.SetScale(2, 1, 2);
            tf1.SetPatchSize(40, 80);
            //tf1.SetScale(0.5f, 0.25f, 0.5f);
            //tf1.SetSwayFactor(1);
            AddFoliageObject(tf1);
            
            
            PlayerFoliageTest player = new PlayerFoliageTest();
            player.SetRotation(0, 180, 0);
            player.SetPosition(0, 5f, 10);
            player.SetOpacity(0);
            SetCameraToFirstPersonGameObject(player, 0f);
            MouseCursorGrab();
            AddGameObject(player);
            /*
            Immovable center = new Immovable();
            center.SetColor(1, 1, 0);
            center.SetScale(0.1f);
            AddGameObject(center);

            Immovable radius = new Immovable();
            radius.SetColor(1, 0, 0);
            radius.SetPosition(0, 0.5f, 0);
            radius.SetScale(4, 1, 8);
            radius.SetOpacity(0.25f);
            AddGameObject(radius);
            */
            Immovable floor = new Immovable();
            floor.SetTexture("./Textures/Grass_01_512.png");
            floor.SetScale(40, 1, 80);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetTextureRepeat(20, 40);
            AddGameObject(floor);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun.SetPosition(250, 250, 250);
            sun.SetColor(1, 1, 0.0f, 3);
            AddLightObject(sun);

            /*
            LightObject sun2 = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun2.SetPosition(-25, 25, -25);
            sun2.SetColor(0, 0, 1.0f, 3);
            AddLightObject(sun2);
            */

            SetColorAmbient(0.25f, 0.25f, 0.25f);

            //SetBackgroundFillColor(1, 1, 1);
            SetBackgroundSkybox("./Textures/skybox.dds");
            SetBackgroundBrightnessMultiplier(4);
        }
    }
}
