using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldRayCollision
{
    public class Player : GameObject
    {
        private float _speed = 0.025f;
        private Pointer _sphere1;
        private Pointer _sphere2;
        private Pointer _sphere3;
        private Pointer _sphere4;

        public Player()
        {
            _sphere1 = new Pointer();
            _sphere1.SetModel("KWSphere");
            _sphere1.SetScale(0.05f);
            _sphere1.SetColor(1, 1, 0);
            _sphere1.SetColorEmissive(1, 0, 0, 5);
            _sphere1.SetOpacity(1);
            CurrentWorld.AddGameObject(_sphere1);

            _sphere2 = new Pointer();
            _sphere2.SetModel("KWSphere");
            _sphere2.SetScale(0.05f);
            _sphere2.SetColor(1, 1, 0);
            _sphere2.SetColorEmissive(1, 0, 0, 5);
            _sphere2.SetOpacity(1);
            CurrentWorld.AddGameObject(_sphere2);

            _sphere3 = new Pointer();
            _sphere3.SetModel("KWSphere");
            _sphere3.SetScale(0.05f);
            _sphere3.SetColor(1, 1, 0);
            _sphere3.SetColorEmissive(1, 1, 0, 5);
            _sphere3.SetOpacity(0);
            CurrentWorld.AddGameObject(_sphere3);

            _sphere4 = new Pointer();
            _sphere4.SetModel("KWSphere");
            _sphere4.SetScale(0.05f);
            _sphere4.SetColor(1, 1, 0);
            _sphere4.SetColorEmissive(1, 1, 0, 5);
            _sphere4.SetOpacity(0);
            CurrentWorld.AddGameObject(_sphere4);
        }

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveAlongVector(LookAtVectorLocalRight, -_speed);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveAlongVector(LookAtVectorLocalRight, +_speed);
            }

            UpdateSpheresOnPlayer();
            HandleFloorCollision();
        }

        private void UpdateSpheresOnPlayer()
        {
            _sphere1.SetPosition(this.Position - this.LookAtVectorLocalRight * this.Scale.X / 2.5f);
            _sphere2.SetPosition(this.Position + this.LookAtVectorLocalRight * this.Scale.X / 2.5f);
        }

        private void HandleFloorCollision()
        {
            List<RayIntersection> rays1 = RaytraceObjectsNearbyFast(_sphere1.Position, -Vector3.UnitY);
            List<RayIntersection> rays2 = RaytraceObjectsNearbyFast(_sphere2.Position, -Vector3.UnitY);

            if(rays1.Count > 0 && rays2.Count > 0)
            {
                Vector3 nearestHitPosition1 = rays1[0].IntersectionPoint;
                Vector3 nearestHitPosition2 = rays2[0].IntersectionPoint;

                _sphere3.SetPosition(nearestHitPosition1);
                _sphere4.SetPosition(nearestHitPosition2);

                _sphere3.SetOpacity(1);
                _sphere4.SetOpacity(1);

                Quaternion slopeRotation = HelperRotation.GetRotationForSlope(nearestHitPosition1, nearestHitPosition2, this.LookAtVector);
                SetRotation(slopeRotation);
            }
            else
            {
                _sphere3.SetOpacity(0);
                _sphere4.SetOpacity(0);
            }
        }
    }
}
