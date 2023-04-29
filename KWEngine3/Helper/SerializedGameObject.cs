using KWEngine3.GameObjects;
using KWEngine3.Model;

namespace KWEngine3.Helper
{
    internal class SerializedGameObject
    {
        // MISC. PROPERTIES
        public bool IsShadowCaster { get; set; }
        public bool IsCollisionObject { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ModelName { get; set; }
        public string ModelPath { get; set; }
        public bool UpdateLast { get; set; }

        // POSITION, ROTATION, SCALE
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scale { get; set; }
        public float[] PositionOffset { get; set; }
        public float[] RotationOffset { get; set; }
        public float[] ScaleOffset { get; set; }
        public int AttachedToID { get; set; }
        public string AttachedToParentBone { get; set; }
        public float[] ScaleHitbox { get; set; }

        // COLOR
        public float[] Color { get; set; }
        public float[] ColorEmissive { get; set; }
        public float Metallic { get; set; }
        public float Roughness { get; set; }
        public MetallicType MetallicType { get; set; }
        public List<float[]> TextureOffset { get; set; }
        public List<float[]> TextureRepeat { get; set; }
        public string[] TextureAlbedo { get; set; }
        public string[] TextureNormal { get; set; }
        public string[] TextureRoughness { get; set; }
        public string[] TextureMetallic { get; set; }
        public string[] TextureEmissive { get; set; }
        public bool[] TextureRoughnessInMetallic { get; set; }
        public float[] TextureTransform { get; set; }

        public static SerializedGameObject GenerateSerializedGameObject(GameObject g, World w)
        {
            SerializedGameObject sg = new SerializedGameObject();
            sg.ID = g.ID;
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

            // Export/import repeat for all materials...
            sg.TextureOffset = new List<float[]>();
            sg.TextureRepeat = new List<float[]>();
            foreach (GeoMaterial mat in g._gModel.Material)
            {
                sg.TextureRepeat.Add(new float[] { mat.TextureAlbedo.UVTransform.X, mat.TextureAlbedo.UVTransform.Y });
                sg.TextureOffset.Add(new float[] { mat.TextureAlbedo.UVTransform.Z, mat.TextureAlbedo.UVTransform.W });
            }

            sg.TextureAlbedo = new string[g._gModel.Material.Length];
            sg.TextureNormal = new string[g._gModel.Material.Length];
            sg.TextureRoughness = new string[g._gModel.Material.Length];
            sg.TextureMetallic = new string[g._gModel.Material.Length];
            sg.TextureEmissive = new string[g._gModel.Material.Length];
            sg.TextureRoughnessInMetallic = new bool[g._gModel.Material.Length];

            int i = 0;
            foreach (GeoMaterial mat in g._gModel.Material)
            {
                sg.TextureAlbedo[i] = mat.TextureAlbedo.IsTextureSet ? mat.TextureAlbedo.Filename : null;
                sg.TextureNormal[i] = mat.TextureNormal.IsTextureSet ? mat.TextureNormal.Filename : null;
                sg.TextureRoughness[i] = mat.TextureRoughness.IsTextureSet ? mat.TextureRoughness.Filename : null;
                sg.TextureMetallic[i] = mat.TextureMetallic.IsTextureSet ? mat.TextureMetallic.Filename : null;
                sg.TextureEmissive[i] = mat.TextureEmissive.IsTextureSet ? mat.TextureEmissive.Filename : null;
                sg.TextureRoughnessInMetallic[i] = mat.TextureRoughnessInMetallic;
                i++;
            }

            sg.TextureTransform = new float[] { g._stateCurrent._uvTransform.X, g._stateCurrent._uvTransform.Y, g._stateCurrent._uvTransform.Z, g._stateCurrent._uvTransform.W };

            if(g.IsAttachedToGameObject)
            {
                GameObject parent = g.GetGameObjectThatIAmAttachedTo();
                if(parent != null)
                {
                    sg.AttachedToID = parent.ID;
                    string boneName = parent.GetBoneNameForAttachedGameObject(g);
                    if(boneName != null)
                    {
                        sg.AttachedToParentBone = boneName;
                    }
                    sg.PositionOffset = new float[] { g._positionOffsetForAttachment.X, g._positionOffsetForAttachment.Y, g._positionOffsetForAttachment.Z};
                    sg.RotationOffset = new float[] { g._rotationOffsetForAttachment.X, g._rotationOffsetForAttachment.Y, g._rotationOffsetForAttachment.Z, g._rotationOffsetForAttachment.W};
                    sg.ScaleOffset = new float[] {g._scaleOffsetForAttachment.X, g._scaleOffsetForAttachment.Y,g._scaleOffsetForAttachment.Z};
                }
                else
                {
                    sg.AttachedToID = 0;
                    sg.AttachedToParentBone = "";
                    sg.PositionOffset = new float[] { 0, 0, 0 };
                    sg.RotationOffset = new float[] { 0, 0, 0, 1 };
                    sg.ScaleOffset = new float[] { 1, 1, 1 };
                }
            }

            return sg;
        }
    }
}
