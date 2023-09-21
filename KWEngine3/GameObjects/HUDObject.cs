using OpenTK.Mathematics;
using KWEngine3.Helper;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// HUD-Oberklasse
    /// </summary>
    public abstract class HUDObject
    {
        /// <summary>
        /// Gibt an, ob das HUDObject sichtbar ist oder nicht (Standard: true)
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Verweis auf die aktuelle Welt
        /// </summary>
        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }

        /// <summary>
        /// Position des Objekts auf dem Bildschirm
        /// (die Koordinaten (0;0) beschreiben die linke obere Ecke)
        /// </summary>
        public Vector2 Position { get; internal set; } = Vector2.Zero;

        /// <summary>
        /// Name der Instanz
        /// </summary>
        public string Name { get; set; } = "undefined HUD object.";

        /// <summary>
        /// Setzt die Position
        /// </summary>
        /// <param name="x">Breite in Pixeln</param>
        /// <param name="y">Höhe in Pixeln</param>
        public void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);
            UpdateMVP();
        }

        /// <summary>
        /// Setzt die Position
        /// </summary>
        /// <param name="p">Höhe in Pixeln</param>
        public void SetPosition(Vector2 p)
        {
            SetPosition(p.X, p.Y);
        }

        /// <summary>
        /// Zentriert das Objekt im Fenster
        /// </summary>
        public void CenterOnScreen()
        {
            SetPosition(KWEngine.Window.ClientRectangle.HalfSize.X, KWEngine.Window.ClientRectangle.HalfSize.Y);
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger auf dem HUD-Objekt ist
        /// </summary>
        /// <returns>true, wenn die Maus auf dem HUD-Objekt ist</returns>
        public abstract bool IsMouseCursorOnMe();

        /// <summary>
        /// Aktuelle Färbung des Objekts
        /// </summary>
        public Vector3 Color { get { return _tint.Xyz; } }

        /// <summary>
        /// Aktuelle Sichtbarkeit des Objekts
        /// </summary>
        public float Opacity { get { return _tint.W; } }

        /// <summary>
        /// AKtuelle Glühfarbe des Objekts
        /// </summary>
        public Vector3 GlowColor { get { return _glow.Xyz; } }

        /// <summary>
        /// Aktuelle Glühintensität des Objekts
        /// </summary>
        public float GlowIntensity { get { return _glow.W; } }

        /// <summary>
        /// Aktuelle Größe des Objekts (bei Text gilt die Größe je Zeichen)
        /// </summary>
        public Vector2 Scale { get { return _scale.Xy; } }

        /// <summary>
        /// Färbung des Objekts
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        public void SetColor(float red, float green, float blue)
        {
            _tint.X = HelperGeneral.Clamp(red, 0, 1);
            _tint.Y = HelperGeneral.Clamp(green, 0, 1);
            _tint.Z = HelperGeneral.Clamp(blue, 0, 1);
            _tint.W = 1.0f;
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Objekts
        /// </summary>
        /// <param name="o">Sichtbarkeit (zwischen 0 und 1)</param>
        public void SetOpacity(float o)
        {
            _tint.W = HelperGeneral.Clamp(o, 0, 1);
        }

        /// <summary>
        /// Setzt die Glühfarbe des Objekts
        /// </summary>
        /// <param name="red">Rot (zwischen 0 und 1)</param>
        /// <param name="green">Grün (zwischen 0 und 1)</param>
        /// <param name="blue">Blau (zwischen 0 und 1)</param>

        public void SetColorGlow(float red, float green, float blue)
        {
            _glow.X = HelperGeneral.Clamp(red, 0, 1);
            _glow.Y = HelperGeneral.Clamp(green, 0, 1);
            _glow.Z = HelperGeneral.Clamp(blue, 0, 1);
        }

        /// <summary>
        /// Setzt die Glühintensität des Objekts
        /// </summary>
        /// <param name="intensity">Intensität (zwischen 0 und 2)</param>
        public void SetColorGlowIntensity(float intensity)
        {
            _glow.W = HelperGeneral.Clamp(intensity, 0, 2);
        }

        /// <summary>
        /// Setzt die Größe (bei Text gilt die Größe je Zeichen)
        /// </summary>
        /// <param name="width">Breite (gültige Werte zwischen 0.001 und 2048)</param>
        /// <param name="height">Höhe (gültige Werte zwischen 0.001 und 2048)</param>
        public void SetScale(float width, float height)
        {
            _scale.X = HelperGeneral.Clamp(width, 0.001f, 2048f);
            _scale.Y = HelperGeneral.Clamp(height, 0.001f, 2048f);
            _scale.Z = 1;
            UpdateMVP();
        }

        /// <summary>
        /// Setzt die Größe (Breite und Höhe gleichermaßen)
        /// </summary>
        /// <param name="scale">Größe der Buchstaben bzw. des Bilds (in Pixeln)</param>
        public void SetScale(float scale)
        {
            SetScale(scale, scale);
        }

        #region Internals
        internal Vector4 _tint = new Vector4(1, 1, 1, 1);
        internal Vector4 _glow = new Vector4(0, 0, 0, 1);
        internal Vector3 _scale = new Vector3(24f, 24f, 1f);
        internal Matrix4 _modelMatrix = Matrix4.Identity;

        internal void UpdateMVP()
        {
            Vector3 p = new Vector3(Position.X, Position.Y, 0);
            _modelMatrix = HelperMatrix.CreateModelMatrixForHUD(ref _scale, ref p);
        }
        #endregion

    }
}
