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
        private Saw _parent = null;

        public SawText(Saw parent)
        {
            _parent = parent;
            SetCharacterDistanceFactor(0.75f);
            SetColorEmissive(1, 1, 1, 0.5f);
        }
    }
}
