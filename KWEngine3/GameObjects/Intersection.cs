using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Kollisionsklasse
    /// </summary>
    public sealed class Intersection
    {
        internal readonly Vector3 UNIT = Vector3.UnitZ;

        /// <summary>
        /// Das Objekt, mit dem kollidiert wurde
        /// </summary>
        public GameObject Object { get; private set; } = null;
        /// <summary>
        /// Der Name der Hitbox, mit der kollidiert wurde
        /// </summary>
        public string MeshName { get; private set; } = "";
        
        internal Vector3 _MTV = Vector3.Zero;
        internal Vector3 _MTVUp = Vector3.Zero;
        internal Vector3 _MTVUpToTop = Vector3.Zero;

        /// <summary>
        /// Art der getroffenen Hitbox
        /// </summary>
        public ColliderType Type { get; private set; } = ColliderType.ConvexHull;

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
        /// Minimal-Translation-Vector für die Y-Achse, der immer auf die oberste Fläche des Kollisionsobjekts verschiebt
        /// </summary>
        public Vector3 MTVUpToTop
        {
            get
            {
                return _MTVUpToTop;
            }
        }

        /// <summary>
        /// Minimal-Translation-Vector für die Y-Achse
        /// </summary>
        public Vector3 MTVUp
        {
            get
            {
                return _MTVUp;
            }
        }

        internal Vector3 _colliderSurfaceNormal = Vector3.UnitZ;

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
        internal GameObjectHitbox _hitboxCollider = null;
        internal Intersection()
        {

        }
        internal Intersection(GameObject collider, GameObjectHitbox hbCaller, GameObjectHitbox hbCollider, Vector3 mtv, Vector3 mtvUp, string mName, Vector3 surfaceNormal, Vector3 mtvUpTop, ColliderType type)
        {
            Type = type;
            _hitboxCaller = hbCaller;
            _hitboxCollider = hbCollider;
            Object = collider;
            MeshName = mName;
            _MTV = mtv;
            _MTVUp = mtvUp;
            _colliderSurfaceNormal = surfaceNormal;
            _MTVUpToTop = mtvUpTop;
        }

        /// <summary>
        /// Berechnet das Volumen der Kollision (wie viel von Objekt B schneidet sich mit Objekt A?)
        /// </summary>
        /// <remarks>
        /// Funktioniert lediglich, wenn beide Hitboxen vom Typ ConvexHull sind
        /// </remarks>
        /// <returns>IntersectionVolume-Instanz mit genaueren Angaben zum Volumen</returns>
        public IntersectionVolume CalculateVolume()
        {
            if (_hitboxCaller._colliderType == ColliderType.ConvexHull && _hitboxCollider._colliderType != ColliderType.ConvexHull)
            {
                List<Vector3> volumeVertices = HelperIntersection.ClipFaces(_hitboxCaller, _hitboxCollider);
                volumeVertices.AddRange(HelperIntersection.ClipFaces(_hitboxCollider, _hitboxCaller));
                if (volumeVertices.Count > 0)
                {
                    IntersectionVolume volume = new IntersectionVolume(volumeVertices, _hitboxCaller._center);
                    return volume;
                }
                else
                {
                    IntersectionVolume volume = new IntersectionVolume();
                    return volume;
                }
            }
            return new IntersectionVolume();
        }
    }
}
