using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using KWEngine3TestProject.Classes.WorldMultipleTerrainTest;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMultipleTerrainTest : World
    {
        public override void Act()
        {
        }

        public override void Prepare()
        {
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap128.png", 10);
            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.SetTexture("./Textures/sand_diffuse.dds");
            t.SetTexture("./Textures/sand_normal.dds");
            AddTerrainObject(t);

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 5, 64);
            AddGameObject(p);

            SetCameraToFirstPersonGameObject(p, 0.25f);

            // Mausempfindlichkeit einstellen (negativ für invertierte Y-Achse):
            KWEngine.MouseSensitivity = 0.05f;

            // Deaktiviert den Mauszeiger und sorgt dafür, dass sich
            // der Cursor nicht außerhalb des Programmfensters bewegen kann:
            MouseCursorGrab();
        }
    }
}
