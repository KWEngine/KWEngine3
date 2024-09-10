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
            p.SetPosition(1, 0.5f, 1);
            //p.SetOpacity(0.9f);
            AddGameObject(p);

            
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap3.png", 32, 32, 2);
            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/iron_panel_albedo.dds");
            //t.SetTexture("./Textures/pavement_06_roughness.dds", TextureType.Roughness);
            t.SetTexture("./Textures/iron_panel_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(4f, 4f);
            t.SetPosition(2, 0, 0);
            AddTerrainObject(t);
            

            /*
            KWEngine3TestProject.Classes.Immovable plane = new KWEngine3TestProject.Classes.Immovable();
            plane.SetScale(32, 1, 32);
            plane.IsShadowCaster = true;
            plane.SetPosition(0, -0.5f, 0);
            plane.SetTexture("./Textures/iron_panel_albedo.dds");
            //plane.SetTexture("./Textures/pavement_06_roughness.dds", TextureType.Roughness);
            plane.SetTexture("./Textures/iron_panel_normal.dds", TextureType.Normal);
            plane.SetTextureRepeat(4f, 4f);
            //plane.SetOpacity(1);
            AddGameObject(plane);
            */
        }
    }
}
