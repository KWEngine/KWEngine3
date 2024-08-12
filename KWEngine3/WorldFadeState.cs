using System;
using OpenTK.Mathematics;

namespace KWEngine3
{
    internal struct WorldFadeState
    {
        public Vector3 Color;
        public float Factor;

        public WorldFadeState()
        {
            Color = Vector3.One;
            Factor = 1.0f;
        }
    }
}
