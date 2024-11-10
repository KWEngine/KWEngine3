using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFoliageTest;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMapTest : World
    {
        private Player _player;

        public override void Act()
        {
            List<Immovable> walls = GetGameObjectsByType<Immovable>();
            for(int i = 0; i < walls.Count; i++)
            {
                Map.SetCamera(new Vector3(_player.Position.X, 25, _player.Position.Z));
                ProjectionBounds bounds = Map.GetScreenCoordinatesNormalizedFor(walls[i]);
                ProjectionBoundsScreen bs = Map.ConvertNormalizedCoordinatesToScreenSpace(bounds, 0.25f, Window.Width / 4 * 2, Window.Height / 4 * 2);
                if (bs.IsInsideRectangle(Window.Width / 4 * 3, Window.Width, Window.Height / 4 * 3, Window.Height))
                {
                    Map.Add(bs.Center, bs.Width, bs.Height, bs.Front);
                }
            }
        }

        public override void Prepare()
        {
            KWEngine.MouseSensitivity = 0.05f;
            //KWEngine.DebugMode = DebugMode.SurfaceNormals;
            KWEngine.BuildTerrainModel("T", "./Textures/heightmap.png", 32, 32, 5);
            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T128;

            HUDObject testBack = new HUDObjectImage();
            testBack.SetZIndex(-5f);
            testBack.SetPosition(300, 300);
            testBack.SetColor(1, 0, 0);
            AddHUDObject(testBack);

            HUDObject testFront = new HUDObjectImage();
            testFront.SetZIndex(5f);
            testFront.SetPosition(500, 300);
            testFront.SetColor(0, 1, 1);
            AddHUDObject(testFront);

            TerrainObject t = new TerrainObject("T");
            t.SetTexture("./Textures/sand_diffuse.dds");
            t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(16, 16);
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            AddTerrainObject(t);

            _player = new Player();
            _player.SetPosition(0, 5, 10);
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
            wall1.SetScale(4, 1, 4);
            wall1.SetPosition(-10, 3, 0);
            wall1.IsCollisionObject = true;
            wall1.IsShadowCaster = true;
            AddGameObject(wall1);

            Immovable wall2 = new Immovable();
            wall2.SetScale(4, 1, 4);
            wall2.SetPosition(10, 5, 0);
            wall2.IsCollisionObject = true;
            wall2.IsShadowCaster = true;
            AddGameObject(wall2);

            Map.SetCamera(_player.Position, ProjectionDirection.NegativeY, 5, 1, 100);
        }
    }
}
