using KWEngine3;
using KWEngine3TestProject.Classes.WorldStencilTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldStencilTest : World
    {
        public override void Act()
        {
        }

        public override void Prepare()
        {
            Immovable wall = new Immovable();
            wall.Name = "Wall";
            wall.SetScale(10, 1, 1);
            wall.SetPosition(0, 0, 0.5f);
            wall.SetColor(1, 0, 1);
            AddGameObject(wall);

            Immovable wall2 = new Immovable();
            wall2.Name = "Wall2";
            wall2.SetScale(1, 10, 1);
            wall2.SetPosition(-5.5f, 0, 0.5f);
            wall2.SetColor(1, 0, 1);
            wall2.SetColorHighlight(0, 1, 1, 2);
            wall2.SetColorHighlightMode(HighlightMode.Disabled);
            AddGameObject(wall2);

            Player player = new Player();
            //player.SetModel("KWQuad");
            //player.SetTexture("./Textures/custom_cursor.png");
            //player.HasTransparencyTexture = true;
            player.Name = "Player";
            player.SetColor(0, 1, 1);
            player.SetColorHighlight(1, 1, 0, 1.25f);
            player.SetColorHighlightMode(HighlightMode.WhenOccluded);
            player.SetPosition(0, 0.5f, -0.5f);
            AddGameObject(player);
        }
    }
}
