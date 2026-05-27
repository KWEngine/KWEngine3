using KWEngine3;
using KWEngine3.Exceptions;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldFollowerCam
{
    internal class Camera : GameObject
    {
        private Player _parent;

        private const float SIMULATION_FRAME_TIME = 1f / 240f;
        private const float MOUSE_MOVEMENT_MINUMUM = 0.00001f;
        private const float PLAYER_MOVEMENT_MINIMUM = 0.00001f;

        private const float ARCBALL_LIMIT_Y_UP = -5f;
        private const float ARCBALL_LIMIT_Y_DOWN = -50f;

        private const float FOLLOW_DISTANCE = 4f;
        private const float FOLLOW_OFFSET_Y_NEAR = 1f;
        private const float FOLLOW_OFFSET_Y_FAR = 2f;

        private const float STRAFE_THRESHOLD = 0.8f;

        private float _yawFollowStrength = 0.5f;    // Positionsglättung für die Anpassung der Kamera hinter die Spielfigur wenn sie sich nach vorne bewegt
        private float _distanceFollowStrength = 0.1f;
        private float _heightFollowStrength = 2f;
        private float _mouseRotationStrength = 10f; // Nur für die Positionsannäherung bei aktiver Maus

        private float _yTarget = 0f;

        private Vector2 _currentRotation = Vector2.Zero; // X = yaw, Y = pitch
        private float _currentDistance = FOLLOW_DISTANCE;

        public Camera(Player player)
        {
            if (player.IsInCurrentWorld)
            {
                _parent = player;

                SetModel("KWSphere");
                SetScale(0.25f);

                ResetView();

                SkipRender = true;

                KWEngine.CurrentWorld.AddGameObject(this);
            }
            else
            {
                throw new GameObjectException("FATAL ERROR: Player object not added to world yet.");
            }
        }

        private void ResetView()
        {
            _currentDistance = FOLLOW_DISTANCE;
            _yTarget = FOLLOW_OFFSET_Y_NEAR;

            Vector3 pivot = _parent.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);
            Vector3 positionInit = pivot - Vector3.UnitZ * FOLLOW_DISTANCE + new Vector3(0f, FOLLOW_OFFSET_Y_FAR, 0f);
            _yTarget = pivot.Y;

            Vector3 directionToPlayer = HelperVector.GetDirectionFromVectorToVector(positionInit, pivot);
            Vector3 directionToPlayerXZ = new Vector3(directionToPlayer.X, 0f, directionToPlayer.Z);

            Vector3 newPosition = pivot + directionToPlayerXZ * FOLLOW_DISTANCE + new Vector3(0f, FOLLOW_OFFSET_Y_FAR, 0f);

            SetPosition(newPosition);
            TurnTowardsXYZ(pivot);

            ArcballRotation rot = HelperRotation.GetArcballRotation(newPosition, pivot, false, false);
            _currentRotation.X = rot.XAngle;
            _currentRotation.Y = rot.YAngle;

            CurrentWorld.SetCameraPosition(newPosition);
            CurrentWorld.SetCameraTarget(newPosition + LookAtVector);
        }

        public override void Act()
        {
            UpdatePositionForThirdPersonView();
        }

        private void UpdatePositionForThirdPersonView()
        {
            if (!IsInCurrentWorld || _parent == null || !_parent.IsInCurrentWorld)
                return;

            // Get the player's motion indicators (and normalize a copy of each):
            Vector3 pMotion = _parent.GetMotionVector();
            Vector3 pMotionN = Vector3.NormalizeFast(pMotion);
            Vector3 pMotionLocal = _parent.GetMotionVectorLocal();
            Vector3 pMotionLocalN = Vector3.NormalizeFast(pMotionLocal);

            // Recalculate current target coordinates:
            // (this smoothes the target height a little)
            Vector3 pivotRaw = _parent.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);
            float tVertical = 1f - MathF.Exp(-_heightFollowStrength * SIMULATION_FRAME_TIME);
            _yTarget += (pivotRaw.Y - _yTarget) * tVertical;
            Vector3 tmpTarget = new Vector3(pivotRaw.X, _yTarget, pivotRaw.Z);

            // Update cam distance value:
            // (smooth it if it deviates from FOLLOW_DISTANCE)
            float tFollow = 1f - MathF.Exp(-_distanceFollowStrength * SIMULATION_FRAME_TIME);
            _currentDistance += (FOLLOW_DISTANCE - _currentDistance) * tFollow;

            Vector3 tmpPosition = Position;
            if(CurrentWorld.MouseMovement.LengthSquared > MOUSE_MOVEMENT_MINUMUM)
            {
                // Mouse is being moved:
                _currentRotation.X += CurrentWorld.MouseMovement.X;
                _currentRotation.Y += CurrentWorld.MouseMovement.Y;
                if (_currentRotation.Y < ARCBALL_LIMIT_Y_DOWN)
                {
                    _currentRotation.Y = ARCBALL_LIMIT_Y_DOWN;
                }
                if (_currentRotation.Y > ARCBALL_LIMIT_Y_UP)
                {
                    _currentRotation.Y = ARCBALL_LIMIT_Y_UP;
                }
            }
            else
            {
                // Mouse is not being moved. But if the player is moving forward (locally),
                // then let the camera move behind him slowly:
                // (currently inactive because the player turns with the camera)
                float pMotionDotProduct = Math.Max(0f, Vector3.Dot(-Vector3.UnitZ, pMotionLocalN));
                if (pMotionDotProduct > STRAFE_THRESHOLD)
                {
                    _currentDistance = Math.Clamp(_currentDistance * 1.001f, FOLLOW_DISTANCE, FOLLOW_DISTANCE * 1.5f);

                    float tAngle = 1f - MathF.Exp(-_yawFollowStrength * SIMULATION_FRAME_TIME);
                    ArcballRotation arc = HelperRotation.GetArcballRotation(tmpTarget - pMotionN * _currentDistance, tmpTarget, false, false);
                    float deltaAngle = NormalizeAngle180(arc.XAngle - _currentRotation.X);
                    _currentRotation.X += deltaAngle * tAngle;
                    _currentRotation.X = NormalizeAngle180(_currentRotation.X);
                    
                }
            }

            // calculate new cam position:
            tmpPosition = HelperRotation.CalculateRotationForArcBallCamera(
                tmpTarget,
                _currentDistance,
                _currentRotation.X,
                _currentRotation.Y,
                false,
                false
            );


            CurrentWorld.SetCameraPosition(tmpPosition);
            CurrentWorld.SetCameraTarget(tmpTarget);
        }


        private float NormalizeAngle180(float angle)
        {
            angle %= 360f;

            if (angle >= 180f)
                angle -= 360f;
            else if (angle < -180f)
                angle += 360f;

            return angle;
        }

    }
}
