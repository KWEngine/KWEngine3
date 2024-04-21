using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

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
            /*
            if(Mouse.IsButtonPressed(MouseButton.Left))
            {
                Console.WriteLine("SPAWN!");
                Spawn();
            }
            */

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

            if (_state == 1)
            {
                MoveOffset(0, _velocity, 0);
                _velocity -= _gravity;
            }

            // Collision detection:
            bool upCorrection = false;
            List<Intersection> floorIntersections = HelperIntersection.GetIntersectionsForObjectWithOffset(this, 0, -0.01f, 0);
            foreach(Intersection i in floorIntersections)
            {
                if(i.MTV.Y > 0)
                {
                    upCorrection = true;
                }
            }

            
            bool obstacleCorrection = false;
            List<Intersection> intersections = GetIntersections();
            foreach (Intersection i in intersections)
            {
                if (i.Object is Obstacle || i.Object is Floor)
                {
                    MoveOffset(i.MTV);
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
            else if(upCorrection && !obstacleCorrection)
            {
                _state = 0;
                _velocity = 0f;
            }

            CurrentWorld.SetCameraPosition(Position + new Vector3(0, 10, 25));
            CurrentWorld.SetCameraTarget(Position);

            /*
            //Raytracing-Tests:

            List<RayIntersection> results = HelperIntersection.RayTraceObjectsForViewVectorFast(this.Center, this.LookAtVector, this, 0, true, typeof(Obstacle), typeof(Weapon));
            if (results.Count > 0)
            {
                foreach (RayIntersection result in results)
                {
                    Console.Write(result.Object.Name + ", ");
                }
                Console.WriteLine();
            }
            
            GameObject gun = CurrentWorld.GetGameObjectByName("Gun");
            if (gun != null)
                Console.WriteLine(HelperIntersection.RaytraceObjectFast(gun, this.Center, this.LookAtVector));

            Console.WriteLine(WorldTime);
            */
        }

        private void Spawn()
        {
            float x = HelperRandom.GetRandomNumber(-5, 5);
            float y = HelperRandom.GetRandomNumber(1, 5);
            float z = HelperRandom.GetRandomNumber(-2, 2);

            Obstacle o = new Obstacle();
            o.SetPosition(x, y, z);
            o.SetColor(1, 0, 1);
            o.IsCollisionObject = true;
            CurrentWorld.AddGameObject(o);
        }
    }
}
