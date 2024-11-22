using KWEngine3;
using KWEngine3TestProject.Classes.WorldMousePositionResetTest;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMousePositionResetTest : World
    {
        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.F1))
                Window.SetWorld(new GameWorldMousePositionResetTest());
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 20, 0);

            Player p = new Player();
            p.SetPosition(5, 0, 0);
            p.IsCollisionObject = true;
            AddGameObject(p);

            Enemy e = new Enemy();
            e.IsCollisionObject = true;
            e.SetPosition(-5, 0, 0);
            AddGameObject(e);

            MouseCursorResetPosition();
        }
    }
}
