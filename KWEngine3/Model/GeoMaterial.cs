using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using KWEngine3.Helper;

namespace KWEngine3.Model
{
    internal struct GeoMaterial
    {
        public string Name;
        public BlendingFactor BlendMode;
        public Vector4 ColorEmissive;
        public Vector4 ColorAlbedo;

        //public float Opacity;

        public GeoTexture TextureAlbedo;
        public GeoTexture TextureNormal;
        public GeoTexture TextureEmissive;
        public GeoTexture TextureMetallic;
        public GeoTexture TextureRoughness;
        public GeoTexture TextureTranparency;
        public GeoTexture TextureHeight;

        public float Metallic;
        public float Roughness;

        public bool TextureRoughnessIsSpecular;
        public bool TextureRoughnessInMetallic;
        public bool RenderBackFace;

        public void SetTexture(string texture, TextureType type, int id)
        {
            int width = 0;
            int height = 0;
            int mipmaps = 0;
            if(id > 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, id);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out width);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out height);
                mipmaps = MathHelper.Max(0, HelperTexture.GetMaxMipMapLevels(width, height) - 2);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            if (type == TextureType.Albedo)
            {
                TextureAlbedo = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(TextureAlbedo.UVTransform.X == 0 ? 1 : TextureAlbedo.UVTransform.X, TextureAlbedo.UVTransform.Y == 0 ? 1 : TextureAlbedo.UVTransform.Y, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else if (type == TextureType.Emissive)
            {
                TextureEmissive = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else if (type == TextureType.Metallic)
            {
                TextureMetallic = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else if (type == TextureType.Roughness)
            {
                TextureRoughness = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
                TextureRoughnessInMetallic = false;
            }
            else if (type == TextureType.Normal)
            {
                TextureNormal = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else if(type == TextureType.Transparency)
            {
                TextureTranparency = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else if (type == TextureType.Height)
            {
                TextureHeight = new GeoTexture()
                {
                    Filename = texture,
                    OpenGLID = id,
                    Type = type,
                    UVMapIndex = 0,
                    UVTransform = new Vector4(1, 1, 0, 0),
                    Width = width,
                    Height = height,
                    MipMaps = mipmaps
                };
            }
            else
                throw new System.Exception("Unknown texture type for current material " + Name);
        }
    }
}
