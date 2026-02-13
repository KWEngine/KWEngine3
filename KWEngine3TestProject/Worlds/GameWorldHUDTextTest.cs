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
    public class GameWorldHUDTextTest : World
    {
        HUDObjectTextInput _t32;
        HUDObjectTextInput _t64;
        HUDObjectTextInput _t128;

        public override void Act()
        {
            if(_t32.IsMouseCursorOnMe())
                _t32.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t32.SetColorOutline(0, 0, 0, 0);

            if (_t64.IsMouseCursorOnMe())
                _t64.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t64.SetColorOutline(0, 0, 0, 0);

            if (_t128.IsMouseCursorOnMe())
                _t128.SetColorOutline(1, 1, 0, 0.75f);
            else
                _t128.SetColorOutline(0, 0, 0, 0);
        }

        public override void Prepare()
        {
            //KWEngine.DebugOverlayEnabled = true;

            _t32 = new HUDObjectTextInput("SensititÄV");
            _t32.SetFont(FontFace.OpenSans);
            _t32.Name = "T32";
            _t32.SetScale(32);
            _t32.SetPosition(8, Window.Height / 2);
            _t32.SetTextAlignment(TextAlignMode.Left);
            //_t32.GetFocus();
            AddHUDObject(_t32);

            _t64 = new HUDObjectTextInput("SensititÄV");
            _t64.SetFont(FontFace.OpenSans);
            _t64.Name = "T64";
            _t64.SetScale(64);
            _t64.SetPosition(Window.Width / 2, Window.Height / 2);
            _t64.SetTextAlignment(TextAlignMode.Center);
            AddHUDObject(_t64);

            _t128 = new HUDObjectTextInput("SensititÄV");
            _t128.SetFont(FontFace.OpenSans);
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

        }
    }
}
