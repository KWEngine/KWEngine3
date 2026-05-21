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
        private Vector3 _motionLocal = Vector3.Zero;
        private Camera _cam;
        private float _speed = 0.01f;
        Dictionary<Keys, bool> keyPressDict = new()
        {
            {
                Keys.W, false
            },
            {
                Keys.A, false
            },
            {
                Keys.S, false
            },
            {
                Keys.D, false
            },
        };

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

            //UpdateDictionaryAndDirection();



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
            if (strafe != 0 || move != 0)
            {
                _motionLocal = Vector3.NormalizeFast(new Vector3(strafe, 0, move));
            }
            else
            {
                _motionLocal = Vector3.Zero;
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

        public Vector3 GetMotionVectorLocal()
        {
            return _motionLocal;
        }

        private void UpdateDictionaryAndDirection()
        {
            Vector3 newDirection = Vector3.Zero;
            if(Keyboard.IsKeyPressed(Keys.W))
            {
                keyPressDict[Keys.W] = true;
            }
            else
            {
                keyPressDict[Keys.W] = false;
            }

            if (Keyboard.IsKeyPressed(Keys.A))
            {
                keyPressDict[Keys.A] = true;
            }
            else
            {
                keyPressDict[Keys.A] = false;
            }

            if (Keyboard.IsKeyPressed(Keys.S))
            {
                keyPressDict[Keys.S] = true;
            }
            else
            {
                keyPressDict[Keys.S] = false;
            }

            if (Keyboard.IsKeyPressed(Keys.D))
            {
                keyPressDict[Keys.D] = true;
            }
            else
            {
                keyPressDict[Keys.D] = false;
            }

            if(Keyboard.IsKeyDown(Keys.W))
            {
                if (keyPressDict[Keys.S])
                {
                    newDirection = Vector3.Zero;
                }
                if (keyPressDict[Keys.D])
                {

                }
                if (keyPressDict[Keys.A])
                {

                }
            }
        }
    }
}
