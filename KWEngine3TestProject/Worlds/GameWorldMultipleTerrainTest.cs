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
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap128.png", 20);
            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.SetTexture("./Textures/grass_albedo.png");
            t.SetTexture("./Textures/grass_normal.png", TextureType.Normal);
            t.SetTexture("./Textures/grass_roughness.png", TextureType.Roughness);
            t.SetTextureForSlope("./Textures/sand_diffuse.dds");
            t.SetTextureForSlope("./Textures/sand_normal.dds", TextureType.Normal);
            t.SetTextureSlopeBlendFactor(0.95f);
            AddTerrainObject(t);

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 25, 64);
            AddGameObject(p);
            SetCameraToFirstPersonGameObject(p, 0.25f);


            LightObjectSun sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(1000, 1000, 1000);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            // Mausempfindlichkeit einstellen (negativ für invertierte Y-Achse):
            KWEngine.MouseSensitivity = 0.05f;

            // Deaktiviert den Mauszeiger und sorgt dafür, dass sich
            // der Cursor nicht außerhalb des Programmfensters bewegen kann:
            MouseCursorGrab();
        }
    }
}
