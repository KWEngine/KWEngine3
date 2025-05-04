
using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal abstract class FrustumShadowMap
    {
        protected Vector4[] _frustumPlanes = new Vector4[6];
        public abstract void Update(ref Matrix4 viewProjectionMatrix);
        public abstract bool IsBoxInFrustum(Vector3 lightCenter, float lightZFar, Vector3 center, float diameter);
    }
}
