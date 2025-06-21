using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldStencilTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.W))
                MoveOffset(0, 0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.S))
                MoveOffset(0, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.A))
                MoveOffset(-0.01f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.D))
                MoveOffset(0.01f, 0, 0);
        }
    }
}
