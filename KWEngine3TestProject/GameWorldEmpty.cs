using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFirstPersonView;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldEmpty : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            SetBackgroundSkybox("./Textures/skybox.png");
            KWEngine.LoadModel("test", "./Models/Tests/glbtest.gltf");
            KWEngine.LoadModel("Arms", "./Models/FirstPersonView/arms.gltf");
            KWEngine.LoadModel("Gun", "./Models/Tests/pistol.obj");

            SetCameraFOV(90);
            SetColorAmbient(1, 1, 1);
            PlayerFirstPerson2 p1 = new PlayerFirstPerson2();
            p1.Name = "Player";
            p1.SetOpacity(0);
            AddGameObject(p1);
        // Verbinde die Kamera mit dem Objekt und 
        // verschiebe sie um 2 Einheiten nach oben: 
        SetCameraToFirstPersonGameObject(p1, 2f);

            ViewSpaceWeapon fpw = new ViewSpaceWeapon();
            fpw.SetModel("Arms");
            fpw.SetOffset(0.025f, -0.200f, 0.050f); // Verschiebung relativ zur Kamera
            fpw.SetScale(1.0f); // Skaliere das Objekt entsprechend
            SetViewSpaceGameObject(fpw); // Weapon_R = BONE


            // Mausempfindlichkeit einstellen (negativ für invertierte Y-Achse):
            KWEngine.MouseSensitivity = 0.05f;
            // Deaktiviert den Mauszeiger und sorgt dafür, dass sich
            // der Cursor nicht außerhalb des Programmfensters bewegen kann:
            MouseCursorGrab();

            Immovable floor = new Immovable();
            floor.Name = "Floor";
            floor.SetScale(100, 1, 100);
            floor.SetPosition(0, -1, 0);
            floor.SetTexture("./Textures/pavement_06_albedo.dds");
            floor.SetTexture("./Textures/pavement_06_normal.dds", TextureType.Normal);
            floor.SetTextureRepeat(50, 50);
            AddGameObject(floor);

            Immovable gun = new Immovable();
            gun.SetModel("Gun");
            gun.Name = "Gun";
            gun.SetPosition(0, 0, 5);
            AddGameObject(gun);

            Immovable plane = new Immovable();
            plane.Name = "Plane";
            plane.SetModel("test");
            plane.SetPosition(-3, 0, 4);
            plane.SetRotation(0, 180, 0);
            AddGameObject(plane);

            Immovable plane2 = new Immovable();
            plane2.Name = "Plane2";
            plane2.SetModel("KWQuad");
            plane2.SetColor(1, 1, 0);
            plane2.SetPosition(3, 0, 4);
            plane2.SetMetallic(0.4444f);
            plane2.SetRoughness(0.125f);
            AddGameObject(plane2);

        }
    }
}
