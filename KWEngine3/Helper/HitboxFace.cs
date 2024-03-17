using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal struct HitboxFace
    {
        public Vector3 V1; 
        public Vector3 V2; 
        public Vector3 V3;
        public Vector3 Normal;
        public GameObjectHitbox Owner;
    }
}
