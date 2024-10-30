using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldRayHitboxTest;

namespace KWEngine3TestProject.Worlds
{
    class GameWorldRayHitboxTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            Player p1 = new Player();
            p1.AddRotationY(205);
            AddGameObject(p1);

            // Verbinde die Kamera mit dem Objekt und 
            // verschiebe sie um 2 Einheiten nach oben: 
            SetCameraToFirstPersonGameObject(p1, 0.5f);
            SetCameraFOV(90);

            SetColorAmbient(1, 1, 1);
            KWEngine.MouseSensitivity = -0.05f;

            // Deaktiviert den Mauszeiger und sorgt dafür, dass sich
            // der Cursor nicht außerhalb des Programmfensters bewegen kann:
            MouseCursorGrab();

            // ======================

            Obstacle o1 = new Obstacle();
            o1.SetColor(1, 0, 1);
            o1.SetScale(1, 2, 1);
            o1.SetPosition(-5, 0, -10);
            o1.IsCollisionObject = true;
            AddGameObject(o1);

            Obstacle o2 = new Obstacle();
            o2.SetColor(1, 1, 0);
            o2.SetScale(1, 2, 1);
            o2.SetPosition(0, 0, -10);
            o2.IsCollisionObject = true;
            AddGameObject(o2);

            Obstacle o3 = new Obstacle();
            o3.SetColor(0, 1, 1);
            o3.SetScale(1, 2, 1);
            o3.SetPosition(+5, 0, -10);
            o3.IsCollisionObject = true;
            AddGameObject(o3);

            HUDObjectImage crosshair = new HUDObjectImage("./Textures/crosshair.dds");
            crosshair.SetScale(8, 8);
            crosshair.CenterOnScreen();
            AddHUDObject(crosshair);
        }
    }
}
