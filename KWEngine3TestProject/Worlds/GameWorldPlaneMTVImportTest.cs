using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlaneMTVCollider;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlaneMTVImportTest : World
    {
        
        public override void Act()
        {
            IndexSphere s = GetGameObjectByName<IndexSphere>("IndexSphere");
            if (s != null)
            {
                
                List<RayIntersectionExt> list = HelperIntersection.RayTraceObjectsForViewVector(HelperIntersection.GetMouseOrigin(), HelperIntersection.GetMouseRay(), 0, true, null, typeof(Immovable));
                if(list.Count > 0)
                {
                    s.SetPosition(list[0].IntersectionPoint);
                }
            }
            if (Keyboard.IsKeyPressed(Keys.F1))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    Console.WriteLine("Setting collider to custom 'Plane'");
                    i.SetColliderModel("Plane");
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F2))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    Console.WriteLine("Setting collider back to default model collider");
                    i.ResetColliderModel();
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F3))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    Console.WriteLine("Setting hitbox 0 to capsule hitbox for Plane object");
                    i.SetHitboxToCapsule(1, 1, 1, Vector3.Zero, CapsuleHitboxType.Sloped);
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F4))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    Console.WriteLine("ACTIVATING COLLISION: adding hitboxes for Plane object");
                    i.IsCollisionObject = true;
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F5))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    Console.WriteLine("DEACTIVATING COLLISION: Removing hitboxes for Plane object");
                    i.IsCollisionObject = false;
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F6))
            {
                Immovable i = GetGameObjectByName<Immovable>("Plane");
                if (i != null)
                {
                    
                }
            }
        }

        public override void Prepare()
        {
            LoadJSON("./Worlds/GameWorldPlaneMTVTest.json");
            GetGameObjectByName("Player").SetHitboxToCapsule(1, 1, 1, Vector3.Zero);
        }

        protected override void OnWorldEvent(WorldEvent e)
        {
            if(e.Description == "PlaneToDefault")
            {
                GameObject g = GetGameObjectByName("Plane");
                if(g != null)
                {
                    g.ResetColliderModel();
                }
            }
            else if(e.Description == "PlaneToPlane")
            {
                GameObject g = GetGameObjectByName("Plane");
                if (g != null)
                {
                    g.SetColliderModel("Plane");
                }
            }
        }
    }
}
