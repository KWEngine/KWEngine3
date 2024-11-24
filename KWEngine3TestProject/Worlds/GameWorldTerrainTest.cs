using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldTerrainTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTerrainTest : World
    {
        public override void Act()
        {
            if (Keyboard.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q))
            {
                Window.SetWorld(new GameWorldTerrainLODTest());
            }
        }

        public override void Prepare()
        {
            
            KWEngine.LoadModel("Player", "./Models/robotERS.fbx");

            SetCameraPosition(0.0f, 100, 100);
            SetCameraFOV(20);
            
            SetColorAmbient(0.5f, 0.5f, 0.5f);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-25, 25, 25);
            sun.SetFOV(20);
            sun.SetShadowBias(0.00004f);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 2.5f);
            sun.SetNearFar(10, 100);
            sun.Name = "Sun";
            AddLightObject(sun);
           
            /*
            LightObject plight = new LightObject(LightType.Point, ShadowQuality.Low);
            plight.SetPosition(0, 5, 0);
            plight.SetColor(1, 1, 0, 3);
            plight.SetNearFar(1, 10);
            AddLightObject(plight);
            */

            Player p = new Player();
            //p.SetModel("Player");
            p.Name = "Player";
            p.IsCollisionObject = true;
            p.IsShadowCaster = true;
            p.SetPosition(1, 10f, 1);
            //p.SetOpacity(0.9f);
            AddGameObject(p);

            
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", 8);
            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/iron_panel_albedo.dds");
            //t.SetTexture("./Textures/pavement_06_roughness.dds", TextureType.Roughness);
            t.SetTexture("./Textures/iron_panel_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(4f, 4f);
            t.SetPosition(0, 0, 0);
            AddTerrainObject(t);

            KWEngine.TerrainTessellationThreshold = TerrainThresholdValue.T128;
            
        }
    }
}
