using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.NewFolder
{
    internal class InputTester : GameObject
    {
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter))
            {
                Console.WriteLine("[InputTester] ENTER was pressed.");
            }
        }
    }
}
