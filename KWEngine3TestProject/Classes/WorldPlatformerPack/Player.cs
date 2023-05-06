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
    public class Player : GameObject
    {
        private float _speed = 0.05f;

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

            // Collision detection:
            List<Intersection> intersections = GetIntersections();
            foreach(Intersection i in intersections)
            {
                if (i.Object is Obstacle)
                {
                    MoveOffset(i.MTV); 
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

            CurrentWorld.SetCameraPosition(Position + new Vector3(0, 10, 25));
            CurrentWorld.SetCameraTarget(Position);

        }
    }
}
