using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Kollisionsklasse für Terrain-Kollisionen
    /// </summary>
    public sealed class IntersectionTerrain
    {
        internal Vector3 _mtv = Vector3.Zero;
        internal TerrainObject _collider;
        internal Vector3 _colliderSurfaceNormal = Vector3.UnitZ;

        /// <summary>
        /// TerrainObject-Instanz, die für die Kollision maßgeblich verantwortlich ist
        /// </summary>
        public TerrainObject Object { get { return _collider; } }

        /// <summary>
        /// Minimal-Translation-Vector (für Kollisionskorrektur)
        /// </summary>
        public Vector3 MTV
        {
            get
            {
                return _mtv;
            }
        }

        /// <summary>
        /// Gibt den Ebenenvektor der Oberfläche des Objekts an, mit dem die Kollision stattfand
        /// </summary>
        public Vector3 ColliderSurfaceNormal
        {
            get
            {
                return _colliderSurfaceNormal;
            }
        }

        internal IntersectionTerrain()
        {

        }
    }
}
