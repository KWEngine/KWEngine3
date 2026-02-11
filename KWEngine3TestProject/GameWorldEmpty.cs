using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldEmpty : World
    {
        public override void Act()
        {
            HUDObject t = GetHUDObjectByName("Input");
            if(t != null)
            {
                HUDObjectText ht = t as HUDObjectText;
                if(ht.IsMouseCursorOnMe())
                {
                    ht.SetColorOutline(2, 2, 2, ht.ColorOutlineWidth);
                }
                else
                {
                    ht.SetColorOutline(0, 0, 0, ht.ColorOutlineWidth);
                }
            }
        }

        public override void Prepare()
        {
            
            HUDObjectTextInput t = new HUDObjectTextInput("Franz");
            t.Name = "Input";
            t.SetFont(FontFace.OpenSans);
            t.SetScale(96);
            t.CursorType = KeyboardCursorType.Dot;
            t.CursorBehaviour = KeyboardCursorBehaviour.Fade;
            t.SetColorOutline(1.9f, 1.6f, 0, 0.75f);
            t.SetColor(0, 0, 0);
            t.SetColorEmissiveIntensity(0f);
            t.SetColorEmissive(0, 1, 1);
            t.SetTextAlignment(TextAlignMode.Center);
            t.CenterOnScreen();
            t.GetFocus();
            AddHUDObject(t);
            

            HUDObjectImage hi = new HUDObjectImage("./Textures/red.png");
            hi.SetScale(768, 96);
            hi.CenterOnScreen();
            hi.SetZIndex(-100);
            AddHUDObject(hi);

            HUDObjectImage dot = new HUDObjectImage("./Textures/blue.png");
            dot.SetScale(4, 4);
            dot.CenterOnScreen();
            dot.SetZIndex(2);
            AddHUDObject(dot);
            
            TextObject txt = new TextObject("Franz 3D");
            txt.SetPosition(0, -2, 0);
            txt.SetColorOutline(1.9f, 1.6f, 0, 0.75f);
            txt.SetScale(1);
            txt.SetColor(1, 1, 1);
            txt.SetFont(FontFace.OpenSans);
            txt.SetColorEmissive(0, 0, 1, 1.1f);
            txt.IsAffectedByLight = true;
            txt.SetTextAlignment(TextAlignMode.Center);
            AddTextObject(txt);

            Immovable i = new Immovable();
            i.SetPosition(0, -2, -0.001f);
            i.SetScale(50, 1, 0.001f);
            AddGameObject(i);
            
        }
    }
}
