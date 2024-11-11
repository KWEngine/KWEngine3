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
        private TerrainObject _t;

        public override void Act()
        {
            List<Immovable> walls = GetGameObjectsByType<Immovable>();
            for(int i = 0; i < walls.Count; i++)
            {
                Map.UpdateCamera(new Vector3(_player.Position.X, _player.Position.Y, _player.Position.Z));
                Map.UpdateCameraRotation(_player.LookAtVectorXZ);
                Map.Add(walls[i], 0, new Vector3(1, 0, 1), Vector3.One, 0, 1, 0, "./Textures/fx_boom.png");
            }
            /*
            ProjectionBounds boundsT = Map.GetScreenCoordinatesNormalizedFor(_t);
            if(boundsT.IsVisibleOnMap())
            {
                //ProjectionBoundsScreen bs = Map.ConvertNormalizedCoordinatesToScreenSpace(boundsT, 0.25f, Window.Width / 4 * 1, Window.Height / 4 * 1);
                Map.Add(boundsT);
            }
            else
            {
                Console.WriteLine("t not visible");
            }
            */
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Test", "./Models/robotERS.fbx");

            KWEngine.MouseSensitivity = 0.05f;
            //KWEngine.DebugMode = DebugMode.SurfaceNormals;
            KWEngine.BuildTerrainModel("T", "./Textures/heightmap.png", 32, 32, 5);
            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T128;

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

            _t = new TerrainObject("T");
            _t.SetTexture("./Textures/sand_diffuse.dds");
            _t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            _t.SetTextureRepeat(16, 16);
            _t.IsCollisionObject = true;
            _t.IsShadowCaster = true;
            AddTerrainObject(_t);

            _player = new Player();
            _player.SetPosition(0, 5, 5);
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
            //wall1.SetModel("Test");
            wall1.SetAnimationID(1);
            wall1.SetAnimationPercentage(0);
            wall1.SetScale(16);
            //wall1.SetHitboxScale(0.5f, 1, 1f);
            wall1.SetPosition(0, 3, 0);
            //wall1.AddRotationY(45);
            wall1.IsCollisionObject = true;
            wall1.IsShadowCaster = true;
            AddGameObject(wall1);
            /*
            Immovable wall2 = new Immovable();
            wall2.SetScale(4, 1, 4);
            wall2.SetPosition(10, 5, 0);
            wall2.IsCollisionObject = true;
            wall2.IsShadowCaster = true;
            AddGameObject(wall2);
            */
            Map.SetBackground("./Textures/uvpattern.png", 100, 100, 1f, 1f, 1f);
            Map.SetViewport(1216, 656, 128, 128, true);
            Map.SetCamera(_player.Position, ProjectionDirection.NegativeY, 50, 50, 1, 100);
        }
    }
}
