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

            /*
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
                Immovable i = GetGameObjectByName<Immovable>("PlaneOnTheFly");
                if (i != null)
                {
                    i.ResetColliderModel();
                    RemoveGameObject(i);
                }
                else
                {
                    i = new Immovable();
                    i.Name = "PlaneOnTheFly";
                    i.SetModel("Plane");
                    i.IsCollisionObject = true;
                    i.SetColliderModel("Plane");
                    i.IsShadowCaster = true;
                    i.SetPosition(-1, -1, -1);
                    AddGameObject(i);
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F7))
            {
                Immovable i = GetGameObjectByName<Immovable>("PlaneOnTheFly");
                if (i != null)
                {
                    i.IsCollisionObject = false;
                    RemoveGameObject(i);
                }
                else
                {
                    i = new Immovable();
                    i.Name = "PlaneOnTheFly";
                    i.SetModel("Plane");
                    i.SetColliderModel("Plane");
                    i.IsShadowCaster = true;
                    i.SetPosition(-1, -1, -1);
                    AddGameObject(i);
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F8))
            {
                Immovable i = GetGameObjectByName<Immovable>("PlaneOnTheFly");
                if (i != null)
                {
                    if(i.IsCollisionObject)
                        Console.WriteLine("Setting PlaneOnTheFly collision state to: deactivated");
                    else
                        Console.WriteLine("Setting PlaneOnTheFly collision state to: activated");
                    i.IsCollisionObject = !i.IsCollisionObject;
                    
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F9))
            {
                Immovable i = GetGameObjectByName<Immovable>("PlaneOnTheFly");
                if (i != null)
                {
                    i.SetModel("KWSphere");

                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F10))
            {
                Immovable i = GetGameObjectByName<Immovable>("PlaneOnTheFly");
                if (i != null)
                {
                    i.SetModel("Plane");
                    i.SetColliderModel("Plane");

                }
            }
            */
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 7.5f, 10.0f);
            SetCameraTarget(0, 1, 0);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            KWEngine.LoadModel("Player", "./Models/Tutorial/Toon.glb");
            KWEngine.LoadModel("Plane", "./Models/OBJTest/PlaneRender.obj");
            KWEngine.LoadCollider("Plane", "./Models/OBJTest/PlaneCollider.obj", ColliderType.PlaneCollider);

            Player p = new Player();
            p.Name = "Player";
            p.SetModel("Player");
            p.SetTexture("./Textures/uvpattern.png");
            p.IsCollisionObject = true;
            p.IsShadowCaster = true;
            p.SetHitboxToCapsule(true, CapsuleHitboxType.Sloped);
            //p.SetPosition(0f, 0f, 0f);
            p.SetPosition(-2.19f, 0.75f, 1.72f);
            p.SetScale(0.5f);
            p.SetHitboxScale(0.75f, 1.0f, 1.5f);
            AddGameObject(p);

            Immovable i = new Immovable();
            i.Name = "Plane";
            i.SetModel("Plane");
            i.SetColliderModel("Plane");
            i.IsCollisionObject = true;
            i.IsShadowCaster = true;
            AddGameObject(i);

            Box box01 = new Box();
            box01.Name = "Box01";
            box01.IsCollisionObject = true;
            box01.SetPosition(-0.5f, 3.5f, -2.0f);
            box01.SetScale(1.5f, 0.5f, 1.5f);
            box01.SetColor(1, 0, 1);
            box01.IsShadowCaster = true;
            AddGameObject(box01);


            IndexSphere sphere = new IndexSphere();
            sphere.SetModel("KWSphere");
            sphere.Name = "IndexSphere";
            sphere.IsAffectedByLight = false;
            sphere.SetColorEmissive(1, 0, 0, 2);
            sphere.SetColor(1, 0, 0);
            sphere.SetScale(0.25f);
            AddGameObject(sphere);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.Low);
            sun.Name = "Sun";
            sun.SetPosition(-10, 10, 0);
            sun.SetNearFar(1, 20);
            sun.SetFOV(16);
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);
        }
    }
}
