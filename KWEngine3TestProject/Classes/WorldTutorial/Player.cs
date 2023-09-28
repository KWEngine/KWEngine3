using KWEngine3.GameObjects;
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
            IsCollisionObject = true;
        }

        public override void Act()
        {
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

            if(WorldTime - _timeLastShot > 0.15f && Mouse.IsButtonDown(MouseButton.Left))
            {
                _timeLastShot = WorldTime;

                Shot s = new Shot();
                s.SetPosition(this.Center + this.LookAtVector * 0.5f);
                s.SetRotation(this.Rotation);
                CurrentWorld.AddGameObject(s);
            }
        }
    }
}
