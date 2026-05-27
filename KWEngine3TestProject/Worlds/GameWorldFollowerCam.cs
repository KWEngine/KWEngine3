using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldFollowerCam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFollowerCam : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            //KWEngine.DebugOverlayEnabled = true;

            KWEngine.LoadModel("Brute", "./Models/JumpAndRunPhysics/ubot.fbx");
            KWEngine.BuildTerrainModel("Terrain", "./Textures/heightmap.png", 5);

            SetCameraPosition(0.0f, 20.0f, 0.0f);
            SetCameraTarget(0.0f, 0.0f, 0.0f);

            //KWEngine.DisableKeyframesForModelAnimation("Brute", 42, AnimationKeyframeType.PositionZ, 1);

            Player p = new Player();
            p.SetPosition(0f, 0f, 30f);
            AddGameObject(p);

            Camera c = new Camera(p);
            AddGameObject(c);

            TerrainObject t = new TerrainObject("Terrain");
            t.Name = "Terrain";
            t.IsCollisionObject = true;
            t.SetTexture("./Textures/sand_diffuse.dds");
            t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(2, 2);
            AddTerrainObject(t);


            LightObjectSun sun = new LightObjectSun(ShadowQuality.Low, SunShadowType.CascadedShadowMap);
            sun.Name = "Sun";
            sun.SetPosition(50, 50, 50);
            sun.SetTarget(0, 0, 0);
            sun.SetCSMFactor(CSMFactor.Four);
            sun.SetFOV(10);
            sun.SetColor(1, 1, 1, 2);
            AddLightObject(sun);

            SetColorAmbient(0.25f, 0.25f, 0.25f);

            MouseCursorGrab();
        }
    }
}
