using KWEngine3.GameObjects;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Helper
{
    internal class SerializedRenderObject
    {
        // MISC. PROPERTIES
        public bool IsShadowCaster { get; set; }
        public bool IsCollisionObject { get; set; }
        public bool BlendTextureStates { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ModelName { get; set; }
        public string ModelPath { get; set; }
        public bool UpdateLast { get; set; }
        public bool IsDepthTesting { get; set; } = true;
        public bool IsAffectedByLight { get; set; } = true;
        public float Opacity { get; set; } = 1;

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
        public List<float> Metallic { get; set; }
        public List<float> Roughness { get; set; }
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
        public int InstanceCount { get; set; }
        public float[] InstanceMatrices { get; set; }

        public static SerializedRenderObject GenerateSerializedRenderObject(RenderObject r, World w)
        {
            SerializedRenderObject rg = new SerializedRenderObject();
            rg.IsAffectedByLight = r.IsAffectedByLight;
            rg.Opacity = r._stateCurrent._opacity;
            rg.IsDepthTesting = r.IsDepthTesting;
            rg.IsShadowCaster = r.IsShadowCaster;
            rg.Name = r.Name;
            rg.ModelName = r._modelNameInDB;
            rg.ModelPath = r._model.ModelOriginal.Filename;
            rg.Type = r.GetType().FullName;

            rg.Position = new float[] { r._stateCurrent._position.X, r._stateCurrent._position.Y, r._stateCurrent._position.Z };
            rg.Rotation = new float[] { r._stateCurrent._rotation.X, r._stateCurrent._rotation.Y, r._stateCurrent._rotation.Z, r._stateCurrent._rotation.W };
            rg.Scale = new float[] { r._stateCurrent._scale.X, r._stateCurrent._scale.Y, r._stateCurrent._scale.Z };
            rg.ScaleHitbox = new float[] { r._stateCurrent._scaleHitbox.X, r._stateCurrent._scaleHitbox.Y, r._stateCurrent._scaleHitbox.Z };

            rg.Color = new float[] { r._stateCurrent._colorTint.X, r._stateCurrent._colorTint.Y, r._stateCurrent._colorTint.Z };
            rg.ColorEmissive = new float[] { r._stateCurrent._colorEmissive.X, r._stateCurrent._colorEmissive.Y, r._stateCurrent._colorEmissive.Z, r._stateCurrent._colorEmissive.W };
            
            rg.MetallicType = r._model._metallicType;

            // Export/import repeat for all materials...
            rg.TextureOffset = new List<float[]>();
            rg.TextureRepeat = new List<float[]>();
            rg.Metallic = new List<float>();
            rg.Roughness = new List<float>();
            rg.TextureAlbedo = new string[r._model.Material.Length];
            rg.TextureNormal = new string[r._model.Material.Length];
            rg.TextureRoughness = new string[r._model.Material.Length];
            rg.TextureMetallic = new string[r._model.Material.Length];
            rg.TextureEmissive = new string[r._model.Material.Length];
            rg.TextureRoughnessInMetallic = new bool[r._model.Material.Length];

            int i = 0;
            foreach (GeoMaterial mat in r._model.Material)
            {
                rg.TextureRepeat.Add(new float[] { mat.TextureAlbedo.UVTransform.X, mat.TextureAlbedo.UVTransform.Y });
                rg.TextureOffset.Add(new float[] { mat.TextureAlbedo.UVTransform.Z, mat.TextureAlbedo.UVTransform.W });
                rg.Metallic.Add(mat.Metallic);
                rg.Roughness.Add(mat.Roughness);

                rg.TextureAlbedo[i] = mat.TextureAlbedo.IsTextureSet ? mat.TextureAlbedo.Filename : null;
                rg.TextureNormal[i] = mat.TextureNormal.IsTextureSet ? mat.TextureNormal.Filename : null;
                rg.TextureRoughness[i] = mat.TextureRoughness.IsTextureSet ? mat.TextureRoughness.Filename : null;
                rg.TextureMetallic[i] = mat.TextureMetallic.IsTextureSet ? mat.TextureMetallic.Filename : null;
                rg.TextureEmissive[i] = mat.TextureEmissive.IsTextureSet ? mat.TextureEmissive.Filename : null;
                rg.TextureRoughnessInMetallic[i] = mat.TextureRoughnessInMetallic;
                i++;
            }

            rg.TextureTransform = new float[] { r._stateCurrent._uvTransform.X, r._stateCurrent._uvTransform.Y, r._stateCurrent._uvTransform.Z, r._stateCurrent._uvTransform.W };

            // Instancing
            rg.InstanceCount = r.InstanceCount;
            rg.InstanceMatrices = new float[r.InstanceCount * 16];
            
            GL.BindBuffer(BufferTarget.UniformBuffer, r._ubo);
            GL.GetBufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, r.InstanceCount * RenderObject.BYTESPERINSTANCE, rg.InstanceMatrices);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            return rg;
        }
    }
}
