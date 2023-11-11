using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlatformerPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldGLTFTest
{
    public class GLTFPlayer : GameObject
    {
        public override void Act()
        {
            if (IsAnimated)
            {
                SetAnimationID(0);
                SetAnimationPercentageAdvance(0.005f);
            }
        }
    }
}
