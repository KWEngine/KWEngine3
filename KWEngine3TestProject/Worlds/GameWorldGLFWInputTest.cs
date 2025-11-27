using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.NewFolder;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGLFWInputTest : World
    {
        public override void Act()
        {
            
            if(Keyboard.IsKeyPressed(Keys.Enter))
            {
                Console.WriteLine("ENTER on GameWorldGLFWInputTest pressed");
            }

            if (Mouse.IsButtonPressed(MouseButton.Left))
            {
                Console.WriteLine("MouseLeft on GameWorldGLFWInputTest pressed");
            }
        }

        public override void Prepare()
        {
            InputTester ipt = new InputTester();
            ipt.SetOpacity(0);
            AddGameObject(ipt);
        }
    }
}
