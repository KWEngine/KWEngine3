using KWEngine3;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3TestProject.Classes;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldCSMTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 5, 5);
            SetCameraTarget(0, 0, 0);
            SetCameraFOV(90);
            SetColorAmbient(0.25f, 0.25f, 0.25f);


            Immovable box = new Immovable();
            box.SetPosition(-3.25f, 0.5f, -3.25f);
            box.SetColor(1, 1, 0);
            box.IsShadowCaster = true;
            AddGameObject(box);

            Immovable floor = new Immovable();
            floor.SetPosition(0, -0.5f, 0);
            floor.SetScale(10, 1, 10);
            floor.SetColor(1, 0, 1);
            floor.IsShadowCaster = true;
            AddGameObject(floor);


            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, ShadowType.CascadedShadowMap);
            sun.SetCSMFactor(CSMFactor.Two);
            sun.SetPosition(25, 25, -25);
            sun.SetTarget(0, 0, 0);
            sun.SetFOV(10);
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);
        }
    }
}
