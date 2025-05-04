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
    public class GameWorldMultipleLightsTest : World
    {
        private HUDObjectText _hudFPS;
        private HUDObjectText _hudDespawn;
        private HUDObjectText _hudSpawn1000Cubes;
        private HUDObjectText _hudSpawn1000CubesInstanced;
        private HUDObjectText _hudSpawn50Lights;
        private HUDObjectText _hudSpawn66Cubes;
        private HUDObjectText _hudSpawn05Lights;

        private List<float> _fps = new List<float>(60);

        public override void Act()
        {
            _fps.Add(KWEngine.FPS);
            if(_fps.Count == 60)
            {
                _hudFPS.SetText("FPS: " + _fps.Average().ToString("000"));
                _fps.Clear();
            }

            // DESPAWN ALL:
            if (_hudDespawn.IsMouseCursorOnMe())
            {
                _hudDespawn.SetColorEmissiveIntensity(0.7f);
                if (Mouse.IsButtonPressed(MouseButton.Left))
                {
                    RemoveAllObjectsAndLights();
                }
            }
            else
            {
                _hudDespawn.SetColorEmissiveIntensity(0.0f);
            }

            // 5 LIGHTS:
            if (_hudSpawn05Lights.IsMouseCursorOnMe())
            {
                if (GetLightObjects().Count == 0)
                {
                    _hudSpawn05Lights.SetColorEmissiveIntensity(0.7f);
                    if (Mouse.IsButtonPressed(MouseButton.Left)) Spawn05Lights();
                }
            }
            else
            {
                _hudSpawn05Lights.SetColorEmissiveIntensity(0.0f);
            }

            // 50 LIGHTS:
            if (_hudSpawn50Lights.IsMouseCursorOnMe())
            {
                if (GetLightObjects().Count == 0)
                {
                    _hudSpawn50Lights.SetColorEmissiveIntensity(0.7f);
                    if (Mouse.IsButtonPressed(MouseButton.Left)) Spawn50Lights();
                }
            }
            else
            {
                _hudSpawn50Lights.SetColorEmissiveIntensity(0.0f);
            }


            // 66 Cubes:
            if (_hudSpawn66Cubes.IsMouseCursorOnMe())
            {
                if (GetGameObjects().Count == 0 && GetRenderObjects().Count == 0)
                {
                    _hudSpawn66Cubes.SetColorEmissiveIntensity(0.7f);
                    if (Mouse.IsButtonPressed(MouseButton.Left)) Spawn66Cubes();
                }
            }
            else
            {
                _hudSpawn66Cubes.SetColorEmissiveIntensity(0.0f);
            }

            // 1000 Cubes:
            if (_hudSpawn1000Cubes.IsMouseCursorOnMe())
            {
                if (GetGameObjects().Count == 0 && GetRenderObjects().Count == 0)
                {
                    _hudSpawn1000Cubes.SetColorEmissiveIntensity(0.7f);
                    if (Mouse.IsButtonPressed(MouseButton.Left)) Spawn1000Cubes();
                }
            }
            else
            {
                _hudSpawn1000Cubes.SetColorEmissiveIntensity(0.0f);
            }

            // 1000 Cubes instanced:
            if (_hudSpawn1000CubesInstanced.IsMouseCursorOnMe())
            {
                if (GetGameObjects().Count == 0 && GetRenderObjects().Count == 0)
                {
                    _hudSpawn1000CubesInstanced.SetColorEmissiveIntensity(0.7f);
                    if (Mouse.IsButtonPressed(MouseButton.Left)) Spawn1000CubesInstanced();
                }
            }
            else
            {
                _hudSpawn1000CubesInstanced.SetColorEmissiveIntensity(0.0f);
            }
        }

        private void RemoveAllObjectsAndLights()
        {
            foreach(GameObject g in GetGameObjects())
            {
                RemoveGameObject(g);
            }
            foreach(LightObject l in GetLightObjects())
            {
                RemoveLightObject(l);
            }
            foreach(RenderObject r in GetRenderObjects())
            {
                RemoveRenderObject(r);
            }
        }

        private void Spawn05Lights()
        {
            for (int i = 0; i < 5; i++)
            {
                LightObject l = new LightObjectPoint(ShadowQuality.NoShadow);
                l.SetColor(Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, 5.0f);
                l.SetPosition(Random.Shared.Next(-25, 26), Random.Shared.Next(0, 21), 5);
                l.SetNearFar(0.1f, 7.5f);
                AddLightObject(l);
            }
        }

        private void Spawn50Lights()
        {
            // Lights
            for (int i = 0; i < 50; i++)
            {
                LightObject l = new LightObjectPoint(ShadowQuality.NoShadow);
                l.SetColor(Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, 3.0f);
                l.SetPosition(Random.Shared.Next(-25, 26), Random.Shared.Next(0, 21), 5);
                l.SetNearFar(0.1f, 7.5f);
                AddLightObject(l);
            }
        }

        private void Spawn66Cubes()
        {
            int i = 0;
            // Objects
            for (int x = -25; x <= 25; x+=5)
            {
                for (int y = 20; y >= 0; y-=4)
                {
                    Immovable cube = new Immovable();
                    cube.IsShadowCaster = true;
                    cube.SetScale(4f);
                    cube.SetColor(Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f, Random.Shared.Next(5, 11) * 0.1f);
                    cube.SetPosition(x, y, 0);
                    AddGameObject(cube);
                    i++;
                }
            }
            Console.WriteLine(i);
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
                    //cube.SetOpacity(0.5f);
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
            //cubes.SetOpacity(0.5f);

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
            //Console.WriteLine(i);
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

            _hudDespawn = new HUDObjectText("Delete all objects");
            _hudDespawn.Name = "Delete all objects";
            _hudDespawn.SetPosition(Window.Width - 128, 32);
            _hudDespawn.SetColorEmissive(1, 1, 1);
            _hudDespawn.SetTextAlignment(TextAlignMode.Right);
            _hudDespawn.SetScale(16);
            AddHUDObject(_hudDespawn);

            _hudSpawn05Lights = new HUDObjectText("Spawn 5 lights");
            _hudSpawn05Lights.Name = "5 lights";
            _hudSpawn05Lights.SetPosition(Window.Width - 128, 64);
            _hudSpawn05Lights.SetColorEmissive(1, 1, 1);
            _hudSpawn05Lights.SetTextAlignment(TextAlignMode.Right);
            _hudSpawn05Lights.SetScale(16);
            AddHUDObject(_hudSpawn05Lights);

            _hudSpawn50Lights = new HUDObjectText("Spawn 50 lights");
            _hudSpawn50Lights.Name = "50 lights";
            _hudSpawn50Lights.SetPosition(Window.Width - 128, 96);
            _hudSpawn50Lights.SetColorEmissive(1, 1, 1);
            _hudSpawn50Lights.SetTextAlignment(TextAlignMode.Right);
            _hudSpawn50Lights.SetScale(16);
            AddHUDObject(_hudSpawn50Lights);

            _hudSpawn66Cubes = new HUDObjectText("Spawn 66 cubes");
            _hudSpawn66Cubes.Name = "66 cubes";
            _hudSpawn66Cubes.SetPosition(Window.Width - 128, 128);
            _hudSpawn66Cubes.SetColorEmissive(1, 1, 1);
            _hudSpawn66Cubes.SetTextAlignment(TextAlignMode.Right);
            _hudSpawn66Cubes.SetScale(16);
            AddHUDObject(_hudSpawn66Cubes);

            _hudSpawn1000Cubes = new HUDObjectText("Spawn 1000 cubes");
            _hudSpawn1000Cubes.Name = "1000 cubes";
            _hudSpawn1000Cubes.SetPosition(Window.Width - 128, 160);
            _hudSpawn1000Cubes.SetColorEmissive(1, 1, 1);
            _hudSpawn1000Cubes.SetTextAlignment(TextAlignMode.Right);
            _hudSpawn1000Cubes.SetScale(16);
            AddHUDObject(_hudSpawn1000Cubes);

            _hudSpawn1000CubesInstanced = new HUDObjectText("Spawn 1000 cubes (i)");
            _hudSpawn1000CubesInstanced.Name = "1000 cubes instanced";
            _hudSpawn1000CubesInstanced.SetPosition(Window.Width - 128, 192);
            _hudSpawn1000CubesInstanced.SetColorEmissive(1, 1, 1);
            _hudSpawn1000CubesInstanced.SetTextAlignment(TextAlignMode.Right);
            _hudSpawn1000CubesInstanced.SetScale(16);
            AddHUDObject(_hudSpawn1000CubesInstanced);
        }
    }
}
