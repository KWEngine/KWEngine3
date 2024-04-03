using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Classes.WorldPlaneCollisionTest
{
    enum State
    {
        OnGround,
        InAir
    }

    public class Player : GameObject
    {
        public static Vector3 PLAYERSTART = new Vector3(3, 0.33f, -4);

        private State _state = State.OnGround;
        private float _velocityY = 0f;
        private float _gravity = 0.00125f;

        private Vector2 _currentCameraRotation = new Vector2(180, -45);
        private float _limitYUp = 5;
        private float _limitYDown = -75;


        public override void Act()
        {
            AddRotationY(-MouseMovement.X * KWEngine.MouseSensitivity);
            UpdateCameraPosition(MouseMovement * KWEngine.MouseSensitivity);

            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveAlongVector(CurrentWorld.CameraLookAtVectorLocalRightXZ, 0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveAlongVector(-CurrentWorld.CameraLookAtVectorLocalRightXZ, 0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveAlongVector(CurrentWorld.CameraLookAtVector, 0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveAlongVector(-CurrentWorld.CameraLookAtVector, 0.01f);
            }

            if (Keyboard.IsKeyPressed(Keys.Space) && _state == State.OnGround)
            {
                _velocityY = 0.05f;
                _state = State.InAir;
            }

            if(_state == State.InAir)
            {
                MoveOffset(0, _velocityY, 0);
                _velocityY -= _gravity;
            }


            HandleGroundDetectionTest();
            
            List<Intersection> intersections = GetIntersections();
            foreach (Intersection intersection in intersections)
            {
                MoveOffset(intersection.MTV);
            }


            if (this.Position.Y < -0.5f)
            {
                _velocityY = 0;
                _state = State.OnGround;
                this.SetPosition(Player.PLAYERSTART);
            }

            UpdateSun();
        }

        private void HandleGroundDetectionTest()
        {
            RayIntersectionExtSet result = RaytraceObjectsBelowPosition(RayMode.EightRaysY, 1f, -0.1f, 0.1f, typeof(GameObject));
            if(result.IsValid)
            {
                if(_state == State.OnGround)
                {
                    if (result.DistanceMin > -0.1f)
                    {
                        SetPositionY(result.IntersectionPointNearest.Y, PositionMode.BottomOfAABBHitbox);
                    }
                        
                }
                else
                {
                    if(result.DistanceMin <= 0f && _velocityY < 0)
                    {
                        SetPositionY(result.IntersectionPointNearest.Y, PositionMode.BottomOfAABBHitbox);
                        _velocityY = 0f;
                        _state = State.OnGround;
                    }
                }
            }
            else
            {
                if (_state == State.OnGround)
                {
                    _state = State.InAir;
                    _velocityY = 0f;
                }
            }
        }

        private void UpdateCameraPosition(Vector2 msMovement)
        {
            _currentCameraRotation.X += msMovement.X;
            _currentCameraRotation.Y += msMovement.Y;

            if (_currentCameraRotation.Y < _limitYDown)
            {
                _currentCameraRotation.Y = _limitYDown;
            }
            if (_currentCameraRotation.Y > _limitYUp)
            {
                _currentCameraRotation.Y = _limitYUp;
            }

            Vector3 newCamPos = HelperRotation.CalculateRotationForArcBallCamera(
                    Center,                  // Drehpunkt
                    7.5f,                             // Distanz zum Drehpunkt
                    _currentCameraRotation.X,        // Drehung links/rechts
                    _currentCameraRotation.Y,        // Drehung oben/unten
                    false,                           // invertiere links/rechts?
                    false                            // invertiere oben/unten?
            );

            CurrentWorld.SetCameraPosition(newCamPos);
            CurrentWorld.SetCameraTarget(Center);
        }

        private void UpdateSun()
        {
            LightObject sun = CurrentWorld.GetLightObjectByName("Sun");
            if(sun != null)
            {
                sun.SetPosition(Position + new Vector3(-25, 25, 12.5f));
                sun.SetTarget(Position);
            }
        }
    }
}
