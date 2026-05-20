using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldAttachments
{
    internal class VSG : ViewSpaceGameObject
    {
        public VSG()
        {
            SetModel("FPS_ARMS");
            SetAnimationID(0);
        }
        public override void Act()
        {
            SetAnimationPercentageAdvance(0.001f);
        }
    }
}
