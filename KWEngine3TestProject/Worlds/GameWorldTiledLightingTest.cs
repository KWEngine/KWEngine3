using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Audio;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldTiledLightingTest : World
    {
        private HUDObjectText _hudFPS;

        public override void Act()
        {
            _hudFPS.SetText("FPS: " + KWEngine.FPS.ToString("000"));
        }
          
        private void Spawn50Lights()
        {
            int lightcount = 0;
            // Lights
            for (int i = -24; i <= 24; i+=4)
            {
                for (int y = 19; y >= 0; y -= 5)
                {
                    LightObject l = new LightObject(LightType.Point, ShadowQuality.NoShadow);
                    l.SetColor(Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, 3.0f);
                    l.SetPosition(i, y, 5);
                    l.SetNearFar(0.1f, 7.5f);
                    AddLightObject(l);
                    lightcount++;
                }  
            }
            Console.WriteLine("spawned " + lightcount + " lights");
        }

        private void Spawn1000Cubes()
        {
            int i = 0;
            // Objects
            for (int x = -24; x <= 24; x += 1)
            {
                for (int y = 19; y >= 0; y -= 1)
                {
                    Immovable cube = new Immovable();
                    cube.IsShadowCaster = true;
                    cube.SetScale(0.75f);
                    cube.SetColor(Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f);
                    cube.SetPosition(x, y, 0);
                    AddGameObject(cube);
                    i++;
                }
            }
            Console.WriteLine(i);
        }

        private void Spawn1000CubesInstanced()
        {
            InstancedObject cubes = new InstancedObject();
            cubes.SetAdditionalInstanceCount(999);
            cubes.SetPosition(-24, 19, 0);
            cubes.SetScale(0.75f);
            cubes.SetColor(1, 1, 1);

            // Objects
            int i = 1;
            bool firstRun = true;
            for (int x = -24; x <= 24; x += 1)
            {
                for (int y = 19; y >= 0; y -= 1)
                {
                    if (!firstRun)
                    {
                        cubes.SetPositionRotationScaleForInstance(i, new Vector3(x, y, 0), Quaternion.Identity, new Vector3(0.75f));
                        i++;
                    }
                    else
                    {
                        firstRun = false;
                    }
                    
                }
            }
            Console.WriteLine(i);
            AddRenderObject(cubes);
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 40);
            SetCameraTarget(0, 10, 0);
            SetColorAmbient(0.15f, 0.15f, 0.15f);

            _hudFPS = new HUDObjectText("FPS: " + KWEngine.FPS.ToString("000"));
            _hudFPS.SetPosition(16, 32);
            _hudFPS.SetScale(16);
            AddHUDObject(_hudFPS);

            Spawn1000Cubes();
            Spawn50Lights();
        }
    }
}
