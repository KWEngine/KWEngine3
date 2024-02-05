using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFoliageTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFoliageTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            
            FoliageObject tf1 = new FoliageObject(256);
            tf1.SetPosition(0, 0, 0);
            tf1.SetScale(1, 1, 1);
            tf1.SetPatchSize(4, 8);
            AddFoliageObject(tf1);
            
            
            PlayerFoliageTest player = new PlayerFoliageTest();
            player.SetRotation(0, 180, 0);
            player.SetPosition(0, 5f, 10);
            player.SetOpacity(0);
            SetCameraToFirstPersonGameObject(player, 0f);
            MouseCursorGrab();
            AddGameObject(player);
            /*
            Immovable center = new Immovable();
            center.SetColor(1, 1, 0);
            center.SetScale(0.1f);
            AddGameObject(center);

            Immovable radius = new Immovable();
            radius.SetColor(1, 0, 0);
            radius.SetPosition(0, 0.5f, 0);
            radius.SetScale(4, 1, 8);
            radius.SetOpacity(0.25f);
            AddGameObject(radius);
            */
            Immovable floor = new Immovable();
            floor.SetTexture("./Textures/Grass_01_512.png");
            floor.SetScale(4, 1, 8);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetTextureRepeat(2, 4);
            AddGameObject(floor);
            
        }
    }
}
