using KWEngine3;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class Shot : GameObject
    {

        public Shot()
        {
            IsCollisionObject = true;
            SetScale(0.1f, 0.1f, 2.0f);
            SetColor(1, 0, 0);
            SetColorEmissive(1, 1, 0, 2);
        }
        public override void Act()
        {
            Move(0.2f);
            if(IsInsideScreenSpace == false)
            {
                CurrentWorld.RemoveGameObject(this);
                return;
            }

            Intersection i = GetIntersection();
            if(i != null)
            {
                if(i.Object is Saw)
                {
                    Saw s = (Saw)i.Object;
                    s.TakeDamage(1);
                    CurrentWorld.RemoveGameObject(this);
                    CastExplosion();
                }
            }

        }

        private void CastExplosion()
        {
            ExplosionObject ex = new ExplosionObject(64, 0.5f, 0.5f, 1.0f, KWEngine3.ExplosionType.Star);
            ex.SetColorEmissive(1, 0, 0, 5);
            ex.SetPosition(this.Position - this.LookAtVector * 0.5f);
            CurrentWorld.AddExplosionObject(ex);
        }
    }
}
