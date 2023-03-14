using KWEngine3.GameObjects;
using KWEngine3;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace KWEngine3TestProject.Classes.WorldThirdPersonView
{
    public class Shot : GameObject
    {
        private float _speed = 0.5f;
        private float _distance = 0;
        private Vector3 _lastPos = Vector3.Zero;
        public override void Act()
        {
            _distance += _speed;
            Move(_speed);

            Intersection i = GetIntersection();
            if (i != null && !(i.Object is Player))
            {
                ExplosionObject ex = new ExplosionObject(8, 0.25f, 0.5f, 2f, ExplosionType.Cube);
                ex.SetPosition(_lastPos != Vector3.Zero ? _lastPos : Position);
                ex.SetColorEmissive(1, 1, 0, 2);
                CurrentWorld.AddExplosionObject(ex);
                CurrentWorld.RemoveGameObject(this);
            }
            else if (_distance > 50)
            {
                CurrentWorld.RemoveGameObject(this);
            }
            _lastPos = Position;
        }
    }
}
