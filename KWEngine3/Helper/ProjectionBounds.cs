﻿using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Gibt die aktuellen Grenzen einer AABB-Hitbox für die aktuelle Projektion an
    /// </summary>
    public struct ProjectionBounds
    {
        /// <summary>
        /// Mitte der Projektion
        /// </summary>
        public Vector2 Center { get; internal set; }
        /// <summary>
        /// Obere Grenzkoordinate
        /// </summary>
        public float Top { get; internal set; }
        /// <summary>
        /// Untere Grenzkoordinate
        /// </summary>
        public float Bottom { get; internal set; }
        /// <summary>
        /// Linke Grenzkoordinate
        /// </summary>
        public float Left { get; internal set; }
        /// <summary>
        /// Rechte Grenzkoordinate
        /// </summary>
        public float Right { get; internal set; }
        /// <summary>
        /// Hintere Grenzkoordinate
        /// </summary>
        public float Back { get; internal set; }
        /// <summary>
        /// Vordere Grenzkoordinate
        /// </summary>
        public float Front { get; internal set; }

        /// <summary>
        /// Prüft ob die Projektion auf der Map sichtbar wäre
        /// </summary>
        /// <returns>true, wenn das Objekt sichtbar wäre</returns>
        public bool IsVisibleOnMap()
        {
            return !(Left > 1f || Right < -1f || Bottom > 1f || Top < -1f || Front < -1f || Back > 1f);
        }
    }
}
