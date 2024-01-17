using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldSwitch2 : World
    {
        private HUDObject h;
        public override void Act()
        {
            if (h.IsMouseCursorOnMe() && Mouse.IsButtonPressed(MouseButton.Left))
            {
                Window.SetWorld(new GameWorldSwitch1());
            }
            else if (Keyboard.IsKeyPressed(Keys.F1))
            {
                Window.SetWorld(new GameWorldSwitch1());
            }
        }

        public override void Prepare()
        {
            h = new HUDObjectText("Switch to World 1");
            h.CenterOnScreen();
            AddHUDObject(h);
        }
    }
}
