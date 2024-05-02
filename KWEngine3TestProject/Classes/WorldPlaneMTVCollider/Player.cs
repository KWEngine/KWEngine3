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
        private float _playerSpeed = 0.01f;
        private const float _velocityReduction = 1.05f;

        private const float VELOCITY_JUMP = 0.04f;
        private const float GRAVITY = 0.0005f;
        private State _currentState = State.OnGround;
        

        public override void Act()
        {
            bool animationIsChanged = false;

            Vector3 slopeNormal = Vector3.UnitY;
            if (_currentState == State.OnGround)
            {
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.85f, -0.5f, 0.1f, typeof(Immovable));
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
                _velocityY -= GRAVITY;
                if (_velocityY < -0.5f)
                    _velocityY = -0.5f;

                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.85f, -0.5f, 0.1f, typeof(Immovable));
                if (set.IsValid == true)
                {
                    if (set.DistanceAvg < 0.001f && _velocityY <= 0)
                    {
                        _currentState = State.OnGround;
                        animationIsChanged = true;
                        _velocityY = 0f;
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
                 
            if(dotVelocityInputVelocity >= 0)
                _velocityXZ += inputVelocity * 0.0005f;
            else
                _velocityXZ += inputVelocity * 0.0001f;

            

            if (Keyboard.IsKeyDown(Keys.Space) && _currentState == State.OnGround)
            {
                _currentState = State.InAir;
                _velocityY += VELOCITY_JUMP;
                _velocitySlopeJump = new Vector2(slopeNormal.X * 0.002f, slopeNormal.Z * 0.002f);
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
                SetPosition(0, 0.5f, 0);
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
            List<Intersection> intersections = GetIntersections<Immovable>(IntersectionTestMode.CheckConvexHullsOnly);
            foreach(Intersection i in intersections)
            {
                MoveOffset(i.MTV);
            }
        }

        private void UpdateAnimation(bool resetPercentage)
        {

            if(HasAnimations)
            {
                if(resetPercentage)
                {
                    SetAnimationPercentage(0f);
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
                    SetAnimationID(6); // Jump
                    if(AnimationPercentage < 1)
                    {
                        SetAnimationPercentageAdvance(0.005f);
                    }
                }
            }
        }
    }
}
