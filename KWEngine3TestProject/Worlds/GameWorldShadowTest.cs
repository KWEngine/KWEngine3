using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldShadowTest;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldShadowTest : World
    {
        private LightObject _pointLight;
        private LightObject _directionalLight;
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.F1))
            {
                _pointLight.SetColor(_pointLight.Color.X, _pointLight.Color.Y, _pointLight.Color.Z, 0);
                _directionalLight.SetColor(_directionalLight.Color.X, _directionalLight.Color.Y, _directionalLight.Color.Z, 4);
            }
            else if(Keyboard.IsKeyPressed(Keys.F2))
            {
                _pointLight.SetColor(_pointLight.Color.X, _pointLight.Color.Y, _pointLight.Color.Z, 4);
                _directionalLight.SetColor(_directionalLight.Color.X, _directionalLight.Color.Y, _directionalLight.Color.Z, 0);
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 3, 7);
            SetCameraTarget(0, 1, 0);
            SetColorAmbient(0.1f, 0.1f, 0.1f);

            KWEngine.LoadModel("Ninja", "./Models/GLTFTest/ninja.glb");
            KWEngine.LoadModel("NinjaBow", "./Models/GLTFTest/ninja_bow.glb");

            Player p = new Player();
            p.SetModel("Ninja");
            p.IsShadowCaster = true;
            AddGameObject(p);

            Immovable floor = new Immovable();
            floor.IsShadowCaster = true;
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(5, 1, 5);
            AddGameObject(floor);

            Immovable wall = new Immovable();
            wall.IsShadowCaster = true;
            wall.SetPosition(0, 3, -4.5f);
            wall.SetScale(10, 6, 1);
            AddGameObject(wall);
            
            _pointLight = new LightObject(LightType.Point, ShadowQuality.High);
            _pointLight.SetPosition(-2.5f, 2.0f, 5);
            _pointLight.SetNearFar(1, 20);
            _pointLight.SetColor(1, 1, 1, 0);
            AddLightObject(_pointLight);
            

            _directionalLight = new LightObject(LightType.Directional, ShadowQuality.High);
            _directionalLight.SetPosition(2.5f, 2, 5);
            _directionalLight.SetTarget(0, 1, 0);
            _directionalLight.SetNearFar(1, 20);
            _directionalLight.SetColor(1, 1, 1, 0);
            _directionalLight.SetFOV(160);
            AddLightObject(_directionalLight);

        }
    }
}
