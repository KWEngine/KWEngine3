using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldGLTFTest
{
    internal class GLTFScooter : GameObject
    {
        public override void Act()
        {
            SetAnimationID(0);
            SetAnimationPercentageAdvance(0.001f);
        }
    }
}
