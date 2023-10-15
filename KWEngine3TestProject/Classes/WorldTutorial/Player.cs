using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class Player : GameObject
    {
        private float _speed = 0.015f;
        private float _timeLastShot = 0;
        private float _cooldown = 0.15f;
        private int _spreadCount = 1;
        private float _spreadAngle = 10f;

        public Player()
        {
            SetModel("Toon");
            SetPosition(0, 0.0f, 95);
            AddRotationY(180);
            SetScale(0.5f);
            IsCollisionObject = true;
        }

        public override void Act()
        {
            Vector3 mouseCursorPos = HelperIntersection.GetMouseIntersectionPointOnPlane(KWEngine3.Plane.XZ, Center.Y);
            TurnTowardsXZ(mouseCursorPos);

            if(Keyboard.IsKeyDown(Keys.A) == true)
            {
                MoveOffset(-_speed, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D) == true)
            {
                MoveOffset(+_speed, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.W) == true)
            {
                MoveOffset(0, 0, -_speed);
            }
            if (Keyboard.IsKeyDown(Keys.S) == true)
            {
                MoveOffset(0, 0, +_speed);
            }

            Shoot();
            HandleCollisions();
            UpdateCamera();
        }

        private void Shoot()
        {
            if (WorldTime - _timeLastShot >= _cooldown && (Mouse.IsButtonDown(MouseButton.Left) || Keyboard.IsKeyDown(Keys.Space)))
            {
                _timeLastShot = WorldTime;
                if (_spreadCount == 1)
                {
                    Shot s = new Shot();
                    s.SetPosition(this.Center + this.LookAtVector * 0.5f + this.LookAtVectorLocalRight * HelperRandom.GetRandomNumber(-0.15f, 0.15f) + this.LookAtVectorLocalUp * HelperRandom.GetRandomNumber(-0.15f, 0.15f));
                    s.SetRotation(this.Rotation);
                    CurrentWorld.AddGameObject(s);
                }
                else
                {
                    float spread = -_spreadAngle / 2;
                    float step = _spreadAngle / (_spreadCount - 1);

                    for(int i = 0; i < _spreadCount; i++)
                    {
                        Shot s = new Shot();
                        s.SetPosition(this.Center + this.LookAtVector * 0.5f + this.LookAtVectorLocalRight * HelperRandom.GetRandomNumber(-0.15f, 0.15f) + this.LookAtVectorLocalUp * HelperRandom.GetRandomNumber(-0.15f, 0.15f));
                        s.SetRotation(this.Rotation);
                        s.AddRotationY(spread);
                        spread += step;
                        CurrentWorld.AddGameObject(s);
                    }
                }
            }
        }

        private void UpdateCamera()
        {
            CurrentWorld.SetCameraPosition(0, 10, Position.Z + 10);
            CurrentWorld.SetCameraTarget(0, 0, Position.Z - 5f);
        }

        private void HandleCollisions()
        {
            List<Intersection> intersections = GetIntersections();
            foreach(Intersection i in intersections)
            {
                if(i.Object is Saw)
                {

                }
                else if(i.Object is PowerUp)
                {
                    (i.Object as PowerUp).Destroy();

                }
            }
        }

        public void DecreaseGunCooldownBy(float factor)
        {
            _cooldown *= factor;
        }

        public void IncreaseSpreadCountBy(int amount)
        {
            _spreadCount = Math.Min(amount + _spreadCount, 5);
        }
    }
}
