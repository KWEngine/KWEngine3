using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldTerrainPOM;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTerrainPOMTest : World
    {
        public override void Act()
        {
        }

        public override void Prepare()
        {
            /*
            int counter = 0;
            int slice = 16;
            int zOffset = -32 + slice / 2;
            int xOffset = -32 + slice / 2;
            
            Vector2i imageSize = HelperTexture.GetResolutionFromImage("./Textures/heightmap.png");
            

            for(int y = 0; y < imageSize.Y; y += slice - 1)
            {
                if (y + slice >= imageSize.Y)
                    break;

                for (int x = 0; x < imageSize.X; x += slice - 1)
                {
                    if (x + slice >= imageSize.X)
                        break;

                    string name = "Terrain|" + x + "|" + y;
                    KWEngine.BuildTerrainModel(name, "./Textures/heightmap.png", 10, x, y, slice, slice);

                    TerrainObject t = new TerrainObject(name);
                    t.Name = name;
                    t.IsCollisionObject = true;
                    t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Color.png");
                    t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_NormalGL.png", TextureType.Normal);
                    t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Roughness.png", TextureType.Roughness);
                    t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Displacement.png", TextureType.Height);
                    //t.SetTextureForSlope("./Textures/limestone-cliffs_albedo.png");
                    //t.SetTextureForSlope("./Textures/limestone-cliffs_normal.png", TextureType.Normal);
                    //t.SetTextureSlopeBlendFactor(0.5f);
                    //t.SetTextureSlopeBlendSmoothingFactor(0.05f);
                    t.SetParallaxOcclusionMappingScale(0.5f);
                    t.SetPosition(xOffset, 0, zOffset);
                    Console.WriteLine("placing new terrain @ x=" + xOffset + "|z=" + zOffset);
                    AddTerrainObject(t);

                    counter++;

                    if (counter % 4 == 0)
                    {
                        xOffset = -32 + (slice / 2);
                        zOffset += slice;
                    }
                    else
                    {
                        xOffset += slice;
                    }
                }
            }
            */

            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap4.png", 10);

            TerrainObject t = new TerrainObject("Terrain");
            t.Name = "Terrain";
            t.IsCollisionObject = true;
            t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Color.png");
            t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_NormalGL.png", TextureType.Normal);
            t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Roughness.png", TextureType.Roughness);
            t.SetTexture("./Textures/heighttest/Rock051_4K-PNG_Displacement.png", TextureType.Height);
            //t.SetTextureForSlope("./Textures/limestone-cliffs_albedo.png");
            //t.SetTextureForSlope("./Textures/limestone-cliffs_normal.png", TextureType.Normal);
            //t.SetTextureSlopeBlendFactor(0.5f);
            //t.SetTextureSlopeBlendSmoothingFactor(0.05f);
            t.SetParallaxOcclusionMappingScale(0.5f);
            t.SetPosition(8, 0, 8);
            AddTerrainObject(t);

            Player p = new Player();
            //p.SetOpacity(0);
            p.SetModel("KWCube");
            p.SetScale(1f);
            p.SetRotation(0, 180, 0);
            p.SetPosition(3f, 1.5f, 5f);
            p.SetColor(1, 0, 0);
            AddGameObject(p);
            //SetCameraToFirstPersonGameObject(p, 0.25f);

            SetCameraPosition(0, 15, 25);
            SetCameraTarget(0, 0, 0);

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
            //MouseCursorGrab();
        }
    }
}
