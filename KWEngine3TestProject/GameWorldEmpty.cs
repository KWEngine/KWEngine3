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
                    ht.SetColorOutline(1, 1, 1, ht.ColorOutlineWidth);
                }
                else
                {
                    ht.SetColorOutline(0, 0, 0, ht.ColorOutlineWidth);
                }
            }

            if(Keyboard.IsKeyDown(Keys.A))
            {
                SetCameraPosition(CameraPosition.X - 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X - 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.D))
            {
                SetCameraPosition(CameraPosition.X + 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X + 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
        }

        public override void Prepare()
        {

            //SetCameraPosition(CameraPosition.X, CameraPosition.Y, -CameraPosition.Z);

            HUDObjectTextInput t = new HUDObjectTextInput("sitivÄY");
            t.Name = "Input";
            t.SetFont(FontFace.OpenSans);
            t.SetScale(64);
            t.CursorType = KeyboardCursorType.Underscore;
            t.CursorBehaviour = KeyboardCursorBehaviour.Fade;
            t.SetColorOutline(1.9f, 1.6f, 0, 1f);
            t.SetColor(0, 0.0f, 1.0f);
            t.SetColorEmissiveIntensity(0f);
            t.SetColorEmissive(0, 1, 1);
            t.SetTextAlignment(TextAlignMode.Center);
            t.SetCharacterDistanceFactor(1);
            t.CenterOnScreen();
            t.GetFocus();
            AddHUDObject(t);

            /*
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
            */
            
            TextObjectCustom txt = new TextObjectCustom();
            txt.SetText("SensitivitÄV");
            txt.SetPosition(0, -2, 0);
            txt.SetColorOutline(1.9f, 1.6f, 0, 0.75f);
            txt.SetScale(1f);
            txt.SetColor(1, 1, 1);
            txt.SetFont(FontFace.OpenSans);
            txt.SetColorEmissive(0, 0, 1, 1.1f);
            txt.IsAffectedByLight = true;
            txt.SetTextAlignment(TextAlignMode.Center); // not rendering correctly
            AddTextObject(txt);
            

            Immovable i = new Immovable();
            i.SetPosition(0, -2, -0.005f);
            i.SetScale(4f, 0.25f, 0.001f);
            AddGameObject(i);
            

            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.Default);
            sun.SetPosition(0, 50, 50);
            sun.SetNearFar(20, 200);
            sun.SetColor(1, 0, 0, 2);
            AddLightObject(sun);
        }
    }
}
