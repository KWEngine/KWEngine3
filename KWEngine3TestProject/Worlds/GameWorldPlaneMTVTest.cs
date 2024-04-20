using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldPlaneMTVCollider;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlaneMTVTest : World
    {
        
        public override void Act()
        {
           
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 5, 5);

            KWEngine.LoadModel("Plane", "./Models/GLTFTest/Plane.glb");
            Immovable i = new Immovable();
            i.SetModel("Plane");
            i.SetColliderType(ColliderType.PlaneCollider);
            i.IsShadowCaster = true;
            AddGameObject(i);

            Player p = new Player();
            p.SetModel("KWCube");
            p.IsCollisionObject = true;
            p.IsShadowCaster = true;
            p.SetPosition(0, 0.4f, 0);
            p.SetHitboxToCapsuleForMesh(0);
            AddGameObject(p);

        }
    }
}
