using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using KWEngine3TestProject.Classes.WorldGPUDebug;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGPUDebug : World
    {
        private const int MAX = 60;
        private Vector3 _camPos = new Vector3(0f, 1.5f, 20f);
        private HUDObjectText _hudFPS;
        private Queue<int> _fpsValues = new Queue<int>(MAX);
        private int _modelQuality;

        public GameWorldGPUDebug(int q = 1)
        {
            KWEngine.LogWriteLine("Loading world with " + (q > 0 ? "high":"low") + " quality model textures.");
            _modelQuality = q;
        }

        private void AddRemoveLights(int q)
        {
            List<LightObject> lights = GetLightObjects();
            if (lights.Count > 0)
            {
                foreach(LightObject light in lights)
                    RemoveLightObject(light);
                SetColorAmbient(1f, 1f, 1f);
                SetBackgroundBrightnessMultiplier(1f);
            }
            else
            {
                SetColorAmbient(0.25f, 0.25f, 0.25f);
                SetBackgroundBrightnessMultiplier(4f);
                AddLights(q);
            }
        }

        private void AddLights(int q)
        {
            LightObject sun = new LightObject(LightType.Sun, q == 0 ? ShadowQuality.Low : q == 1 ? ShadowQuality.Medium : ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetColor(1f, 0.9f, 0.8f, 2.5f);
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetFOV(25);
            sun.SetNearFar(20, 200);
            AddLightObject(sun);

            LightObject point = new LightObject(LightType.Point, q == 0 ? ShadowQuality.Low : q == 1 ? ShadowQuality.Medium : ShadowQuality.High);
            point.Name = "Point";
            point.SetPosition(0, 3, 0);
            point.SetColor(0, 1, 0, 3);
            point.SetNearFar(1f, 25f);
            AddLightObject(point);
        }

        private void AddRemoveBarn()
        {
            Barn b = GetRenderObjectByName<Barn>("Barn");
            if(b != null)
            {
                RemoveRenderObject(b);
            }
            else
            {
                b = new Barn();
                b.Name = "Barn";
                b.SetModel("Barn");
                b.IsShadowCaster = true;
                b.SetRotation(0, 180, 0);
                AddRenderObject(b);
            }
            
        }

        public override void Act()
        {
            if(_fpsValues.Count >= MAX)
            {
                _fpsValues.Dequeue();
            }
            _fpsValues.Enqueue(KWEngine.FPS);
            _hudFPS.SetText("FPS: " + (int)_fpsValues.Average());

            float sin = MathF.Sin(WorldTime * 1.0f) * 15f;

            SetCameraPosition(_camPos.X + sin, _camPos.Y, _camPos.Z);
            SetCameraTarget(_camPos.X + sin, _camPos.Y, 0);

            if (Keyboard.IsKeyPressed(Keys.Escape)) { Window.SetWorld(new GameWorldGPUDebug(_modelQuality > 0 ? 0 : 1)); }
            else if (Keyboard.IsKeyPressed(Keys.F1)) { AddRemoveLights(0); }
            else if (Keyboard.IsKeyPressed(Keys.F2)) { AddRemoveLights(1); }
            else if (Keyboard.IsKeyPressed(Keys.F3)) { AddRemoveLights(2); }
            else if (Keyboard.IsKeyPressed(Keys.F5)) { AddRemoveBarn(); }
        }

        public override void Prepare()
        {
            _hudFPS = new HUDObjectText("FPS: 0");
            _hudFPS.SetPosition(32, 32);
            _hudFPS.SetScale(32);
            AddHUDObject(_hudFPS);
            if(_modelQuality == 1)
                KWEngine.LoadModel("Barn", "./Models/Barn/barntest.gltf");
            else
                KWEngine.LoadModel("Barn", "./Models/BarnLQ/barntest.gltf");
            KWEngine.BuildTerrainModel("Terrain", "./Models/Barn/heightmap.png", 3);

            TerrainObject t = new TerrainObject("Terrain");
            t.SetTexture("./Textures/Grass_02_512.png");
            t.SetTextureRepeat(1f, 1f);
            t.IsShadowCaster = true;
            AddTerrainObject(t);

            AddRemoveBarn();
            AddRemoveLights(2);

            SetCameraPosition(_camPos);
            SetCameraTarget(_camPos.X, _camPos.Y, 0);

            SetBackgroundSkybox("./Textures/skybox.dds");
        }
    }
}
