using glTFLoader.Schema;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal class EngineObjectModel
    {
        public GeoModel ModelOriginal { get; set; } = null;
        public GeoMaterial[] Material { get; set; }
        internal float _metallicTerrain = 0f;
        internal float _roughnessTerrain = 1f;
        internal MetallicType _metallicType = MetallicType.Default;
        public Vector4 Center { get; internal set; } = new Vector4(0, 0, 0, 1);
        public Vector4 DimensionsMin { get; internal set; } = new Vector4(0, 0, 0, 1);
        public Vector4 DimensionsMax { get; internal set; } = new Vector4(0, 0, 0, 1);


        public EngineObjectModel(GeoModel mOrg)
        {
            ModelOriginal = mOrg;
            Material = new GeoMaterial[ModelOriginal.Meshes.Count];

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            foreach (GeoMeshHitbox mhb in ModelOriginal.MeshCollider.MeshHitboxes)
            {
                if (mhb.minX < minX) minX = mhb.minX;
                if (mhb.maxX > maxX) maxX = mhb.maxX;

                if (mhb.minY < minY) minY = mhb.minY;
                if (mhb.maxY > maxY) maxY = mhb.maxY;

                if (mhb.minZ < minZ) minZ = mhb.minZ;
                if (mhb.maxZ > maxZ) maxZ = mhb.maxZ;
            }
            DimensionsMax = new(maxX, maxY, maxZ, 1f);
            DimensionsMin = new(minX, minY, minZ, 1f);
            Center = new ((DimensionsMax.Xyz + DimensionsMin.Xyz) / 2f, 1f);
        }
    

        public void SetTexture(string filename, TextureType type, int meshId)
        {
            if (Material.Length > meshId)
            {
                int textureId;
                if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
                {
                    textureId = KWEngine.CurrentWorld._customTextures[filename];
                }
                else
                {
                    textureId = HelperTexture.LoadTextureForModelExternal(filename, out int mipMaps);
                    if (textureId < 0)
                    {
                        textureId = KWEngine.TextureDefault;
                    }
                    else
                    {
                        KWEngine.CurrentWorld._customTextures.Add(filename, textureId);
                    }

                }
                Material[meshId].SetTexture(filename, type, textureId);
            }
            else
            {
                KWEngine.LogWriteLine("[GameObject] Invalid texture id");
            }
        }

        public void UnsetTextureForPrimitive(TextureType type, int meshId = 0)
        {
            Material[meshId].SetTexture(null, type, -1);
        }
    }
}
