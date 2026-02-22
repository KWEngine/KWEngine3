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
    public class GameWorldHUDTextMultilineTest : World
    {
        HUDObjectTextInput _t16;
        HUDObjectText _t24;
        HUDObjectTextInput _t64;
        HUDObjectTextInput _t128;

        public override void Act()
        {
            if (_t16?.IsMouseCursorOnMe() == true)
                _t16?.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t16?.SetColorOutline(0, 0, 0, 0);

            if (_t24?.IsMouseCursorOnMe() == true)
                _t24?.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t24?.SetColorOutline(0.5f, 0.5f, 0.5f, 0.5f);

            if (_t64?.IsMouseCursorOnMe() == true)
                _t64?.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t64?.SetColorOutline(0, 0, 0, 0);

            if (_t128?.IsMouseCursorOnMe() == true)
                _t128?.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t128?.SetColorOutline(0, 0, 0, 0);
        }

        private FontFace _face = FontFace.Anonymous;

        public override void Prepare()
        {
            KWEngine.LoadFont("Playwrite", "./Fonts/Playwrite.png", "./Fonts/Playwrite.json");

            /*
            //KWEngine.DebugOverlayEnabled = true;
            _t16 = new HUDObjectTextInput("SensititÄV");
            _t16.SetFont(_face);
            _t16.Name = "T16";
            _t16.SetScale(16);
            _t16.SetPosition(8, Window.Height / 2 + 64);
            _t16.SetTextAlignment(TextAlignMode.Left);
            //_t16.GetFocus();
            AddHUDObject(_t16);
            */

            HUDObjectImage img = new HUDObjectImage("./Textures/red.png");
            img.SetScale(240, 500);
            img.SetPosition(8 + 120, Window.Height / 2);
            AddHUDObject(img);

            //_t24 = new HUDObjectText("DiesisteinTestgeländefür mehrere Zeilen Text, die einfach nicht enden wollen möchten. Oder siehst Du hier ein Ende?");
            _t24 = new HUDObjectText("Diesistein");
            _t24.SetFont(_face);
            _t24.Name = "T24";
            _t24.SetScale(24);
            //_t24.SetMaxLineWidth(240);
            //_t24.SetLineHeightFactor(0.8f);
            _t24.SetPosition(8, Window.Height / 2);
            _t24.SetTextAlignment(TextAlignMode.Left);
            _t24.SetCharacterDistanceFactor(1f);
            //_t24.GetFocus();
            AddHUDObject(_t24);
            /*
            _t64 = new HUDObjectTextInput("SensititÄV");
            _t64.SetFont("Playwrite");
            _t64.Name = "T64";
            _t64.SetScale(64);
            _t64.SetColorEmissiveIntensity(1);
            _t64.SetColorEmissive(1, 0, 0);
            _t64.SetOpacity(0.2f);
            _t64.SetPosition(Window.Width / 2, Window.Height / 2);
            _t64.SetTextAlignment(TextAlignMode.Center);
            AddHUDObject(_t64);

            _t128 = new HUDObjectTextInput("SensititÄV");
            _t128.SetFont(_face);
            _t128.Name = "T64";
            _t128.SetScale(128);
            _t128.SetPosition(Window.Width -8, Window.Height / 2 + 64);
            _t128.SetTextAlignment(TextAlignMode.Right);
            AddHUDObject(_t128);

            Immovable i = new Immovable();
            i.Name = "Debug test object";
            AddGameObject(i);

            Immovable i2 = new Immovable();
            i2.Name = "Debug test object #2";
            AddGameObject(i2);

            TextObject t = new TextObject("SensititÄV");
            t.Name = "TextObject test instance";
            t.SetFont(_face);
            t.SetScale(2f);
            //t.IsAffectedByLight = false;
            t.SetPosition(0, -1.5f, 0);
            t.SetColorOutline(2, 0, 2, 1);
            t.SetColorEmissive(1.0f, 1.0f, 1.0f, 0.25f);
            t.SetTextAlignment(TextAlignMode.Center);
            AddTextObject(t);


            TextObject t2 = new TextObject("SensititÄV");
            t2.Name = "TextObject test instance";
            t2.SetFont(_face);
            t2.SetScale(1.5f);
            //t2.IsAffectedByLight = false;
            t2.SetPosition(0, -3.5f, 0);
            t2.SetColorOutline(0, 0, 0, 0);
            t2.SetColorEmissive(1.0f, 1.0f, 1.0f, 1.0f);
            t2.SetTextAlignment(TextAlignMode.Center);
            t2.SetOpacity(0.5f);
            AddTextObject(t2);
            */

        }
    }
}
