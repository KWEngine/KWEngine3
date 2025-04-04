﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3
{
    /// <summary>
    /// Ereignisklasse für World-Instanzen
    /// </summary>
    public class WorldEvent : IComparable<WorldEvent>
    {
        /// <summary>
        /// Gibt an, ob das Event durch ein Eingabeelement (Texteingabefeld) ausgelöst wurde
        /// </summary>
        public bool GeneratedByInputFocusLost { get; internal set; }

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

        /// <summary>
        /// Standardvergleichmethode für das Sortieren der Ereignisobjekte (wird intern benötigt)
        /// </summary>
        /// <param name="other">Vergleichsereignis</param>
        /// <returns>Vergleichsergebnis (-1, 0 oder 1)</returns>
        public int CompareTo(WorldEvent other)
        {
            return Timestamp.CompareTo(other.Timestamp) * -1;
        }

        internal World Owner { get; set; } = null;
    }
}
