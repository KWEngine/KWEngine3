using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Wrapper-Klasse, die die Entfernung des vom Strahl getroffenen Objekts und einen Verweis auf das getroffene Objekt selbst beinhaltet
    /// </summary>
    public struct RayIntersection : IComparable<RayIntersection>
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
        /// Standardkonstruktor für die Instanz
        /// </summary>
        public RayIntersection()
        {
            Object = null;
            Distance = 0;
        }

        /// <summary>
        /// Gibt an, ob die Messung gültig und somit verwendbar ist
        /// </summary>
        public bool IsValid {  get { return Object != null; } }

        /// <summary>
        /// Vergleichsmethode, um zwei Objektdistanzen miteinander zu vergleichen
        /// </summary>
        /// <param name="other">Vergleichsobjekt</param>
        /// <returns>Sortierreihenfolge (-1 = näher, 0 = gleiche Entfernung, 1 = entfernter</returns>
        public int CompareTo(RayIntersection other)
        {
            return this.Distance < other.Distance ? -1 : this.Distance == other.Distance ? 0 : 1;  
        }
    }
}
