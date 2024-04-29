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

        private float _velocity = 0f;
        private const float VELOCITY_JUMP = 0.04f;
        private const float GRAVITY = 0.0005f;
        private State _currentState = State.OnGround;
        

        public override void Act()
        {
            Vector3 slopeNormal = Vector3.UnitY;
            if (_currentState == State.OnGround)
            {
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.EightRaysY, 0.67f, -2f, 1.5f, typeof(Immovable));
                if (set.IsValid)
                {
                    slopeNormal = set.SurfaceNormalAvg;
                    this.SetPositionY(set.IntersectionPointAvg.Y, PositionMode.BottomOfAABBHitbox);
                }
                else
                {
                    _currentState = State.InAir;
                    _velocity = 0f;
                }
            }
            else
            {
                MoveOffset(0f, _velocity, 0f);
                _velocity -= GRAVITY;
                if (_velocity < -0.5f)
                    _velocity = -0.5f;

                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.EightRaysY, 0.67f, -2f, 1.5f, typeof(Immovable));
                if (set.IsValid == true)
                {
                    if (set.DistanceAvg < 0.05f)
                    {
                        _currentState = State.OnGround;
                        _velocity = 0f;
                    }
                }

            }

            Vector3 playerVelocity = Vector3.Zero;
            if (Keyboard.IsKeyDown(Keys.A))
            {
                playerVelocity.X -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                playerVelocity.X += 1f;
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                playerVelocity.Z -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                playerVelocity.Z += 1f;
            }

            float slopeFactor = 1;
            float playerVelocityLength = playerVelocity.LengthSquared;
            if (playerVelocityLength > 0)
            {
                playerVelocity.NormalizeFast();
                float dotSlopePlayer = Vector3.Dot(slopeNormal, playerVelocity);

                if(dotSlopePlayer < 0 && dotSlopePlayer >= -0.8f)
                {
                    slopeFactor = 1f - Math.Abs(dotSlopePlayer);
                }
                else if(dotSlopePlayer < 0 && dotSlopePlayer < -0.8f)
                {
                    slopeFactor = 0f;
                }
                
                MoveOffset(playerVelocity * 0.01f * slopeFactor);
            }

            if (Keyboard.IsKeyDown(Keys.Space) && _currentState == State.OnGround)
            {
                _currentState = State.InAir;
                _velocity = VELOCITY_JUMP;
            }

            if (Position.Y < -20)
            {
                SetPosition(0, 0.5f, 0);
                _velocity = 0f;
                _currentState = State.OnGround;
            }
        }
    }
}
