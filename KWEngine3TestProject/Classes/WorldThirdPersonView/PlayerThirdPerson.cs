using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldThirdPersonView
{
    public class PlayerThirdPerson : GameObject
    {
        private float _rotationScaleFactor = 1f;
        private Vector2 _currentCameraRotation = new Vector2(0, -5);
        private float _limitYUp = 5;
        private float _limitYDown = -75;
        private PlayerState _state = PlayerState.Fall;
        private float _gravity = 0.005f;
        private float _speed = 0.025f;
        private float _momentum = 0;
        private bool _upKeyPressed = false;
        private readonly float _cooldown = 0.25f;
        private float _lastShot = -1;
        private bool _debug = false;

        private AimingSphere _aimingSphere = null;

        private enum PlayerState
        {
            OnFloor = 0,
            Jump = 2,
            Fall = 3
        }

        public void SetAimingSphere(AimingSphere g)
        {
            _aimingSphere = g;
        }

        public override void Act()
        {
            if (Position.Y < -25)
            {
                SetPosition(0, 0, 0);
                return;
            }

            // Wenn sich die Spielfigur mitdrehen soll, muss hier
            // die Mausbewegung auch zur Player-Rotation addiert werden!
            this.AddRotationY(-MouseMovement.X * KWEngine.MouseSensitivity);

            // Diese Methode berechnet die neue Kameraposition:
            UpdateCameraPosition(MouseMovement * KWEngine.MouseSensitivity);

            bool running = false;
            int move = 0;
            int strafe = 0;
            if(Keyboard.IsKeyDown(Keys.A))
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

            if (move != 0 || strafe != 0)
                running = true;
            MoveAndStrafeAlongCameraXZ(move, strafe, _speed);

            if (_state == PlayerState.OnFloor && (Keyboard.IsKeyDown(Keys.Space) || Mouse.IsButtonDown(MouseButton.Right)))
            {
                if (!_upKeyPressed)
                {
                    _state = PlayerState.Jump;
                    SetAnimationPercentage(0);
                    _momentum = 0.25f;
                    _upKeyPressed = true;
                }
            }
            else if (!(Keyboard.IsKeyDown(Keys.Space) || Mouse.IsButtonDown(MouseButton.Right)))
            {
                _upKeyPressed = false;
            }

            if (Keyboard.IsKeyDown(Keys.LeftShift) || Mouse.IsButtonDown(MouseButton.Left))
            {
                DoShoot();
            }

            if(_debug && _aimingSphere != null)
            {
                bool result = HelperIntersection.IsMouseCursorOnAny<Immovable>(out Immovable o, false);
                Vector3 target;
                if (result)
                {
                    result = HelperIntersection.GetIntersectionPointOnObjectForRay(o, CurrentWorld.CameraPosition, HelperIntersection.GetMouseRay(), out target, out Vector3 faceNormal);
                    if(result)
                    {
                        _aimingSphere.SetPosition(target);
                        _aimingSphere.SetOpacity(1);
                    }
                    else
                    {
                        _aimingSphere.SetOpacity(0);
                    }
                }
                else
                {
                    _aimingSphere.SetOpacity(0);
                }
            }
          
            DoStates();
            DoCollisionDetection();
            DoAnimation(running);
        }

        private void DoShoot()
        {
            if (WorldTime - _lastShot > _cooldown)
            {
                bool result = HelperIntersection.IsMouseCursorOnAny<Immovable>(out Immovable o, false);
                Vector3 target;
                if (result)
                {
                    result = HelperIntersection.GetIntersectionPointOnObjectForRay(o, CurrentWorld.CameraPosition, HelperIntersection.GetMouseRay(), out target, out Vector3 faceNormal);
                    if(!result)
                    {
                        target = HelperIntersection.GetMouseIntersectionPointOnPlane(Center + LookAtVector * 10, Plane.Camera);
                    }
                }
                else
                {
                    target = HelperIntersection.GetMouseIntersectionPointOnPlane(Center + LookAtVector * 10, Plane.Camera);
                }

                Shot s = new Shot();
                s.SetModel("KWSphere");
                s.SetScale(0.125f);
                s.SetColor(0, 0, 1);
                s.SetColorEmissive(0, 0, 1, 1.25f);
                s.IsCollisionObject = true;
                s.SetPosition(Center + LookAtVector * 0.75f);
                s.TurnTowardsXYZ(target);

                CurrentWorld.AddGameObject(s);
                _lastShot = WorldTime;  
            }
        }

        private void DoStates()
        {
            if (_state == PlayerState.Jump)
            {
                MoveOffset(0, _momentum, 0);
                _momentum -= _gravity;
                if (_momentum < 0)
                {
                    _momentum = 0;
                    _state = PlayerState.Fall;
                }
            }
            else if (_state == PlayerState.Fall)
            {
                MoveOffset(0, _momentum, 0);
                _momentum -= _gravity;
            }
            else if (_state == PlayerState.OnFloor)
            {
                MoveOffset(0, -0.0001f, 0);
            }
        }

        private void DoCollisionDetection()
        {
            List<Intersection> collisionlist = GetIntersections();
            bool upCorrection = false;
            float maxYUpCorrection = 0;
            foreach (Intersection i in collisionlist)
            {
                if (i.Object is Shot)
                    continue;

                if (i.MTV.Y > maxYUpCorrection)
                    maxYUpCorrection = i.MTV.Y;

                MoveOffset(new Vector3(i.MTV.X, 0, i.MTV.Z));
                if (i.MTV.Y > 0)
                {
                    if (_state == PlayerState.OnFloor)
                    {
                        upCorrection = true;
                    }
                    else if (_state == PlayerState.Fall)
                    {
                        upCorrection = true;
                        _state = PlayerState.OnFloor;
                    }
                }
                else if (i.MTV.Y < 0 && Math.Abs(i.MTV.Y) > Math.Abs(i.MTV.X) && Math.Abs(i.MTV.Y) > Math.Abs(i.MTV.Z))
                {
                    if (_state == PlayerState.Jump)
                    {
                        _state = PlayerState.Fall;
                        _momentum = 0;
                        SetAnimationPercentage(0.5f);
                    }
                }
            }
            MoveOffset(0, maxYUpCorrection, 0);

            if (_state == PlayerState.OnFloor && !upCorrection)
            {
                _state = PlayerState.Fall;
                _momentum = 0;
                SetAnimationPercentage(0.5f);
            }
        }
        private void DoAnimation(bool running)
        {
            if (this.HasAnimations)
            {
                if (_state == PlayerState.OnFloor)
                {
                    if (running)
                    {
                        SetAnimationID(2);
                        SetAnimationPercentageAdvance(0.01f);
                    }
                    else
                    {
                        SetAnimationID(0);
                        SetAnimationPercentageAdvance(0.00125f);
                    }
                }
                else if (_state == PlayerState.Jump || _state == PlayerState.Fall)
                {
                    SetAnimationID(3);
                    SetAnimationPercentageAdvance(0.0075f);
                }
            }
        }

        private void UpdateCameraPosition(Vector2 msMovement)
        {
            // Berechne anhand der Mausbewegung, um wieviel Grad die Kamera
            // sich drehen müsste:
            _currentCameraRotation.X += msMovement.X * _rotationScaleFactor;
            _currentCameraRotation.Y -= msMovement.Y * _rotationScaleFactor;
            // Damit die Kamera nicht "über Kopf" geht, wird die Rotation nach
            // oben und unten begrenzt:
            if (_currentCameraRotation.Y < _limitYDown)
            {
                _currentCameraRotation.Y = _limitYDown;
            }
            if (_currentCameraRotation.Y > _limitYUp)
            {
                _currentCameraRotation.Y = _limitYUp;
            }
            // Erfrage aktuelle Blickrichtung und Position der Spielfigur:
            Vector3 lookAtVector = LookAtVector;
            Vector3 playerPosition = Center;
            // Berechne für Kameraposition und -ziel einen individuellen Offset-Wert:
            float lav_factor = (0.00012f * (_currentCameraRotation.Y * _currentCameraRotation.Y) + 0.02099f * _currentCameraRotation.Y + 0.89190f);
            float lav_factor2 = _currentCameraRotation.Y >= -15 ? (_currentCameraRotation.Y + 15) / 20f : 0f;
            Vector3 offsetCamPos = HelperRotation.RotateVector(lookAtVector, 90, Plane.XZ) + lookAtVector * 5 * lav_factor;
            Vector3 offsetCamTarget = HelperRotation.RotateVector(lookAtVector, 90, Plane.XZ) + lookAtVector * 2 + Vector3.UnitY * 2 * lav_factor2;

            // Berechne die neue Kameraposition anhand der gesammelten Infos:
            Vector3 newCamPos = HelperRotation.CalculateRotationForArcBallCamera(
                    playerPosition,                  // Drehpunkt
                    10f,                             // Distanz zum Drehpunkt
                    _currentCameraRotation.X,        // Drehung links/rechts
                    _currentCameraRotation.Y,        // Drehung oben/unten
                    false,                           // invertiere links/rechts?
                    false                            // invertiere oben/unten?
            );
            // Setze die neue Kameraposition und das Kameraziel:
            CurrentWorld.SetCameraPosition(newCamPos + offsetCamPos);
            CurrentWorld.SetCameraTarget(playerPosition + offsetCamTarget);
        }
    }
}
