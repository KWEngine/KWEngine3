using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using KWEngine3;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPlatformerPack
{
    public class PlayerPlatformerPack : GameObject
    {
        private float _speed = 0.05f;
        private int _state = 0; // 0 = stand, 1 = fall
        private float _gravity = 0.0025f;
        private float _velocity = 0f;

        public override void Act()
        {
            // Movement:
            bool isMoving = false;

            if (Keyboard.IsKeyDown(Keys.A))
                AddRotationY(+1);
            if (Keyboard.IsKeyDown(Keys.D))
                AddRotationY(-1);

            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveAlongVector(LookAtVector, _speed);
                isMoving = true;
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveAlongVector(LookAtVector, -_speed);
                isMoving = true;
            }

            // Animation:
            if(isMoving == true)
            {
                SetAnimationID(11);
                SetAnimationPercentageAdvance(0.005f);
            }
            else
            {
                SetAnimationID(3);
                SetAnimationPercentageAdvance(0.001f);
            }

            if (_state == 0)
            {
                MoveOffset(0, -_gravity / 10f, 0);
            }
            else
            {
                MoveOffset(0, _velocity, 0);
                _velocity -= _gravity;
            }

            // Collision detection:
            List<Intersection> intersections = GetIntersections();
            bool upCorrection = false;
            bool obstacleCorrection = false;
            foreach(Intersection i in intersections)
            {
                if (i.Object is Obstacle || i.Object is Floor)
                {
                    
                    if (i.MTV.Y > 0)
                        upCorrection = true;

                    if (i.Object.GetModelName() == "Ramp01")
                    {
                        MoveOffset(i.MTV);
                    }
                    else
                    {
                        MoveOffset(i.MTV);
                    }
                    
                    obstacleCorrection = true;
                }
                else if (i.Object is Weapon)
                {
                    AttachGameObjectToBone(i.Object, "Fist.R");
                    i.Object.IsCollisionObject = false;
                    HelperGameObjectAttachment.SetRotationForAttachment(i.Object, -90, 0, 180);
                    HelperGameObjectAttachment.SetPositionOffsetForAttachment(i.Object, 0, 0.05f, 0);
                    HelperGameObjectAttachment.SetScaleForAttachment(i.Object, 1.25f, 1.25f, 1.25f);
                }
            }

            if(obstacleCorrection && !upCorrection)
            {
                _state = 1; // set state to 'fall'
            }
            else if(!obstacleCorrection && !upCorrection)
            {
                _state = 1; // set state to 'fall'
            }
            else if(obstacleCorrection && upCorrection)
            {
                _state = 0; // set state to 'stand'
                _velocity = 0f;
            }

            CurrentWorld.SetCameraPosition(Position + new Vector3(0, 10, 25));
            CurrentWorld.SetCameraTarget(Position);

        }
    }
}
