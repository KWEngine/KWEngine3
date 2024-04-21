using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldRayCollision;
using OpenTK.Mathematics;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldRaytracedCollision : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            SetCameraPosition(0, 1, 50);
            SetCameraTarget(0, 1, 0);
            SetCameraFOV(10);

            Player p = new Player();
            p.SetModel("KWQuad");
            p.SetPosition(-5.6f, 2f, 0);
            p.SetScale(1);
            p.SetTexture("./Textures/spritesheet.png");
            p.SetTextureRepeat(1f / 10f, 1f / 3f);
            p.HasTransparencyTexture = true;
            p.IsCollisionObject = true;
            AddGameObject(p);

            Immovable floorleft = new Immovable();
            floorleft.SetPosition(-4, 0.0f, 0);
            floorleft.SetScale(4, 1, 1);
            floorleft.AddRotationZ(-14);
            floorleft.IsCollisionObject = true;
            AddGameObject(floorleft);

            Immovable floormid = new Immovable();
            floormid.SetPosition(0, -0.5f, 0);
            floormid.SetScale(4, 1, 1);
            floormid.IsCollisionObject = true;
            AddGameObject(floormid);

            Immovable floorright = new Immovable();
            floorright.SetPosition(+4, 0.0f, 0);
            floorright.SetScale(4, 1, 1);
            floorright.AddRotationZ(14);
            floorright.IsCollisionObject = true;
            AddGameObject(floorright);

        }
    }
}
