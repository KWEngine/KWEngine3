using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldPlaneMTVCollider
{
    public class Player : GameObject
    {
        // Aufzählungsdatentypen:
        private enum State
        {
            OnGround,
            InAir
        }

        // Konstanten:
        private const float VELOCITY_JUMP = 0.05f;
        private const float VELOCITY_REDUCTION = 1.033f;
        private const float GRAVITY = 0.0005f;
        public static readonly Vector3 PLAYER_START = new Vector3(0f, 0f, 0f);

        // Für die Steuerung der Spielfigur benötigte Eigenschaften:
        private Vector2 _velocityXZ = Vector2.Zero;
        private float _velocityY = 0f;
        private float _playerSpeed = 0.02f;
        
        private State _currentState = State.OnGround;

        //Für die Animation der Spielfigur benötigte Eigenschaften:
        private float _animationStep = 0f;
        private bool _animationNeedsToChange = false;

        public override void Act()
        {
            // Prüfe, ob Kontakt zum Boden besteht und setze
            // _currentState dementsprechend auf den korrekten
            // Wert:
            Vector3 currentSlopeNormal = HandleGroundDetection();

            // Prüfe, welche Steuerungstasten gedrückt wurden und
            // bestimme so die durch die Eingabetasten entstehende
            // Bewegung:
            Vector2 inputVelocity = HandleMovement();

            // Wende die durch die Eingabe entstandene Geschwindigkeit
            // auf die aktuell für die Spielfigur gültige 
            // Geschwindigkeit an:
            ApplyVelocity(inputVelocity, currentSlopeNormal);

            // Prüfe, ob interaktive Objekte (Gegenstände, bewegliche
            // Plattformen, usw.) berührt werden:
            HandleIntersectionsWithObjects();
            
            // Passe die Animationen der Spielfigur dem aktuellen
            // Status (_currentState) an:
            HandleAnimation();

            // Falls die Spielfigur vom Rand der Welt fällt, 
            // setze sie wieder zurück auf den Ausgangspunkt:
            HandlePlayerFallOff();
        }

        private Vector3 HandleGroundDetection()
        {
            // Gehe zunächst von einer ebenen Fläche
            // (deren Ebenenvektor gerade nach oben zeigt) aus:
            Vector3 slopeNormal = Vector3.UnitY;

            // Ist die Spielfigur gerade im Zustand "am Boden"...
            if (_currentState == State.OnGround)
            {
                // ...prüfe, ob sich Objekte des Typs Immovable oder Box weniger als 0.1 Einheiten unterhalb 
                // der Spielfigur befinden:
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.75f, -1.0f, 0.1f, typeof(Immovable), typeof(Box));

                // Enthält die Ergebnismenge (ein Set = Menge an Ergebnissen) valide Daten,
                // wurde mindestens ein Objekt gefunden, das so nah unterhalb des Players liegt:
                if (set.IsValid)
                {
                    // Überschreibe die Steigung der aktuellen Ebene mit der gemessenen Steigung:
                    slopeNormal = set.SurfaceNormalAvg;

                    // Setze die Spielfigur direkt auf den Punkt, der von ihrer Mitte aus unterhalb 
                    // gemessen wurde:
                    this.SetPositionY(set.IntersectionPointCenter.Y, PositionMode.BottomOfAABBHitbox);
                }
                else
                {
                    // Wenn die Ergebnismenge kein valides Ergebnis hat, dann wechsele den
                    // aktuellen Zustand auf "in der Luft", weil die Spielfigur dann nämlich
                    // in der Luft ist:
                    _currentState = State.InAir;
                    _animationNeedsToChange = true;
                }
            }
            else
            {
                // Ist die Spielfigur in der Luft, wird von ihrer aktuellen 
                // vertikalen Geschwindigkeit immer wieder ein Stück der aktuellen
                // Gravitation abgezogen, so dass sich die Fallgeschwindigkeit pro
                // Frame erhöht. In diesem Beispiel wird die Fallhöhe dann aber auf
                // -0.5f begrenzt, damit die Spielfigur nicht zu schnell fällt:
                _velocityY = _velocityY - GRAVITY;
                if (_velocityY < -0.5f)
                    _velocityY = -0.5f;

                // Auch in der Luft muss geprüft werden, ob sich ein Bodenobjekt nahe den Füßen
                // der Spielfigur befindet. Hier muss ein Objekt sich aber näher als 0.05f Einheiten
                // unterhalb der Figur befinden, damit die Figur dies als gültigen Boden erkennt:
                RayIntersectionExtSet set = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.75f, -0.5f, 0.001f, typeof(Immovable), typeof(Box));

                // Wurde ein Boden gefunden, ist die Ergebnismenge "valide":
                if (set.IsValid == true)
                {
                    // Ist dann die vertikale Geschwindigkeit auch noch kleiner/gleich 0
                    // kann die Spielfigur "landen". Würden wir hier nicht auf <= 0 prüfen
                    // würde die Spielfigur sofort ihre Aufwärtsbewegung stoppen, wenn sie 
                    // eine bodenähnliche Oberfläche streift:
                    if (_velocityY <= 0)
                    {
                        _currentState = State.OnGround;
                        this.SetPositionY(set.IntersectionPointCenter.Y, PositionMode.BottomOfAABBHitbox);
                        _animationNeedsToChange = true;
                        _velocityY = 0f;
                    }
                }
            }
            return slopeNormal;
        }

        private void HandleIntersectionsWithObjects()
        {
            // Die Kollisionsprüfung wird ausschließlich für Kollisionsobjekte des Typs Box und 
            // dem Hitboxmodus "Convex Hull" getätigt.
            List<Intersection> intersections = GetIntersections<Box>(IntersectionTestMode.CheckConvexHullsOnly);
            foreach(Intersection i in intersections)
            {
                MoveOffset(i.MTV);

                if (_currentState == State.InAir)
                {
                    if (Vector3.Dot(Vector3.NormalizeFast(i.MTV), -KWEngine.WorldUp) > 0.1f)
                    {
                        _velocityY = -0.005f;
                    }
                    if(i.MTV.Xz.LengthSquared > 0)
                    {
                        _velocityXZ = Vector2.Zero;
                    }
                }
            }
        }

        private void HandleAnimation()
        {
            if(HasAnimations)
            {
                if(_animationNeedsToChange)
                {
                    _animationStep = 0f;
                    SetAnimationPercentage(_animationStep);
                }
                if(_currentState == State.OnGround)
                {
                    if(_velocityXZ.LengthFast > 0.001f)
                    {
                        SetAnimationID(11); // Walk
                        SetAnimationPercentageAdvance(0.005f);
                    }
                    else
                    {
                        SetAnimationID(3); // Idle
                        SetAnimationPercentageAdvance(0.001f);
                    }
                }
                else
                {
                    if (_animationNeedsToChange)
                    {
                        SetAnimationID(6);
                        _animationStep = 0.10f;
                        _animationNeedsToChange = false;
                    }
                    
                    if(AnimationID == 6)
                    {
                        SetAnimationPercentage(_animationStep);
                        if (_animationStep >= 1)
                        {
                            SetAnimationID(7);
                            _animationStep = 0f;
                            SetAnimationPercentage(_animationStep);
                        }
                        else
                        {
                            _animationStep += 0.006f;
                        }
                    }
                    else if(AnimationID == 7)
                    {
                        SetAnimationPercentageAdvance(0.01f);
                    }
                }
                _animationNeedsToChange = false;
            }
        }

        private void HandlePlayerFallOff()
        {

            if (Position.Y < -20)
            {
                SetPosition(PLAYER_START);
                _velocityY = 0f;
                _velocityXZ = Vector2.Zero;
                _currentState = State.OnGround;
                _animationNeedsToChange = true;
            }
        }

        private Vector2 HandleMovement()
        {
            Vector2 inputVelocity = Vector2.Zero;
            if (Keyboard.IsKeyDown(Keys.A))
            {
                inputVelocity.X -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                inputVelocity.X += 1f;
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                inputVelocity.Y -= 1f;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                inputVelocity.Y += 1f;
            }
            float inputVelocityLengthSq = inputVelocity.LengthSquared;
            if (inputVelocityLengthSq > 0)
            {
                inputVelocity.NormalizeFast();
            }

            if ((Keyboard.IsKeyPressed(Keys.Space) || Mouse.IsButtonPressed(MouseButton.Right)) && _currentState == State.OnGround)
            {
                _currentState = State.InAir;
                _velocityY += VELOCITY_JUMP;
                _animationNeedsToChange = true;
            }

            return inputVelocity;
        }

        private void ApplyVelocity(Vector2 inputVelocity, Vector3 currentSlopeNormal)
        {
            float dotVelocityInputVelocity = Vector2.Dot(inputVelocity, _velocityXZ);
            float dotVelocitySlope = Math.Min(0, Vector3.Dot(Vector3.NormalizeFast(new Vector3(_velocityXZ.X, 0, _velocityXZ.Y)), currentSlopeNormal));
            if (dotVelocityInputVelocity >= 0)
            {
                _velocityXZ += inputVelocity * 0.0005f + inputVelocity * dotVelocitySlope * 0.00045f;
            }
            else
            {
                _velocityXZ += inputVelocity * 0.0001f;
            }

            float velocitySum = _velocityXZ.LengthFast;
            if (velocitySum > _playerSpeed)
            {
                _velocityXZ = Vector2.NormalizeFast(_velocityXZ) * _playerSpeed;
            }
            if (inputVelocity.LengthSquared > 0)
            {
                TurnTowardsXZ(this.Position + new Vector3(_velocityXZ.X, 0, _velocityXZ.Y));
            }
            MoveOffset(new Vector3(_velocityXZ.X, _velocityY, _velocityXZ.Y));

            _velocityXZ /= VELOCITY_REDUCTION;
        }
    }
}
