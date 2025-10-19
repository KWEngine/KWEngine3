namespace KWEngine3.Helper
{
    /// <summary>
    /// Basisklasse für Explosions und Partikeleffekte
    /// </summary>
    public abstract class TimeBasedObject
    {
        /// <summary>
        /// Gibt an, ob die Animationsphase des Objekts abgeschlossen ist. Abgeschlossene Objekte dieses Typs werden automatisch von der Engine entfernt.
        /// </summary>
        public bool Finished { get; internal set; } = false;

        internal abstract void Act();
    }
}
