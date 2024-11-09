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
        private HUDObjectImage[] _mapEntries = new HUDObjectImage[2];
        private Player _player;

        public override void Act()
        {
            List<Immovable> walls = GetGameObjectsByType<Immovable>();
            for(int i = 0; i < walls.Count; i++)
            {
                ProjectionBounds bounds = HelperVector.GetScreenCoordinatesNormalizedFor(walls[i], new Vector3(_player.Position.X, 25, _player.Position.Z), ProjectionDirection.NegativeY, 50, 1, 100);
                ProjectionBoundsScreen bs = HelperVector.ConvertProjectionBoundsToScreenSpace(bounds, 1f, 0, 0);
                _mapEntries[i].SetPosition(bs.Center);
                _mapEntries[i].SetScale(bs.Width, bs.Height);
                _mapEntries[i].SetTextureRepeat(bs.Width / 10f, bs.Height / 10f);
                _mapEntries[i].SetZIndex(bs.Front);
            }
        }

        public override void Prepare()
        {
            _mapEntries[0] = new HUDObjectImage("./Textures/Rock_02_512.png");
            _mapEntries[0].SetColor(1, 0, 0);
            _mapEntries[0].SetTextureRepeat(1, 1);
            AddHUDObject(_mapEntries[0]);

            _mapEntries[1] = new HUDObjectImage("./Textures/Rock_02_512.png");
            _mapEntries[1].SetColor(1, 0, 0);
            _mapEntries[1].SetTextureRepeat(1, 1);
            AddHUDObject(_mapEntries[1]);

            KWEngine.MouseSensitivity = 0.05f;
            //KWEngine.DebugMode = DebugMode.SurfaceNormals;
            KWEngine.BuildTerrainModel("T", "./Textures/heightmap.png", 32, 32, 5);
            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T128;

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
            wall2.SetPosition(10, 3, 0);
            wall2.IsCollisionObject = true;
            wall2.IsShadowCaster = true;
            AddGameObject(wall2);
        }
    }
}
