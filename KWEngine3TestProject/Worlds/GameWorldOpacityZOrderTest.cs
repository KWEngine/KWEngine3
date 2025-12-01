using KWEngine3;
using KWEngine3TestProject.Classes.WorldOpacityZOrderTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldOpacityZOrderTest : World
    {
        private Player _player;

        public override void Act()
        {
            List<Obstacle> obstacles = GetGameObjectsByType<Obstacle>();
            foreach (Obstacle obstacle in obstacles)
            {

            }
        }

        public override void Prepare()
        {
            _player = new Player();
            _player.SetModel("KWQuad2D");
            _player.HasTransparencyTexture = true;
            _player.SetTexture("./Textures/custom_cursor.png");
            _player.SetPosition(0, 0, 0);
            _player.SetHue(180);
            _player.Name = "Player";
            AddGameObject(_player);

            Obstacle o1 = new Obstacle();
            o1.SetPosition(0, 2, 3);
            o1.SetScale(20, 4, 1);
            o1.SetOpacity(0.5f);
            o1.SetColor(1, 0, 0);
            o1.Name = "Obstacle";
            AddGameObject(o1);
            /*
            Obstacle o2 = new Obstacle();
            o2.SetPosition(3, 2, 3);
            o2.SetScale(2, 4, 1);
            o2.SetOpacity(0.5f);
            o2.SetColor(1, 0, 0);
            AddGameObject(o2);

            Obstacle o3 = new Obstacle();
            o3.SetPosition(-3, 2, 3);
            o3.SetScale(2, 4, 1);
            o3.SetOpacity(0.5f);
            o3.SetColor(1, 0, 0);
            AddGameObject(o3);
            */

            Floor f = new Floor();
            f.SetPosition(0, -0.5f, 0);
            f.SetScale(20, 1, 20);
            f.SetColor(0, 0, 1);
            AddGameObject(f);
        }
    }
}
