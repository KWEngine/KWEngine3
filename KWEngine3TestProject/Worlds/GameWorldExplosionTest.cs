using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldExplosionTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Globalization;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldExplosionTest : World
    {
        private float _timestamp = 0f;
        private ExplosionAnimation _algorithm = ExplosionAnimation.Spread;
        private int _typeIndex = 0;
        private readonly ExplosionType[] _types = { ExplosionType.Cube, ExplosionType.CubeRingY, ExplosionType.CubeRingZ };
        private int _directionIndex = 0;
        private readonly Vector3[] _directions = { Vector3.Zero, new Vector3(0, 1, 0), new Vector3(0, 3, 0), new Vector3(3, 3, 3), new Vector3(-2, 0.5f, 0), new Vector3(0, -3, 0), new Vector3(1, -3, 0) };
        private HUDObjectText _info;

        public override void Act()
        {
            // arrow keys: left/right = algorithm, up/down = explosion type, tab = direction
            ExplosionAnimation[] algorithms = Enum.GetValues<ExplosionAnimation>();
            int algorithmIndex = Array.IndexOf(algorithms, _algorithm);
            int typeIndex = _typeIndex;
            if (Keyboard.IsKeyPressed(Keys.Right))
                algorithmIndex = (algorithmIndex + 1) % algorithms.Length;
            if (Keyboard.IsKeyPressed(Keys.Left))
                algorithmIndex = (algorithmIndex + algorithms.Length - 1) % algorithms.Length;
            if (Keyboard.IsKeyPressed(Keys.Up))
                typeIndex = (typeIndex + 1) % _types.Length;
            if (Keyboard.IsKeyPressed(Keys.Down))
                typeIndex = (typeIndex + _types.Length - 1) % _types.Length;
            int directionIndex = _directionIndex;
            if (Keyboard.IsKeyPressed(Keys.Tab))
                directionIndex = (directionIndex + 1) % _directions.Length;
            if (algorithms[algorithmIndex] != _algorithm || typeIndex != _typeIndex || directionIndex != _directionIndex)
            {
                _algorithm = algorithms[algorithmIndex];
                _typeIndex = typeIndex;
                _directionIndex = directionIndex;
                UpdateInfo();
            }

            if (WorldTime - _timestamp > 1.5f)
            {
                _timestamp = WorldTime;
                ExplosionObject ex = new ExplosionObject(128, 0.2f, 2f, 1.5f, _types[_typeIndex]);
                ex.SetPosition(0, 3, 0);
                ex.SetAlgorithm(_algorithm);
                ex.SetDirection(_directions[_directionIndex]);
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

            _info = new HUDObjectText();
            _info.SetPosition(16, 32);
            _info.SetScale(20);
            AddHUDObject(_info);
            UpdateInfo();

            MouseCursorGrab();

        }

        private void UpdateInfo()
        {
            Vector3 d = _directions[_directionIndex];
            string direction = d == Vector3.Zero ? "keine" : string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", d.X, d.Y, d.Z);
            _info.SetText("Algorithmus (links/rechts): " + _algorithm + " | Typ (hoch/runter): " + _types[_typeIndex] + " | Richtung (Tab): " + direction);
        }
    }
}
