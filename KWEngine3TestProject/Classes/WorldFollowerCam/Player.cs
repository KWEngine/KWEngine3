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
        private Vector3 _motion = Vector3.Zero;
        //private Camera _cam;
        //private uint _state = 
        private float _speed = 0.01f;

        public Player()
        {
            SetModel("Brute");
            IsCollisionObject = true;
            UpdateLast = true;

            SetPosition(0f, 0f, 0f);
            SetScale(1f);
            SetHitboxScale(0.5f, 1f, 1f);
            SetRotation(0, 180, 0);
            SetAnimationID(0);
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
            
            if (tmpLookAtVector.LengthSquared > 0.1f)
            {
                
                tmpLookAtVector.NormalizeFast();
                TurnTowardsXZ(Center + tmpLookAtVector);
            }

            _motion = Vector3.Zero;
            if (move != 0 || strafe != 0)
            {
                MoveAndStrafeAlongCameraXZ(move, strafe, _speed);
                _motion = MoveAndStrafeAlongCameraXZ(move, strafe, _speed); 
            }
        }

        public Vector3 GetMotionVector()
        {
            return _motion;
        }
    }
}
