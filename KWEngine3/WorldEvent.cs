using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3
{
    /// <summary>
    /// Ereignisklasse für World-Instanzen
    /// </summary>
    public class WorldEvent
    {
        /// <summary>
        /// Zeitstempel (in Sekunden) seit Beginn der aktuellen Weltzeit. Zu diesem Zeitpunkt wird das Event ausgeführt.
        /// </summary>
        public float Timestamp { get; private set; }

        /// <summary>
        /// Wert des Ereignisses (Objekttyp frei wählbar)
        /// </summary>
        public object Tag { get; private set; }

        /// <summary>
        /// Beschreibung (oder Schlüsselwort) des Ereignisses
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Konstruktormethode eines Ereignisses
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="description"></param>
        /// <param name="tag"></param>
        public WorldEvent(float timestamp, string description, object tag = null)
        {
            Timestamp = timestamp;
            Description = description == null ? "" : description;
            Tag = tag;
        }
    }
}
