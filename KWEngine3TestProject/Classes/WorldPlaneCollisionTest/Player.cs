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
            if(Keyboard.IsKeyDown(Keys.D))
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
                if (groundDetected)
                {
                    if(yNew != AABBLow)
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
                    if (deltaHeight < 0.01f)
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
                Console.WriteLine(intersection.MeshName);
                MoveOffset(intersection.MTV);
            }

            CurrentWorld.SetCameraPosition(Position.X + 0, 5, Position.Z - 5);
            CurrentWorld.SetCameraTarget(Center.X , 0, Center.Z);
        }

        private bool HandleGroundDetectionTest(out float yNew)
        {
            yNew = 0;

            //RayIntersectionGroupResult result = RaytraceObjectsBelowPositionMultiRay(RayTestPosition.Bottom, 1f, MultiRayMode.SingleRayY, typeof(GameObject));
            RayIntersectionExt result = RaytraceObjectsBelowPosition(RayTestPosition.Bottom, typeof(GameObject));
            if (result.IsValid)
            {
                yNew = result.IntersectionPoint.Y;
                return true;
            }

            return false;
        }

        private bool HandleGroundDetection(out float yNew)
        {
            float yNewLeft = float.MinValue;
            float yNewRight = float.MinValue;
            float yNewBack = float.MinValue;

            bool yFound = false;

            RayIntersectionExt rtFrontLeft = RaytraceObjectsNearby(Center + LookAtVector * 0.1f + LookAtVectorLocalRight * -0.1f, -Vector3.UnitY, typeof(Immovable)).FirstOrDefault();
            if(rtFrontLeft.IsValid)
            {
                float distance = AABBLow - rtFrontLeft.IntersectionPoint.Y;
                if (distance <= 0.00f)
                {
                    yFound = true;
                    yNewLeft = rtFrontLeft.IntersectionPoint.Y;
                }
            }
            
            RayIntersectionExt rtFrontRight = RaytraceObjectsNearby(Center + LookAtVector * 0.1f + LookAtVectorLocalRight * +0.1f, -Vector3.UnitY, typeof(Immovable)).FirstOrDefault();
            if (rtFrontRight.IsValid)
            {
                float distance = AABBLow - rtFrontRight.IntersectionPoint.Y;
                if (distance <= 0.00f)
                {
                    yFound = true;
                    yNewRight = rtFrontRight.IntersectionPoint.Y;
                }
            }

            RayIntersectionExt rtFrontBack = RaytraceObjectsNearby(Center - LookAtVector * 0.1f, -Vector3.UnitY, typeof(Immovable)).FirstOrDefault();
            if (rtFrontBack.IsValid)
            {
                float distance = AABBLow - rtFrontBack.IntersectionPoint.Y;
                if (distance <= 0.00f)
                {
                    yFound = true;
                    yNewBack = rtFrontBack.IntersectionPoint.Y;
                }
            }
            

            yNew = Math.Max(Math.Max(yNewLeft, yNewRight), yNewBack);

            return yFound;
        }
    }
}
