using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Helper
{
    internal struct KWTexture
    {
        public int ID;
        public TextureTarget Target;

        public KWTexture(int id, TextureTarget target)
        {
            ID = id;
            Target = target;
        }
    }
}
