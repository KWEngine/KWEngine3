using KWEngine3.GameObjects;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldMousePositionResetTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            SetPosition(HelperIntersection.GetMouseIntersectionPointOnPlane(KWEngine3.Plane.XZ, 0f));
            Intersection i = GetIntersection();
            if(i != null)
            {
                SetColorEmissive(1, 1, 0, 2);
            }
            else
            {
                SetColorEmissive(1, 1, 0, 0);
            }
        }
    }
}
