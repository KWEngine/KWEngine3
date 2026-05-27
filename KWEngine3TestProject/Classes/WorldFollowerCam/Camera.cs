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

        private const float STRAFE_THRESHOLD = 0.5f;

        private float _yawFollowStrength = 8f;      // Positionsglättung
        private float _pitchFollowStrength = 2f;    // Verzögerung bei Höhenänderungen
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

            Vector3 pivot = _parent.Center + new Vector3(0f, _yTarget, 0f);

            Vector3 directionToPlayer = HelperVector.GetDirectionFromVectorToVector(Position, pivot);
            Vector3 directionToPlayerXZ = new Vector3(directionToPlayer.X, 0f, directionToPlayer.Z);

            Vector3 newPosition = pivot - directionToPlayerXZ * FOLLOW_DISTANCE + new Vector3(0f, FOLLOW_OFFSET_Y_FAR, 0f);

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
            Console.WriteLine("------------------");
            if (!IsInCurrentWorld || _parent == null || !_parent.IsInCurrentWorld)
                return;

            

            Vector3 pivotRaw = _parent.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);

            float tVertical = 1f - MathF.Exp(-_pitchFollowStrength * SIMULATION_FRAME_TIME);
            _yTarget += (pivotRaw.Y - _yTarget) * tVertical;

            Vector3 newTarget = new Vector3(pivotRaw.X, _yTarget, pivotRaw.Z);

            // Movement:
            Vector3 pMotion = _parent.GetMotionVector();
            Vector3 pMotionNormalized = Vector3.NormalizeFast(pMotion);
            Vector3 pMotionLocal = _parent.GetMotionVectorLocal();
            bool mouseIsMoving = CurrentWorld.MouseMovement.LengthSquared > MOUSE_MOVEMENT_MINUMUM;
            float playerLocalForwardFactor = Vector3.Dot(-Vector3.UnitZ, pMotionLocal);
            Console.WriteLine("dot local forward: " + playerLocalForwardFactor);

            Vector3 directionToPlayerXZ = HelperVector.GetDirectionFromVectorToVectorXZ(Position, newTarget);
            if (directionToPlayerXZ.LengthSquared <= PLAYER_MOVEMENT_MINIMUM)
                directionToPlayerXZ = -Vector3.UnitZ;

            Vector3 newPosition = newTarget + Vector3.UnitZ * _currentDistance + new Vector3(0f, FOLLOW_OFFSET_Y_FAR, 0f);

            CurrentWorld.SetCameraPosition(newPosition);
            CurrentWorld.SetCameraTarget(newTarget);

            ArcballRotation rot = HelperRotation.GetArcballRotation(newPosition, newTarget, false, false);
            _currentRotation.X = rot.XAngle;
            _currentRotation.Y = rot.YAngle;
        }

        private static float LerpAngleDegrees(float current, float target, float t)
        {
            float delta = target - current;

            while (delta > 180f)
                delta -= 360f;

            while (delta < -180f)
                delta += 360f;

            return current + delta * t;
        }
    }
}
