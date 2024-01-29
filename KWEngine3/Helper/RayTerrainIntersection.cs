using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Wrapper-Klasse, die die Entfernung eines Objekts und den Schnittpunkt eines Strahls auf dem Terrain-Objekt beinhaltet
    /// </summary>
    public struct RayTerrainIntersection
    {
        /// <summary>
        /// Das vom Strahl getroffene Terrain-Objekt
        /// </summary>
        public TerrainObject Object { get; internal set; }
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
        public RayTerrainIntersection()
        {
            Object = null;
            Distance = 0;
            IntersectionPoint = Vector3.Zero;
            SurfaceNormal = KWEngine.WorldUp;
        }

        /// <summary>
        /// Gibt an, ob die Messung gültig und somit verwendbar ist
        /// </summary>
        public bool IsValid { get { return Object != null; } }
    }
}
