using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal struct GeoTexture
    {
        public string Filename;
        public int OpenGLID;
        public int UVMapIndex;
        public Vector4 UVTransform;
        public TextureType Type;
        public int Width;
        public int Height;
        public int MipMaps;

        public bool IsTextureSet { get { return OpenGLID > 0; } }

        public GeoTexture()
        {
            Filename = "";
            OpenGLID = -1;
            UVMapIndex = 0;
            UVTransform = new Vector4(1, 1, 0, 0);
            Type = TextureType.Albedo;
            Width = 0;
            Height = 0;
            MipMaps = 0;
        }
    }
}
