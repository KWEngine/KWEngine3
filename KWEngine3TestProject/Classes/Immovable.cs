using KWEngine3.GameObjects;
using KWEngine3.Helper;

namespace KWEngine3TestProject.Classes
{
    public class Immovable : GameObject
    {
        private float _rotation0 = 0f;

        private float _offsetY = 0.0f;
        private float _offsetTime = 0f;
        public override void Act()
        {
            HelperRotation.SetMeshPreRotationYZX(this, 0, 0, _rotation0, 0);
            _rotation0 = (_rotation0 + 0.125f) % 360;


            SetTextureOffset(0f, _offsetY, 1);
            SetTextureRepeat(1f, 0.25f, 1);

            if(WorldTime - _offsetTime > 0.067f)
            {
                _offsetY = (_offsetY + 1f) % 4;
                _offsetTime = WorldTime;
            }

        }
    }
}
