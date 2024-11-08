using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFoliageTest;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMapTest : World
    {
        

        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.MouseSensitivity = 0.05f;

            KWEngine.BuildTerrainModel("T", "./Textures/heightmap.png", 32, 32, 5);

            TerrainObject t = new TerrainObject("T");
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/Rock_01_512.png");
            t.SetTextureRepeat(16, 16);
            t.IsCollisionObject = true;
            AddTerrainObject(t);

            Player p = new Player();
            p.SetPosition(0, 5, 10);
            p.SetRotation(0, 180, 0);
            p.IsCollisionObject = true;
            p.IsFirstPersonObject = true;
            p.SetOpacity(0);
            AddGameObject(p);

            SetCameraToFirstPersonGameObject(p, 0.6f);
            MouseCursorGrab();

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.NoShadow);
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 3.5f);
            AddLightObject(sun);

            SetColorAmbient(0.25f, 0.25f, 0.25f);
        }
    }
}
