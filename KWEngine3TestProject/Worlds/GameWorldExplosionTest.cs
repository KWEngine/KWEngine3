using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldExplosionTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldExplosionTest : World
    {
        private float _timestamp = 0f;
        public override void Act()
        {
            if(WorldTime - _timestamp > 1.5f)
            {
                _timestamp = WorldTime;
                ExplosionObject ex = new ExplosionObject(128, 0.2f, 2f, 1.5f, ExplosionType.Cube);
                ex.SetPosition(0, 3, 0);
                ex.SetAlgorithm(ExplosionAnimation.WhirlwindUp);
                ex.SetDirection(3, 3, 3);
                ex.SetColor(1f, 1f, 1f);
                ex.SetColorEmissive(0.5f, 0.5f, 0.5f);
                AddExplosionObject(ex);
            }
        }

        public override void Prepare()
        {

            SetBackgroundSkybox("./Textures/skybox_planecollisiontest.dds", 0f, SkyboxType.Equirectangular);
            SetBackgroundBrightnessMultiplier(5);
            SetColorAmbient(0.1f, 0.1f, 0.1f);

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
            ceiling.SetScale(0.5f);
            ceiling.SetColor(1, 0, 0);
            ceiling.SetPosition(0, 3f, 0);
            AddGameObject(ceiling);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.NoShadow, SunShadowType.Default);
            sun.SetPosition(50, 50, 50);
            sun.SetColor(1, 1, 1, 2);
            sun.Name = "Sun";
            AddLightObject(sun);

            PlayerFirstPerson p = new PlayerFirstPerson();
            p.SetRotation(0, 180, 0);
            p.SetPosition(0, 5, 10);
            p.IsCollisionObject = false;
            p.IsShadowCaster = false;
            p.SkipRender = true;
            AddGameObject(p);

            SetCameraToFirstPersonGameObject(p);

            MouseCursorGrab();

        }
    }
}
