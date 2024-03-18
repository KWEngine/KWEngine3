using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPlaneCollisionTest
{
    enum State
    {
        OnGround,
        InAir
    }

    public class Player : GameObject
    {
        private State _state = State.OnGround;
        private float _velocityY = 0f;
        private float _gravity = 0.0015f;


        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.Q))
            {
                AddRotationY(0.25f, true);
            }
            if (Keyboard.IsKeyDown(Keys.E))
            {
                AddRotationY(-0.25f, true);
            }

            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(-0.01f,0,0);
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(+0.01f,0,0);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0, +0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, 0, -0.01f);
            }
           
            if (Keyboard.IsKeyDown(Keys.Space) && _state == State.OnGround)
            {
                _velocityY = 0.05f;
                _state = State.InAir;
            }

            if(_state == State.InAir)
            {
                MoveOffset(0, _velocityY, 0);
                _velocityY -= _gravity;
            }


            Console.WriteLine("vel: " + _velocityY);
            bool groundDetected = HandleGroundDetectionTest(out float yNew);
            if(_state == State.OnGround)
            {
                if (groundDetected && AABBLow - yNew < 0.01f)
                {
                    SetPositionY(yNew, PositionMode.BottomOfAABBHitbox);
                }
                else
                {
                    _velocityY = 0f;
                    _state = State.InAir;
                }
            }
            else
            {
                if(groundDetected)
                {
                    float deltaHeight = AABBLow - yNew;
                    Console.WriteLine("dH: " + deltaHeight);
                    if (deltaHeight <= 0.001f)
                    {
                        SetPositionY(yNew, PositionMode.BottomOfAABBHitbox);
                        _velocityY = 0;
                        _state = State.OnGround;
                    }
                }
                
            }


            List<Intersection> intersections = GetIntersections();
            foreach (Intersection intersection in intersections)
            {
                if (_state == State.OnGround && HelperVector.IsVectorPointingUpward(intersection.MTV))
                {
                    // do nothing
                }
                else
                {
                    MoveOffset(intersection.MTV);
                }
            }

            CurrentWorld.SetCameraPosition(Position.X + 0, 5, Position.Z - 5);
            CurrentWorld.SetCameraTarget(Center.X , 0, Center.Z);
        }

        private bool HandleGroundDetectionTest(out float yNew)
        {
            yNew = 0;
            RayIntersectionExtSet result = RaytraceObjectsBelowPositionMultiRay(MultiRayMode.EightRaysY, 1f, 1f, typeof(GameObject));
            if(result.IsValid)
            {
                yNew = result.IntersectionPointNearest.Y;
                return true;
            }
            return false;
        }
    }
}
