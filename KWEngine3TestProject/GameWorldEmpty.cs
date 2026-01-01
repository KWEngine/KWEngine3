using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Graphics.ES11;
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
                HUDObjectText text = new HUDObjectText("Hello World!");
                text.SetColor(1, 1, 0);
                text.SetTextAlignment(TextAlignMode.Left);
                text.SetPosition(64, 64);
                AddHUDObject(text);

                //HUDObjectImage img = new HUDObjectImage("./Textures/Bark_01_512.png");
                //img.CenterOnScreen();
                //AddHUDObject(img);
        }
    }
}
