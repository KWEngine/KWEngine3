using KWEngine3;
using KWEngine3.Audio;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldJumpAndRunPhysics
{
    public class Player : GameObject
    {
        private enum JumpState
        {
            Stand,
            Jump,
            Fall
        }

        private readonly Vector3 GRAVITY = new Vector3(0, 0.005f, 0);        // gravity (always stays the same)
        private readonly Vector3 VELOCITYJUMP = new Vector3(0, 0.175f, 0);   // velocity per jump

        private JumpState _currentState = JumpState.Fall;                    // three states for stand, jump and fall (default)
        private Vector3 _velocity = Vector3.Zero;                            // jump velocity (will become > 0 at jump start and then starts decreasing)
        private float _walkSpeed = 0.05f;                                    // default walking speed
        private bool _upKeyReleased = true;                                  // true, if the up key has been released between jumps

        public override void Act()
        {
            bool stateJustSwitched = false;                                  // becomes true if state just switched
            
            ProcessMovement();                                               // handles basic movement inputs

            if (IsJumpKeyPressed())                                          // if jump is pressed, switch state
            {                                                                // and apply velocity:
                if (_currentState == JumpState.Stand && _upKeyReleased == true)
                {
                    Audio.PlaySound(@"./sfx/jumpUp.ogg");
                    _currentState = JumpState.Jump;
                    _velocity = VELOCITYJUMP;
                    _upKeyReleased = false;
                    stateJustSwitched = true;

                    if(IsLeftKeyPressed())
                    {
                        _velocity.X = -_walkSpeed;
                    }
                    if (IsRightKeyPressed())
                    {
                        _velocity.X = _walkSpeed;
                    }
                }
            }
            else
            {
                _upKeyReleased = true;
            }

            // if player is in jump or fall state, gravity is subtracted from its current velocity.
            // if velocity passes < 0, state switches from jump (upwards) to fall (downwards):
            if (_currentState == JumpState.Jump)
            {
                MoveOffset(_velocity);
                _velocity = _velocity - GRAVITY;
                if (_velocity.Y <= 0)
                {
                    _currentState = JumpState.Fall;
                    stateJustSwitched = true;
                }
            }
            else if (_currentState == JumpState.Fall)
            {
                MoveOffset(_velocity);
                _velocity = _velocity - GRAVITY;
            }
            else if(_currentState == JumpState.Stand)
            {
                // While standing, just move the player down a little bit.
                // If the collision detection loop (see foreach-loop down below)
                // had to correct the player upwards, he still is standing
                // on the ground. If not, the player must have moved off of an
                // edge.
                MoveOffset(-GRAVITY / 10);
                //MoveOffset(_velocity);
            }

            // Turns true, if the player had a collision with any object
            // directly underneath him:
            bool yUpCorrection = false;

            // Get a list of all intersections for the player:
            List<Intersection> intersectionList = GetIntersections();
            // Iterate through all intersections and apply each
            // intersection's minimum-translation-vector (mtv):
            foreach (Intersection i in intersectionList)
            {
                // If the player has to be moved upwards in order
                // to solve the collision, he must be standing on
                // something. Switch yUpCorrection to true then:
                if (i.MTV.Y > 0 && i.MTV.Y > i.MTV.X && i.MTV.Y > i.MTV.Z)
                    yUpCorrection = true;

                // If the player has to be corrected downwards and still is jumping up,
                // he has hit his head somewhere. The jump state must then switch to
                // the fall state:
                // (Math.Round ensures that rounding errors don't interfere here)
                if(Math.Round(i.MTV.Y,3) < 0 && _currentState == JumpState.Jump)
                {
                    _velocity.Y = 0;
                    _currentState = JumpState.Fall;
                }

                MoveOffset(i.MTV);
                
                // If the mtv tells the player to correct its position
                // upwards, the player object must have hit the floor.
                // Switch state to "stand" then:
                if (yUpCorrection && _currentState == JumpState.Fall)
                {
                    Audio.PlaySound(@"./sfx/jumpLand.ogg");
                    stateJustSwitched = true;
                    _currentState = JumpState.Stand;                    
                    _velocity = Vector3.Zero;                               // reset velocity to 0 as well!
                }
            }

            // If the player is in 'stand' mode and is no longer
            // touching the ground, switch state to 'fall':
            if (!yUpCorrection && _currentState == JumpState.Stand)
            {
                _currentState = JumpState.Fall;
                stateJustSwitched = true;
            }

            // Process the player model's animations:
            DoAnimation(stateJustSwitched);
        }

        private void ProcessMovement()
        {
            // if the player is on the floor (incl. running as well),
            // turn the object in walking direction and move if by
            // its walking speed:
            if (_currentState == JumpState.Stand)
            {
                if (IsLeftKeyPressed())
                {
                    SetRotation(0, -90, 0);
                    MoveOffset((-_walkSpeed), 0, 0);
                    _velocity.X = -_walkSpeed;
                }
                if (IsRightKeyPressed())
                {
                    SetRotation(0, 90, 0);
                    MoveOffset((_walkSpeed), 0, 0);
                    _velocity.X = _walkSpeed;
                }
            }
            else
            {
                // If the player is jumping but has already released the jump key
                // decrease the upwards velocity a little more in order to make
                // the jump shorter:
                if(!IsJumpKeyPressed())
                {
                    if(_velocity.Y > 0)
                    {
                        _velocity.Y = MathHelper.Clamp(_velocity.Y - (GRAVITY.Y / 1.5f), 0, VELOCITYJUMP.Y);
                    }
                }

                // If the player is in the air and left/right is pressed, make sure that the player only has 1/8th
                // of the speed (sideways). This effectively reduces air control:
                bool leftRight = false;
                if (IsLeftKeyPressed())
                {
                    _velocity.X = MathHelper.Clamp(_velocity.X - (_walkSpeed / 4), -_walkSpeed, _walkSpeed);
                    leftRight = true;
                }
                if (IsRightKeyPressed())
                {
                    _velocity.X = MathHelper.Clamp(_velocity.X + (_walkSpeed / 4), -_walkSpeed, _walkSpeed);
                    leftRight = true;
                }

                // If the player did a side jump and is in the air but has released left/right keys
                // make sure to slowly reduce the sideways velocity.
                // This makes jumps shorter (in horizontal direction) if the direction keys
                // (left/right) are only pressed at the beginning of the jump:
                if(!leftRight)
                {
                    if(_velocity.X < 0)
                    {
                        _velocity.X = MathHelper.Clamp(_velocity.X + (_walkSpeed / 32), -_walkSpeed, 0);
                    }
                    else
                    {
                        _velocity.X = MathHelper.Clamp(_velocity.X - (_walkSpeed / 32), 0, _walkSpeed);
                    }
                    
                }
            }
        }

        private bool IsJumpKeyPressed()
        {
            return Keyboard.IsKeyDown(Keys.Up) || Keyboard.IsKeyDown(Keys.W);
        }

        private bool IsLeftKeyPressed()
        {
            return Keyboard.IsKeyDown(Keys.Left) || Keyboard.IsKeyDown(Keys.A);
        }

        private bool IsRightKeyPressed()
        {
            return Keyboard.IsKeyDown(Keys.Right) || Keyboard.IsKeyDown(Keys.D);
        }

        private void DoAnimation(bool animationJustSwitched)
        {
            // If the player is on the floor...
            if(_currentState == JumpState.Stand)
            {
                // ..check if it is walking left or right:
                if(IsLeftKeyPressed() || IsRightKeyPressed())
                {
                    // switch to walk animation id:
                    SetAnimationID(2);
                    SetAnimationPercentageAdvance(0.02f);
                }
                else
                {
                    // If player is standing still, switch to idle animation id:
                    SetAnimationID(0);
                    SetAnimationPercentageAdvance(0.00125f);
                }
            }
            else
            {
                // If the player is not on the floor, switch to its jump animation id:
                SetAnimationID(3);
                if (_currentState == JumpState.Jump)
                {
                    // If the jump just started, make sure to rewind the animation percentage to 0
                    // in order to play the jump animation from the beginning:
                    if(animationJustSwitched)
                    {
                        SetAnimationPercentage(0);
                    }
                    SetAnimationPercentageAdvance(0.0125f);
                }
                else
                {
                    if (animationJustSwitched)
                    {
                        SetAnimationPercentage(0);
                    }
                    SetAnimationPercentageAdvance(0.0125f);
                }
            }
        }
    }
}
