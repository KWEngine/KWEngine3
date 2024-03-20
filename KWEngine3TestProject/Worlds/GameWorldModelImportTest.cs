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
            //KWEngine.LoadModel("PirateShip", "./Models/OBJTest/CubePBR.obj");
            KWEngine.LoadModel("PirateShip", "./Models/FBXTest/CubePBR_embedded_noRMTextures.fbx");

            SetCameraPosition(-10, 10, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            Immovable ship = new Immovable();
            ship.SetModel("PirateShip");
            //ship.SetRotation(0, 90, 0);
            AddGameObject(ship);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 25);
            sun.SetColor(1, 1, 1, 5);
            sun.SetFOV(25);
            AddLightObject(sun);
        }
    }
}
