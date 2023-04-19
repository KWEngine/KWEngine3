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
        private Vector3 _pivot;
        private float _degrees = 0;
        private float _distance = 50;
        private float _height = 10;

        public override void Act()
        {
            Vector3 newCamPos = HelperRotation.CalculatePositionAfterRotationAroundPointOnAxis(_pivot, _distance, _degrees);
            newCamPos.Y = _height;
            SetCameraPosition(newCamPos);
            _degrees = (_degrees + 0.5f) % 360;
        }

        public override void Prepare()
        {
            _pivot = Vector3.Zero;
            SetCameraPosition(0, _height, _distance);

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
