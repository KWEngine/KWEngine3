using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Models.WorldKWQuadTest
{
    public class PlayerQuad : GameObject
    {
        public override void Act()
        {
            Vector3 mousePos3D = HelperIntersection.GetMouseIntersectionPointOnPlane(KWEngine3.Plane.XZ, 0);
            TurnTowardsXZForKWQuad(mousePos3D);

            Console.WriteLine(HelperVector.GetScreenAngleBetween(this.Position, mousePos3D));
            Console.WriteLine(this.LookAtVectorLocalRight);
            Console.WriteLine(  "----");
            //AddRotationZ(-90);
        }
        
    }
}
