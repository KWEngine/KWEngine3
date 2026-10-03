using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedFogVolume
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public float[] Position { get; set; }
        public float[] Scale { get; set; }
        public float[] Rotation { get; set; }
        public float[] Color { get; set; }
        public float Density { get; set; }
        public float HeightFalloff { get; set; }
        public float EdgeSoftness { get; set; }

        public static SerializedFogVolume GenerateSerializedFogVolume(FogVolume v)
        {
            SerializedFogVolume sv = new SerializedFogVolume();
            sv.Type = v.GetType().FullName;
            sv.Name = v.Name;
            sv.Position = new float[] { v.Position.X, v.Position.Y, v.Position.Z };
            sv.Scale = new float[] { v.Scale.X, v.Scale.Y, v.Scale.Z };
            sv.Rotation = new float[] { v.Rotation.X, v.Rotation.Y, v.Rotation.Z, v.Rotation.W };
            sv.Color = new float[] { v.Color.X, v.Color.Y, v.Color.Z };
            sv.Density = v.Density;
            sv.HeightFalloff = v.HeightFalloff;
            sv.EdgeSoftness = v.EdgeSoftness;
            return sv;
        }
    }
}
