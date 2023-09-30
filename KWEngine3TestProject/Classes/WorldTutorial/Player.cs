using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class Player : GameObject
    {
        private float _speed = 0.01f;
        private float _timeLastShot = 0;

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

            if(WorldTime - _timeLastShot > 0.05f && (Mouse.IsButtonDown(MouseButton.Left) || Keyboard.IsKeyDown(Keys.Space)))
            {
                _timeLastShot = WorldTime;

                Shot s = new Shot();
                s.SetPosition(this.Center + this.LookAtVector * 0.5f + this.LookAtVectorLocalRight * HelperRandom.GetRandomNumber(-0.15f, 0.15f) + this.LookAtVectorLocalUp * HelperRandom.GetRandomNumber(-0.15f, 0.15f));
                s.SetRotation(this.Rotation);
                CurrentWorld.AddGameObject(s);
            }
        }
    }
}
