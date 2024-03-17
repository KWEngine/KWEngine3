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
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", "./Textures/pavement_06_albedo.dds", 25, 2, 25);
            KWEngine.LoadModel("Player", "./Models/GLTFTest/FanplasticDan.glb");

            SetCameraPosition(0.0f, 25.0f, 25.0f);
            SetCameraFOV(20);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-25, 25, 25);
            sun.SetFOV(20);
            sun.SetShadowBias(0.00004f);
            sun.SetTarget(0, 0, 0);
            sun.SetColor(1, 1, 1, 5);
            sun.SetNearFar(1, 50);
            sun.Name = "Sun";
            AddLightObject(sun);

            Player p = new Player();
            p.SetModel("Player");
            p.Name = "Player";
            p.SetCollisionType(ColliderType.ConvexHull);
            p.IsShadowCaster = true;
            AddGameObject(p);

            TerrainObject t = new TerrainObject("Terrain");
            t.IsCollisionObject = true;
            t.IsShadowCaster = true;
            t.SetTexture("./Textures/pavement_06_roughness.dds", TextureType.Roughness);
            t.SetTexture("./Textures/pavement_06_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(12.5f, 12.5f);
            AddTerrainObject(t);
        }
    }
}
