using System;
using OpenTK.Mathematics;

namespace KWEngine3
{
    internal struct WorldBackgroundState
    {
        public Vector2 Clip;
        public Vector2 Scale;
        public Vector2 Offset;

        public WorldBackgroundState()
        {
            Clip = Vector2.One;
            Scale = Vector2.One;
            Offset = Vector2.Zero;
        }
    }
}
