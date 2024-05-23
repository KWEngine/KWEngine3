using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldReuseTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldReuseTest : World
    {
        Player _p;
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.F1))
            {
                // Remove player:
                Console.WriteLine("Removing player from world (but not from memory)..");
                RemoveGameObject(_p);
            }
            else if(Keyboard.IsKeyPressed(Keys.F2))
            {
                // Add it back again:
                Console.WriteLine("Adding player back from memory..");
                AddGameObject(_p);
            }
        }

        public override void Prepare()
        {
            _p = new Player();
            _p.IsCollisionObject = true;
            AddGameObject(_p);
        }
    }
}
