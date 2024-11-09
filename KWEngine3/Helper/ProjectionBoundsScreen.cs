using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Gibt die aktuellen Grenzen einer AABB-Hitbox für die aktuelle Projektion an
    /// </summary>
    public struct ProjectionBoundsScreen
    {
        /// <summary>
        /// Mitte der Projektion
        /// </summary>
        public Vector2i Center { get; internal set; }
        /// <summary>
        /// Breite des Grenzbereichs
        /// </summary>
        public int Width { get { return Right - Left; } }
        /// <summary>
        /// Höhe des Grenzbereichs
        /// </summary>
        public int Height { get { return Bottom - Top; } }
        /// <summary>
        /// Obere Grenzkoordinate
        /// </summary>
        public int Top { get; internal set; }
        /// <summary>
        /// Untere Grenzkoordinate
        /// </summary>
        public int Bottom { get; internal set; }
        /// <summary>
        /// Linke Grenzkoordinate
        /// </summary>
        public int Left { get; internal set; }
        /// <summary>
        /// Rechte Grenzkoordinate
        /// </summary>
        public int Right { get; internal set; }
        /// <summary>
        /// Hintere Grenzkoordinate
        /// </summary>
        public float Back { get; internal set; }
        /// <summary>
        /// Vordere Grenzkoordinate
        /// </summary>
        public float Front { get; internal set; }

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        /// <param name="bounds">uu konvertierende Koordinaten</param>
        /// <param name="scale">Skalierungsfaktor</param>
        /// <param name="offsetX">Verschiebung in X-Richtung (in Pixeln)</param>
        /// <param name="offsetY">Verschiebung in Y-Richtung (in Pixeln)</param>
        public ProjectionBoundsScreen(ProjectionBounds bounds, float scale = 1f, int offsetX = 0, int offsetY = 0)
        {
            Top = (int)((1f - (bounds.Top * scale * 0.5f + 0.5f)) * KWEngine.Window.Height) + offsetY;
            Bottom = (int)((1f - (bounds.Bottom * scale * 0.5f + 0.5f)) * KWEngine.Window.Height) + offsetY;
            Left = (int)((bounds.Left * scale * 0.5f + 0.5f) * KWEngine.Window.Width) + offsetX;
            Right = (int)((bounds.Right * scale * 0.5f + 0.5f) * KWEngine.Window.Width) + offsetX;
            Back = bounds.Back;
            Front = bounds.Front;
            Center = new Vector2i((Left + Right) / 2, (Top + Bottom) / 2);
        }
    }
}
