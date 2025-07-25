﻿using KWEngine3;
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
            int counter = 0;
            int zOffset = -32 + 8;
            int xOffset = -32 + 8;
            
            Vector2i imageSize = HelperTexture.GetResolutionFromImage("./Textures/heightmap.png");
            int slice = 16;

            for(int y = 0; y < imageSize.Y; y += slice - 1)
            {
                if (y + slice >= imageSize.Y)
                    break;

                for (int x = 0; x < imageSize.X; x += slice - 1)
                {
                    if (x + slice >= imageSize.X)
                        break;

                    string name = "Terrain|" + x + "|" + y;
                    KWEngine.BuildTerrainModel(name, "./Textures/heightmap.png", 5, x, y, slice, slice);

                    TerrainObject t = new TerrainObject(name);
                    t.Name = name;
                    t.IsCollisionObject = true;
                    t.SetTexture("./Textures/grass_albedo.png");
                    t.SetTexture("./Textures/grass_normal.png", TextureType.Normal);
                    t.SetTexture("./Textures/grass_roughness.png", TextureType.Roughness);
                    t.SetTextureForSlope("./Textures/limestone-cliffs_albedo.png");
                    t.SetTextureForSlope("./Textures/limestone-cliffs_normal.png", TextureType.Normal);
                    t.SetTextureSlopeBlendFactor(0.5f);
                    t.SetTextureSlopeBlendSmoothingFactor(0.05f);
                    t.SetPosition(xOffset, 0, zOffset);
                    Console.WriteLine("placing new terrain @ x=" + xOffset + "|z=" + zOffset);
                    AddTerrainObject(t);

                    counter++;

                    if (counter % 4 == 0)
                    {
                        xOffset = -32 + 8;
                        zOffset += 16;
                    }
                    else
                    {
                        xOffset += 16;
                    }
                }
            }

            Player p = new Player();
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 25, 50);
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
