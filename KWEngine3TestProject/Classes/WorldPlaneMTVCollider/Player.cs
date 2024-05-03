using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldPlaneMTVCollider
{
    public class Player : GameObject
    {
        private enum State
        {
            OnGround,
            InAir
        }

        private Vector2 _velocityXZ = Vector2.Zero;
        private float _velocityY = 0f;
        private Vector2 _velocitySlopeJump = Vector2.Zero;
        private float _playerSpeed = 0.02f;
        private const float _velocityReduction = 1.033f;
        private float _animationStep = 0f;

        private const float VELOCITY_JUMP = 0.04f;
        private const float GRAVITY = 0.0005f;
        private State _currentState = State.OnGround;
        private bool _jumpingUp = false;
        

        public override void Act()
        {
            bool animationIsChanged = false;

            Vector3 slopeNormal = Vector3.UnitY;
            if (_currentState == State.OnGround)
            {
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.EightRaysY, 0.75f, -1.0f, 0.1f, typeof(Immovable), typeof(Box));
                if (set.IsValid)
                {
                    slopeNormal = set.SurfaceNormalAvg;
                    this.SetPositionY(set.IntersectionPointAvg.Y, PositionMode.BottomOfAABBHitbox);
                }
                else
                {
                    _currentState = State.InAir;
                    animationIsChanged = true;
                }
            }
            else
            {
                if(_jumpingUp)
                {
                    if(_velocityY < VELOCITY_JUMP)
                    {
                        _velocityY *= 1.1f;
                    }
                    else
                    {
                        _jumpingUp = false;
                    }
                }
                _velocityY -= GRAVITY;
                if (_velocityY < -0.5f)
                    _velocityY = -0.5f;

                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.EightRaysY, 0.75f, -0.5f, 0.05f, typeof(Immovable), typeof(Box));
                if (set.IsValid == true)
                {
                    if (set.DistanceAvg < 0.0001f &&  _velocityY <= 0)
                    {
                        _currentState = State.OnGround;
                        animationIsChanged = true;
                        _velocityY = 0f;
                        _jumpingUp = false;
                        _velocitySlopeJump = Vector2.Zero;
                    }
                }
            }

            Vector2 inputVelocity = Vector2.Zero;
            if (Keyboard.IsKeyDown(Keys.A))
            {
                inputVelocity.X -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                inputVelocity.X += 1f;
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                inputVelocity.Y -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                inputVelocity.Y += 1f;
            }
            if(inputVelocity.LengthSquared > 0)
            {
                inputVelocity.NormalizeFast();
            }
            float dotVelocityInputVelocity = Vector2.Dot(inputVelocity, _velocityXZ);
            float dotVelocitySlope = Math.Min(0, Vector3.Dot(Vector3.NormalizeFast(new Vector3(_velocityXZ.X, 0, _velocityXZ.Y)), slopeNormal));
            if (dotVelocityInputVelocity >= 0)
            {
                _velocityXZ += inputVelocity * 0.0005f + inputVelocity * dotVelocitySlope * 0.00045f;
            }
            else
            {
                _velocityXZ += inputVelocity * 0.0001f;
            }

            

            if ((Keyboard.IsKeyPressed(Keys.Space) || Mouse.IsButtonPressed(MouseButton.Right)) && _currentState == State.OnGround)
            {
                _currentState = State.InAir;
                _jumpingUp = true;
                _velocityY += VELOCITY_JUMP * 0.33f;
                _velocitySlopeJump = new Vector2(slopeNormal.X * 0.001f, slopeNormal.Z * 0.001f);
                animationIsChanged = true;
            }

            _velocityXZ += _velocitySlopeJump;

            float velocitySum = _velocityXZ.LengthFast;
            if (velocitySum > _playerSpeed)
            {
                _velocityXZ = Vector2.NormalizeFast(_velocityXZ) * _playerSpeed;
            }
            if (velocitySum > 0.001f)
            {
                TurnTowardsXZ(this.Position + new Vector3(_velocityXZ.X, 0, _velocityXZ.Y));
            }
            MoveOffset(new Vector3(_velocityXZ.X, _velocityY, _velocityXZ.Y));

            _velocityXZ /= _velocityReduction;
            _velocitySlopeJump /= _velocityReduction;

            HandleConvexHulls();

            if (Position.Y < -20)
            {
                SetPosition(0, 0.0f, 0);
                _jumpingUp = false;
                _velocityY = 0f;
                _velocityXZ = Vector2.Zero;
                _velocitySlopeJump = Vector2.Zero;
                _currentState = State.OnGround;
                animationIsChanged = true;
            }

            UpdateAnimation(animationIsChanged);
        }

        private void HandleConvexHulls()
        {
            List<Intersection> intersections = GetIntersections<Box>(IntersectionTestMode.CheckConvexHullsOnly);
            foreach(Intersection i in intersections)
            {
                MoveOffset(i.MTV);
                if(_currentState == State.InAir && Vector3.Dot(Vector3.NormalizeFast(i.MTV), -KWEngine.WorldUp) > 0.1f)
                {
                    _velocityY = 0f;
                    _jumpingUp = false;
                }
            }
        }

        private void UpdateAnimation(bool resetPercentage)
        {

            if(HasAnimations)
            {
                if(resetPercentage)
                {
                    _animationStep = 0f;
                    SetAnimationPercentage(_animationStep);
                }
                if(_currentState == State.OnGround)
                {
                    if(_velocityXZ.LengthFast > 0.001f)
                    {
                        SetAnimationID(11); // Walk
                        SetAnimationPercentageAdvance(0.005f);
                    }
                    else
                    {
                        SetAnimationID(3); // Idle
                        SetAnimationPercentageAdvance(0.001f);
                    }
                }
                else
                {
                    if (resetPercentage)
                    {
                        SetAnimationID(6);
                        _animationStep = 0.10f;
                    }
                    
                    if(AnimationID == 6)
                    {
                        SetAnimationPercentage(_animationStep);
                        if (_animationStep >= 1)
                        {
                            SetAnimationID(7);
                            _animationStep = 0f;
                            SetAnimationPercentage(_animationStep);
                        }
                        else
                        {
                            _animationStep += 0.006f;
                        }
                    }
                    else if(AnimationID == 7)
                    {
                        SetAnimationPercentageAdvance(0.01f);
                    }
                }
            }
        }

    }
}
