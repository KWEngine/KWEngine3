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
    public class GameWorldModelImportTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("PirateShip", "./Models/FBXTest/PirateShipScene.fbx");

            SetCameraPosition(-10, 10, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            RenderTestClass ship = new RenderTestClass();
            ship.SetModel("PirateShip");
            //ship.SetRotation(0, 90, 0);
            AddRenderObject(ship);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 25);
            sun.SetColor(1, 1, 1, 5);
            sun.SetFOV(25);
            AddLightObject(sun);
        }
    }
}
