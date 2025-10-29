using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldLightAndShadow;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldVolumetricLightTest : World
    {
        private LightObject _light;

        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 10);

            _light = new LightObjectDirectional(ShadowQuality.High);
            _light.SetPosition(15, 15, -15);
            _light.SetTarget(0, 0, 0);
            _light.SetVolume(1.0f);
            _light.SetVolumeBias(0.5f);
            AddLightObject(_light);


            Immovable floor = new Immovable();
            floor.Name = "Floor";
            floor.SetScale(20, 2, 20);
            floor.SetPosition(0, -1, 0);
            floor.IsShadowCaster = true;
            AddGameObject(floor);

            Immovable cube = new Immovable();
            cube.Name = "Crate";
            cube.SetScale(2, 2, 2);
            cube.SetPosition(2, 1, -2);
            cube.IsShadowCaster = true;
            AddGameObject(cube);

        }
    }
}
