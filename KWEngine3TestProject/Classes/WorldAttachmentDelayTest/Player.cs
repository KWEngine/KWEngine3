using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldAttachmentDelayTest
{
    internal class Player : GameObject
    {
        private enum State
        {
            OnGround,
            InAir
        }

        private bool _idleAnimationEnabled = true;
        private float _velocityY = 0f;
        private float _gravity = 0.001f;
        private State _state = State.OnGround;

        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.F1))
            {
                if (_state == State.OnGround)
                {
                    StartJump();
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F2))
            {
                SetPosition(0, 0, 0);
                _velocityY = 0f;
                _state = State.OnGround;
            }
            else if (Keyboard.IsKeyPressed(Keys.F3))
            {
                _idleAnimationEnabled = true;
            }
            else if (Keyboard.IsKeyPressed(Keys.F4))
            {
                _idleAnimationEnabled = false;
            }
            else if (Keyboard.IsKeyPressed(Keys.F5))
            {
                SetPosition(0, 4, 0);
                _velocityY = 0f;
                _state = State.OnGround;
            }

            if (_idleAnimationEnabled)
            {
                SetAnimationPercentageAdvance(0.001f);
            }



            if (_state == State.InAir)
            {
                MoveOffset(0, _velocityY, 0f);
                _velocityY -= _gravity;
                if (Position.Y <= 0f)
                {
                    _velocityY = 0f;
                    SetPosition(0, 0, 0);
                    _state = State.OnGround;
                    //StartJump();
                }
            }
        }

        public void StartJump()
        {
            _velocityY = 0.07f;
            _state = State.InAir;
        }

        private void StopJump()
        {
            SetPosition(0, 0, 0);
            _velocityY = 0f;
            _state = State.OnGround;
        }
    }
}
