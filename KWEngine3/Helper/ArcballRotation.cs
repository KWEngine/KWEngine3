namespace KWEngine3.Helper
{
    /// <summary>
    /// Gibt eine Arcball-Rotation an
    /// </summary>
    public struct ArcballRotation
    {
        /// <summary>
        /// Distanz zum Drehpunkt
        /// </summary>
        public float Distance;
        /// <summary>
        /// X-Winkel in Grad
        /// </summary>
        public float XAngle;
        /// <summary>
        /// Y-Winkel in Grad
        /// </summary>
        public float YAngle;
        /// <summary>
        /// Gibt an, ob das Ergebnis gültig ist
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Gibt eine menschenlesbare Version der Werte zurück
        /// </summary>
        /// <returns>Lesbare Version der Instanz</returns>
        public override string ToString()
        {
            return "X: " + XAngle + "°, Y: " + YAngle + "°, d=" + Distance + (IsValid ? " (valid)" : " (invalid)");
        }
    }
}
