using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldInputTest : World
    {
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.Enter) || Keyboard.IsKeyPressed(Keys.Escape))
            {
                Console.WriteLine("ENTER on World1 pressed");
                Window.SetWorld(new GameWorldInputTest2());
            }

            HUDObjectTextInput h = GetHUDObjectTextInputByName("InputTest");
            if(h != null && h.IsMouseCursorOnMe() && Mouse.IsButtonPressed(MouseButton.Left))
            {
                h.SetColor(1, 1, 0);
                h.GetFocus();
            }
        }

        public override void Prepare()
        {
            HUDObject h = new HUDObjectText("äÄüÜöÖß");
            h.SetPosition(200, 100);
            AddHUDObject(h);

            HUDObjectTextInput inputTest = new HUDObjectTextInput("Dies ist ein Test");
            inputTest.Name = "InputTest";
            inputTest.CursorBehaviour = KeyboardCursorBehaviour.Fade;
            inputTest.CursorType = KeyboardCursorType.Block;
            inputTest.SetTextAlignment(TextAlignMode.Right);
            //inputTest.CursorPosition = 0;
            inputTest.GetFocus();
            inputTest.CenterOnScreen();
            AddHUDObject(inputTest);
        }

        protected override void OnWorldEvent(WorldEvent e)
        {
            if(e.GeneratedByInputFocusLost)
            {
                HUDObjectTextInput h = e.Tag as HUDObjectTextInput;
                h.SetColor(1, 0, 0);
            }
        }
    }
}
