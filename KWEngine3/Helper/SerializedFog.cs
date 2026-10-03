using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedFog
    {
        public float[] Color { get; set; }
        public float Density { get; set; }
        public float Height { get; set; }
        public float HeightFalloff { get; set; }
        public float NoiseStrength { get; set; }
        public float NoiseSize { get; set; }
        public float NoiseHeight { get; set; }
        public float[] WindDirection { get; set; }
        public float WindSpeed { get; set; }

        public static SerializedFog GenerateSerializedFog(World w)
        {
            SerializedFog sf = new SerializedFog();
            sf.Color = new float[] { w._fogColor.X, w._fogColor.Y, w._fogColor.Z };
            sf.Density = w._fogDensity;
            sf.Height = w._fogHeight;
            sf.HeightFalloff = w._fogHeightFalloff;
            sf.NoiseStrength = w._fogNoiseStrength;
            sf.NoiseSize = w._fogNoiseSize;
            sf.NoiseHeight = w._fogHeightNoise;
            sf.WindDirection = new float[] { w._fogWindDirection.X, w._fogWindDirection.Y, w._fogWindDirection.Z };
            sf.WindSpeed = w._fogWindSpeed;
            return sf;
        }
    }
}
