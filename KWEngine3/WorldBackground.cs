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
        internal Matrix3 _rotationReflection = Matrix3.Identity;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 Clip { get; set; } = Vector2.One;
        public float _brightnessMultiplier = 1f;
        public string _filename = "";
        public SkyboxType SkyBoxType { get; set; } = SkyboxType.CubeMap;

        public void SetSkybox(string filename, float rotation = 0f, SkyboxType type = SkyboxType.CubeMap)
        {
            filename = filename == null ? "" : filename.Trim();
            int texId;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename.ToLower()))
            {
                texId = KWEngine.CurrentWorld._customTextures[filename.ToLower()];
            }
            else
            {
                if (type == SkyboxType.CubeMap)
                    texId = HelperTexture.LoadTextureSkybox(filename, out _mipMapLevels);
                else
                    texId = HelperTexture.LoadTextureSkyboxEquirectangular(filename, out _mipMapLevels, out int w, out int h);
                if(texId > 0)
                    KWEngine.CurrentWorld._customTextures.Add(filename.ToLower(), texId);
            }
            if(texId > 0)
            {
                SkyBoxType = type;
                if (type == SkyboxType.CubeMap)
                    _rotation = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation));
                else
                {
                    _rotation = Matrix3.CreateRotationX(-(float)Math.PI / 2f) * Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation + 180));
                    _rotationReflection = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation - 135));
                }
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
            int texId;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename.ToLower()))
            {
                texId = KWEngine.CurrentWorld._customTextures[filename.ToLower()];
            }
            else
            {
                texId = HelperTexture.LoadTextureForBackgroundExternal(filename, out _mipMapLevels);
                if (texId > 0)
                    KWEngine.CurrentWorld._customTextures.Add(filename.ToLower(), texId);
            }
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
