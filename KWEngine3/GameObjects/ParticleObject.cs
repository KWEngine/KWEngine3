using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Partikelklasse
    /// </summary>
    public sealed class ParticleObject : TimeBasedObject
    {
        internal GeoModel _model = KWEngine.GetModel("KWQuad");
        internal Vector3 _position = new Vector3(0, 0, 0);
        /// <summary>
        /// Position des Partikels
        /// </summary>
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        private Vector3 _scale = new Vector3(1, 1, 1);
        private Vector3 _scaleCurrent = new Vector3(1, 1, 1);
        internal Matrix4 _rotation = Matrix4.Identity;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Vector4 _tint = new Vector4(1f, 1f, 1f, 1f);
        internal ParticleType _type = ParticleType.BurstFire1;
        internal float _starttime = -1;
        internal float _lastUpdate = -1;
        internal long _durationInMS = 5000;
        internal int _frame = 0;
        internal float _aliveInMS = 0;
        internal float _scaleFactor = 1;
        internal ParticleInfo _info;
        internal float _hue = 0;

        /// <summary>
        /// Setzt die Dauer der Loop-Partikel
        /// </summary>
        /// <param name="durationInSeconds">Dauer (in Sekunden)</param>
        public void SetDuration(float durationInSeconds)
        {
            if (_type == ParticleType.LoopSmoke1 || _type == ParticleType.LoopSmoke2 || _type == ParticleType.LoopSmoke3)
                _durationInMS = durationInSeconds > 0 ? (int)(durationInSeconds * 1000) : 5000;
            else
            {
                KWEngine.LogWriteLine("Cannot set duration on this ParticleObject.");
            }
        }

        /// <summary>
        /// Setzt die Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <param name="hue">Farbverschiebung (in Grad)</param>
        public void SetHue(float hue)
        {
            _hue = MathHelper.DegreesToRadians(hue % 360f);
        }

        /// <summary>
        /// Erfragt die aktuelle Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <returns>Aktuelle Farbverschiebung (in Grad)</returns>
        public float GetHue()
        {
            return MathHelper.RadiansToDegrees(_hue);
        }

        /// <summary>
        /// Setzt die Positon
        /// </summary>
        /// <param name="pos">Positionsdaten</param>
        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        /// <summary>
        /// Setzt die Position
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Partikelfärbung
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        /// <param name="alpha">Transparenz (0 bis 1)</param>
        public void SetColor(float red, float green, float blue, float alpha)
        {
            _tint.X = HelperGeneral.Clamp(red, 0, 1);
            _tint.Y = HelperGeneral.Clamp(green, 0, 1);
            _tint.Z = HelperGeneral.Clamp(blue, 0, 1);
            _tint.W = HelperGeneral.Clamp(alpha, 0, 1);
        }

        /// <summary>
        /// Erfragt die aktuelle Färbung des Partikelobjekts
        /// </summary>
        public Vector4 Color
        {
            get { return _tint; }
        }


        /// <summary>
        /// Konstruktormethode für Partikel
        /// </summary>
        /// <param name="scale">Skalierung</param>
        /// <param name="type">Art der Partikelanimation</param>
        public ParticleObject(float scale, ParticleType type)
        {
            _scale.X = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);
            _scale.Y = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);
            _scale.Z = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);
            _scaleCurrent.X = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);
            _scaleCurrent.Y = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);
            _scaleCurrent.Z = HelperGeneral.Clamp(scale, 0.0001f, float.MaxValue);

            _type = type;

            _info = KWEngine.ParticleDictionary[_type];
            _modelMatrix = Matrix4.CreateScale(_scaleCurrent) * _rotation * Matrix4.CreateTranslation(Position);
        }

        internal Matrix4 GetViewMatrixTowardsCamera()
        {
            Matrix3 currentViewMatrix = new Matrix3(KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewMatrix);
            currentViewMatrix.Invert();
            return new Matrix4(currentViewMatrix);
        }

        internal override void Act()
        {
            float now = KWEngine.WorldTime;      
            float diff = _lastUpdate < 0 ? 0 : now - _lastUpdate;
            _aliveInMS += diff * 1000;
            _frame = (int)(_aliveInMS / 32);
            int frameloop = _frame % _info.Samples;

            if (_type == ParticleType.LoopSmoke1 || _type == ParticleType.LoopSmoke2 || _type == ParticleType.LoopSmoke3)
            {
                _frame = frameloop;
                // f(x) = -64000(x - 0.5)¹⁶ + 1
                _scaleFactor = -64000f * (float)Math.Pow(_aliveInMS / _durationInMS - 0.5f, 16) + 1;
                _scaleCurrent.X = _scale.X * _scaleFactor;
                _scaleCurrent.Y = _scale.Y * _scaleFactor;
                _scaleCurrent.Z = _scale.Z * _scaleFactor;

                if (_aliveInMS > _durationInMS)
                {
                    Finished = true;
                }
            }
            else
            {
                if (_frame > _info.Samples - 1)
                {
                    Finished = true;
                }
            }

            _modelMatrix = GetViewMatrixTowardsCamera();
            _modelMatrix.Row0 *= _scaleCurrent.X;
            _modelMatrix.Row1 *= _scaleCurrent.Y;
            _modelMatrix.Row2 *= _scaleCurrent.Z;
            _modelMatrix.M41 = Position.X;
            _modelMatrix.M42 = Position.Y;
            _modelMatrix.M43 = Position.Z;
            _modelMatrix.M44 = 1;
            _lastUpdate = now;
        }
    }
}
