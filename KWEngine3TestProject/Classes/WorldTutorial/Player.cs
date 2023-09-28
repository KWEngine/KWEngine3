using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class Player : GameObject
    {
        private float _speed = 0.01f;

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
        }
    }
}
