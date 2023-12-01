using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldHitboxConcurrencyTest
{
    internal class ShotPlayer : GameObject
    {
        public override void Act()
        {
            if(IsInsideScreenSpace == false)
            {
                CurrentWorld.RemoveGameObject(this);
                return;
            }

            Move(0.1f);
            
            foreach(Intersection i in GetIntersections<Enemy>())
            {
                CurrentWorld.RemoveGameObject(i.Object);
                CurrentWorld.RemoveGameObject(this);
                ExplosionObject e = new ExplosionObject(32, 1.0f, 2.0f, 2.0f, KWEngine3.ExplosionType.Star);
                e.SetPosition(i.Object.Position);
                e.SetColorEmissive(1, 1, 0, 2);
                CurrentWorld.AddExplosionObject(e);
            }
        }
    }
}
