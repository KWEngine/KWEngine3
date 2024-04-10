using KWEngine3;
using KWEngine3.GameObjects;
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
        public override void Act()
        {
            
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
            
            Player p = new Player();
            p.SetModel("Toon");
            p.Name = "Player #1";
            p.SetPosition(Player.PLAYERSTART.X, Player.PLAYERSTART.Y, Player.PLAYERSTART.Z, PositionMode.BottomOfAABBHitbox);
            p.SetScale(0.25f);
            p.SetColliderType(ColliderType.ConvexHull);
            p.SetHitboxScale(0.5f, 0.75f, 1f);
            p.IsShadowCaster = true;
            AddGameObject(p);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(25, 25, 0);
            sun.SetColor(1, 1, 1, 2);
            sun.SetFOV(45);
            sun.SetNearFar(20, 80);
            sun.SetShadowBias(0.000027f);
            AddLightObject(sun);

            Immovable labyrinth = new Immovable();
            labyrinth.Name = "Labyrinth";
            labyrinth.SetModel("Labyrinth");
            labyrinth.SetPosition(15.0f, 1.0f, -2.5f);
            labyrinth.IsCollisionObject = true;
            labyrinth.SetScale(0.11f, 0.14f, 0.11f);
            labyrinth.IsShadowCaster = true;
            AddGameObject(labyrinth);

            MouseCursorGrab();
        }
    }
}
