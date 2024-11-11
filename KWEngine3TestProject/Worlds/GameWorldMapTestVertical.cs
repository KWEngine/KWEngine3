using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldMapTest;
using OpenTK.Mathematics;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMapTestVertical : World
    {
        private Player _player;
        public override void Act()
        {
            List<MapObject> walls = GetGameObjectsByType<MapObject>();
            Map.UpdateCamera(new Vector3(_player.Position.X, _player.Position.Y, _player.Position.Z + 25));
            for (int i = 0; i < walls.Count; i++)
            {
                Map.Add(walls[i], 0, new Vector3(1, 0, 1), Vector3.One, 0, 1, 0);
            }
            Map.Add(_player, 1, Vector3.One, Vector3.Zero, 0, 1, 0, "./Textures/uvpattern.png", 10, 10);
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Test", "./Models/robotERS.fbx");

            HUDObject testBack = new HUDObjectImage();
            testBack.SetZIndex(-5f);
            testBack.SetPosition(300, 300);
            testBack.SetColor(1, 0, 0);
            AddHUDObject(testBack);

            HUDObject testBack2 = new HUDObjectImage();
            testBack2.SetZIndex(-100f);
            testBack2.SetPosition(290, 310);
            testBack2.SetColor(1, 1, 0);
            AddHUDObject(testBack2);

            HUDObject testFront = new HUDObjectImage();
            testFront.SetZIndex(5f);
            testFront.SetPosition(500, 300);
            testFront.SetColor(0, 1, 1);
            AddHUDObject(testFront);

            _player = new Player();
            _player.SetModel("Test");
            _player.SetPosition(0, -4, 0);
            _player.SetRotation(0, 90, 0);
            _player.IsCollisionObject = true;
            AddGameObject(_player);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 3.5f);
            AddLightObject(sun);

            SetColorAmbient(0.25f, 0.25f, 0.25f);

            MapObject wall1 = new MapObject();
            wall1.SetScale(5, 1f, 1f);
            wall1.SetPosition(0, -4.5f, 0);
            wall1.IsCollisionObject = true;
            wall1.IsShadowCaster = true;
            AddGameObject(wall1);

            Map.Enabled = true;
            Map.SetViewport(Window.Width - 256, Window.Height - 256, 512, 512, true);
            Map.SetCamera(_player.Position, ProjectionDirection.NegativeZ, 10, 10, 1, 100);
            Map.SetBackground("./Textures/uvpattern.png", 100, 100, 1f, 1f, 1f);
            
        }
    }
}
