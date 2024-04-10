using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPlaneCollisionTest
{
    public class Enemy : GameObject
    {
        public override void Act()
        {
            FlowField f = CurrentWorld.GetFlowField();
            if (f != null && f.HasTarget)
            {
                Vector3 direction = f.GetBestDirectionForPosition(this.Position);
                MoveAlongVector(direction, 0.005f);
            }

            foreach(Intersection i in GetIntersections())
            {
                MoveOffset(i.MTV);
            }
        }
    }
}
