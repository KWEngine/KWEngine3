using KWEngine3;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGlyphRenderingTest : World
    {
        private HUDObjectText text1;
        private HUDObjectText buttonText;
        private HUDObjectImage button;
        private HUDObjectText text2;


        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            text1 = new HUDObjectText("0");
            text1.SetScale(64);
            text1.CenterOnScreen();
            AddHUDObject(text1);

            button = new HUDObjectImage("./Textures/button.png");
            button.CenterOnScreen();
            button.SetScale(text1.Width, text1.Height);
            AddHUDObject(button);
        }
    }
}
