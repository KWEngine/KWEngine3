using KWEngine3;
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
        private const float SIMULATION_FRAME_TIME = 1f / 240f;  // Nur wichtig für das Interpolieren (Lerp)
        private const float MOUSE_MOVEMENT_MINUMUM = 0.001f;    // Menge an Mausbewegung, ab die der Orb die Maus als "sich bewegend" sieht

        private const float LIMIT_Y_UP = -5f;                   // Limit für Y-Achse (oben, in Grad)
        private const float LIMIT_Y_DOWN = -50f;                // Limit für Y-Achse (unten, in Grad)
        private const float FOLLOW_DISTANCE = 8f;              // Standarddistanz zum Player
        private const float FOLLOW_OFFSET_Y_FAR = 2f;           // Y-Abstand zur Player-Höhe direkt am Orb
        private const float FOLLOW_OFFSET_Y_NEAR = 1f;          // Y-Abstand zur Player-Höhe (für den Messpunkt direkt am Player)

        private float _followStrength = 0.5f;                    // Je höher die Zahl, desto schneller passt sich die Kamera (bzw. der Orb) an

        private Vector2 currentRotation = Vector2.Zero;         // Enthält die aktuelle Kamerarotation
        private float _distance = FOLLOW_DISTANCE;                           // Enthält die aktuelle Distanz zum Player (pivot point)
        private float _distanceXYZ = 0f;

        public Camera()
        {
            SetModel("KWSphere");
            SetScale(0.1f);
            SetColorEmissive(1, 1, 0, 1.5f);
            SetPosition(0, 5, 5);
            
            IsCollisionObject = true;
        }

        public override void Act()
        {
        }

        public void ResetViewFor(Player p)
        {
            Vector3 pivot = p.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);
            Vector3 directionToPlayerXZ = HelperVector.GetDirectionFromVectorToVectorXZ(Position, pivot);
            Vector3 newPosition = pivot - directionToPlayerXZ * FOLLOW_DISTANCE + new Vector3(0f, Position.Y - pivot.Y, 0f);
            SetPosition(newPosition);
            TurnTowardsXYZ(pivot);
        }

        public void UpdateViewFor(Player p)
        {
            if (!IsInCurrentWorld || p == null || !p.IsInCurrentWorld)
                return;

            // Berechne den neuen Dreh-/Angelpunkt anhand der Spielermitte:
            Vector3 pivot = p.Center + new Vector3(0f, FOLLOW_OFFSET_Y_NEAR, 0f);

            // Dieser Wert wird fast immer weiter unten überschrieben:
            Vector3 newPosition = Position;
            Vector3 directionToPlayerXZ = HelperVector.GetDirectionFromVectorToVectorXZ(newPosition, pivot);
            Vector3 directionToPlayer = HelperVector.GetDirectionFromVectorToVector(newPosition, pivot);

            Vector3 finalPos;

            // Hole anhand der aktuellen Position des Orbs und dem berechneten Pivot (Player-Position) die Orbrotation:
            ArcballRotation rot = HelperRotation.GetArcballRotation(Position, pivot, false, false);
            // Überschreibe currentRotation mit diesen Grad-Werten:
            currentRotation.X = rot.XAngle;
            currentRotation.Y = rot.YAngle;

            float t = 1f - MathF.Exp(-_followStrength * SIMULATION_FRAME_TIME);

            // Wenn sich die Maus irgendwie bewegt..
            if (CurrentWorld.MouseMovement.LengthSquared > MOUSE_MOVEMENT_MINUMUM)
            {
                // Berechne den Interpolationsfaktor _t:
                float tRotation = 1f - MathF.Exp(-_followStrength * 50f * SIMULATION_FRAME_TIME);
                

                // Interpoliere die neue Distanz des Orbs zwischen der eigentlichen Follow-Distanz und der aktuell gemessenen Distanz:
                _distance = _distance + (FOLLOW_DISTANCE - _distance) * t;

                // Ändere die Rotation gemäß Mausbewegung:
                currentRotation.X += CurrentWorld.MouseMovement.X;
                currentRotation.Y += CurrentWorld.MouseMovement.Y;
                if (currentRotation.Y > LIMIT_Y_UP)
                    currentRotation.Y = LIMIT_Y_UP;
                if (currentRotation.Y < LIMIT_Y_DOWN)
                    currentRotation.Y = LIMIT_Y_DOWN;

                newPosition = HelperRotation.CalculateRotationForArcBallCamera(
                    pivot,
                    _distance,
                    currentRotation.X,
                    currentRotation.Y,
                    false,
                    false);

                finalPos = Vector3.Lerp(Position, newPosition, tRotation);
            }
            else
            {
                Vector3 pMotion = p.GetMotionVector();
                Vector3 pMotionNormalized = Vector3.NormalizeFast(pMotion);

                // Wenn der Player sich gerade aktiv bewegt, berechne die neue Position anhand der Follow-Distanz:
                if (pMotion != Vector3.Zero)
                {
                     // Interpoliere die neue Distanz des Orbs zwischen der eigentlichen Follow-Distanz und der aktuell gemessenen Distanz:
                    _distance = _distance + (FOLLOW_DISTANCE - _distance) * t;

                    float dot = Vector3.Dot(directionToPlayerXZ, pMotionNormalized);
                    dot = HelperVector.SmootherStep(0f, 1f, dot);
                    if(dot > 0)
                    {
                        newPosition = 
                            (  dot   ) * (pivot - directionToPlayer * _distance) + 
                            (1f - dot) * (pivot - (Vector3.NormalizeFast(pMotionNormalized + new Vector3(0f, (pivot - directionToPlayer).Y, 0f)) * _distance));
                    }
                    else
                    {
                        newPosition = pivot - directionToPlayer * _distance;
                    }
                    
                }

                newPosition = pivot - directionToPlayer * _distance;
                finalPos = Vector3.Lerp(Position, newPosition, t);
            }

            // Interpoliere (bei Vector3 nennt man das Lerpen) die neue Position und die alte Position gemäß des Faktors _t:
            SetPosition(finalPos);

            _distanceXYZ = (finalPos - pivot).LengthFast;

            TurnTowardsXYZ(pivot);

            CurrentWorld.SetCameraPosition(finalPos);
            CurrentWorld.SetCameraTarget(pivot);

            p.TurnTowardsXZ(pivot + CurrentWorld.CameraLookAtVectorXZ);
        }
    }
}
