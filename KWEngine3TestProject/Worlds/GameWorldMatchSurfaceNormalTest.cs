using KWEngine3;
using KWEngine3TestProject.Classes.GameWorldMatchSurfaceNormal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMatchSurfaceNormalTest : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 10);

            Floor f2 = new Floor();
            f2.SetPosition(-4.5f, 2, 0);
            f2.SetScale(1, 4, 10);
            f2.IsCollisionObject = true;
            f2.SetColor(1, 1, 0);
            AddGameObject(f2);

            Floor f = new Floor();
            f.SetPosition(0, -1, 0);
            f.SetScale(10, 1, 10);
            f.IsCollisionObject = true;
            f.SetColor(0, 0, 1);
            AddGameObject(f);

            MovingObject mo = new MovingObject();
            mo.SetPosition(1, 1, 1);
            mo.SetScale(0.5f);
            mo.TurnTowardsXYZ(0, 0, 0);
            //mo.TurnTowardsXYZ(-1, 1, 1);
            mo.SetColor(0, 1, 0);
            mo.IsCollisionObject = true;
            AddGameObject(mo);

        }
    }
}
