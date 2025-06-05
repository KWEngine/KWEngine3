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
            List<string> terrainNames = new List<string>();
            int tIndex = 0;
            foreach(FileInfo fi in new DirectoryInfo("./Textures/TerrainTest").GetFiles())
            {
                string tname = "Terrain" + tIndex.ToString().PadLeft(2, '0');
                KWEngine.BuildTerrainModel(tname, fi.FullName, 50);
                terrainNames.Add(tname);
                tIndex++;
            }

            int counter = 0;
            int counterZ = 0;
            int zOffset = -64 + 16;
            int xOffset = -64 + 16;
            foreach (string tname in terrainNames)
            {
                TerrainObject t = new TerrainObject(tname);
                t.IsCollisionObject = true;
                t.SetTexture("./Textures/grass_albedo.png");
                t.SetTexture("./Textures/grass_normal.png", TextureType.Normal);
                t.SetTexture("./Textures/grass_roughness.png", TextureType.Roughness);
                t.SetTextureForSlope("./Textures/limestone-cliffs_albedo.png");
                t.SetTextureForSlope("./Textures/limestone-cliffs_normal.png", TextureType.Normal);
                t.SetTextureSlopeBlendFactor(0.5f);
                t.SetPosition(xOffset, 0, zOffset);
                Console.WriteLine("placing new terrain @ x="+xOffset+"|z=" + zOffset);
                AddTerrainObject(t);

                counter++;
                counterZ++;
                if(counter > 0 && counter % 4 == 0)
                {
                    xOffset += 32;
                }
                if(counterZ == 4)
                {
                    zOffset += 32;
                    counterZ = 0;
                }
            }

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 50, 150);
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
