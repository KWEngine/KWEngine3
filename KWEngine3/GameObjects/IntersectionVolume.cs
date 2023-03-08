using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Enthält erweiterte Informationen zum Kollisionsvolumen
    /// </summary>
    public struct IntersectionVolume
    {
        /// <summary>
        /// Gibt den Mittelpunkt des Kollisionsvolumens an
        /// </summary>
        public Vector3 Center { get; private set; }
        /// <summary>
        /// Gibt die Breite des Kollisionsvolumens entlang der X-Achse an
        /// </summary>
        public float SpanX { get; private set; }
        /// <summary>
        /// Gibt die Höhe des Kollisionsvolumens entlang der Y-Achse an
        /// </summary>
        public float SpanY { get; private set; }
        /// <summary>
        /// Gibt die Tiefe des Kollisionsvolumens entlang der Z-Achse an
        /// </summary>
        public float SpanZ { get; private set; }
        /// <summary>
        /// Gibt die relative Richtung ausgehend von der Hitbox-Mitte hin zum Kollisionsvolumen an
        /// </summary>
        public Vector3 DirectionFromHitboxCenter { get; private set; }
        /// <summary>
        /// Gibt die Distanz des Kollisionsvolumens zur Hitbox-Mitte an
        /// </summary>
        public float DistanceFromHitboxCenter { get; private set; }
        /// <summary>
        /// Die Eckpunkte des Kollisionsvolumens
        /// </summary>
        public List<Vector3> VolumeVertices { get; private set; }
        /// <summary>
        /// Gibt an, ob die Messungen valide sind (nur dann sollten die Messdaten verwendet werden!)
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Konstruktormethode für das Volumen einer Kollision
        /// </summary>
        /// <param name="vertices">Liste der Schnittpunkte</param>
        /// <param name="hitboxCenter">Zentraler Punkt der Hitbox des Aufrufers</param>
        public IntersectionVolume(List<Vector3> vertices, Vector3 hitboxCenter)
        {
            VolumeVertices = vertices;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            foreach(Vector3 v in vertices)
            {
                if (v.X > maxX)
                    maxX = v.X;
                if (v.Y > maxY)
                    maxY = v.Y;
                if (v.Z > maxZ)
                    maxZ = v.Z;
                if (v.X < minX)
                    minX = v.X;
                if (v.Y < minY)
                    minY = v.Y;
                if (v.Z < minZ)
                    minZ = v.Z;
            }
            Center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            SpanX = maxX - minX;
            SpanY = maxY - minY;
            SpanZ = maxZ - minZ;
            Vector3 hitboxCenterToClippingVolumeCenter = Center - hitboxCenter;
            DistanceFromHitboxCenter = hitboxCenterToClippingVolumeCenter.LengthFast;
            DirectionFromHitboxCenter = Vector3.NormalizeFast(hitboxCenterToClippingVolumeCenter);
            IsValid = true;
        }
    }
}
