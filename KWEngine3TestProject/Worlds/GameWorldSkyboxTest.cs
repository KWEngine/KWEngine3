using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldSkyboxTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetColorAmbient(0.33f, 0.33f, 0.33f);
            SetCameraPosition(0, 0, 5);
            SetCameraFOV(90);
            SetBackgroundSkybox("./Textures/equirectangular.jpg", 0, SkyboxType.Equirectangular);

            LightObject sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(100, 100, 100);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 3.25f);
            AddLightObject(sun);

            Immovable sphere = new Immovable();
            sphere.SetModel("KWSphere");
            sphere.SetScale(2);
            sphere.SetPosition(0, 2, 0);
            sphere.SetRoughness(0.25f);
            sphere.SetMetallic(0.5f);
            AddGameObject(sphere);

            Immovable plane = new Immovable();
            plane.SetModel("KWCube");
            plane.SetScale(10, 1, 10);
            plane.SetPosition(0, -2, 0);
            plane.SetRoughness(0.25f);
            plane.SetMetallic(0.5f);
            AddGameObject(plane);

        }
    }
}
