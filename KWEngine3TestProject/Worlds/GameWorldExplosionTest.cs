using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldExplosionTest : World
    {
        private float[] _timestamps = new float[5];
        
        public override void Act()
        {
            if(WorldTime == 0f)
            {

            }
            else
            {
                for(int i = 0; i < _timestamps.Length; i++)
                {
                    if(WorldTime - _timestamps[i] >= 1f)
                    {
                        ExplosionObject ex = new ExplosionObject(8, 0.5f, 2, 1, ExplosionType.Cube);
                        ex.SetPosition((i - 2) * 3f, 2f, 0f);
                        ex.SetColor(1, 1, 1);
                        ex.SetColorEmissive(0, 0, 0);
                        //ex.SetRoughness(1.0f);
                        //ex.SetMetallic(0.0f);

                        AddExplosionObject(ex);
                        _timestamps[i] = WorldTime;
                    }
                }
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 5, 15);

            SetBackgroundSkybox("./Textures/skybox_planecollisiontest.dds", 0f, SkyboxType.Equirectangular);
            SetBackgroundBrightnessMultiplier(2);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            Immovable floor = new Immovable();
            floor.Name = "Floor";
            floor.SetPosition(0f, -1f, 0f);
            floor.SetScale(50f, 2f, 50f);
            floor.SetTexture("./Textures/mpanel_diffuse.dds");
            floor.SetTexture("./Textures/mpanel_normal.dds", TextureType.Normal);
            floor.SetTexture("./Textures/mpanel_roughness.dds", TextureType.Roughness);
            floor.SetTexture("./Textures/mpanel_metallic.dds", TextureType.Metallic);
            floor.SetTextureRepeat(5, 5);
            AddGameObject(floor);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(50, 50, 50);
            sun.SetColor(1, 1, 1, 2);
            sun.Name = "Sun";
            AddLightObject(sun);
        }
    }
}
