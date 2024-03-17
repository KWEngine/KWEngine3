using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Wrapper-Klasse, die die Entfernung des vom Strahl getroffenen Objekts und einen Verweis auf das getroffene Objekt selbst beinhaltet
    /// </summary>
    public struct RayIntersectionGroupResult
    {
        /// <summary>
        /// Das vom Strahl getroffene Objekt
        /// </summary>
        public List<GameObject> Objects { get; internal set; }
        /// <summary>
        /// Die kleinste Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float DistanceMin { get; internal set; } = float.MaxValue;

        /// <summary>
        /// Die größte Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float DistanceMax { get; internal set; } = float.MinValue;

        /// <summary>
        /// Die durchschnittliche Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float DistanceAvg { get; internal set; } = 0f;

        /// <summary>
        /// Punkt an dem der Strahl auf das Objekt trifft
        /// </summary>
        public List<Vector3> IntersectionPoints { get; internal set; }
        /// <summary>
        /// Punkt an dem der Strahl auf das Objekt trifft
        /// </summary>
        public List<Vector3> SurfaceNormals { get; internal set; }

        /// <summary>
        /// Enthält den Schnittpunkt, der von allen Strahlen am nächsten am Strahlursprung war
        /// </summary>
        public Vector3 IntersectionPointNearest { get; internal set; }
        /// <summary>
        /// Enthält den Durchschnittswert aller ermittelten Schnittpunkte
        /// </summary>
        public Vector3 IntersectionPointAvg { get; internal set; }

        /// <summary>
        /// Enthält den Vektor der Oberfläche des am nächsten liegenden Schnittpunkts
        /// </summary>
        public Vector3 SurfaceNormalNearest { get; internal set; }

        /// <summary>
        /// Enthält den durchschnittlichen Oberflächenvektor aller ermittelten Schnittpunktflächen
        /// </summary>
        public Vector3 SurfaceNormalAvg { get; internal set; }

        /// <summary>
        /// Standardkonstruktor für die Instanz
        /// </summary>
        public RayIntersectionGroupResult()
        {
            Objects = new List<GameObject>();
            
            DistanceAvg = 0f;
            DistanceMin = float.MaxValue;
            DistanceMax = float.MinValue;
            
            IntersectionPointAvg = Vector3.Zero;
            IntersectionPointNearest = Vector3.Zero;

            SurfaceNormalNearest = Vector3.UnitY;
            SurfaceNormalAvg = Vector3.UnitY;

            IntersectionPoints = new List<Vector3>();
            SurfaceNormals = new List<Vector3>();
        }

        internal void AddObject(GameObject g)
        {
            Objects.Add(g);
        }

        internal void AddSurfaceNormal(Vector3 n)
        {
            SurfaceNormals.Add(n);
        }

        /// <summary>
        /// Gibt an, ob die Messung gültig und somit verwendbar ist
        /// </summary>
        public bool IsValid {  get { return Objects.Count > 0; } }
    }
}
