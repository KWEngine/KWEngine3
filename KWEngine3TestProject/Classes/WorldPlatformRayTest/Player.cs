using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.ComponentModel.Design;

namespace KWEngine3TestProject.Classes.WorldPlatformRayTest
{
    internal class Player : GameObject
    {
        public const float SPEED = 0.015f;

        private const float MIN_LOOKAT_LENGTH = 0.00001f;
        private const float PLAYER_MOVEMENT_MINIMUM = 0.00001f;
        private const float GRAVITY = 0.00075f;
        private const float VELOCITY_JUMP = 0.05f;
        private float SPEED_AIR = 0.005f;

        private Vector3 _motion = Vector3.Zero;
        private Vector3 _motionLocal = Vector3.Zero;
        private float _motionVertical = 0f;

        private PrimaryState _state = PrimaryState.OnGround;
        private ActionState _stateAction = ActionState.Idle;

        private enum PrimaryState
        {
            OnGround,
            InAir
        }

        private enum ActionState
        {
            Idle,
            Melee,
            Projectile,
            Heal,
            Hurt
        }

        public Player()
        {
            Name = "Player";
            SetModel("UBot");
            IsCollisionObject = true;
            IsShadowCaster = true;
            UpdateLast = true;

            SetHitboxToCapsule();
            SetHitboxScale(0.6f, 1f, 1f, -0.04f, 0f, 0.08f);
            SetScale(1f);
            SetRotation(0, 180, 0);
            SetAnimationID(0);

            HandleGroundCollision();
        }

        public override void Act()
        {
            int move = 0;
            int strafe = 0;

            Vector3 tmpLookAtVector = Vector3.Zero;
            Vector3 camLookAtXZ = CurrentWorld.CameraLookAtVectorXZ;
            Vector3 camLookAtRightXZ = CurrentWorld.CameraLookAtVectorLocalRightXZ;

            if (Keyboard.IsKeyDown(Keys.W))
            {
                move += 1;
                tmpLookAtVector += camLookAtXZ;
            }

            if (Keyboard.IsKeyDown(Keys.A))
            {
                strafe -= 1;
                tmpLookAtVector -= camLookAtRightXZ;
            }

            if (Keyboard.IsKeyDown(Keys.S))
            {
                move -= 1;
                tmpLookAtVector -= camLookAtXZ;
            }

            if (Keyboard.IsKeyDown(Keys.D))
            {
                strafe += 1;
                tmpLookAtVector += camLookAtRightXZ;
            }

            if (Mouse.IsButtonPressed(MouseButton.Right) && _state == PrimaryState.OnGround)
            {
                _state = PrimaryState.InAir;
                _motionVertical = VELOCITY_JUMP;
                MoveOffset(0f, _motionVertical, 0f);
            }

            if (_state == PrimaryState.InAir)
            {
                _motionVertical -= GRAVITY;
                MoveOffset(0f, _motionVertical, 0f);
            }




            if (tmpLookAtVector.LengthSquared > MIN_LOOKAT_LENGTH && _state == PrimaryState.OnGround)
            {
                tmpLookAtVector.NormalizeFast();
                TurnTowardsXZ(Center + tmpLookAtVector);
            }

            _motion = Vector3.Zero;
            _motionLocal = Vector3.Zero;
            if (move != 0 || strafe != 0)
            {
                _motion = MoveAndStrafeAlongCameraXZ(move, strafe, SPEED);
                _motionLocal = Vector3.NormalizeFast(move * -Vector3.UnitZ + strafe * Vector3.UnitX) * SPEED;
            }



            HandleGroundCollision();
            HandleCollisions();
            UpdateSun();
            HandleAnimations();
        }

        private void HandleGroundCollision()
        {
            float distanceNearest = float.MaxValue;
            float yPositionNearest = Position.Y;
            bool groundFound = false;


            RayTerrainIntersectionSet rti = RaytraceTerrainBelowPositionExt(Position, RayMode.FourRaysY, 1f);
            if (rti.IsValid && rti.DistanceAvg <= 0.01f)
            {
                distanceNearest = rti.DistanceAvg;
                groundFound = true;
                yPositionNearest = rti.IntersectionPointAvg.Y;
            }

            if (_motionVertical <= 0f)
            {
                RayIntersectionExtSet rti2 = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.5f, -0.5f, 0.01f, typeof(GameObject));
                if (rti2.IsValid && Vector3.Dot(rti2.SurfaceNormalNearest, Vector3.UnitY) > 0.5f)
                {
                    if (groundFound)
                    {
                        if (rti2.DistanceMin < distanceNearest)
                        {
                            distanceNearest = rti2.DistanceMin;
                            yPositionNearest = rti2.IntersectionPointNearest.Y;
                        }
                    }
                    else
                    {
                        distanceNearest = rti2.DistanceMin;
                        groundFound = true;
                        yPositionNearest = rti2.IntersectionPointNearest.Y;
                    }
                }
            }

            if (groundFound && distanceNearest < 0.01f)
            {
                SetPositionY(yPositionNearest);
                if (_state == PrimaryState.InAir)
                {
                    _state = PrimaryState.OnGround;
                    _motionVertical = 0f;
                }
            }
            else
            {
                if (_state == PrimaryState.OnGround)
                {
                    _state = PrimaryState.InAir;
                    _motionVertical = 0f;
                }
            }
        }

        private bool HandleCollisions()
        {
            List<Intersection> intersections = GetIntersections();
            foreach (Intersection i in intersections)
            {
                float dot = Vector3.Dot(i.ColliderSurfaceNormal, Vector3.UnitY);
                if (dot <= 0.5f)
                    MoveOffset(i.MTV);
            }

            return intersections.Count > 0;
        }

        private void UpdateSun()
        {
            LightObject sun = CurrentWorld.GetLightObjectByName("Sun");
            if (sun == null)
                return;

            sun.SetPosition(Position.X + 50f, 50f, Position.Z + 50f);
            sun.SetTarget(Position.X, 0f, Position.Z);
        }

        private void HandleAnimations()
        {
            if (_motion.LengthSquared > PLAYER_MOVEMENT_MINIMUM)
            {
                SetAnimationID(2);
                SetAnimationPercentageAdvance(0.01f);
            }
            else
            {
                SetAnimationID(0);
                SetAnimationPercentageAdvance(0.001f);
            }
        }

        public Vector3 GetMotionVector()
        {
            return _motion;
        }

        public Vector3 GetMotionVectorLocal()
        {
            return _motionLocal;
        }
    }
}
