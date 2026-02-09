using glTFLoader.Schema;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using SpriteSheetQuad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldParticleCustomTest : World
    {
        private float _t = 0f;

        public override void Act()
        { 
            
            if (WorldTime - _t > 1.5f)
            {
                SpritesheetQuad q = new SpritesheetQuad(
                    "C:/Users/lutzk/OneDrive - Eugen-Reintjes-Schule/Modelspack_Release3/Textures/Spritesheets/VisualEffects/explosion_09_8x8.dds", 
                    8, 
                    8);
                q.SetSpriteSheetLooping(false);
                q.SetSpriteSheetEmissiveLevel(0.5f);
                q.SetSpriteSheetSpeed(SpritesheetQuad.Speed.FPS120);
                q.SetScale(4);
                q.SetPosition(0, 2f, 0);
                AddRenderObject(q);
                _t = WorldTime;
            }
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 1, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            SetBackgroundBrightnessMultiplier(2);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(50, 50, 50);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);

            SetBackgroundSkybox("./Textures/skybox_planecollisiontest.dds", 0, SkyboxType.Equirectangular);

            Immovable floor = new Immovable();
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(50, 1, 50);
            floor.SetTexture("./Textures/grass_albedo.png", TextureType.Albedo);
            floor.SetTexture("./Textures/grass_normal.png", TextureType.Normal);
            floor.SetTexture("./Textures/grass_roughness.png", TextureType.Roughness);
            floor.SetTextureRepeat(5, 5);
            AddGameObject(floor);
            
            
            /*SpritesheetQuad q = new SpritesheetQuad("F:/EmberGen_Export/candle_01_12x12_loop.png", 12, 12, true);
            q.SetScale(1f, 4f, 1f);
            q.SetPosition(0f, 0f, 0);
            AddGameObject(q);*/
            
        }
    }
}
