using KWEngine3.Framebuffers;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Sonnen-/Punktlichter und gerichtete Lichter (spot lights)
    /// </summary>
    public sealed class LightObject
    {
        internal LightObjectState _stateCurrent;
        internal LightObjectState _statePrevious;
        internal LightObjectState _stateRender;
        internal FramebufferShadowMap _fbShadowMap = null;
        internal int _shadowMapSize;
        internal float _shadowBias = 0.00006f;
        internal float _shadowOffset = 0f;

        /// <summary>
        /// Befinden sich das Lichtobjekt und seine Lichtstrahlen aktuell auf dem Bildschirm?
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;
        /// <summary>
        /// Engine-interne ID
        /// </summary>
        public int ID { get; internal set; } = 0;
        /// <summary>
        /// Name des Objekts
        /// </summary>
        public string Name { get; set; } = "(no name)";

        /// <summary>
        /// Schattenqualität des Lichts (maximal 3 Schattenlichter pro Welt möglich)
        /// </summary>
        public ShadowQuality ShadowCasterType { get; internal set; } = ShadowQuality.NoShadow;

        /// <summary>
        /// Standardkonstruktormethode für Lichtobjekte
        /// </summary>
        /// <param name="lightType">Art des Lichts (Sonne, Punktlicht oder gerichtetes Licht?)</param>
        /// <param name="shadowType">Schattenqualität des Lichts</param>
        public LightObject(LightType lightType, ShadowQuality shadowType = ShadowQuality.NoShadow)
        {
            if(shadowType != ShadowQuality.NoShadow && Framebuffer.ShadowMapCount >= KWEngine.MAX_SHADOWMAPS)
            {
                KWEngine.LogWriteLine("New LightObject instance cannot cast shadows!");
                KWEngine.LogWriteLine("Reason: > 3 shadow casters in world already.");
                shadowType = ShadowQuality.NoShadow;
            }

            ShadowCasterType = shadowType;
            _shadowMapSize = ShadowCasterType == ShadowQuality.Low ? 512
                : ShadowCasterType == ShadowQuality.Medium ? 1024
                : ShadowCasterType == ShadowQuality.High ? 2048
                : -1;

            if (lightType == LightType.Point)
                _shadowBias = 0.0006f;

            _stateCurrent = new LightObjectState(this, lightType);
            _statePrevious = _stateCurrent;
            _stateRender = new LightObjectState(this, lightType);
            UpdateLookAtVector();
        }

        /// <summary>
        /// Feintuning für den Schatteneffekt des Lichts (im Bereich [-0.1f;+0.1f])
        /// </summary>
        /// <param name="bias">Bias-Wert (Standardwert: 0.00006f)</param>
        public void SetShadowBias(float bias)
        {
            _shadowBias = MathHelper.Clamp(bias, -0.1f, 0.1f);
        }

        /// <summary>
        /// Art des Lichts
        /// </summary>
        public LightType Type
        {
            get
            {
                return _stateCurrent._nearFarFOVType.W == 0f ? LightType.Point : _stateCurrent._nearFarFOVType.W > 0f ? LightType.Directional : LightType.Sun;
            }
        }

        /// <summary>
        /// Setzt die Position des Lichtobjekts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Position des Lichtobjekts
        /// </summary>
        /// <param name="position">3D-Koordinate</param>
        public void SetPosition(Vector3 position)
        {
            _stateCurrent._position = position;
            UpdateLookAtVector();
        }

        /// <summary>
        /// Setzt das Ziel des Lichts (bzw. dadurch dessen Richtung)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetTarget(float x, float y, float z)
        {
            SetTarget(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt das Ziel des Lichts (bzw. dadurch dessen Richtung)
        /// </summary>
        /// <param name="target">3D-Koordinate</param>
        public void SetTarget(Vector3 target)
        {
            _stateCurrent._target = target;
            UpdateLookAtVector();
            
        }

        /// <summary>
        /// Setzt Lichtfarbe- und intensität
        /// </summary>
        /// <param name="r">Rotintensität (0 bis 1)</param>
        /// <param name="g">Grünintensität (0 bis 1)</param>
        /// <param name="b">Blauintensität (0 bis 1)</param>
        /// <param name="intensity">Helligkeit (0 bis 4096)</param>
        public void SetColor(float r, float g, float b, float intensity)
        {
            SetColor(new Vector4(r,g,b,intensity));
        }

        /// <summary>
        /// Setzt Lichtfarbe- und intensität
        /// </summary>
        /// <param name="c">Rot-/Grün-/Blau-Intensität (0 bis 1) plus Helligkeit (0 bis 4096)</param>
        public void SetColor(Vector4 c)
        {
            _stateCurrent._color.X = MathHelper.Clamp(c.X, 0f, 1f);
            _stateCurrent._color.Y = MathHelper.Clamp(c.Y, 0f, 1f);
            _stateCurrent._color.Z = MathHelper.Clamp(c.Z, 0f, 1f);
            _stateCurrent._color.W = MathHelper.Clamp(c.W, 0f, 4096f);

        }

        /// <summary>
        /// Gibt die aktuelle Lichtfarbe zurück
        /// </summary>
        public Vector4 Color
        {
            get
            {
                return _stateCurrent._color;
            }
        }

        /// <summary>
        /// Setzt den Blickwinkel des Lichts (gilt nur für Sonnenlicht und gerichtetes Licht)
        /// </summary>
        /// <param name="fov">Blickwinkel in Grad</param>
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

        /// <summary>
        /// Setzt die Nah- und Ferngrenze (Reichweite) des Lichts
        /// </summary>
        /// <param name="near">Nahgrenze (mind. 0.1f)</param>
        /// <param name="far">Ferngrenze (muss größer als die Nahgrenze sein)</param>
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

        /// <summary>
        /// Gibt die aktuelle Position des Lichtobjekts zurück
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _stateCurrent._position;
            }
        }

        /// <summary>
        /// Gibt die aktuelle Zielposition des Lichtobjekts zurück (nur für gerichtete Lichter gültig)
        /// </summary>
        public Vector3 Target
        {
            get
            {
                return _stateCurrent._target;
            }
        }
    }
}
