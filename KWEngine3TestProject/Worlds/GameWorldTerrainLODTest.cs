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
            SetCameraFOV(90);

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 2, 10);

            AddGameObject(p);

            Immovable i1 = new Immovable();
            i1.SetColor(1, 1, 0);
            i1.SetPosition(0, 0.5f, 0);
            AddGameObject(i1);

            SetCameraToFirstPersonGameObject(p, 0.5f);

            MouseCursorGrab();
        }
    }
}
