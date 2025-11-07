using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldTerrainMTVTest;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTerrainMTVTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            //SetCameraPosition(-5, 10, -1);
            SetCameraPosition(0, 80, 0);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            //KWEngine.BuildTerrainModel("Terrain", "./Textures/Terrains/16x16inv.png", 5);
            KWEngine.BuildTerrainModel("Terrain", "./Textures/Terrains/16x16.png", 5);
            //KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap3.png", 5);
            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/grass_albedo.png");
            AddTerrainObject(t);

            Sphere s = new Sphere();
            s.SetModel("KWSphere");
            s.SetScale(1.0f);
            s.SetColor(1, 0.5f, 0f);
            s.SetPosition(0, 5.25f, 7.5f);
            s.IsCollisionObject = true;
            s.IsShadowCaster = true;
            AddGameObject(s);

            KWEngine.DebugMode = DebugMode.TerrainCollisionModel;

            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.Default);
            sun.SetPosition(-100, 100, -100);
            sun.SetColor(1, 1, 1, 2.5f);
            sun.SetFOV(50);
            AddLightObject(sun);
        }
    }
}
