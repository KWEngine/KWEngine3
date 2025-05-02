using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedLightObject
    {
        public string Name { get; set; }
        public ShadowQuality ShadowCasterType { get; set; }
        public SunShadowType ShadowCasterSunType { get; set; }
        public LightType LightType { get; set; }
        public float ShadowBias { get; set; }
        public float[] Position { get; set; }
        public float[] Target { get; set; }
        public float Far { get; set; }
        public float Near { get; set; }
        public float FOV { get; set; }
        public float[] Color { get; set; }
        public float ShadowOffset { get; set; }

        public static SerializedLightObject GenerateSerializedLightObject(LightObject l)
        {
            SerializedLightObject sl = new SerializedLightObject();

            sl.Name = l.Name;
            sl.ShadowCasterType = l.ShadowQualityLevel;
            sl.LightType = l.Type;
            sl.ShadowBias = l._shadowBias;
            sl.Position = new float[] { l._stateCurrent._position.X, l._stateCurrent._position.Y, l._stateCurrent._position.Z };
            sl.Target = new float[] { l._stateCurrent._target.X, l._stateCurrent._target.Y, l._stateCurrent._target.Z };
            sl.Far = l._stateCurrent._nearFarFOVType.Y;
            sl.Near = l._stateCurrent._nearFarFOVType.X;
            sl.FOV = l._stateCurrent._nearFarFOVType.Z;
            sl.Color = new float[] { l._stateCurrent._color.X, l._stateCurrent._color.Y, l._stateCurrent._color.Z, l._stateCurrent._color.W };
            sl.ShadowOffset = l._shadowOffset;

            return sl;
        }
    }
}
