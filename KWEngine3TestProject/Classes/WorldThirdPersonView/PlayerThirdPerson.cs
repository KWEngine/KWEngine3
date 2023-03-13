using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldThirdPersonView
{
    public class PlayerThirdPerson : GameObject
    {
        private float _rotationScaleFactor = 1f;
        private Vector2 _currentCameraRotation = new Vector2(0, 0);
        private float _limitYUp = 5;
        private float _limitYDown = -75;

        public override void Act()
        {
            // Wenn sich die Spielfigur mitdrehen soll, muss hier
            // die Mausbewegung auch zur Player-Rotation addiert werden!
            this.AddRotationY(-MouseMovement.X * KWEngine.MouseSensitivity);

            // Diese Methode berechnet die neue Kameraposition:
            UpdateCameraPosition(MouseMovement * KWEngine.MouseSensitivity);
        }

        private void UpdateCameraPosition(Vector2 msMovement)
        {
            // Berechne anhand der Mausbewegung, um wieviel Grad die Kamera
            // sich drehen müsste:
            _currentCameraRotation.X += msMovement.X * _rotationScaleFactor;
            _currentCameraRotation.Y += msMovement.Y * _rotationScaleFactor;
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
            Vector3 offsetCamPos = HelperRotation.RotateVector(lookAtVector, -90, Plane.XZ) + lookAtVector * 5 * lav_factor;
            Vector3 offsetCamTarget = HelperRotation.RotateVector(lookAtVector, -90, Plane.XZ) + lookAtVector * 2 + Vector3.UnitY * 2 * lav_factor2;

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
