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
        private float _speed = 0.1f;

        public override void Act()
        {
            // Movement:
            bool isMoving = false;

            if (Keyboard.IsKeyDown(Keys.A))
                AddRotationY(+2);
            if (Keyboard.IsKeyDown(Keys.D))
                AddRotationY(-2);

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
                SetAnimationPercentageAdvance(0.01f);
            }
            else
            {
                SetAnimationID(3);
                SetAnimationPercentageAdvance(0.001f);
            }

            // Collision detection:
            List<Intersection> intersections = GetIntersections();
            Vector3 mtv = Vector3.Zero;
            foreach(Intersection i in intersections)
            {
                if (i.Object is Obstacle)
                {
                    if (Math.Abs(i.MTV.X) > Math.Abs(mtv.X))
                        mtv.X = i.MTV.X;
                    if (Math.Abs(i.MTV.Y) > Math.Abs(mtv.Y))
                        mtv.Y = i.MTV.Y;
                    if (Math.Abs(i.MTV.Z) > Math.Abs(mtv.Z))
                        mtv.Z = i.MTV.Z;
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
            MoveOffset(mtv);

            CurrentWorld.SetCameraPosition(Center + new Vector3(0, 10, 25));
            CurrentWorld.SetCameraTarget(Center);

        }
    }
}
