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
            for(int i = 0; i < _timestamps.Length; i++)
            {
                if(WorldTime - _timestamps[i] >= 1f)
                {
                    ExplosionObject ex = new ExplosionObject(8, 1f, 1, 0.95f, ExplosionType.Shard);
                        
                    ex.SetPosition((i - 2) * 3f, 4f, 0f);
                    ex.SetColor(1, 0, 0);
                    ex.SetColorEmissive(0.55f, 0.0f, 0.55f);
                    ex.SetDirection(4, 0, 0);
                    ex.SetAlgorithm(ExplosionAnimation.Spark);

                    AddExplosionObject(ex);
                    _timestamps[i] = WorldTime;
                }
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 1, 10);
            SetCameraTarget(0, 1, 0);

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

            Immovable ceiling = new Immovable();
            ceiling.Name = "Ceiling";
            ceiling.SetScale(10f, 1f, 1f);
            ceiling.SetColor(1, 0, 0);
            ceiling.SetPosition(0, 5.5f, 0);
            AddGameObject(ceiling);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(50, 50, 50);
            sun.SetColor(1, 1, 1, 2);
            sun.Name = "Sun";
            AddLightObject(sun);
        }
    }
}
