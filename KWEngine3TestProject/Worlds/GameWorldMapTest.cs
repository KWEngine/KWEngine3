using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMapTest : World
    {
        private Player _player;
        private TerrainObject _t;

        public override void Act()
        {
            if(HelperRandom.GetRandomNumber(0, 100) == 0)
            {
                StartCameraShake(2.5f, 2.5f, 2.5f, 1.0f, 100f, ShakeMode.Additive);
            }

            if(Map.Enabled)
            {
                Map.UpdateCamera(new Vector3(_player.Position.X, _player.Position.Y + 10, _player.Position.Z));
                Map.UpdateCameraRotation(_player.LookAtVectorXZ);

                List<Immovable> walls = GetGameObjectsByType<Immovable>();
                for (int i = 0; i < walls.Count; i++)
                {
                    Map.Add(walls[i], 0, new Vector3(1, 0, 1), Vector3.One, 0, 1, 0, "./Textures/fx_boom.png");
                }
                Map.Add(_t, -1f, Vector3.UnitZ, 0.5f);
            }
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Test", "./Models/robotERS.fbx");

            KWEngine.MouseSensitivity = 0.05f;
            //KWEngine.DebugMode = DebugMode.SurfaceNormals;
            KWEngine.BuildTerrainModel("T", "./Textures/heightmap.png", 2);
            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T128;

            HUDObject testBack = new HUDObjectImage();
            testBack.SetZIndex(-5f);
            testBack.SetPosition(300, 300);
            testBack.SetScale(128, 128);
            testBack.SetColor(1, 0, 0);
            testBack.SetColorEmissive(1, 0, 1);
            testBack.SetColorEmissiveIntensity(1.5f);
            AddHUDObject(testBack);
            
            HUDObjectText testBack2 = new HUDObjectText("Test");
            testBack2.SetZIndex(-100f);
            testBack2.SetPosition(450, 310);
            testBack2.SetScale(48);
            testBack2.SetColor(1, 1, 0);
            testBack2.SetColorEmissive(0, 1, 1);
            testBack2.SetColorEmissiveIntensity(2);
            AddHUDObject(testBack2);

            HUDObject testFront = new HUDObjectImage("./Textures/mapcircle.png");
            testFront.SetZIndex(5f);
            testFront.SetPosition(Window.Width - 384 / 2, Window.Height - 384 / 2);
            //testFront.SetColor(0, 1, 1);
            AddHUDObject(testFront);

            _t = new TerrainObject("T");
            _t.SetTexture("./Textures/sand_diffuse.dds");
            _t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            _t.SetTextureRepeat(16, 16);
            _t.IsCollisionObject = true;
            _t.IsShadowCaster = true;
            AddTerrainObject(_t);

            _player = new Player();
            _player.SetPosition(0, 5, 0);
            _player.SetRotation(0, 180, 0);
            _player.IsCollisionObject = true;
            _player.IsFirstPersonObject = true;
            _player.IsShadowCaster = true;
            _player.SetOpacity(0);
            AddGameObject(_player);

            SetCameraToFirstPersonGameObject(_player, 0f);
            MouseCursorGrab();

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 3.5f);
            AddLightObject(sun);

            SetColorAmbient(0.25f, 0.25f, 0.25f);

            Immovable wall1 = new Immovable();
            wall1.SetAnimationID(1);
            wall1.SetAnimationPercentage(0);
            wall1.SetScale(5);
            wall1.SetPosition(0, 1, 0);
            wall1.AddRotationY(45);
            wall1.IsCollisionObject = true;
            wall1.IsShadowCaster = true;
            AddGameObject(wall1);
            
            Immovable wall2 = new Immovable();
            wall2.SetScale(4, 1, 4);
            wall2.SetPosition(10, 5, 0);
            wall2.IsCollisionObject = true;
            wall2.IsShadowCaster = true;
            AddGameObject(wall2);

            Immovable west = new Immovable();
            west.SetColor(1, 0, 0);
            west.SetScale(4);
            west.SetPosition(-30, 5, 0);
            AddGameObject(west);

            Immovable north = new Immovable();
            north.SetColor(0, 1, 0);
            north.SetColorEmissive(0, 1, 0, 1.5f);
            north.SetScale(4);
            north.SetPosition(0, 5, -30);
            AddGameObject(north);

            Immovable east = new Immovable();
            east.SetColor(0, 0, 1);
            east.SetScale(4);
            east.SetColorEmissive(1, 1, 0, 4);
            east.SetPosition(30, 5, 0);
            AddGameObject(east);

            SetBackgroundSkybox("./Textures/skybox.png");
            SetBackgroundBrightnessMultiplier(4);

            
            Map.SetCamera(_player.Position, ProjectionDirection.NegativeY, 50, 50, 1, 100);
            Map.SetViewport(Window.Width - 384 / 2, Window.Height - 384 / 2, 274, 274, true);
            Map.SetBackground("./Textures/mapgrid.png", 100, 100, 0.9f, 1f, 1f);
            Map.Enabled = true;
        }
    }
}
