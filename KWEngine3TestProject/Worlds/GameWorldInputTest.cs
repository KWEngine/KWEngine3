using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldInputTest : World
    {
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.Enter))
            {
                Console.WriteLine("ENTER on World1 pressed");
                Window.SetWorld(new GameWorldInputTest2());
            }
        }

        public override void Prepare()
        {
            HUDObject h = new HUDObjectText("World 1");
            h.CenterOnScreen();
            AddHUDObject(h);
        }
    }
}
