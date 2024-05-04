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
        public static Vector3 PLAYERSTART = new Vector3(3, 0.19f, -4.5f);

        private State _state = State.OnGround;
        private float _velocityY = 0f;
        private const float VELOCITYJUMP = 0.0325f;
        private float _gravity = 0.00075f;

        private Vector2 _currentCameraRotation = new Vector2(180, -45);
        private float _limitYUp = 5;
        private float _limitYDown = -75;

        public override void Act()
        {
            AddRotationY(-MouseMovement.X * KWEngine.MouseSensitivity);
            UpdateCameraPosition(MouseMovement * KWEngine.MouseSensitivity);

            Vector3 movementVector = Vector3.Zero;
            if (Keyboard.IsKeyDown(Keys.D))
            {
                movementVector += CurrentWorld.CameraLookAtVectorLocalRightXZ;
            }
            if (Keyboard.IsKeyDown(Keys.A))
            {
                movementVector -= CurrentWorld.CameraLookAtVectorLocalRightXZ;
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                movementVector += CurrentWorld.CameraLookAtVectorXZ;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                movementVector -= CurrentWorld.CameraLookAtVectorXZ;
            }

            bool wasdPressed = false;
            bool animationSwitched = false;
            if(movementVector.LengthSquared > 0.1f)
            {
                wasdPressed = true;
                movementVector.Normalize();
                MoveAlongVector(movementVector, 0.011f);
            }

            if (Keyboard.IsKeyPressed(Keys.Space) && _state == State.OnGround)
            {
                _velocityY = VELOCITYJUMP;
                _state = State.InAir;
                animationSwitched = true;
            }

            if(_state == State.InAir)
            {
                MoveOffset(0, _velocityY, 0);
                _velocityY -= _gravity;
            }
            else
            {
                //MoveOffset(0, -_gravity, 0);
            }

            HandleGroundDetectionTest();

            List<Intersection> intersections = GetIntersections(IntersectionTestMode.CheckConvexHullsOnly);
            //List<Intersection> intersections = GetIntersections(IntersectionTestMode.CheckPlanesOnly);
            foreach (Intersection intersection in intersections)
            {
                MoveOffset(intersection.MTV);
                /*if(intersection.MTV.Y > 0 && _state == State.InAir)
                {
                    _state = State.OnGround;
                    _velocityY = 0;
                }*/
            }

            if (this.Position.Y < -0.5f)
            {
                _velocityY = 0;
                _state = State.OnGround;
                this.SetPosition(Player.PLAYERSTART);
            }

            UpdateSun();
            UpdateAnimation(wasdPressed, animationSwitched);
        }

        private void UpdateAnimation(bool isMoving, bool animationJustSwitched)
        {
            if (_state == State.InAir)
            {
                SetAnimationID(6);
                if (animationJustSwitched)
                {
                    SetAnimationPercentage(0.5f);
                }
                SetAnimationPercentageAdvance(0.005f);
                if(AnimationPercentage > 1f)
                {
                    SetAnimationPercentage(0.5f);
                }
            }
            else
            {
                if(isMoving)
                {
                    SetAnimationID(11);
                    SetAnimationPercentageAdvance(0.005f);
                }
                else
                {
                    SetAnimationID(4);
                    SetAnimationPercentageAdvance(0.001f);
                }

            }
        }

        private void HandleGroundDetectionTest()
        {
            RayIntersectionExtSet result = RaytraceObjectsBelowPosition(RayMode.SevenRaysY, 1f, -0.1f, 0.1f, typeof(GameObject));
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
