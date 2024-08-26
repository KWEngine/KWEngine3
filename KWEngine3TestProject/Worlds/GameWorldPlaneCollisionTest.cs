using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlaneCollisionTest;
using OpenTK.Mathematics;
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
            KWEngine.LoadCollider("Beach_Planes", "./Models/OBJTest/WorldADV03_Beach_Planes.obj", ColliderType.PlaneCollider);
            KWEngine.LoadCollider("Beach_Hitboxes", "./Models/OBJTest/WorldADV03_Beach_Hitboxes.obj", ColliderType.ConvexHull);
            KWEngine.LoadModel("Toon", "./Models/PlatformerPack/Toon.glb");
            KWEngine.LoadModel("Bee", "./Models/PlatformerPack/Bee.gltf");
            KWEngine.LoadModel("Rock01", "./Models/OBJTest/rock01.obj");
            KWEngine.LoadModel("Rock02", "./Models/OBJTest/rock02.obj");
            KWEngine.LoadModel("Rock03", "./Models/OBJTest/rock03.obj");

            MapRender r = new MapRender();
            r.SetModel("Beach_Render");
            r.IsShadowCaster = true;
            AddRenderObject(r);

            Floor beachPlanes = new Floor();
            beachPlanes.SetColliderModel("Beach_Planes");
            beachPlanes.Name = "Beach Collider Planes";
            beachPlanes.IsCollisionObject = true;
            beachPlanes.SetOpacity(0);
            AddGameObject(beachPlanes);

            
            Immovable beachHitboxes = new Immovable();
            beachHitboxes.SetColliderModel("Beach_Hitboxes");
            beachHitboxes.Name = "Beach Hitboxes";
            beachHitboxes.IsCollisionObject = true;
            beachHitboxes.SetOpacity(0);
            AddGameObject(beachHitboxes);
            

            _player = new Player();
            _player.SetModel("Toon");
            _player.Name = "Player #1";
            _player.SetPosition(Player.PLAYERSTART.X, Player.PLAYERSTART.Y, Player.PLAYERSTART.Z, PositionMode.BottomOfAABBHitbox);
            _player.SetScale(0.25f);
            _player.IsCollisionObject = true;
            _player.SetHitboxToCapsule();
            _player.SetHitboxScale(0.5f, 0.75f, 1f);
            _player.IsShadowCaster = true;
            AddGameObject(_player);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(25, 25, 0);
            sun.SetColor(1, 1, 1, 2);
            sun.SetFOV(45);
            sun.SetNearFar(20, 80);
            AddLightObject(sun);

            Enemy e1 = new Enemy();
            e1.Name = "Enemy #1";
            e1.IsCollisionObject = true;
            e1.SetModel("Bee");
            e1.SetScale(0.5f);
            e1.SetHitboxScale(0.67f, 1f, 0.9f);
            e1.SetColor(1f, 1f, 1f);
            e1.SetPosition(18f, 1.0f, -2f);
            e1.IsShadowCaster = true;
            AddGameObject(e1);

            Immovable rock01 = new Immovable();
            rock01.Name = "Rock #01";
            rock01.SetModel("Rock01");
            rock01.SetPosition(15.0f, 0.75f, -2.0f);
            rock01.SetScale(2f, 2f, 2f);
            rock01.IsCollisionObject = true;
            rock01.FlowFieldCost = 255;
            rock01.IsShadowCaster = true;
            AddGameObject(rock01);

            Immovable rock02 = new Immovable();
            rock02.Name = "Rock #02";
            rock02.SetModel("Rock02");
            rock02.SetPosition(18.5f, 0.75f, -4.0f);
            rock02.AddRotationY(90);
            rock02.SetScale(2f, 2f, 2f);
            rock02.IsCollisionObject = true;
            rock02.FlowFieldCost = 255;
            rock02.IsShadowCaster = true;
            AddGameObject(rock02);

            Immovable rock03 = new Immovable();
            rock03.Name = "Rock #03";
            rock03.SetModel("Rock03");
            rock03.SetPosition(14.0f, 1.15f, +2.5f);
            rock03.SetScale(2f, 2f, 2f);
            rock03.IsCollisionObject = true;
            rock03.FlowFieldCost = 255;
            rock03.IsShadowCaster = true;
            AddGameObject(rock03);

            Immovable rock04 = new Immovable();
            rock04.Name = "Rock #04";
            rock04.SetModel("Rock01");
            rock04.SetPosition(12.5f, 2.0f, 0.0f);
            rock04.SetScale(2f, 2f, 2f);
            rock04.IsCollisionObject = true;
            rock04.AddRotationZ(180);
            rock04.FlowFieldCost = 255;
            rock04.IsShadowCaster = true;
            AddGameObject(rock04);

            Immovable rock05 = new Immovable();
            rock05.Name = "Rock #05";
            rock05.SetModel("Rock02");
            rock05.SetPosition(14.0f, 0.75f, -6.0f);
            rock05.SetScale(3f, 2f, 2f);
            rock05.IsCollisionObject = true;
            rock05.FlowFieldCost = 255;
            rock05.IsShadowCaster = true;
            AddGameObject(rock05);

            Immovable rock06 = new Immovable();
            rock06.Name = "Rock #06";
            rock06.SetModel("Rock03");
            rock06.SetPosition(17.0f, 0.67f, 1.0f);
            rock06.AddRotationY(10);
            rock06.SetScale(2f, 2f, 2f);
            rock06.IsCollisionObject = true;
            rock06.FlowFieldCost = 255;
            rock06.IsShadowCaster = true;
            AddGameObject(rock06);

            FlowField flowField = new FlowField(15f, 1.5f, -1.5f, 17, 27, 0.25f, 1, FlowFieldMode.Box, typeof(Immovable));
            flowField.IsVisible = false;
            flowField.Update();
            SetFlowField(flowField);

            MouseCursorGrab();
        }
    }
}
