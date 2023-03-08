using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3
{
    internal class WorldBackground
    {
        internal int _skyboxId = -1;
        internal int _standardId = -1;
        internal int _mipMapLevels = -1;

        public BackgroundType Type { get; set; } = BackgroundType.None;
        internal Matrix3 _rotation  = Matrix3.Identity;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 Clip { get; set; } = Vector2.One;
        public float _brightnessMultiplier = 1f;
        public string _filename = "";

        public void SetSkybox(string filename, float rotation = 0f)
        {
            filename = filename == null ? "" : filename.Trim();
            int texId = HelperTexture.LoadTextureSkybox(filename, out _mipMapLevels);
            if(texId > 0)
            {
                _rotation = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation));
                _skyboxId = texId;
                DeleteStandard();
                Type = BackgroundType.Skybox;
                _filename = filename;
            }
            else
            {
                _filename = "";
            }

        }

        public void SetBrightnessMultiplier(float m)
        {
            _brightnessMultiplier = MathHelper.Max(0f, m);
        }

        public void SetStandard(string filename)
        {
            filename = filename == null ? "" : filename.Trim();
            int texId = HelperTexture.LoadTextureForBackgroundExternal(filename, out _mipMapLevels);
            if(texId > 0)
            {
                _standardId = texId;
                DeleteSkybox();
                Type = BackgroundType.Standard;
                _filename = filename;
            }
            else
            {
                _filename = "";
            }
        }

        public void SetOffset(float x, float y)
        {
            Offset = new Vector2(x, y);
        }

        public void SetClip(float x, float y)
        {
            Clip = new Vector2(x, y);
        }

        public void SetRepeat(float x, float y)
        {
            Scale = new Vector2(x, y);
        }

        public void Unset()
        {
            DeleteSkybox();
            DeleteStandard();
            Type = BackgroundType.None;
            Scale = Vector2.One;
            Offset = Vector2.Zero;
            Clip = Vector2.One;
            _rotation = Matrix3.Identity;
        }

        internal void DeleteStandard()
        {
            if (_standardId > 0)
            { 
                GL.DeleteTextures(1, new int[] { _standardId });
                _standardId = -1;
            }
        }

        internal void DeleteSkybox()
        {
            if (_skyboxId > 0)
            {
                GL.DeleteTextures(1, new int[] { _skyboxId });
                _skyboxId = -1;
            }
        }
    }
}
