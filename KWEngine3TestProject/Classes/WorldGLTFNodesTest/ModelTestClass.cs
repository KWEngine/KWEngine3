using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldGLTFNodesTest
{
    internal class ModelTestClass : RenderObject
    {
        public override void Act()
        {
            if(HasAnimations)
            {
                SetAnimationID(0);
                SetAnimationPercentageAdvance(0.001f);
            }
        }
    }
}
