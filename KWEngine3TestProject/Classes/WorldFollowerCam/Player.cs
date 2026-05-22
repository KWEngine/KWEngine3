using KWEngine3.Exceptions;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;



namespace KWEngine3TestProject.Classes.WorldFollowerCam
{
    internal class Player : GameObject
    {
        private Vector3 _motion = Vector3.Zero;
        private Vector3 _motionInput = Vector3.Zero;
        private Camera _cam;
        private float _speed = 0.01f;

        public Player(Camera cam)
        {
            if (cam == null || !cam.IsInCurrentWorld)
                throw new EngineException("Camera not in world yet. Cannot proceed.");
            _cam = cam;

            SetModel("Robot");
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

            if (Keyboard.IsKeyDown(Keys.A))
            {
                strafe--;
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                strafe++;
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                move++;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                move--;
            }

            _motion = MoveAndStrafeAlongCameraXZ(move, strafe, _speed);
            if(_cam != null)
            {
                _cam.UpdateViewFor(this);
            }
        }

        public Vector3 GetMotionVector()
        {
            return _motion;
        }

        public Vector3 GetMotionInputVector()
        {
            return _motionInput;
        }
    }
}
