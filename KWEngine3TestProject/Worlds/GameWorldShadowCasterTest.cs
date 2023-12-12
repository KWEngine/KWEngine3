using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldShadowCasterTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 0);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(0.1f, 0.1f, 0.1f);
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", "./Textures/Grass_01_512.png", 10, 0.1f, 10);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 25);
            sun.SetTarget(0, 1, 0);
            sun.SetNearFar(20, 60);
            sun.SetFOV(20);
            sun.SetColor(1, 1, 1, 2);
            AddLightObject(sun);

            TerrainObject t = new TerrainObject("Terrain");
            t.IsShadowCaster = true;
            t.IsCollisionObject = true;
            AddTerrainObject(t);
            /*
            Immovable floor = new Immovable();
            floor.SetScale(10, 1, 10);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetColor(1, 1, 1);
            floor.IsShadowCaster = true;
            AddGameObject(floor);
            */
            /*
            Immovable platform = new Immovable();
            platform.SetScale(5, 1, 5);
            platform.SetPosition(0, 2, 0);
            platform.IsShadowCaster = true;
            AddGameObject(platform);
            */

            Immovable quadTest = new Immovable();
            quadTest.SetModel("KWQuad");
            quadTest.SetScale(1);
            quadTest.SetPosition(1, 0.5f, 0);
            quadTest.IsShadowCaster = true;
            quadTest.SetTexture("./Textures/fx_boom.png");
            quadTest.HasTransparencyTexture = true;
            quadTest.AddRotationX(-90);
            AddGameObject(quadTest);
            /*
            Immovable quadTest2 = new Immovable();
            quadTest2.SetModel("KWQuad");
            quadTest2.SetScale(1);
            quadTest2.SetPosition(-1, 0.5f, 0);
            quadTest2.IsShadowCaster = false;
            quadTest2.SetTexture("./Textures/fx_boom.png");
            quadTest2.HasTransparencyTexture = false;
            AddGameObject(quadTest2);
            */
        }
    }
}
