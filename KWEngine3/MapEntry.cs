using OpenTK.Mathematics;

namespace KWEngine3
{
    /// <summary>
    /// Hilfsobjekt für das Platzieren eines Objekts auf dem optionalen Karten-Overlay
    /// </summary>
    public struct MapEntry
    {
        /// <summary>
        /// Position der Instanz
        /// </summary>
        public Vector2i Position { get; set; }
        /// <summary>
        /// Textur der Instanz (Standard: null)
        /// </summary>
        public string Texture { get; set; }
        /// <summary>
        /// Texturwiederholung in X-Richtung
        /// </summary>
        public float TextureRepeatX { get; set; }
        /// <summary>
        /// Texturwiederholung in Y-Richtung
        /// </summary>
        public float TextureRepeatY { get; set; }
        /// <summary>
        /// Größe der Instanz (Breite, Höhe)
        /// </summary>
        public Vector2i Scale { get; set; }
        /// <summary>
        /// Färbung der Instanz (RGB-Werte zwischen 0f und 1f)
        /// </summary>
        public Vector3 Color { get; set; }
        /// <summary>
        /// Sichtbarkeit (Werte zwischen 0f und 1f)
        /// </summary>
        public float Opacity { get; set; }
        /// <summary>
        /// Leuchtfarbe der Instanz (RGB-Werte zwischen 0f und 1f)
        /// </summary>
        public Vector3 ColorEmissive { get; set; }
        /// <summary>
        /// Leuchtintensität der Instanz (Wert muss zwischen 0f und 1f liegen)
        /// </summary>
        public float ColorEmissiveIntensity { get; set; }
        /// <summary>
        /// ZIndex des Objekts (muss zwischen -100f und +100f liegen)
        /// </summary>
        public float ZIndex { get; set; }

        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        public MapEntry()
        {
            Position = new Vector2i((int)KWEngine.Window.Center.X, (int)KWEngine.Window.Center.Y);
            Texture = null;
            TextureRepeatX = 1f;
            TextureRepeatY = 1f;
            Scale = new Vector2i(32, 32);
            Color = new Vector3(1f);
            Opacity = 1f;
            ColorEmissive = new Vector3(1f);
            ColorEmissiveIntensity = 0f;
            ZIndex = 0f;
        }
    }
}
