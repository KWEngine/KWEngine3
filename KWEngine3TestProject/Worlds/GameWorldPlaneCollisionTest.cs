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
            
            MapRender r = new MapRender();
            r.SetModel("Beach_Render");
            r.IsShadowCaster = true;
            AddRenderObject(r);

            Immovable x = new Immovable();
            x.SetModel("Beach_Planes");
            x.SetColliderType(ColliderType.RayCollider);
            x.SetOpacity(0);
            AddGameObject(x);

            Immovable y = new Immovable();
            y.SetModel("Beach_Hitboxes");
            y.SetColliderType(ColliderType.ConvexHull);
            y.SetOpacity(0);
            AddGameObject(y);
            
            Player p = new Player();
            p.SetModel("Toon");
            p.SetPosition(Player.PLAYERSTART.X, Player.PLAYERSTART.Y, Player.PLAYERSTART.Z, PositionMode.BottomOfAABBHitbox);
            p.SetScale(0.25f);
            p.SetColliderType(ColliderType.ConvexHull);
            p.SetHitboxScale(0.5f, 0.95f, 1f);
            p.IsShadowCaster = true;
            AddGameObject(p);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(25, 25, 0);
            sun.SetColor(1, 1, 1, 2);
            sun.SetFOV(45);
            sun.SetNearFar(20, 80);
            AddLightObject(sun);

            MouseCursorGrab();
        }
    }
}
