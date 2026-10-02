using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal struct HitboxFace
    {
        public int FaceIndex; // vertices are read from Owner on demand (no array per face)
        public Vector3 Normal;
        public bool NormalFlip;
        public GameObjectHitbox Owner;
    }
}
