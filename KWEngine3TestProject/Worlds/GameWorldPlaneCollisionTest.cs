using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlaneCollisionTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlaneCollisionTest : World
    {
        private Player _player;

        public override void Act()
        {
            FlowField f = GetFlowField();
            if(f != null)
            {
                if(f.ContainsXZ(_player))
                {
                    f.SetTarget(_player.Center);
                    f.Update();
                }
                else
                {
                    f.UnsetTarget();
                }
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 10);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            KWEngine.LoadModel("Beach_Render", "./Models/GLTFTest/WorldADV03_Beach_Map.obj");
            KWEngine.LoadModel("Beach_Planes", "./Models/GLTFTest/WorldADV03_Beach_Floors.glb");
            KWEngine.LoadModel("Beach_Hitboxes", "./Models/GLTFTest/WorldADV03_Beach_Hitboxes.glb");
            KWEngine.LoadModel("Toon", "./Models/PlatformerPack/Toon.glb");
            KWEngine.LoadModel("Labyrinth", "./Models/OBJTest/Labyrinth.obj");
            
            MapRender r = new MapRender();
            r.SetModel("Beach_Render");
            r.IsShadowCaster = true;
            AddRenderObject(r);

            Immovable beachPlanes = new Immovable();
            beachPlanes.SetModel("Beach_Planes");
            beachPlanes.Name = "Beach Collider Planes";
            beachPlanes.SetColliderType(ColliderType.RayCollider);
            beachPlanes.SetOpacity(0);
            AddGameObject(beachPlanes);

            Immovable beachHitboxes = new Immovable();
            beachHitboxes.SetModel("Beach_Hitboxes");
            beachHitboxes.Name = "Beach Hitboxes";
            beachHitboxes.SetColliderType(ColliderType.ConvexHull);
            beachHitboxes.SetOpacity(0);
            AddGameObject(beachHitboxes);
            
            _player = new Player();
            _player.SetModel("Toon");
            _player.Name = "Player #1";
            _player.SetPosition(Player.PLAYERSTART.X, Player.PLAYERSTART.Y, Player.PLAYERSTART.Z, PositionMode.BottomOfAABBHitbox);
            _player.SetScale(0.25f);
            _player.SetColliderType(ColliderType.ConvexHull);
            _player.SetHitboxScale(0.5f, 0.75f, 1f);
            _player.IsShadowCaster = true;
            AddGameObject(_player);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(25, 25, 0);
            sun.SetColor(1, 1, 1, 2);
            sun.SetFOV(45);
            sun.SetNearFar(20, 80);
            sun.SetShadowBias(0.000026f);
            AddLightObject(sun);

            Immovable labyrinth = new Immovable();
            labyrinth.Name = "Labyrinth";
            labyrinth.SetModel("Labyrinth");
            labyrinth.SetPosition(15.0f, 1.0f, -2.5f);
            labyrinth.IsCollisionObject = true;
            labyrinth.SetScale(0.11f, 0.14f, 0.11f);
            labyrinth.IsShadowCaster = true;
            labyrinth.FlowFieldCost = 255;
            AddGameObject(labyrinth);

            FlowField flowField = new FlowField(15f, 1.5f, -1.5f, 40, 65, 0.1f, 1, FlowFieldMode.Box, typeof(Immovable));
            flowField.IsVisible = false;
            flowField.Update();
            SetFlowField(flowField);

            Enemy e1 = new Enemy();
            e1.Name = "Enemy #1";
            e1.IsCollisionObject = true;
            e1.SetModel("KWCube");
            e1.SetScale(0.5f);
            e1.SetColor(1f, 0f, 0f);
            e1.SetPosition(15f, 1.25f, -3f);
            AddGameObject(e1);

            MouseCursorGrab();
        }
    }
}
