using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal struct HitboxFace
    {
        public Vector3[] Vertices;
        public Vector3 Normal;
        public GameObjectHitbox Owner;
    }
}
