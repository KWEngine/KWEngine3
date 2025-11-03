using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Kollisionsklasse für Terrain-Kollisionen
    /// </summary>
    public class IntersectionTerrain
    {
        internal readonly Vector3 UNIT = Vector3.UnitZ;

        /// <summary>
        /// Das Objekt, mit dem kollidiert wurde
        /// </summary>
        public TerrainObject Object { get; private set; } = null;
        /// <summary>
        /// Der Name der Hitbox, mit der kollidiert wurde
        /// </summary>
        
        private Vector3 _MTV = Vector3.Zero;

        /// <summary>
        /// Minimal-Translation-Vector (für Kollisionskorrektur)
        /// </summary>
        public Vector3 MTV
        {
            get
            {
                return _MTV;
            }
        }

        private Vector3 _colliderSurfaceNormal = Vector3.UnitZ;

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

        internal GameObjectHitbox _hitboxCaller = null;

        internal IntersectionTerrain(TerrainObject collider, GameObjectHitbox hbCaller, Vector3 mtv, Vector3 surfaceNormal)
        {
            _hitboxCaller = hbCaller;
            Object = collider;
            _MTV = mtv;
            _colliderSurfaceNormal = surfaceNormal;
        }
    }
}
