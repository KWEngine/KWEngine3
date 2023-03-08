using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal enum GeoKeyframeType { Rotation, Translation, Scale }

    internal struct GeoAnimationKeyframe
    {
        public GeoKeyframeType Type { get; internal set; }

        public Quaternion Rotation { get; internal set; }
        public Vector3 Translation { get; internal set; }
        public Vector3 Scale { get; internal set; }

        public float Time { get; internal set; }

    }
}
