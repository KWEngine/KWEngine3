using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Sonnen-/Punktlichter und gerichtete Lichter (spot lights)
    /// </summary>
    public abstract class LightObject
    {
        internal LightObjectState _stateCurrent;
        internal LightObjectState _statePrevious;
        internal LightObjectState _stateRender;
        internal FramebufferShadowMap _fbShadowMap = null;
        internal int _shadowMapSize;
        internal float _shadowBias = 0.00002f;
        internal float _shadowOffset = 0.0001f;
        internal float _lightVolume = 1.0f;
        internal float _lightVolumeBias = 0.5f;
        
        /// <summary>
        /// Engine-interne ID
        /// </summary>
        public ushort ID { get; internal set; } = 0;
        /// <summary>
        /// Name des Objekts
        /// </summary>
        public string Name { get; set; } = "(no name)";

        /// <summary>
        /// Setzt das Volumen des Lichts (0.0f bis 1.0f)
        /// </summary>
        /// <param name="v">Lichtvolumen</param>
        public void SetVolume(float v)
        {
            v = MathF.Max(MathF.Min(v, 1f), 0f);
            _lightVolume = v;
        }

        /// <summary>
        /// Gibt an, ob das Licht beim Rendern berücksichtigt werden soll oder nicht (Standardwert: true)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Regelt die Dichteverteilung des Volumens je Entfernung (Werte zwischen 0f und 1f)
        /// </summary>
        /// <param name="b">Dichteverteilung</param>
        public void SetVolumeBias(float b)
        {
            b = MathF.Max(MathF.Min(b, 1f), 0f);
            _lightVolumeBias = b;
        }

        /// <summary>
        /// Schattenqualität des Lichts (maximal 3 Schattenlichter pro Welt möglich)
        /// </summary>
        public ShadowQuality ShadowQualityLevel { get; internal set; } = ShadowQuality.NoShadow;

        /// <summary>
        /// Art der Schattenberechnung (Standard oder kaskadiert)
        /// </summary>
        public SunShadowType ShadowType { get; internal set; } = SunShadowType.Default;

        /// <summary>
        /// Init-Methode für Lichtobjekte
        /// </summary>
        /// <param name="lightType">Art des Lichts (Sonne, Punktlicht oder gerichtetes Licht?)</param>
        /// <param name="shadowQuality">Schattenqualität des Lichts</param>
        /// <param name="shadowType">Schattentyp</param>
        internal void Init(LightType lightType, ShadowQuality shadowQuality, SunShadowType shadowType)
        {
            if(shadowQuality != ShadowQuality.NoShadow && Framebuffer.ShadowMapCount >= KWEngine.MAX_SHADOWMAPS)
            {
                KWEngine.LogWriteLine("New LightObject instance cannot cast shadows!");
                KWEngine.LogWriteLine("\tReason: > 3 shadow casters in world already.");
                shadowQuality = ShadowQuality.NoShadow;
            }

            ShadowQualityLevel = shadowQuality;
            if (lightType == LightType.Point)
            {
                _shadowMapSize = (int)ShadowQualityLevel / 2;
            }
            else
            {
                _shadowMapSize = (int)ShadowQualityLevel;
            }

            if (lightType == LightType.Point && KWEngine.Window._renderQuality > RenderQualityLevel.Low)
                _shadowBias = 0.0002f;

            _stateCurrent = new LightObjectState(this, lightType);
            _statePrevious = _stateCurrent;
            _stateRender = new LightObjectState(this, lightType);
            UpdateLookAtVector();

            if (lightType == LightType.Sun)
            {
                _frustumShadowMap = new FrustumShadowMapOrthographic();
            }
            else if (lightType == LightType.Directional)
            {
                _frustumShadowMap = new FrustumShadowMapPerspective();
            }
            else
            {
                _frustumShadowMap = new FrustumShadowMapPerspectiveCube();
            }
            ShadowType = shadowType;
        }

        /// <summary>
        /// Feintuning für den Schatteneffekt des Lichts (im Bereich [-0.1f;+0.1f])
        /// </summary>
        /// <param name="bias">Bias-Wert (Standardwert: 0.00002f)</param>
        public void SetShadowBias(float bias)
        {
            _shadowBias = MathHelper.Clamp(bias, -0.1f, 0.1f);
        }

        /// <summary>
        /// Feintuning für die Position des Schatteneffekts (Standardwert: 0.0001f)
        /// </summary>
        /// <param name="offset">Erlaubte Werte zwischen 0.0f und 0.1f</param>
        public void SetShadowOffset(float offset)
        {
            _shadowOffset = MathHelper.Clamp(offset, 0f, 0.1f);
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
        /// Verweis auf die aktuelle Welt
        /// </summary>
        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }

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
        /// Bewegt die Instanz in die entsprechende Achsenrichtung
        /// </summary>
        /// <param name="x">Bewegung in x-Achse</param>
        /// <param name="y">Bewegung in y-Achse</param>
        /// <param name="z">Bewegung in z-Achse</param>
        public void MoveOffset(float x, float y, float z)
        {
            SetPosition(Position.X + x, Position.Y + y, Position.Z + z);
        }

        /// <summary>
        /// Bewegt die Zielposition der Instanz in die entsprechende Achsenrichtung
        /// </summary>
        /// <param name="x">Bewegung in x-Achse</param>
        /// <param name="y">Bewegung in y-Achse</param>
        /// <param name="z">Bewegung in z-Achse</param>
        public void MoveOffsetTarget(float x, float y, float z)
        {
            SetTarget(Target.X + x, Target.Y + y, Target.Z + z);
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
        /// <param name="near">Nahgrenze (mind. 0.01f)</param>
        /// <param name="far">Ferngrenze (muss größer als die Nahgrenze sein, Standardwert: 100)</param>
        public void SetNearFar(float near, float far)
        {
            near = Math.Max(Math.Abs(near), 0.01f);
            if(far - near < 1)
                far = near + 1;
            _stateCurrent._nearFarFOVType = new Vector4(near, far, _stateCurrent._nearFarFOVType.Z, _stateCurrent._nearFarFOVType.W);
        }

        internal void UpdateLookAtVector()
        {
            if (Type != LightType.Point)
            {
                _stateCurrent._lookAtVector = Vector3.NormalizeFast(_stateCurrent._target - _stateCurrent._position);
                CheckForIllegalAngles();
            }
        }

        internal Vector3 _ndcPosition;
        internal float _ndcRadius;
        internal bool _behindCamera;

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

        internal FrustumShadowMap _frustumShadowMap;

        internal void UpdateFrustum()
        {
            _frustumShadowMap.Update(this);
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
                if(KWEngine.Window._renderQuality == RenderQualityLevel.Low)
                {
                    _fbShadowMap = new FramebufferShadowMapLQ(_shadowMapSize, _shadowMapSize, Type, ShadowType);
                }
                else
                {
                    _fbShadowMap = new FramebufferShadowMap(_shadowMapSize, _shadowMapSize, Type, ShadowType);
                }
                
                Framebuffer.UpdateGlobalShadowMapCounter(true);
            }
        }

        internal void DeleteShadowMap()
        {
            if (_fbShadowMap != null)
            {
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
