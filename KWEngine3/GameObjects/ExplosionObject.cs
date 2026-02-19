using System;
using OpenTK.Mathematics;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Explosionsklasse
    /// </summary>
    public sealed class ExplosionObject : TimeBasedObject
    {
        /// <summary>
        /// Glühfarbe der Explosion
        /// </summary>
        public Vector3 ColorEmissive
        {
            get
            {
                return _colorEmissive;
            }
            internal set
            {
                _colorEmissive.X = HelperGeneral.Clamp(value.X, 0f, 1f);
                _colorEmissive.Y = HelperGeneral.Clamp(value.Y, 0f, 1f);
                _colorEmissive.Z = HelperGeneral.Clamp(value.Z, 0f, 1f);
            }
        }

        /// <summary>
        /// Basisfarbe der Explosion (wird mit der Umgebungshelligkeit vermischt)
        /// </summary>
        public Vector3 Color
        {
            get
            {
                return _color;
            }
            internal set
            {
                _color.X = HelperGeneral.Clamp(value.X, 0f, 1f);
                _color.Y = HelperGeneral.Clamp(value.Y, 0f, 1f);
                _color.Z = HelperGeneral.Clamp(value.Z, 0f, 1f);
            }
        }

        /// <summary>
        /// Setzt die Glühfarbe der Explosionspartikel
        /// </summary>
        /// <param name="red">Rot (0 bis 1)</param>
        /// <param name="green">Grün (0 bis 1)</param>
        /// <param name="blue">Blau (0 bis 1)</param>
        /// <param name="intensity">Helligkeit (0 bis 2)</param>
        [Obsolete("Seit KWEngine 3.0.6 wird die Farb- und Leuchtstärke anders berechnet. Bitte verwenden Sie die SetColorEmissive(float, float, float)")]
        public void SetColorEmissive(float red, float green, float blue, float intensity)
        {
            ColorEmissive = new Vector3(red * intensity, green * intensity, blue * intensity);
        }

        /// <summary>
        /// Setzt die Glühfarbe der Explosionspartikel
        /// </summary>
        /// <param name="red">Rot (0 bis 2)</param>
        /// <param name="green">Grün (0 bis 2)</param>
        /// <param name="blue">Blau (0 bis 2)</param>
        public void SetColorEmissive(float red, float green, float blue)
        {
            ColorEmissive = new Vector3(red, green, blue);
        }

        /// <summary>
        /// Setzt die Basisfarbe der Explosionspartikel (wird mit der Umgebungshelligkeit vermischt)
        /// </summary>
        /// <param name="red">Rot (0 bis 1)</param>
        /// <param name="green">Grün (0 bis 1)</param>
        /// <param name="blue">Blau (0 bis 1)</param>
        public void SetColor(float red, float green, float blue)
        {
            ColorEmissive = new Vector3(red, green, blue);
        }


        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="position">Position</param>
        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        /// <summary>
        /// Setzt den Explosionsradius
        /// </summary>
        /// <param name="radius">Radius</param>
        public void SetRadius(float radius)
        {
            _spread = radius > 0 ? radius : 10;
        }

        /// <summary>
        /// Setzt die Partikelgröße
        /// </summary>
        /// <param name="size">Größe je Partikel</param>
        public void SetParticleSize(float size)
        {
            _particleSize = size > 0 ? size : 1f;
        }

        /// <summary>
        /// Setzt den Bewegungsalgorithmus der Partikel
        /// </summary>
        /// <param name="e">Algorithmustyp</param>
        public void SetAlgorithm(ExplosionAnimation e)
        {
            _algorithm = e;
        }

        /// <summary>
        /// Setzt die Rauheit der Oberfläche von Explosionspartikeln
        /// </summary>
        /// <param name="r">Rauheitswert (0f bis 1f, Standardwert: 1f)</param>
        public void SetRoughness(float r)
        {
            _roughness = Math.Clamp(r, 0f, 1f);
        }

        /// <summary>
        /// Setzt fest, wie metallisch die Oberfläche von Explosionspartikeln ist
        /// </summary>
        /// <param name="m">Metallwert (0f bis 1f, Standardwert: 0f)</param>
        public void SetMetallic(float m)
        {
            _metallic = Math.Clamp(m, 0f, 1f);
        }

        /// <summary>
        /// Explosionskonstruktormethode
        /// </summary>
        /// <param name="particleCount">Anzahl der Partikel (max: 512)</param>
        /// <param name="particleSize">Größe der Partikel</param>
        /// <param name="explosionRadius">Radius der Explosion</param>
        /// <param name="durationInSeconds">Dauer der Explosion in Sekunden</param>
        /// <param name="type">Art der Explosion</param>
        public ExplosionObject(int particleCount, float particleSize, float explosionRadius, float durationInSeconds, ExplosionType type)
        {
            if (type == ExplosionType.Cube || type == ExplosionType.CubeRingY || type == ExplosionType.CubeRingZ)
            {
                _model = KWEngine.GetModel("KWCube");
            }
            else if (type == ExplosionType.Sphere || type == ExplosionType.SphereRingY || type == ExplosionType.SphereRingZ)
            {
                _model = KWEngine.GetModel("KWSphere");
            }
            else if (type == ExplosionType.Star || type == ExplosionType.StarRingY || type == ExplosionType.StarRingZ)
            {
                _model = KWEngine.KWStar;
            }
            else if (type == ExplosionType.Heart || type == ExplosionType.HeartRingY || type == ExplosionType.HeartRingZ)
            {
                _model = (KWEngine.KWHeart);
            }
            else if (type == ExplosionType.Skull || type == ExplosionType.SkullRingY || type == ExplosionType.SkullRingZ)
            {
                _model = (KWEngine.KWSkull);
            }
            else
            {
                _model = (KWEngine.KWDollar);
            }

            _type = type;
            _amount = particleCount >= 4 && particleCount <= MAX_PARTICLES ? particleCount : particleCount < 4 ? 4 : MAX_PARTICLES;
            _spread = explosionRadius > 0 ? explosionRadius : 10;
            _duration = durationInSeconds > 0 ? durationInSeconds : 2;
            _particleSize = particleSize > 0 ? particleSize : 1f;

            for (int i = 0, arrayindex = 0; i < _amount; i++, arrayindex += 2)
            {

                if ((int)type < 100)
                {
                    int randomIndex = HelperRandom.GetRandomNumber(0, AxesCount - 1);
                    _directions[arrayindex] = randomIndex;
                    _directions[arrayindex + 1] = HelperRandom.GetRandomNumber(0.1f, 1.0f);
                }
                else if ((int)type >= 100 && (int)type < 1000)
                {
                    _directions[arrayindex] = 1;
                    _directions[arrayindex + 1] = HelperRandom.GetRandomNumber(0.1f, 1.0f);

                }
                else
                {
                    _directions[arrayindex] = 2;
                    _directions[arrayindex + 1] = HelperRandom.GetRandomNumber(0.1f, 1.0f);
                }
            }
        }


        #region Internals
        internal int _amount = 32;
        internal float _spread = 10;
        internal float _duration = 2;
        internal float _starttime = -1;
        internal float _secondsAlive = 0;
        internal float _particleSize = 0.5f;
        internal float[] _directions = new float[MAX_PARTICLES * 2];
        internal ExplosionAnimation _algorithm = 0;
        internal ExplosionType _type = ExplosionType.Cube;
        internal static readonly Vector3[] Axes = new Vector3[] {
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitX,
            -Vector3.UnitY,
            -Vector3.UnitZ,

            new Vector3(0.707107f,0.707107f,0),   // right       up
            new Vector3(0.577351f,0.577351f,0.577351f),   // right front up
            new Vector3(0,0.707107f,0.707107f),   // front       up
            new Vector3(-0.577351f,0.577351f,0.577351f),  // left front  up
            new Vector3(-0.707107f,0.707107f,0),  // left        up
            new Vector3(-0.577351f,0.577351f,-0.577351f), // left back   up
            new Vector3(0,0.707107f,-0.707107f),  // back        up
            new Vector3(0.577351f,0.577351f,-0.577351f),  // right back  up

            new Vector3(0.707107f,-0.707107f,0),   // right       down
            new Vector3(0.707107f,-0.707107f,0.707107f),   // right front down
            new Vector3(0,-0.707107f,0.707107f),   // front       down
            new Vector3(-0.577351f,-0.577351f,0.577351f),  // left front  down
            new Vector3(-0.707107f,-0.707107f,0),  // left        down
            new Vector3(-0.577351f,-0.577351f,-0.577351f), // left back   down
            new Vector3(0,-0.707107f,-0.707107f),  // back        down
            new Vector3(0.577351f,-0.577351f,-0.577351f),  // right back  down
        };
        internal static int AxesCount = Axes.Length;
        internal const int MAX_PARTICLES = 512;

        internal GeoModel _model;
        internal Vector3 _colorEmissive = Vector3.Zero;
        internal Vector3 _color = Vector3.One;
        internal float _roughness = 1f;
        internal float _metallic = 0f;

        internal override void Act()
        {
            if(_starttime >= 0)
            {
                float currentTime = KWEngine.WorldTime;
                _secondsAlive = currentTime - _starttime;
                if(_secondsAlive > _duration)
                {
                    Finished = true;
                }
            }
        }
        #endregion
    }
}
