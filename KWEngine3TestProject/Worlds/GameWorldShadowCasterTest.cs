using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldShadowCasterTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldShadowCasterTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            //KWEngine.LoadModel("Dan", "./Models/GLTFTest/FanplasticDan.glb");
            SetCameraPosition(5, 5, 10);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(0.1f, 0.1f, 0.1f);
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", 2);
            
            LightObject sun = new LightObjectSun(ShadowQuality.High, SunShadowType.Default);
            sun.SetPosition(-10, 25, 10);
            sun.SetTarget(0, 1, 0);
            sun.SetNearFar(20, 40);
            sun.SetFOV(25);
            sun.SetColor(1, 1, 1, 2);
            AddLightObject(sun);
            
            Immovable floor = new Immovable();
            floor.SetScale(10, 1, 10);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetColor(1, 1, 1);
            floor.IsShadowCaster = true;
            AddGameObject(floor);
            
            
            Immovable platform = new Immovable();
            platform.SetScale(5, 1, 5);
            platform.SetPosition(0, 2, 0);
            platform.IsShadowCaster = true;
            AddGameObject(platform);
            
            
            Immovable quadTest = new Immovable();
            quadTest.SetModel("KWQuad");
            quadTest.SetScale(1);
            quadTest.SetPosition(1, 0.5f, 0);
            quadTest.IsShadowCaster = true;
            quadTest.SetTexture("./Textures/fx_boom.png");
            quadTest.HasTransparencyTexture = true;
            quadTest.AddRotationX(-90);
            AddGameObject(quadTest);
            
            Immovable quadTest2 = new Immovable();
            quadTest2.SetModel("KWQuad");
            quadTest2.SetScale(1);
            quadTest2.SetPosition(-0.5f, 0.5f, 0);
            quadTest2.IsShadowCaster = true;
            quadTest2.SetTexture("./Textures/fx_boom.png");
            quadTest2.HasTransparencyTexture = true;
            AddGameObject(quadTest2);

            TextObject tx = new TextObject("Hello World!");
            tx.IsAffectedByLight = true;
            tx.IsShadowReceiver = true;
            tx.SetCharacterDistanceFactor(0.75f);
            tx.SetScale(0.5f);
            tx.SetPosition(2, 1, 4.5f);
            tx.SetColor(1, 1, 1);
            tx.SetColorEmissive(1, 0, 0, 2);
            AddTextObject(tx);
        }
    }
}
