using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldFlowFieldTest
{
    public class Enemy : GameObject
    {
        private Queue<Vector3> _lastDirections = new Queue<Vector3>(60);

        public override void Act()
        {
            
            Vector3 myNewDirection = Vector3.Zero;

            FlowField f = CurrentWorld.GetFlowField();
            if(f != null)
            {
                if(f.ContainsXZ(this) )
                {
                    if (f.HasTarget)
                    {
                        _lastDirections.Enqueue(f.GetBestDirectionForPosition(this.Position));
                        if (_lastDirections.Count > 60)
                            _lastDirections.Dequeue();

                        foreach (Vector3 v in _lastDirections)
                        {
                            myNewDirection += v;
                        }
                        myNewDirection = myNewDirection / 60f;
                    }
                }
                else
                {
                    if(f.HasTarget)
                    {
                        myNewDirection = f.GetLinearDirectionForPosition(this.Position);
                    }
                }
                
            }

            if(myNewDirection != Vector3.Zero)
            {
                MoveAlongVector(myNewDirection, 0.02f);
            }

            List<Intersection> intersections = GetIntersections();
            foreach(Intersection intersection in intersections)
            {
                MoveOffset(intersection.MTV);
            }
            
        }
    }
}
