using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedGameObject
    {
        // MISC. PROPERTIES
        public bool IsShadowCaster { get; set; }
        public bool IsCollisionObject { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ModelName { get; set; }
        public string ModelPath { get; set; }
        public bool UpdateLast { get; set; }

        // POSITION, ROTATION, SCALE
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scale { get; set; }
        public float[] ScaleHitbox { get; set; }

        // COLOR
        public float[] Color { get; set; }
        public float[] ColorEmissive { get; set; }
        public float Metallic { get; set; }
        public float Roughness { get; set; }
        public MetallicType MetallicType { get; set; }
        public float[] TextureOffset { get; set; }
        public float[] TextureRepeat { get; set; }
        public string TextureAlbedo { get; set; }
        public string TextureNormal { get; set; }
        public string TextureRoughness { get; set; }
        public string TextureMetallic { get; set; }
        public string TextureEmissive { get; set; }

        public static SerializedGameObject GenerateSerializedGameObject(GameObject g)
        {
            SerializedGameObject sg = new SerializedGameObject();

            sg.IsShadowCaster = g.IsShadowCaster;
            sg.IsCollisionObject = g.IsCollisionObject;
            sg.Name = g.Name;
            sg.ModelName = g._modelNameInDB;
            sg.ModelPath = g._gModel.ModelOriginal.Filename;
            sg.Type = g.GetType().FullName;
            sg.UpdateLast = g.UpdateLast;

            sg.Position = new float[] { g._stateCurrent._position.X, g._stateCurrent._position.Y, g._stateCurrent._position.Z };
            sg.Rotation = new float[] { g._stateCurrent._rotation.X, g._stateCurrent._rotation.Y, g._stateCurrent._rotation.Z, g._stateCurrent._rotation.W };
            sg.Scale = new float[] { g._stateCurrent._scale.X, g._stateCurrent._scale.Y, g._stateCurrent._scale.Z };
            sg.ScaleHitbox = new float[] { g._stateCurrent._scaleHitbox.X, g._stateCurrent._scaleHitbox.Y, g._stateCurrent._scaleHitbox.Z };

            sg.Color = new float[] { g._stateCurrent._colorTint.X, g._stateCurrent._colorTint.Y, g._stateCurrent._colorTint.Z };
            sg.ColorEmissive = new float[] { g._stateCurrent._colorEmissive.X, g._stateCurrent._colorEmissive.Y, g._stateCurrent._colorEmissive.Z, g._stateCurrent._colorEmissive.W };
            sg.Metallic = g._gModel._metallic;
            sg.Roughness = g._gModel._roughness;
            sg.MetallicType = g._gModel._metallicType;
            sg.TextureOffset = new float[] { g._gModel.Material[0].TextureAlbedo.UVTransform.Z, g._gModel.Material[0].TextureAlbedo.UVTransform.W };
            sg.TextureRepeat = new float[] { g._gModel.Material[0].TextureAlbedo.UVTransform.X, g._gModel.Material[0].TextureAlbedo.UVTransform.Y };
            sg.TextureAlbedo = g._gModel.Material[0].TextureAlbedo.Filename;
            sg.TextureNormal = g._gModel.Material[0].TextureNormal.Filename;
            sg.TextureRoughness = g._gModel.Material[0].TextureRoughness.Filename;
            sg.TextureMetallic = g._gModel.Material[0].TextureMetallic.Filename;
            sg.TextureEmissive = g._gModel.Material[0].TextureEmissive.Filename;

            return sg;
        }
    }
}
