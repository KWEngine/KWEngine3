using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldSwitch1 : World
    {
        private HUDObject h;
        public override void Act()
        {
            if(Mouse.IsButtonPressed(MouseButton.Right))
            {
                Console.WriteLine("World 1 - RIGHT: " + WorldTime);
            }

            if(h.IsMouseCursorOnMe() && Mouse.IsButtonPressed(MouseButton.Left))
            {
                Window.SetWorld(new GameWorldSwitch2());
            }
            else if(Keyboard.IsKeyPressed(Keys.F1))
            {
                Window.SetWorld(new GameWorldSwitch2());
            }
        }

        public override void Prepare()
        {
            h = new HUDObjectText("Switch to World 2");
            h.CenterOnScreen();
            AddHUDObject(h);

            RenderObjectDefault block = new RenderObjectDefault();
            block.SetColor(1, 1, 0);
            block.SetPosition(-3, 0, 0);
            AddRenderObject(block);
        }
    }
}
