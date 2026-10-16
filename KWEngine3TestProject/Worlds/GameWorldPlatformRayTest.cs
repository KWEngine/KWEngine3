using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlatformRayTest;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlatformRayTest : World
    {
        private Player _p;

        public override void Act()
        {

        }

        public override void Prepare()
        {
            KWEngine.MouseSensitivity = 0.025f;
            KWEngine.DebugOverlayMode = DebugOverlayMode.MembersOnly;
            SetupLoadingScreen();

            KWEngine.LoadModel("UBot", "./Models/ThirdPersonView/ubot.fbx");
            UpdateLoadingScreen(0.4f);
            KWEngine.LoadModel("Brute", "./Models/Brute/brute.fbx");
            UpdateLoadingScreen(0.7f);

            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", 5);

            TerrainObject t = new TerrainObject("Terrain");
            t.Name = "Terrain";
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/sand_diffuse.dds");
            t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(2, 2);
            AddTerrainObject(t);

            PlaceObstacles();
            UpdateLoadingScreen(0.8f);

            _p = new Player();
            _p.SetPositionZ(15f);
            AddGameObject(_p);

            Camera c = new Camera(_p);
            AddGameObject(c);

            Enemy e = new Enemy("Manfred");
            e.SetPositionZ(0f);
            e.AddPatrolPoint(new Vector3(-5, 0, -5));
            e.AddPatrolPoint(new Vector3(+9, 0, +2));
            e.AddPatrolPoint(new Vector3(+5, 0, +12));
            AddGameObject(e);

            UpdateLoadingScreen(1.0f);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.CascadedShadowMap);
            sun.Name = "Sun";
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetCSMFactor(CSMFactor.Eight);
            sun.SetFOV(10);
            sun.SetNearFar(40, 120);
            sun.SetShadowOffset(0f);
            sun.SetColor(1, 1, 1, 2);
            AddLightObject(sun);

            SetColorAmbient(0.25f, 0.25f, 0.25f);
            SetBackgroundSkybox("./Textures/skybox.png", 0f, SkyboxType.CubeMap);
            SetBackgroundBrightnessMultiplier(2.5f);

            if (!Camera.CAM_DISABLE)
                MouseCursorGrab();

            SetCameraPosition(0, 50, -2);
            SetCameraTarget(0, 0, -2);

            SetFogDensity(0.01f);
            SetFogHeight(2f, 0.125f);

        }

        public Player GetPlayer()
        {
            return _p;
        }

        private void SetupLoadingScreen()
        {
            LoadingScreen.SetBackgroundFillColor(0f, 0f, 0f);
            LoadingScreen.SetTextColor(1f, 1f, 1f);
            LoadingScreen.SetTextColorOutline(0.5f, 0.5f, 0.5f, 1f);
            LoadingScreen.SetTextScale(16);
            LoadingScreen.SetTextAlignment(TextAlignMode.Right);
            LoadingScreen.SetText("Loading...");
            LoadingScreen.SetTextPosition(Window.Width - (int)LoadingScreen.TextScale / 2 * LoadingScreen.Text.Length, Window.Height - 32);

            LoadingScreen.SetProgressVisible(true);
            LoadingScreen.SetProgressBorderThickness(0.25f);
            LoadingScreen.SetProgressIndicatorType(ProgressIndicatorType.FilledCircle);
            LoadingScreen.SetProgressColor(1f, 1f, 1f, 1f);
            LoadingScreen.SetProgressSize(32, 32);
            LoadingScreen.SetProgressPosition(Window.Width - 32, Window.Height - 32);
            LoadingScreen.SetProgressValue(0.01f);

            LoadingScreen.Update();
        }

        private void UpdateLoadingScreen(float progress)
        {
            LoadingScreen.SetProgressValue(progress);

            LoadingScreen.Update();
        }

        private void PlaceObstacles()
        {
            string jsonRaw = File.ReadAllText("./Worlds/WorldData/ObstaclePositions.json");
            using JsonDocument doc = JsonDocument.Parse(jsonRaw);

            int i = 0;
            foreach (JsonElement elem in doc.RootElement.GetProperty("ObstaclePositions").EnumerateArray())
            {
                float x = elem.GetProperty("x").GetSingle();
                float y = elem.GetProperty("y").GetSingle();
                float z = elem.GetProperty("z").GetSingle();

                Immovable immo = new Immovable();
                immo.Name = "Obstacle #" + i;
                immo.SetScale(1.5f);
                RayTerrainIntersection rti = HelperIntersection.RaytraceTerrainBelowPosition(new Vector3(x, y, z));
                if (rti.IsValid)
                {
                    immo.SetRotationToMatchSurfaceNormal(rti.SurfaceNormal);
                    immo.SetPosition(rti.IntersectionPoint + rti.SurfaceNormal * immo.Scale.Y * 0.5f);
                    immo.SetTexture("./Textures/scifi_tile_03_albedo.dds");
                    immo.SetTexture("./Textures/scifi_tile_03_normal.dds", TextureType.Normal);
                    immo.SetTexture("./Textures/scifi_tile_03_metallic.dds", TextureType.Metallic);
                    immo.SetTexture("./Textures/scifi_tile_03_roughness.dds", TextureType.Roughness);
                    immo.SetTexture("./Textures/scifi_tile_03_emissive.dds", TextureType.Emissive);
                    immo.SetColorEmissive(0f, 0f, 0f, 1.5f);
                    immo.IsCollisionObject = true;
                    immo.IsShadowCaster = true;
                    AddGameObject(immo);
                }
                else
                {
                    KWEngine.LogWriteLine("[PlaceObstacles()] Invalid position(s) found");
                }
                i++;
            }
        }
    }
}
