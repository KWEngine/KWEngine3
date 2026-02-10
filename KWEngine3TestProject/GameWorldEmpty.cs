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
            HUDObjectTextInput t = new HUDObjectTextInput("Franz");
            t.SetFont(FontFace.OpenSans);
            t.SetScale(48);
            t.CursorType = KeyboardCursorType.Dot;
            t.CursorBehaviour = KeyboardCursorBehaviour.Fade;
            t.SetColorOutline(1.9f, 1.6f, 0, 0.75f);
            t.SetColor(0, 0, 0);
            t.SetColorEmissiveIntensity(1.1f);
            t.SetColorEmissive(0, 0, 1);
            t.SetTextAlignment(TextAlignMode.Center);
            t.CenterOnScreen();
            t.GetFocus();
            AddHUDObject(t);

            TextObject txt = new TextObject("Franz 3D");
            txt.SetPosition(0, -2, 0);
            txt.SetColor(1, 1, 1);
            txt.IsAffectedByLight = true;
            AddTextObject(txt);
        }
    }
}
