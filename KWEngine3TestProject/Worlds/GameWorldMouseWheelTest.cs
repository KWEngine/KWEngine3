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
    internal class GameWorldMouseWheelTest : World
    {
        private HUDObjectText h;
        private int pos = 0;
        public override void Act()
        {
            if (Mouse.MouseScrollDelta != 0)
            {
                pos += Mouse.MouseScrollDelta;
                h.SetText("" + pos);
            }
        }

        public override void Prepare()
        {
            h = new HUDObjectText("0");
            h.CenterOnScreen();
            h.SetTextAlignment(TextAlignMode.Center);
            h.SetScale(48, 48);
            AddHUDObject(h);
        }
    }
}
