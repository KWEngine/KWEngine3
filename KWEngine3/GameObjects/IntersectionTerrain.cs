using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Kollisionsklasse für Terrain
    /// </summary>
    public class IntersectionTerrain
    {
        internal readonly Vector3 UNIT = Vector3.UnitZ;

        /// <summary>
        /// Das Objekt, mit dem kollidiert wurde
        /// </summary>
        public TerrainObject Object { get; private set; } = null;
        
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

        /// <summary>
        /// Minimal-Translation-Vector für die Y-Achse
        /// </summary>
        public Vector3 MTVUp { get; internal set; } = Vector3.Zero;

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

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        /// <param name="collider">Kollisionsobjekt</param>
        /// <param name="hbCaller">Hitbox-Instanz des abfragenden Objekts</param>
        /// <param name="mtv">Korrektur-MTV</param>
        /// <param name="mtvUp">TerrainHöhe</param>
        /// <param name="mName">Mesh-Name</param>
        /// <param name="surfaceNormal">Ebenenvektor der Kollisionsoberfläche</param>
        internal IntersectionTerrain(TerrainObject collider, GameObjectHitbox hbCaller, Vector3 mtv, Vector3 mtvUp, string mName, Vector3 surfaceNormal)
        {
            _hitboxCaller = hbCaller;
            Object = collider;
            _MTV = mtv;
            MTVUp = mtvUp;
            _colliderSurfaceNormal = surfaceNormal;
        }
    }
}
