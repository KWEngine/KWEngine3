using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Kollisionsklasse für Terrain-Kollisionen
    /// </summary>
    public sealed class IntersectionTerrain
    {
        /// <summary>
        /// TerrainObject-Instanz, die für die Kollision maßgeblich verantwortlich ist
        /// </summary>
        public TerrainObject Object { get; internal set; }

        /// <summary>
        /// Minimal-Translation-Vector (für Kollisionskorrektur)
        /// </summary>
        public Vector3 MTV { get; internal set; }

        /// <summary>
        /// Gibt den maßgeblich beteiligten Kollisionspunkt
        /// </summary>
        public Vector3 ContactPoint { get; internal set; }

        /// <summary>
        /// Eine Liste aller relevanten Schnittpunkte mit dem Terrain-Objekt
        /// </summary>
        public List<Vector3> ContactPoints { get; internal set; }

        /// <summary>
        /// Gibt den Ebenenvektor der Oberfläche des Objekts an, mit dem die Kollision stattfand
        /// </summary>
        public Vector3 ColliderSurfaceNormal { get; internal set; }
       

        internal IntersectionTerrain()
        {

        }
    }
}
