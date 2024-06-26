﻿using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Wrapper-Klasse, die die Entfernung der von mehreren Strahlen getroffenen Objekte und Verweise auf diese beinhaltet
    /// </summary>
    public struct RayTerrainIntersectionSet
    {
        /// <summary>
        /// Liste der getroffenen Objekte
        /// </summary>
        public List<TerrainObject> Objects { get; internal set; }
        /// <summary>
        /// Die kleinste Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float DistanceMin { get; internal set; } = float.MaxValue;

        /// <summary>
        /// Die durchschnittliche Distanz vom Aufrufer zum getroffenen Objekt
        /// </summary>
        public float DistanceAvg { get; internal set; } = 0f;

        /// <summary>
        /// Punkte, an dem die Strahlen auf Objekte trafen
        /// </summary>
        public List<Vector3> IntersectionPoints { get; internal set; }

        /// <summary>
        /// Ebenenvektoren der getroffenen Objekte
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
        public RayTerrainIntersectionSet()
        {
            Objects = new List<TerrainObject>();
            
            DistanceAvg = 0f;
            DistanceMin = float.MaxValue;
            
            IntersectionPointAvg = Vector3.Zero;
            IntersectionPointNearest = Vector3.Zero;

            SurfaceNormalNearest = Vector3.UnitY;
            SurfaceNormalAvg = Vector3.UnitY;

            IntersectionPoints = new List<Vector3>();
            SurfaceNormals = new List<Vector3>();
        }

        internal void AddObject(TerrainObject t)
        {
            if(Objects.Contains(t) == false)
                Objects.Add(t);
        }

        internal void AddIntersectionPoint(Vector3 i)
        {
            IntersectionPoints.Add(i);
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
