using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlaneMTVCollider;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlaneMTVTest : World
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
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 5, 5);

            KWEngine.LoadModel("Plane", "./Models/OBJTest/PlaneRender.obj");
            KWEngine.LoadCollider("Plane", "./Models/OBJTest/PlaneCollider.obj", ColliderType.PlaneCollider);

            Immovable i = new Immovable();
            i.Name = "Plane";
            i.SetModel("Plane");
            i.IsCollisionObject = true;
            i.SetColliderModel("Plane");
            i.IsShadowCaster = true;
            AddGameObject(i);

            IndexSphere sphere = new IndexSphere();
            sphere.SetModel("KWSphere");
            sphere.Name = "IndexSphere";
            sphere.IsAffectedByLight = false;
            sphere.SetColorEmissive(1, 0, 0, 2);
            sphere.SetColor(1, 0, 0);
            sphere.SetScale(0.25f);
            AddGameObject(sphere);

            Player p = new Player();
            p.Name = "Player";
            p.SetModel("KWSphere");
            p.IsCollisionObject = true;
            p.IsShadowCaster = true;
            //p.SetHitboxToCapsuleForMesh(0, CapsuleHitboxMode.Default, CapsuleHitboxType.Sloped);
            p.SetPosition(0.55f, 0.92f, -3f);
            AddGameObject(p);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(-10, 10, 0);
            sun.SetNearFar(1, 20);
            sun.SetFOV(16);
            sun.SetColor(1, 1, 1, 1.75f);
            AddLightObject(sun);

        }
    }
}
