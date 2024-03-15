using KWEngine3;
using KWEngine3TestProject.Classes.WorldPlaneColliderTest;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    public class GameWorldPlaneColliderTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 15, 15);


            KWEngine.LoadModel("Test01", "./Models/GLTFTest/planecollidertest01.glb");

            Immovable collider = new Immovable();
            collider.SetModel("Test01");
            collider.IsCollisionObject = true;
            AddGameObject(collider);

            Player p = new Player();
            p.SetModel("KWCube");
            p.SetPosition(0, 0.5f, 5f);
            p.SetColor(1, 0, 1);
            p.IsCollisionObject = true;
            AddGameObject(p);
        }
    }
}
