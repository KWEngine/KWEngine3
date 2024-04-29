using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        {/*
            Vector3 _slopeNormal = Vector3.Zero;
            if (_currentState == State.OnGround)
            {
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 1f, -2f, 1.5f, typeof(Immovable));
                if (set.IsValid)
                {
                    _slopeNormal = set.SurfaceNormalNearest;
                    this.SetPositionY(set.IntersectionPointNearest.Y, PositionMode.BottomOfAABBHitbox);
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

                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 1f, -2f, 1.5f, typeof(Immovable));
                if (set.IsValid == true)
                {
                    if (set.DistanceMin < 0.05f)
                    {
                        _currentState = State.OnGround;
                        _velocity = 0f;
                    }
                }

            }
            */
            Vector3 pVelocity = Vector3.Zero;
            if (Keyboard.IsKeyDown(Keys.A))
            {
                pVelocity += new Vector3(-0.01f, 0f, 0f);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                pVelocity += new Vector3(+0.01f, 0f, 0f);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                pVelocity += new Vector3(0f, 0f, -0.01f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                pVelocity += new Vector3(0f, 0f, +0.01f);
            }
            if(Keyboard.IsKeyDown(Keys.Space) && _currentState == State.OnGround)
            {
                _currentState = State.InAir;
                _velocity = VELOCITY_JUMP;
            }

            if(_currentState == State.OnGround)
            {
                if(pVelocity != Vector3.Zero)
                    MoveOffset(pVelocity);
            }
            else
            {
                _velocity -= GRAVITY;
                MoveOffset(pVelocity.X, _velocity, pVelocity.Z);
            }

            if(_currentState == State.OnGround)
            {
                List<Intersection> floorTest = GetIntersections<Immovable>(new Vector3(0, -0.001f, 0f), IntersectionTestMode.CheckAllHitboxTypes);
                if(floorTest.Count == 0) 
                {
                    _currentState = State.InAir;
                    _velocity = 0;
                }
            }

            
            
            
            
            List<Intersection> it = GetIntersections<Immovable>(IntersectionTestMode.CheckAllHitboxTypes);
            foreach (Intersection i in it)
            {
                MoveOffset(i.MTV);
                if(i.MTV.Y > 0 && _currentState == State.InAir)
                {
                    _currentState = State.OnGround;
                    _velocity = 0f;
                }

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
