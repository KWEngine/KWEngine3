using KWEngine3.GameObjects;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Helper
{
    internal class SerializedFoliageObject
    {
        public bool IsAffectedByLight { get; set; }
        public bool IsShadowReceiver { get; set; }
        public bool IsSizeReducedAtCorners { get; set; }
        public string Name { get; set; }
        public FoliageType FoliageType { get; set; }

        // POSITION, ROTATION, SCALE
        public float[] Position { get; set; }

        public float[] PatchSize { get; set; }
        public float[] Scale { get; set; }
        public int AttachedToID { get; set; }

        // COLOR
        public float[] Color { get; set; }
        public float Roughness { get; set; }
        public int InstanceCount { get; set; }
        public float SwayFactor { get; set; }

        public static SerializedFoliageObject GenerateSerializedFoliageObject(FoliageObject f, World w)
        {
            SerializedFoliageObject sfobject = new SerializedFoliageObject();
            sfobject.IsAffectedByLight = f.IsAffectedByLight;
            sfobject.IsShadowReceiver = f.IsShadowReceiver;
            sfobject.IsSizeReducedAtCorners = f.IsSizeReducedAtCorners;
            sfobject.Name = f.Name;
            sfobject.FoliageType = f.Type;
            sfobject.Roughness = f._roughness;
            sfobject.SwayFactor = f._swayFactor;
            sfobject.PatchSize = new float[] { f._patchSize.X, f._patchSize.Y };

            sfobject.Position = new float[] { f._position.X, f._position.Y, f._position.Z };
            sfobject.Scale = new float[] { f._scale.X, f._scale.Y, f._scale.Z };
            sfobject.Color = new float[] { f._color.X, f._color.Y, f._color.Z, f._color.W};


            // Instancing
            sfobject.InstanceCount = f.FoliageCount;
            sfobject.AttachedToID = f._terrainObject != null ? f._terrainObject.ID : -1;
            
            return sfobject;
        }
    }
}
