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
        /// Gibt den dazugehörigen Punkt auf der Kollisionsebene an
        /// </summary>
        public Vector3 ContactPoint { get; internal set; }

        /// <summary>
        /// Gibt den Punkt an, der die Mitte des Kollisionsvolumens bildet
        /// </summary>
        public Vector3 IntersectionVolumeCenter { get; internal set; }

        /// <summary>
        /// Gibt den Ebenenvektor der Oberfläche des Objekts an, mit dem die Kollision stattfand
        /// </summary>
        public Vector3 ColliderSurfaceNormal { get; internal set; }
       

        internal IntersectionTerrain()
        {

        }
    }
}
