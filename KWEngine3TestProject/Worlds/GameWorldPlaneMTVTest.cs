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

            KWEngine.LoadModel("Plane", "./Models/GLTFTest/Plane2.glb");
            KWEngine.LoadModelCollider("Plane", "./Models/GLTFTest/Plane2.glb");

            Immovable i = new Immovable();
            i.SetModel("Plane");
            i.IsCollisionObject = true;
            i.SetCustomColliderModel("Plane");
            i.IsShadowCaster = true;
            AddGameObject(i);

            Player p = new Player();
            p.SetModel("KWCube");
            p.IsCollisionObject = true;
            p.IsShadowCaster = true;
            
            p.SetHitboxToCapsuleForMesh(0, CapsuleHitboxMode.Default, CapsuleHitboxType.Sloped);
            p.SetPosition(0.55f, 0.92f, -3f);
            AddGameObject(p);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-10, 10, 0);
            sun.SetNearFar(1, 20);
            sun.SetFOV(16);
            sun.SetColor(1, 1, 1, 1.75f);
            AddLightObject(sun);

        }
    }
}
