using KWEngine3;
using KWEngine3.Exceptions;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;



namespace KWEngine3TestProject.Classes.WorldFollowerCam
{
    internal enum StateMovement
    {
        Idle = 0,                   // erstes bit
        Moving = 1                  // erstes bit
    }

    internal enum StateLocation { 
        OnGround = 2,
        InAir = 3
    }

    internal class Player : GameObject
    {
        private const float MIN_LOOKAT_LENGTH = 0.00001f;
        private const float PLAYER_MOVEMENT_MINIMUM = 0.00001f;
        private Vector3 _motion = Vector3.Zero;
        private Vector3 _motionLocal = Vector3.Zero;
        private float _speed = 0.01f;

        public Player()
        {
            SetModel("Brute");
            IsCollisionObject = true;
            IsShadowCaster = true;
            UpdateLast = true;

            
            SetScale(1f);
            SetHitboxToCapsule(1, Vector3.Zero, CapsuleHitboxType.Default);
            SetHitboxScale(1, 0.5f, 1f, 1f);
            SetRotation(0, 180, 0);
            SetAnimationID(0);

            HandleTerrain();
        }

        public override void Act()
        {
            int move = 0;
            int strafe = 0;

            Vector3 tmpLookAtVector = Vector3.Zero;
            Vector3 camLookAtXZ = CurrentWorld.CameraLookAtVectorXZ;
            Vector3 camLookAtRightXZ = CurrentWorld.CameraLookAtVectorLocalRightXZ;

            if (KWEngine.Keyboard.IsKeyDown(Keys.W))
            {
                move += 1;
                tmpLookAtVector += camLookAtXZ;
            }

            if (KWEngine.Keyboard.IsKeyDown(Keys.A))
            {
                strafe -= 1;
                tmpLookAtVector -= camLookAtRightXZ;
            }

            if (KWEngine.Keyboard.IsKeyDown(Keys.S))
            {
                move -= 1;
                tmpLookAtVector -= camLookAtXZ;
            }

            if (KWEngine.Keyboard.IsKeyDown(Keys.D))
            {
                strafe += 1;
                tmpLookAtVector += camLookAtRightXZ;
            }
            
            if (tmpLookAtVector.LengthSquared > MIN_LOOKAT_LENGTH)
            {
                
                tmpLookAtVector.NormalizeFast();
                TurnTowardsXZ(Center + tmpLookAtVector);
            }

            _motion = Vector3.Zero;
            _motionLocal = Vector3.Zero;
            if (move != 0 || strafe != 0)
            {
                _motion = MoveAndStrafeAlongCameraXZ(move, strafe, _speed);
                _motionLocal = Vector3.NormalizeFast(move * -Vector3.UnitZ + strafe * Vector3.UnitX) * _speed;
            }

            HandleTerrain();
            UpdateSun();
            HandleAnimations();
        }

        private void HandleTerrain()
        {
            RayTerrainIntersection rti = RaytraceTerrainBelowPosition(Position);
            if (rti.IsValid)
            {
                SetPositionY(rti.IntersectionPoint.Y);
            }
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
            if(_motion.LengthSquared > PLAYER_MOVEMENT_MINIMUM)
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
