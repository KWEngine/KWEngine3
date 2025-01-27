using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.GameWorldMatchSurfaceNormal
{
    internal class MovingObject : GameObject
    {
        public override void Act()
        {
            Move(0.005f);
            List<Intersection> intersections = GetIntersections();
            foreach (Intersection intersection in intersections)
            {
                if(intersection.Object is Floor)
                {
                    if (HelperIntersection.RaytraceObjectFastest(intersection.Object, this.Position, this.LookAtVector, out float distance))
                    {
                        CurrentWorld.RemoveGameObject(this);
                        Decal d = new Decal();
                        d.SetModel("KWQuad");
                        d.SetColor(1, 0, 0);
                        d.SetPosition(this.Position + this.LookAtVector * distance);
                        d.TurnTowardsXYZ(d.Position + intersection.ColliderSurfaceNormal);
                        CurrentWorld.AddGameObject(d);
                    }
                }
            }
        }
    }
}
