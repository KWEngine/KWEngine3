using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class SawText : TextObject
    {
        private float _counter = 0;
        public SawText()
        {
            SetCharacterDistanceFactor(0.75f);
            SetColorEmissive(1, 1, 1, 0.5f);
        }
        public override void Act()
        {
            _counter = (_counter + (float)Math.PI / 360) % ((float)Math.PI * 2.0f);
            float tmp = (float)Math.Sin(_counter);
            SetRotation(tmp * 0.5f, 0, tmp * 0.5f);

            float tmp2 = (tmp + 2.0f) / 20.0f + 0.85f;
            SetScale(tmp2);
        }
    }
}
