using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class PowerUpText : TextObject
    {
        private float _counter = 0;
        public PowerUpText()
        {
            SetCharacterDistanceFactor(1.125f);
            SetColorEmissive(1, 1, 1, 0.75f);
        }

        public override void Act()
        {
            _counter = (_counter + (float)Math.PI / 360) % ((float)Math.PI * 2.0f);
            float tmp = (float)Math.Sin(_counter) * 2.0f;
            SetRotation(tmp, 0, tmp);
        }
    }
}
