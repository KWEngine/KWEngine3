using System.Collections.Generic;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Beschreibt einen einzelnen Animations-Layer für ein EngineObject.
    /// Bis zu 4 Layer können gleichzeitig aktiv sein und werden gewichtet gemischt.
    /// </summary>
    internal struct AnimationLayer
    {
        /// <summary>
        /// Index der Animation (entspricht dem Index in der Animations-Liste des Modells; -1 = inaktiv)
        /// </summary>
        public int AnimationID;

        /// <summary>
        /// Aktueller Abspielfortschritt (0 bis 1)
        /// </summary>
        public float Percentage;

        /// <summary>
        /// Blending-Gewicht dieses Layers (0 bis 1).
        /// Alle aktiven Layer werden normalisiert gemischt.
        /// </summary>
        public float Weight;

        /// <summary>
        /// Optionale Menge von Knochennamen, für die dieser Layer gilt (intern gesetzt via Preset-Name).
        /// Ist die Menge null oder leer, gilt der Layer für alle Knochen.
        /// </summary>
        internal HashSet<string> BoneMask;

        /// <summary>
        /// Gibt an, ob dieser Layer aktiv ist
        /// </summary>
        public bool Active => AnimationID >= 0;

        internal static AnimationLayer CreateDefault()
        {
            return new AnimationLayer
            {
                AnimationID = -1,
                Percentage = 0f,
                Weight = 1f,
                BoneMask = null
            };
        }
    }
}
