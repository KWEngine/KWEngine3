using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldEmpty : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            HUDObjectText t = new HUDObjectText("Hello World!");
            t.SetScale(128);
            t.SetTextAlignment(TextAlignMode.Center);
            t.CenterOnScreen();
            AddHUDObject(t);
        }
    }
}
