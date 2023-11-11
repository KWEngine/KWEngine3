using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Wrapper-Klasse, die die Entfernung eines Objekts und den Schnittpunkt eines Strahls auf dem Objekt beinhaltet
    /// </summary>
    public struct RayIntersectionExt : IComparable<RayIntersectionExt>
    {
        /// <summary>
        /// Das vom Strahl getroffene Objekt
        /// </summary>
        public GameObject Object { get; internal set; }
        /// <summary>
        /// Die Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float Distance { get; internal set; } = float.MaxValue;
        /// <summary>
        /// Der genaue Schnittpunkt des Strahls auf dem Objekt
        /// </summary>
        public Vector3 IntersectionPoint { get; internal set; } = Vector3.Zero;

        /// <summary>
        /// Der Ebenenvektor des Schnittpunkts
        /// </summary>
        public Vector3 SurfaceNormal { get; internal set; } = KWEngine.WorldUp;

        /// <summary>
        /// Standardkonstruktor für die Instanz
        /// </summary>
        public RayIntersectionExt()
        {
            Object = null;
            Distance = 0;
            IntersectionPoint = Vector3.Zero;
            SurfaceNormal = KWEngine.WorldUp;
        }

        /// <summary>
        /// Vergleichsmethode, um zwei Objektdistanzen miteinander zu vergleichen
        /// </summary>
        /// <param name="other">Vergleichsobjekt</param>
        /// <returns>Sortierreihenfolge (-1 = näher, 0 = gleiche Entfernung, 1 = entfernter</returns>
        public int CompareTo(RayIntersectionExt other)
        {
            return this.Distance < other.Distance ? -1 : this.Distance == other.Distance ? 0 : 1;  
        }
    }
}
