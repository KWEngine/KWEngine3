using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGlyphRenderingTest : World
    {
        private float df = 1f;
        private HUDObjectText text1;
        private HUDObjectText buttonText;
        private HUDObjectImage button;
        private HUDObjectText text2;


        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.F1))
            {
                df -= 0.001f;
                df = Math.Max(df, 0);
                text1.SetCharacterDistanceFactor(df);
                Console.WriteLine(df);
            }
            else if(Keyboard.IsKeyDown(Keys.F2))
            {
                df += 0.001f;
                df = Math.Min(df, 10);
                text1.SetCharacterDistanceFactor(df);
                Console.WriteLine(df);
            }
            else if(Keyboard.IsKeyDown(Keys.F3))
            {

            }
            else if (Keyboard.IsKeyDown(Keys.F4))
            {

            }
        }

        public override void Prepare()
        {
            text1 = new HUDObjectText("a");
            text1.SetScale(128);
            text1.SetTextAlignment(TextAlignMode.Center);
            text1.CenterOnScreen();
            AddHUDObject(text1);

            button = new HUDObjectImage("./Textures/button.png");
            button.CenterOnScreen();
            button.SetScale(text1.Width, text1.Height);
            AddHUDObject(button);
        }
    }
}
