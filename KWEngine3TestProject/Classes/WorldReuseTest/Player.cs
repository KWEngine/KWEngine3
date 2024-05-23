using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldReuseTest
{
    public class Player : GameObject
    {
        private float _speed = 0.025f;

        public override void Act()
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
    }
}
