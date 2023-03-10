using glTFLoader.Schema;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal class GameObjectModel
    {
        public GeoModel ModelOriginal { get; set; } = null;
        public GeoMaterial[] Material { get; set; }
        internal float _metallic = 0f;
        internal float _roughness = 1f;
        internal MetallicType _metallicType = MetallicType.Default;

        public GameObjectModel(GeoModel mOrg)
        {
            ModelOriginal = mOrg;
            Material = new GeoMaterial[ModelOriginal.Meshes.Count];
        }

        public void SetTexture(string filename, TextureType type, int meshId)
        {
            int textureId;
            if(KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
            {
                textureId = KWEngine.CurrentWorld._customTextures[filename];
            }
            else
            {
                HelperGeneral.CheckGLErrors();
                textureId = HelperTexture.LoadTextureForModelExternal(filename, out int mipMaps);
                HelperGeneral.CheckGLErrors();
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

        public void UnsetTextureForPrimitive(TextureType type)
        {
            Material[0].SetTexture(null, type, -1);
        }
    }
}
