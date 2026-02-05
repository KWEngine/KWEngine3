using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
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
            if (WorldTime < 0.01f || WorldTime - _t > 3)
            {
                SpritesheetQuad q = new SpritesheetQuad("./Textures/VFX/___artillery_explosion_1_kar_render_all.png", 9, 9);
                q.SetScale(5);
                q.SetPosition(0, -1.25f, 0);
                AddGameObject(q);
                _t = WorldTime;
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 1, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

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


        }
    }
}
