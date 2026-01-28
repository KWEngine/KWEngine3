using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Basisklasse für Explosions und Partikeleffekte
    /// </summary>
    public abstract class TimeBasedObject : IComparable<TimeBasedObject>
    {
        /// <summary>
        /// Gibt an, ob die Animationsphase des Objekts abgeschlossen ist. Abgeschlossene Objekte dieses Typs werden automatisch von der Engine entfernt.
        /// </summary>
        public bool Finished { get; internal set; } = false;

        /// <summary>
        /// Position der Instanz
        /// </summary>
        public Vector3 Position { get; internal set; } = new Vector3(0, 0, 0);

        /// <summary>
        /// Vergleicht die aktuelle Instanz mit einem anderen TimeBasedObject basierend auf der Distanz zur Kamera.
        /// </summary>
        /// <param name="other">andere Instanz</param>
        /// <returns>-1, 0 oder 1</returns>
        public int CompareTo(TimeBasedObject other)
        {
            Vector3 camPos = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position;
            float distSqThis = (camPos - Position).LengthSquared;
            float distSqOther = (camPos - other.Position).LengthSquared;

            return distSqOther.CompareTo(distSqThis);
        }

        internal abstract void Act();
    }
}
