using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class TextObjectCustom : TextObject
    {


        public override void Act()
        {
            AddRotationY(0.5f);
            //AddRotationZ(0.25f);
            //Console.WriteLine(IsInsideScreenSpace);
        }
    }
}
