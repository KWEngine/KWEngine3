using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK;
using OpenTK.Mathematics;
using KWEngine3TestProject.Classes.WorldTerrainLOD;


namespace KWEngine3TestProject.Worlds
{
    class GameWorldTerrainLODTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.BuildTerrainModel("KarauTest", "./Textures/heightmap.png", 32, 32, 5);

            SetCameraFOV(90);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 10, 25);

            AddGameObject(p);

            Immovable i1 = new Immovable();
            i1.SetColor(1, 1, 0);
            i1.SetPosition(0, 0.5f, 0);
            AddGameObject(i1);

            SetCameraToFirstPersonGameObject(p, 0.5f);

            MouseCursorGrab();


            TerrainObject t = new TerrainObject("KarauTest");
            t.SetTexture("./Textures/uvpattern.png");
            t.SetTextureRepeat(1, 1);
            t.IsShadowCaster = true;
            t.IsCollisionObject = true;
            t.SetPosition(0, 0, 0);
            AddTerrainObject(t);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-100, 100, 100);
            sun.SetFOV(20);
            //sun.SetShadowBias(0.00004f);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 2.5f);
            sun.SetNearFar(50, 500);
            sun.Name = "Sun";
            AddLightObject(sun);
        }
    }
}
