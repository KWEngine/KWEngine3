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
            Material = new GeoMaterial[ModelOriginal.Meshes.Count + (mOrg.IsTerrain ? 1 : 0)];

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
            if(ModelOriginal.MeshCollider.MeshHitboxes.Count == 0)
            {
                // fallback: use the max/min of the meshes' vertices for calculating dimensions
                foreach(GeoMesh mesh in ModelOriginal.Meshes.Values)
                {
                    if (mesh.Vertices != null && mesh.Vertices.Length > 0)
                    {
                        foreach (GeoVertex v in mesh.Vertices)
                        {
                            if (v.X < minX) minX = v.X;
                            if (v.X > maxX) maxX = v.X;

                            if (v.Y < minY) minY = v.Y;
                            if (v.Y > maxY) maxY = v.Y;

                            if (v.Z < minZ) minZ = v.Z;
                            if (v.Z > maxZ) maxZ = v.Z;
                        }
                    }
                    else
                    {
                        GeoMesh.GenerateVerticesFromVBO(mesh.VBOPosition, ref minX, ref maxX, ref minY, ref maxY, ref minZ, ref maxZ);
                    }
                }
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
                    textureId = KWEngine.CurrentWorld._customTextures[filename].ID;
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
                        KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(textureId, OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D));
                    }

                }
                Material[meshId].SetTexture(filename, type, textureId);
            }
            else
            {
                KWEngine.LogWriteLine("[EngineObject] Invalid texture id");
            }
        }

        public void UnsetTextureForPrimitive(TextureType type, int meshId = 0)
        {
            Material[meshId].SetTexture(null, type, -1);
        }
    }
}
