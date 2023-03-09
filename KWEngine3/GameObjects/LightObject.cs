using KWEngine3.Framebuffers;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    public sealed class LightObject
    {
        internal LightObjectState _stateCurrent;
        internal LightObjectState _statePrevious;
        internal LightObjectState _stateRender;
        internal FramebufferShadowMap _fbShadowMap = null;
        internal int _shadowMapSize;
        internal float _shadowBias = 0.00006f;
        internal float _shadowOffset = 0f;

        public bool IsInsideScreenSpace { get; internal set; } = true;
        public int ID { get; internal set; } = 0;
        public string Name { get; set; } = "(no name)";

        public ShadowQuality ShadowCasterType { get; internal set; } = ShadowQuality.NoShadow;

        public LightObject(LightType type, ShadowQuality casterType = ShadowQuality.NoShadow)
        {
            if(casterType != ShadowQuality.NoShadow && Framebuffer.ShadowMapCount >= KWEngine.MAX_SHADOWMAPS)
            {
                KWEngine.LogWriteLine("New LightObject instance cannot cast shadows!");
                KWEngine.LogWriteLine("Reason: > 3 shadow casters in world already.");
                casterType = ShadowQuality.NoShadow;
            }

            ShadowCasterType = casterType;
            _shadowMapSize = ShadowCasterType == ShadowQuality.Low ? 512
                : ShadowCasterType == ShadowQuality.Medium ? 1024
                : ShadowCasterType == ShadowQuality.High ? 2048
                : -1;

            if (type == LightType.Point)
                _shadowBias = 0.0006f;

            _stateCurrent = new LightObjectState(this, type);
            _statePrevious = _stateCurrent;
            _stateRender = new LightObjectState(this, type);
            UpdateLookAtVector();
        }

        public void SetShadowBias(float bias)
        {
            _shadowBias = MathHelper.Clamp(bias, -0.1f, 0.1f);
        }

        public LightType Type
        {
            get
            {
                return _stateCurrent._nearFarFOVType.W == 0f ? LightType.Point : _stateCurrent._nearFarFOVType.W > 0f ? LightType.Directional : LightType.Sun;
            }
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetPosition(Vector3 position)
        {
            _stateCurrent._position = position;
            UpdateLookAtVector();
        }

        public void SetTarget(float x, float y, float z)
        {
            SetTarget(new Vector3(x, y, z));
        }

        public void SetTarget(Vector3 target)
        {
            _stateCurrent._target = target;
            UpdateLookAtVector();
            
        }

        public void SetColor(float r, float g, float b, float intensity)
        {
            SetColor(new Vector4(r,g,b,intensity));
        }

        public void SetColor(Vector4 c)
        {
            _stateCurrent._color.X = MathHelper.Clamp(c.X, 0f, 1f);
            _stateCurrent._color.Y = MathHelper.Clamp(c.Y, 0f, 1f);
            _stateCurrent._color.Z = MathHelper.Clamp(c.Z, 0f, 1f);
            _stateCurrent._color.W = MathHelper.Clamp(c.W, 0f, 4096f);

        }

        public Vector4 Color
        {
            get
            {
                return _stateCurrent._color;
            }
        }

        public void SetFOV(float fov)
        {
            //LightType.Point ? 0 : l._type == LightType.Sun ? -1 : 1
            if (_stateCurrent._nearFarFOVType.W > 0f) // directional
            {
                fov = Math.Min(Math.Max(fov, 10f), 179);
                _stateCurrent._nearFarFOVType = new Vector4(_stateCurrent._nearFarFOVType.X, _stateCurrent._nearFarFOVType.Y, fov, _stateCurrent._nearFarFOVType.W);
            }
            else if (_stateCurrent._nearFarFOVType.W < 0f) // sun
            {
                fov = Math.Min(Math.Max(fov, 10f), _shadowMapSize);
                _stateCurrent._nearFarFOVType = new Vector4(_stateCurrent._nearFarFOVType.X, _stateCurrent._nearFarFOVType.Y, fov, _stateCurrent._nearFarFOVType.W);
            }
            else
            {
                // ignore because point lights all have 90° for shadow mapping
            }
        }

        public void SetNearFar(float near, float far)
        {
            near = Math.Max(Math.Abs(near), 0.1f);
            if(far - near < 1)
                far = near + 1;
            _stateCurrent._nearFarFOVType = new Vector4(near, far, _stateCurrent._nearFarFOVType.Z, _stateCurrent._nearFarFOVType.W);
        }

        internal void UpdateLookAtVector()
        {
            _stateCurrent._lookAtVector = Vector3.NormalizeFast(_stateCurrent._target - _stateCurrent._position);
            CheckForIllegalAngles();
        }

        internal void GetVolume(out Vector3 center, out Vector3 dimensions)
        {
            if(Type == LightType.Sun)
            {
                center = Vector3.Zero;
                dimensions.X = float.MaxValue * 0.5f;
                dimensions.Y = float.MaxValue * 0.5f;
                dimensions.Z = float.MaxValue * 0.5f;
            }
            else if(Type == LightType.Point)
            {
                center = _stateCurrent._position;
                dimensions = new Vector3(_stateCurrent._nearFarFOVType.Y);
            }
            else
            {
                center = _stateCurrent._position + _stateCurrent._lookAtVector * _stateCurrent._nearFarFOVType.Y / 2;
                dimensions = new Vector3(_stateCurrent._nearFarFOVType.Y);
            }
        }

        internal void CheckForIllegalAngles()
        {
            float dot = Vector3.Dot(_stateCurrent._lookAtVector, KWEngine.WorldUp);
            if (dot < -0.9995f)
            {
                SetTarget(_stateCurrent._target + new Vector3(0, 0, 0.001f));
            }
            else if ((_stateCurrent._target - _stateCurrent._position).Length == 0)
            {
                SetTarget(_stateCurrent._target + new Vector3(0, -0.000001f, 0.0001f));
            }
        }

        internal void AttachShadowMap()
        {
            if (_fbShadowMap == null)
            {
                _fbShadowMap = new FramebufferShadowMap(_shadowMapSize, _shadowMapSize, Type);
                Framebuffer.UpdateGlobalShadowMapCounter(true);
            }
        }

        internal void DeleteShadowMap()
        {
            if (_fbShadowMap != null)
            {
                if (_fbShadowMap._blurBuffer1 != null)
                {
                    _fbShadowMap._blurBuffer1.Dispose();
                }
                if (_fbShadowMap._blurBuffer2 != null)
                {
                    _fbShadowMap._blurBuffer2.Dispose();
                }

                _fbShadowMap.Dispose();
                _fbShadowMap = null;
                Framebuffer.UpdateGlobalShadowMapCounter(false);
            }
        }
    }
}
