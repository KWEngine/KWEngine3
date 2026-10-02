using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Quaderförmiger Bereich, in dem Nebel liegt (Nebel-Box)
    /// </summary>
    /// <remarks>Die Box wird mit World.AddFogVolume() hinzugefügt. Ihr Nebel wird zum globalen Nebel der Welt addiert und nutzt dessen Schwaden (SetFogNoise), wabernde Oberkante (SetFogHeightNoise) und Wind (SetFogWind).</remarks>
    public class FogVolume
    {
        internal const int UBO_FLOATS = 24;

        internal Vector3 _position = Vector3.Zero;
        internal Vector3 _scale = Vector3.One;
        internal Quaternion _rotation = Quaternion.Identity;
        internal Vector3 _color = new(0.75f, 0.8f, 0.85f);
        internal float _density = 0.5f;
        internal float _heightFalloff = 0f;
        internal float _edgeSoftness = 0f;

        // packed shader data (see struct FogVolume in fog.glsl), rebuilt only when the box changes
        internal readonly float[] _uboData = new float[UBO_FLOATS];
        internal Vector3 _aabbCenter = Vector3.Zero;
        internal Vector3 _aabbHalfExtent = new(0.5f);

        /// <summary>
        /// Name der Nebel-Box
        /// </summary>
        public string Name { get; set; } = "FogVolume";

        /// <summary>
        /// Erzeugt eine Nebel-Box (Größe 1 x 1 x 1 am Weltursprung)
        /// </summary>
        public FogVolume()
        {
            UpdateData();
        }

        /// <summary>
        /// Mittelpunkt der Box
        /// </summary>
        public Vector3 Position { get { return _position; } }

        /// <summary>
        /// Größe der Box (Breite, Höhe, Tiefe)
        /// </summary>
        public Vector3 Scale { get { return _scale; } }

        /// <summary>
        /// Rotation der Box
        /// </summary>
        public Quaternion Rotation { get { return _rotation; } }

        /// <summary>
        /// Nebelfarbe der Box
        /// </summary>
        public Vector3 Color { get { return _color; } }

        /// <summary>
        /// Nebeldichte der Box (0 = kein Nebel)
        /// </summary>
        public float Density { get { return _density; } }

        /// <summary>
        /// Wie schnell der Nebel vom Boden der Box nach oben hin dünner wird (0 = überall gleich dicht)
        /// </summary>
        public float HeightFalloff { get { return _heightFalloff; } }

        /// <summary>
        /// Breite des weichen Übergangs an den Rändern der Box (in Welteinheiten, 0 = harte Kante)
        /// </summary>
        public float EdgeSoftness { get { return _edgeSoftness; } }

        /// <summary>
        /// Setzt den Mittelpunkt der Box
        /// </summary>
        /// <param name="x">X-Koordinate</param>
        /// <param name="y">Y-Koordinate</param>
        /// <param name="z">Z-Koordinate</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt den Mittelpunkt der Box
        /// </summary>
        /// <param name="position">Position</param>
        public void SetPosition(Vector3 position)
        {
            _position = position;
            UpdateData();
        }

        /// <summary>
        /// Setzt die Größe der Box
        /// </summary>
        /// <param name="width">Breite (X)</param>
        /// <param name="height">Höhe (Y)</param>
        /// <param name="depth">Tiefe (Z)</param>
        public void SetScale(float width, float height, float depth)
        {
            SetScale(new Vector3(width, height, depth));
        }

        /// <summary>
        /// Setzt die Größe der Box (Breite, Höhe, Tiefe)
        /// </summary>
        /// <param name="scale">Größe</param>
        public void SetScale(Vector3 scale)
        {
            _scale = new Vector3(Math.Max(Math.Abs(scale.X), 0.001f), Math.Max(Math.Abs(scale.Y), 0.001f), Math.Max(Math.Abs(scale.Z), 0.001f));
            UpdateData();
        }

        /// <summary>
        /// Setzt die Größe der Box gleichmäßig in alle Richtungen
        /// </summary>
        /// <param name="scale">Kantenlänge</param>
        public void SetScale(float scale)
        {
            SetScale(new Vector3(scale));
        }

        /// <summary>
        /// Setzt die Rotation der Box
        /// </summary>
        /// <param name="x">Rotation um die X-Achse (in Grad)</param>
        /// <param name="y">Rotation um die Y-Achse (in Grad)</param>
        /// <param name="z">Rotation um die Z-Achse (in Grad)</param>
        public void SetRotation(float x, float y, float z)
        {
            SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z)));
        }

        /// <summary>
        /// Setzt die Rotation der Box
        /// </summary>
        /// <param name="rotation">Rotation</param>
        public void SetRotation(Quaternion rotation)
        {
            _rotation = Quaternion.Normalize(rotation);
            UpdateData();
        }

        /// <summary>
        /// Setzt die Nebelfarbe der Box
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 1)</param>
        /// <param name="g">Grünanteil (0 bis 1)</param>
        /// <param name="b">Blauanteil (0 bis 1)</param>
        public void SetColor(float r, float g, float b)
        {
            SetColor(new Vector3(r, g, b));
        }

        /// <summary>
        /// Setzt die Nebelfarbe der Box
        /// </summary>
        /// <param name="color">Rot-/Grün-/Blauanteil (jeweils 0 bis 1)</param>
        public void SetColor(Vector3 color)
        {
            _color = new Vector3(MathHelper.Clamp(color.X, 0f, 1f), MathHelper.Clamp(color.Y, 0f, 1f), MathHelper.Clamp(color.Z, 0f, 1f));
            UpdateData();
        }

        /// <summary>
        /// Setzt die Nebeldichte der Box (0 = kein Nebel)
        /// </summary>
        /// <remarks>Faustregel wie beim globalen Nebel: Nach etwa 3 / Dichte Welteinheiten ist die Sicht zu 95% vernebelt.</remarks>
        /// <param name="density">Nebeldichte (0 bis 1)</param>
        public void SetDensity(float density)
        {
            _density = MathHelper.Clamp(density, 0f, 1f);
            UpdateData();
        }

        /// <summary>
        /// Legt fest, dass der Nebel vom Boden der Box nach oben hin dünner wird
        /// </summary>
        /// <param name="falloff">Wie schnell der Nebel nach oben dünner wird (0 = überall gleich dicht, 1 = deutlich, 3 = flache Bodenschicht)</param>
        public void SetHeightFalloff(float falloff)
        {
            _heightFalloff = MathHelper.Clamp(falloff, 0f, 10f);
            UpdateData();
        }

        /// <summary>
        /// Legt fest, wie weich der Nebel an den Rändern der Box ausläuft
        /// </summary>
        /// <param name="softness">Breite des Übergangs in Welteinheiten (0 = harte Kante)</param>
        public void SetEdgeSoftness(float softness)
        {
            _edgeSoftness = Math.Max(0f, softness);
            UpdateData();
        }

        private void UpdateData()
        {
            Matrix4 model = HelperMatrix.CreateModelMatrix(ref _scale, ref _rotation, ref _position);
            Matrix4 inverse = Matrix4.Invert(model);

            // world -> local unit box (-0.5..0.5)
            WriteVector(0, inverse.Column0);
            WriteVector(4, inverse.Column1);
            WriteVector(8, inverse.Column2);
            WriteVector(12, new Vector4(_color, _density));

            Vector3 min = new(float.MaxValue);
            Vector3 max = new(float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new((i & 1) == 0 ? -0.5f : 0.5f, (i & 2) == 0 ? -0.5f : 0.5f, (i & 4) == 0 ? -0.5f : 0.5f);
                Vector3 worldCorner = Vector3.TransformPosition(corner, model);
                min = Vector3.ComponentMin(min, worldCorner);
                max = Vector3.ComponentMax(max, worldCorner);
            }
            _aabbCenter = (min + max) * 0.5f;
            _aabbHalfExtent = (max - min) * 0.5f;

            WriteVector(16, new Vector4(min.Y, _heightFalloff, 0f, 0f));
            WriteVector(20, new Vector4(
                Math.Max(0.5f - _edgeSoftness / _scale.X, 0f),
                Math.Max(0.5f - _edgeSoftness / _scale.Y, 0f),
                Math.Max(0.5f - _edgeSoftness / _scale.Z, 0f),
                0f));
        }

        private void WriteVector(int offset, Vector4 v)
        {
            _uboData[offset + 0] = v.X;
            _uboData[offset + 1] = v.Y;
            _uboData[offset + 2] = v.Z;
            _uboData[offset + 3] = v.W;
        }
    }
}
