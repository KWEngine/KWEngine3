using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.Audio;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldEmpty : World
    {
        TextObject t;
        float df = 1;

        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.Up))
            {
                df = Math.Clamp(df + 0.01f, 0.75f, 2f);
                t.SetCharacterDistanceFactor(df);
                Console.WriteLine(t.CharacterDistanceFactor);
            }
            else if(Keyboard.IsKeyDown(Keys.Down))
            {
                df = Math.Clamp(df - 0.01f, 0.75f, 2f);
                t.SetCharacterDistanceFactor(df);
                Console.WriteLine(t.CharacterDistanceFactor);
            }
        }

        public override void Prepare()
        {
            t = new TextObject("MMM");
            t.SetTextAlignment(TextAlignMode.Center);
            t.SetPosition(0, 0, 0);
            t.SetCharacterDistanceFactor(df);
            t.IsAffectedByLight = false;
            AddTextObject(t);

        }
    }
}
