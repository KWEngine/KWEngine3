using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldMotionTest;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMotionTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 50);

            for(int i = 0; i < 100; i++)
            {
                MovingCube mc = new MovingCube(
                    new Vector3(
                        HelperRandom.GetRandomNumber(-25f, 25f),
                        HelperRandom.GetRandomNumber(-25f, 25f),
                        HelperRandom.GetRandomNumber(-25f, 25f)
                    ), 
                    HelperRandom.GetRandomNumber(0f, 359f));
                mc.SetColor(
                        HelperRandom.GetRandomNumber(0.3f, 1f),
                        HelperRandom.GetRandomNumber(0.3f, 1f),
                        HelperRandom.GetRandomNumber(0.3f, 1f)
                    );
                AddGameObject(mc);
            }
        }
    }
}
