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

        internal WorldBackgroundState _stateCurrent = new WorldBackgroundState();
        internal WorldBackgroundState _statePrevious = new WorldBackgroundState();
        internal WorldBackgroundState _stateRender = new WorldBackgroundState();

        public BackgroundType Type { get; set; } = BackgroundType.None;
        internal Matrix3 _rotation  = Matrix3.Identity;
        internal Matrix3 _rotationReflection = Matrix3.Identity;
        public float _brightnessMultiplier = 1f;
        public string _filename = "";
        public SkyboxType SkyBoxType { get; set; } = SkyboxType.CubeMap;

        public void SetSkybox(string filename, float rotation = 0f, SkyboxType type = SkyboxType.CubeMap)
        {
            ResetScaleOffsetClip();
            filename = HelperGeneral.EqualizePathDividers(filename == null ? "" : filename.Trim());
            int texId;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
            {
                texId = KWEngine.CurrentWorld._customTextures[filename].ID;
            }
            else
            {
                if (type == SkyboxType.CubeMap)
                    texId = HelperTexture.LoadTextureSkybox(filename, out _mipMapLevels);
                else
                    texId = HelperTexture.LoadTextureSkyboxEquirectangular(filename, out _mipMapLevels, out int w, out int h);
                if(texId > 0)
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(texId, type == SkyboxType.CubeMap ? TextureTarget.TextureCubeMap : TextureTarget.Texture2D));
            }
            if(texId > 0)
            {
                SkyBoxType = type;
                if (type == SkyboxType.CubeMap)
                    _rotation = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation));
                else
                {
                    //_rotation = Matrix3.CreateRotationX(-(float)Math.PI / 2f) * Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation + 180));
                    _rotation = Matrix3.CreateRotationX(0) * Matrix3.CreateRotationY(MathHelper.DegreesToRadians(rotation + 180));
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
            ResetScaleOffsetClip();
            filename = HelperGeneral.EqualizePathDividers(filename == null ? "" : filename.Trim());
            int texId;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
            {
                texId = KWEngine.CurrentWorld._customTextures[filename].ID;
            }
            else
            {
                texId = HelperTexture.LoadTextureForBackgroundExternal(filename, out _mipMapLevels);
                if (texId > 0)
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(texId, TextureTarget.Texture2D));
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
            _stateCurrent.Offset = new Vector2(x, y);
        }

        public void SetClip(float x, float y)
        {
            _stateCurrent.Clip = new Vector2(x, y);
        }

        public void SetRepeat(float x, float y)
        {
            _stateCurrent.Scale = new Vector2(x, y);
        }

        public void Unset()
        {
            DeleteSkybox();
            DeleteStandard();
            Type = BackgroundType.None;
            ResetScaleOffsetClip();
            _rotation = Matrix3.Identity;
        }

        public void ResetScaleOffsetClip()
        {
            _stateCurrent.Scale = Vector2.One;
            _stateCurrent.Offset = Vector2.Zero;
            _stateCurrent.Clip = Vector2.One;
            _statePrevious.Scale = Vector2.One;
            _statePrevious.Offset = Vector2.Zero;
            _statePrevious.Clip = Vector2.One;
            _stateRender.Scale = Vector2.One;
            _stateRender.Offset = Vector2.Zero;
            _stateRender.Clip = Vector2.One;
        }

        internal void DeleteStandard()
        {
            if (_standardId > 0)
            {
                KWEngine.CurrentWorld._customTextures.Remove(_filename);
                GL.DeleteTextures(1, new int[] { _standardId });
                _standardId = -1;
            }
        }

        internal void DeleteSkybox()
        {
            if (_skyboxId > 0)
            {
                KWEngine.CurrentWorld._customTextures.Remove(_filename);
                GL.DeleteTextures(1, new int[] { _skyboxId });
                _skyboxId = -1;
            }
        }
    }
}
