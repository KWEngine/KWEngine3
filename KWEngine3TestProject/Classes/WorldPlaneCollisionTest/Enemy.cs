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
        private Queue<Vector3> _directionVectors = new();

        public override void Act()
        {
            FlowField f = CurrentWorld.GetFlowField();
            if (f != null && f.HasTarget)
            {
                Vector3 direction = f.GetBestDirectionForPosition(this.Position);
                if (_directionVectors.Count == 30)
                {
                    _directionVectors.Dequeue();
                }
                _directionVectors.Enqueue(direction);
                Vector3 avgDirection = Vector3.Zero;
                foreach (Vector3 dir in _directionVectors)
                {
                    avgDirection += dir;
                }
                avgDirection /= _directionVectors.Count;

                MoveAlongVector(avgDirection, 0.005f);

                Player p = CurrentWorld.GetGameObjectByName<Player>("Player #1");
                if (p != null)
                {
                    List<RayIntersection> results = HelperIntersection.RayTraceObjectsForViewVectorFastest(
                        this.Center,
                        Vector3.NormalizeFast(p.Center - this.Center),
                        this,
                        0,
                        true,
                        typeof(Player), typeof(Immovable)
                        );
                    if (results.Count > 0 && results[0].Object == p)
                    {
                        TurnTowardsXZ(p.Center);
                    }
                    else
                    {
                        TurnTowardsXZ(Center + avgDirection);
                    }
                }
                else
                {
                    TurnTowardsXZ(Center + avgDirection);
                }
                
            }
            else
            {
                _directionVectors.Clear();
            }

            foreach (Intersection i in GetIntersections())
            {
                MoveOffset(i.MTV);
            }
        }
    }
}
