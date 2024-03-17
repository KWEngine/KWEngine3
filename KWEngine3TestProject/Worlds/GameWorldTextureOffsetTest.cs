using KWEngine3;
using KWEngine3TestProject.Classes.WorldCollisionTest;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldTextureOffsetTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraFOV(10);
            SetCameraPosition(0, 0, 50);


            Immovable i01 = new Immovable();
            i01.Name = "Floor";
            i01.SetCollisionType(ColliderType.ConvexHull);
            i01.SetPosition(0, -2, 0);
            i01.SetScale(5, 2, 2);
            i01.SetRotation(0, 0, -15);
            i01.SetColor(1, 0, 0);
            AddGameObject(i01);

            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.SetCollisionType(ColliderType.ConvexHull);
            p1.SetModel("KWQuad");
            p1.SetTexture(@".\textures\spritesheet.png");
            p1.SetTextureRepeat(0.1f, 0.33f);
            p1.SetTextureOffset(0, 1);
            p1.SetScale(1, 1.33f, 1);
            p1.HasTransparencyTexture = true;
            p1.SetHitboxScale(1, 1, 100);
            p1.SetColor(1, 1, 1);
            p1.SetPosition(0, 2, 0);
            AddGameObject(p1);
        }
    }
}
