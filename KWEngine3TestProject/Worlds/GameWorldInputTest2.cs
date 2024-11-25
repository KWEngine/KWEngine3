using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldInputTest2 : World
    {
        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.Enter))
            {
                Console.WriteLine("ENTER on World2 pressed");
                Window.SetWorld(new GameWorldInputTest());
            }
        }

        public override void Prepare()
        {
            HUDObject h = new HUDObjectText("World 2");
            h.CenterOnScreen();
            AddHUDObject(h);
        }
    }
}
