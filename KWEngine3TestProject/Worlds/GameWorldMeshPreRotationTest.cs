using KWEngine3;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldGLTFTest;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine3.GameObjects;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMeshPreRotationTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(10f, 1f, 10f);
            SetCameraTarget(0f, 0f, 0f);
            SetColorAmbient(0.75f, 0.75f, 0.75f);
            SetBackgroundFillColor(1f, 1f, 1f);

            KWEngine.LoadModel("Scooter", "./Models/ScooterKAR.obj");

            Scooter s = new Scooter();
            s.SetModel("Scooter");
            AddGameObject(s);
        }
    }

    internal class Scooter : GameObject
    {
        public override void Act()
        {
            
        }
    }
}
