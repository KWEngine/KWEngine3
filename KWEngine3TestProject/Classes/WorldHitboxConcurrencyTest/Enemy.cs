using KWEngine3.GameObjects;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldHitboxConcurrencyTest
{
    internal class Enemy : GameObject
    {
        private bool _hasSwitchedModel = false;
        public override void Act()
        {
            if(IsInsideScreenSpace == false)
            {
                CurrentWorld.RemoveGameObject(this);
                return;
            }

            int r = HelperRandom.GetRandomNumber(1, 1000);
            if(r == 1 && _hasSwitchedModel == false)
            {
                _hasSwitchedModel = true;
                SetModel("EnemyShip");
            }

            MoveOffset(0, 0, +0.015f);
        }
    }
}
