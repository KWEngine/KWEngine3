using KWEngine3;
using KWEngine3.Exceptions;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldFollowerCam
{
    internal class Camera : GameObject
    {
        private Player _parent;

        private const float SIMULATION_FRAME_TIME = 1f / 240f;
        private const float MOUSE_MOVEMENT_MINUMUM = 0.00001f;

        private const float LIMIT_Y_UP = -5f;
        private const float LIMIT_Y_DOWN = -50f;

        private const float FOLLOW_DISTANCE = 4f;
        private const float FOLLOW_OFFSET_Y_NEAR = 1f;

        private const float STRAFE_THRESHOLD = 0.5f;

        private float _followStrength = 8f;            // Positionsglättung
        private float _yawFollowStrength = 7f;         // Auto-Follow für links/rechts
        private float _verticalFollowStrength = 2f;    // Verzögerung bei Höhenänderungen
        private float _mouseRotationStrength = 14f;    // Nur für die Positionsannäherung bei aktiver Maus

        private float _smoothedPivotY;
        private bool _pivotYInitialized = false;
        private bool _rotationInitialized = false;

        private Vector2 currentRotation = Vector2.Zero; // X = yaw, Y = pitch
        private float _distance = FOLLOW_DISTANCE;

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
            Vector3 pivotRaw = _parent.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);

            _smoothedPivotY = pivotRaw.Y;
            _pivotYInitialized = true;

            Vector3 pivot = new Vector3(pivotRaw.X, _smoothedPivotY, pivotRaw.Z);

            Vector3 directionToPlayerXZ = HelperVector.GetDirectionFromVectorToVectorXZ(Position, pivot);

            if (directionToPlayerXZ.LengthSquared <= 0.000001f)
                directionToPlayerXZ = -Vector3.UnitZ;

            Vector3 newPosition = pivot - directionToPlayerXZ * FOLLOW_DISTANCE + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);

            SetPosition(newPosition);
            TurnTowardsXYZ(pivot);

            ArcballRotation rot = HelperRotation.GetArcballRotation(newPosition, pivot, false, false);
            currentRotation.X = rot.XAngle;
            currentRotation.Y = rot.YAngle;
            _rotationInitialized = true;

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

            // Pivot (Dreh- und Angelpunkt) aktualisieren:
            Vector3 pivotRaw = _parent.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);

            if (!_pivotYInitialized)
            {
                _smoothedPivotY = pivotRaw.Y;
                _pivotYInitialized = true;
            }

            float tVertical = 1f - MathF.Exp(-_verticalFollowStrength * SIMULATION_FRAME_TIME);
            _smoothedPivotY += (pivotRaw.Y - _smoothedPivotY) * tVertical;

            Vector3 pivot = new Vector3(pivotRaw.X, _smoothedPivotY, pivotRaw.Z);

            // Rotation:
            if (!_rotationInitialized)
            {
                ArcballRotation rotInit = HelperRotation.GetArcballRotation(Position, pivot, false, false);
                currentRotation.X = rotInit.XAngle;
                currentRotation.Y = rotInit.YAngle;
                _rotationInitialized = true;
            }

            // Movement:
            Vector3 pMotion = _parent.GetMotionVector();
            Vector3 flatMotion = new Vector3(pMotion.X, 0f, pMotion.Z);

            bool hasHorizontalMotion = flatMotion.LengthSquared > 0.000001f;
            Vector3 flatMotionNormalized = hasHorizontalMotion
                ? Vector3.NormalizeFast(flatMotion)
                : Vector3.Zero;

            bool mouseIsMoving = CurrentWorld.MouseMovement.LengthSquared > MOUSE_MOVEMENT_MINUMUM;

            Vector3 directionToPlayerXZ = HelperVector.GetDirectionFromVectorToVectorXZ(Position, pivot);
            if (directionToPlayerXZ.LengthSquared <= 0.000001f)
                directionToPlayerXZ = -Vector3.UnitZ;

            float tFollow = 1f - MathF.Exp(-_followStrength * SIMULATION_FRAME_TIME);
            float tYaw = 1f - MathF.Exp(-_yawFollowStrength * SIMULATION_FRAME_TIME);
            float tMousePosition = 1f - MathF.Exp(-_mouseRotationStrength * SIMULATION_FRAME_TIME);

            // Distanz immer weich in Richtung und mit Standarddistanz
            _distance += (FOLLOW_DISTANCE - _distance) * tFollow;

            // Auf Mausbewegung prüfen:
            if (mouseIsMoving)
            {
                // Maus steuert direkt Yaw + Pitch
                currentRotation.X += CurrentWorld.MouseMovement.X;
                currentRotation.Y += CurrentWorld.MouseMovement.Y;

                if (currentRotation.Y > LIMIT_Y_UP)
                    currentRotation.Y = LIMIT_Y_UP;
                if (currentRotation.Y < LIMIT_Y_DOWN)
                    currentRotation.Y = LIMIT_Y_DOWN;
            }
            else if (hasHorizontalMotion)
            {
                float dotLeftRight = Vector3.Dot(CurrentWorld.CameraLookAtVectorLocalRightXZ, flatMotionNormalized);

                bool isStrafing = MathF.Abs(dotLeftRight) > STRAFE_THRESHOLD;

                if (!isStrafing)
                {
                    float dot = Vector3.Dot(directionToPlayerXZ, flatMotionNormalized);

                    Vector3 desiredDirectionToPlayerXZ;

                    if (dot > 0f)
                    {
                        desiredDirectionToPlayerXZ = Vector3.NormalizeFast(
                            dot * directionToPlayerXZ + (1f - dot) * flatMotionNormalized
                        );
                    }
                    else
                    {
                        desiredDirectionToPlayerXZ = directionToPlayerXZ;
                    }

                    Vector3 yawTargetPosition = pivot - desiredDirectionToPlayerXZ * _distance;

                    ArcballRotation targetRot = HelperRotation.GetArcballRotation(
                        yawTargetPosition,
                        pivot,
                        false,
                        false);

                    currentRotation.X = LerpAngleDegrees(currentRotation.X, targetRot.XAngle, tYaw);
                }

                // Wenn gestrafet wird:
                // currentRotation.X bleibt einfach unverändert.
                // Dadurch wandert die Kamera automatisch mit dem Pivot mit
                // und fühlt sich wie echtes strafen an.
            }


            Vector3 desiredPosition = HelperRotation.CalculateRotationForArcBallCamera(
                pivot,
                _distance,
                currentRotation.X,
                currentRotation.Y,
                false,
                false);

            float tPosition = mouseIsMoving ? tMousePosition : tFollow;
            Vector3 finalPos = Vector3.Lerp(Position, desiredPosition, tPosition);


            SetPosition(finalPos);
            TurnTowardsXYZ(pivot);

            CurrentWorld.SetCameraPosition(finalPos);
            CurrentWorld.SetCameraTarget(finalPos + LookAtVector);

            _parent.TurnTowardsXZ(pivot + CurrentWorld.CameraLookAtVectorXZ);
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
