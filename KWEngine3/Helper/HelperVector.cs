using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Vektoroperationen
    /// </summary>
    public static class HelperVector
    {
        /// <summary>
        /// Reflektiert den eingehenden Vektor 'directionIn' am Ebenenvektor 'surfaceNormal'
        /// </summary>
        /// <param name="directionIn">Eingehender Vektor</param>
        /// <param name="surfaceNormal">Ebenenvektor</param>
        /// <returns>Reflektierter Vektor</returns>
        public static Vector3 ReflectVector(Vector3 directionIn, Vector3 surfaceNormal)
        {
            Vector3 reflectedVector = directionIn - 2 * Vector3.Dot(directionIn, surfaceNormal) * surfaceNormal;
            return Vector3.NormalizeFast(reflectedVector);
        }

        /// <summary>
        /// Berechnet die Rotation (als Quaternion), auf die ein GameObject gesetzt werden müsste, um entsprechend der aktuellen Ausrichtung 
        /// auf dem durch den Ebenenvektor (surface normal) definierten Boden gerade zu stehen
        /// </summary>
        /// <param name="lav">Aktueller Look-At-Vektor</param>
        /// <param name="surfaceNormal">Ebenenvektor</param>
        /// <returns>Neue zur Ebene passende Rotation (als Quaternion)</returns>
        public static Quaternion GetRotationToMatchSurfaceNormal(Vector3 lav, Vector3 surfaceNormal)
        {
            Vector3 playerRight = Vector3.NormalizeFast(Vector3.Cross(-lav, KWEngine.WorldUp));
            Vector3 cross = Vector3.NormalizeFast(Vector3.Cross(playerRight, surfaceNormal));
            if(Vector3.Dot(lav, cross) < 0)
            {
                return LookRotation(-cross, surfaceNormal);
            }
            else
            {
                return LookRotation(cross, surfaceNormal);
            }
        }
        
        /// <summary>
        /// Berechnet den Vektor, der für den gegebenen Look-At-Vektor (LAV) "oben" darstellt (senkrecht zum LAV)
        /// </summary>
        /// <param name="lav">Look-At-Vektor</param>
        /// <returns>"Oben"-Vektor</returns>
        public static Vector3 GetUpVectorFromLookAtVector(Vector3 lav)
        {
            return Vector3.NormalizeFast(Vector3.Cross(Vector3.NormalizeFast(Vector3.Cross(lav, KWEngine.WorldUp)), lav));
        }

        /// <summary>
        /// Rotiert einen Vektor mit Hilfe der angegebenen Quaternion (Hamilton-Produkt)
        /// </summary>
        /// <param name="source">zu rotierender Vektor</param>
        /// <param name="rotation">Rotation als Quaternion</param>
        /// <returns>rotierter Vektor</returns>
        public static Vector3 RotateVectorByQuaternion(Vector3 source, Quaternion rotation)
        {
            Quaternion qri;
            Quaternion.Invert(rotation, out qri);
            Quaternion sourceQ = new Quaternion(source, 0);
            return (rotation * sourceQ * qri).Xyz;
        }

        /// <summary>
        /// Berechnet den Vektor, der entsteht, wenn der übergebene Vektor um die angegebenen Grad rotiert wird
        /// </summary>
        /// <param name="vector">zu rotierender Vektor</param>
        /// <param name="degrees">Rotation (in Grad)</param>
        /// <param name="plane">Einheitsvektor, um den rotiert wird</param>
        /// <returns>Rotierter Vektor</returns>
        public static Vector3 RotateVector(Vector3 vector, float degrees, Plane plane)
        {
            if (plane == Plane.YZ)
            {
                return HelperVector.RotateVectorByQuaternion(vector, Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(degrees)));
            }
            else if (plane == Plane.XZ)
            {
                return HelperVector.RotateVectorByQuaternion(vector, Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(degrees)));

            }
            else if (plane == Plane.XY)
            {
                return HelperVector.RotateVectorByQuaternion(vector, Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(degrees)));
            }
            else
                return HelperVector.RotateVectorByQuaternion(vector, Quaternion.FromAxisAngle(KWEngine.CurrentWorld._cameraGame._stateCurrent.LookAtVector, MathHelper.DegreesToRadians(degrees)));
        }

        /// <summary>
        /// Berechnet den Vektor, der entsteht, wenn der übergebene Vektor um die angegebenen Grad rotiert wird
        /// </summary>
        /// <param name="vector">zu rotierender Vektor</param>
        /// <param name="degrees">Rotation (in Grad)</param>
        /// <param name="axis">Achse, um den rotiert wird</param>
        /// <returns>Rotierter Vektor</returns>
        public static Vector3 RotateVector(Vector3 vector, float degrees, Axis axis)
        {
            return RotateVector(vector, degrees, axis == Axis.X ? Plane.YZ : axis == Axis.Y ? Plane.XZ : Plane.XY);
        }

        /// <summary>
        /// Berechnet die 2D-Bildschirmkoordinaten für die übergebene GameObject-Instanz.
        /// </summary>
        /// <param name="g">GameObject-Instanz</param>
        /// <returns>2D-Bildschirmkoordinaten als Vector2i-Instanz</returns>
        public static Vector2i GetScreenCoordinatesFor(GameObject g)
        {
            return GetScreenCoordinatesFor(g.Center);
        }

        /// <summary>
        /// Berechnet den Winkel (in Grad), der zwischen zwei 3D-Objekten auf dem Bildschirm existiert.
        /// Die Gradzahlen beginnen bei 0° (oben) und gehen im Uhrzeigersinn bis 359.9°.
        /// </summary>
        /// <param name="source">Quellposition (in 3D-Koordinaten)</param>
        /// <param name="target">Zielposition (in 3D-Koordinaten)</param>
        /// <returns>Winkel (in Grad)</returns>
        public static float GetScreenAngleBetween(Vector3 source, Vector3 target)
        {
            Vector2 source2d = GetScreenCoordinatesFor(source);
            Vector2 target2d = GetScreenCoordinatesFor(target);
            Vector2 delta = target2d - source2d;

            float dot = Vector2.Dot(delta, -Vector2.UnitY);
            float denominator = delta.Length * 1;

            float rad = (float)Math.Acos(dot / denominator);
            float value = MathHelper.RadiansToDegrees(rad);
            if (float.IsNaN(value))
                value = 0;
            if (delta.X < 0)
                value = 360 - value;
            return value;
        }

        /// <summary>
        /// Berechnet die 2D-Bildschirmkoordinaten für die übergebene 3D-Position.
        /// </summary>
        /// <param name="position">Positionsangabe in 3D</param>
        /// <returns>2D-Bildschirmkoordinaten als Vector2i-Instanz</returns>
        public static Vector2i GetScreenCoordinatesFor(Vector3 position)
        {
            Vector2i screenCoordinates = new Vector2i();

            Vector4 posClipSpace = Vector4.TransformRow(new Vector4(position, 1f), KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewProjectionMatrix);
            posClipSpace.X /= posClipSpace.W;
            posClipSpace.Y /= posClipSpace.W;

            screenCoordinates.X = (int)((posClipSpace.X * 0.5f + 0.5f) * KWEngine.Window.ClientSize.X);
            screenCoordinates.Y = KWEngine.Window.ClientSize.Y - (int)((posClipSpace.Y * 0.5f + 0.5f) * KWEngine.Window.ClientSize.Y);

            return screenCoordinates;
        }

        internal static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
            forward.NormalizeFast();

            Vector3 vector = Vector3.NormalizeFast(forward);
            Vector3 vector2 = Vector3.NormalizeFast(Vector3.Cross(up, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            var m00 = vector2.X;
            var m01 = vector2.Y;
            var m02 = vector2.Z;
            var m10 = vector3.X;
            var m11 = vector3.Y;
            var m12 = vector3.Z;
            var m20 = vector.X;
            var m21 = vector.Y;
            var m22 = vector.Z;


            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }
    }
}
