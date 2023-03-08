using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes
{
    public class Player : GameObject
    {
        private float _speed = 0.025f;

        public bool IsFirstPersonObject { get; set; } = false; // don't change yet!
        public bool LetCamFollowMe { get; set; } = false;
        public override void Act()
        {
            if(IsFirstPersonObject)
            {
                CurrentWorld.SetCameraPosition(Center, 0);
                CurrentWorld.AddCameraRotation(MouseMovement);
                int move = 0;
                int strafe = 0;
                if (Keyboard.IsKeyDown(Keys.A))
                {
                    strafe--;
                }
                if (Keyboard.IsKeyDown(Keys.D))
                {
                    strafe++;
                }
                if (Keyboard.IsKeyDown(Keys.W))
                {
                    move++;
                }
                if (Keyboard.IsKeyDown(Keys.S))
                {
                    move--;
                }
                MoveAndStrafeAlongCamera(move, strafe, _speed);
                if(Keyboard.IsKeyDown(Keys.Q))
                {
                    MoveAlongVector(CurrentWorld.CameraLookAtVectorLocalUp, -_speed);
                }
                if (Keyboard.IsKeyDown(Keys.E))
                {
                    MoveAlongVector(CurrentWorld.CameraLookAtVectorLocalUp, +_speed);
                }

               
                
                bool hit = HelperIntersection.GetMouseIntersectionPointOnAnyTerrain(out Vector3 terrainIntersection);
                if(hit)
                {
                    GameObject g = CurrentWorld.GetGameObjectByName("PointerSphere");
                    if(g != null)
                    {
                        g.SetPosition(terrainIntersection);
                    }
                }
                
            }
            else
            {
                if (Keyboard.IsKeyDown(Keys.A))
                {
                    MoveOffset(-_speed, 0f, 0f);
                }
                if (Keyboard.IsKeyDown(Keys.D))
                {
                    MoveOffset(+_speed, 0f, 0f);
                }
                if (Keyboard.IsKeyDown(Keys.W))
                {
                    MoveOffset(0f, 0f, -_speed);
                }
                if (Keyboard.IsKeyDown(Keys.S))
                {
                    MoveOffset(0f, 0f, +_speed);
                }
                if (Keyboard.IsKeyDown(Keys.Q))
                {
                    MoveOffset(0f, -_speed, 0f);
                }
                if (Keyboard.IsKeyDown(Keys.E))
                {
                    MoveOffset(0f, +_speed, 0f);
                }
            }

            if (IsCollisionObject)
            {
                List<Intersection> intersections = GetIntersections();
                foreach (Intersection i in intersections)
                {
                    MoveOffset(i.MTV);
                    i.Object.SetColorEmissive(0, 1, 1, 2);
                }
            }

            if(!IsFirstPersonObject && LetCamFollowMe)
            {
                CurrentWorld.SetCameraPosition(Center + new Vector3(0, 5, 5));
                CurrentWorld.SetCameraTarget(Center);
            }
        }
    }
}
