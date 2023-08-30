using OpenTK.Mathematics;
using KWEngine3;
using KWEngine3.Helper;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Rotationsberechnungen
    /// </summary>
    public static class HelperRotation
    {
        private static Matrix4 translationPointMatrix = Matrix4.Identity;
        private static Matrix4 rotationMatrix = Matrix4.Identity;
        private static Matrix4 translationMatrix = Matrix4.Identity;
        private static Matrix4 tempMatrix = Matrix4.Identity;
        private static Matrix4 spinMatrix = Matrix4.Identity;
        private static Vector3 finalTranslationPoint = Vector3.Zero;
        private static Vector3 zeroVector = Vector3.Zero;
        //private static Quaternion Turn180 = Quaternion.FromAxisAngle(KWEngine.WorldUp, (float)Math.PI);

        /// <summary>
        /// Berechnet den Vektor, der entsteht, wenn der übergebene Vektor um die angegebenen Grad rotiert wird
        /// </summary>
        /// <param name="vector">zu rotierender Vektor</param>
        /// <param name="degrees">Rotation (in Grad)</param>
        /// <param name="plane">Einheitsvektor, um den rotiert wird</param>
        /// <returns>Rotierter Vektor</returns>
        public static Vector3 RotateVector(Vector3 vector, float degrees, Plane plane)
        {
            return HelperVector.RotateVector(vector, degrees, plane);
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
            return HelperVector.RotateVector(vector, degrees, axis == Axis.X ? Plane.YZ : axis == Axis.Y ? Plane.XZ : Plane.XY);
        }

        /// <summary>
        /// Rotiert einen Vektor mit Hilfe der angegebenen Quaternion (Hamilton-Produkt)
        /// </summary>
        /// <param name="source">zu rotierender Vektor</param>
        /// <param name="rotation">Rotation als Quaternion</param>
        /// <returns>rotierter Vektor</returns>
        public static Vector3 RotateVectorByQuaternion(Vector3 source, Quaternion rotation)
        {
            return HelperVector.RotateVectorByQuaternion(source, rotation);
        }

        /// <summary>
        /// Konvertiert eine in Quaternion angegebene Rotation in eine XYZ-Rotation (in Grad)
        /// </summary>
        /// <param name="q">zu konvertierendes Quaternion</param>
        /// <returns>XYZ-Rotation als Vector3 (in Grad)</returns>
        public static Vector3 ConvertQuaternionToEulerAngles(Quaternion q)
        {
            Vector3 result = new Vector3(0, 0, 0);
            // roll (x-axis rotation)
            double sinr = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            result.X = (float)Math.Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                result.Y = sinp < 0 ? ((float)Math.PI / 2.0f) * -1.0f : (float)Math.PI / 2.0f;
            }
            else
                result.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            result.Z = (float)Math.Atan2(siny, cosy);

            result.X = CalculateDegreesFromRadians(result.X);
            result.Y = CalculateDegreesFromRadians(result.Y);
            result.Z = CalculateDegreesFromRadians(result.Z);

            return result;
        }

        /// <summary>
        /// Berechnet die neue Kameraposition in Abhängigkeit der Mausbewegung.
        /// </summary>
        /// <param name="pivot">Dreh- und Angelpunkt</param>
        /// <param name="distance">Distanz zum Dreh- und Angelpunkt</param>
        /// <param name="degreesLeftRight">Grad der Rotation nach links oder rechts</param>
        /// <param name="degreesUpDown">Grad der Rotation nach unten oder oben</param>
        /// <param name="invertX">invertiert die Links-Rechts-Rotation, wenn aktiv</param>
        /// <param name="invertY">invertiert die Oben-Unten-Rotation, wenn aktiv</param>
        /// <returns>Neue Kameraposition</returns>
        public static Vector3 CalculateRotationForArcBallCamera(Vector3 pivot, float distance, float degreesLeftRight, float degreesUpDown, bool invertX = false, bool invertY = false)
        {
            float radiansLeftRight = MathHelper.DegreesToRadians(invertX ? degreesLeftRight : -degreesLeftRight);
            float radiansUpDown = MathHelper.DegreesToRadians(invertY ? degreesUpDown : -degreesUpDown);

            Quaternion yaw = Quaternion.FromAxisAngle(KWEngine.WorldUp, radiansLeftRight);
            Vector3 rotatedVector1 = HelperVector.RotateVectorByQuaternion(Vector3.UnitZ, yaw);
            Vector3 cross = Vector3.Cross(rotatedVector1, KWEngine.WorldUp);
            Quaternion pitch = Quaternion.FromAxisAngle(cross, radiansUpDown);
            Vector3 rotatedVector2 = HelperVector.RotateVectorByQuaternion(rotatedVector1, pitch);
            return pivot + rotatedVector2 * distance;
        }
        
        /// <summary>
        /// Berechnet die Position eines Punkts, der um einen angegeben Punkt entlang einer Achse rotiert wird
        /// </summary>
        /// <param name="point">Mittelpunkt der Rotation</param>
        /// <param name="distance">Distanz zum Mittelpunkt</param>
        /// <param name="degrees">Grad der Rotation</param>
        /// <param name="axis">Achse der Rotation (Standard: Y)</param>
        /// <returns>Position des rotierten Punkts</returns>
        public static Vector3 CalculatePositionAfterRotationAroundPointOnAxis(Vector3 point, float distance, float degrees, Axis axis = Axis.Y)
        {
            float radians = MathHelper.DegreesToRadians(degrees % 360);
            Matrix4.CreateTranslation(point, out translationPointMatrix);

            if (axis == Axis.X)
            {
                Matrix4.CreateRotationX(radians, out rotationMatrix);
                Matrix4.CreateTranslation(0, 0, distance, out translationMatrix);
            }
            else if (axis == Axis.Y)
            {
                Matrix4.CreateRotationY(radians, out rotationMatrix);
                Matrix4.CreateTranslation(0, 0, distance, out translationMatrix);
            }
            else if (axis == Axis.Z)
            {
                Matrix4.CreateRotationZ(radians, out rotationMatrix);
                Matrix4.CreateTranslation(0, distance, 0, out translationMatrix);
            }
            else if(axis == Axis.Camera)
            {
                Vector3 camLookAt = KWEngine.CurrentWorld._cameraGame._stateCurrent.LookAtVector;
                rotationMatrix = HelperMatrix.CreateRotationMatrixForAxisAngle(ref camLookAt, ref radians);
                Vector3 cross = Vector3.Cross(camLookAt, KWEngine.WorldUp);
                Matrix4.CreateTranslation(-cross.X, -cross.Y, -cross.Z, out translationMatrix);
            }

            Matrix4.Mult(translationMatrix, rotationMatrix, out tempMatrix);
            Matrix4.Mult(tempMatrix, translationPointMatrix, out spinMatrix);
            Vector3.TransformPosition(zeroVector, spinMatrix, out finalTranslationPoint);

            return finalTranslationPoint;
        }
        
        
        /// <summary>
        /// Berechnet die Position eines Punkts, der um einen angegeben Punkt entlang einer Achse rotiert wird
        /// </summary>
        /// <param name="point">Mittelpunkt der Rotation</param>
        /// <param name="distance">Distanz zum Mittelpunkt</param>
        /// <param name="degrees">Grad der Rotation</param>
        /// <param name="axis">Achse der Rotation (normalisiert!)</param>
        /// <returns>Position des rotierten Punkts</returns>
        public static Vector3 CalculatePositionAfterRotationAroundPointOnAxis(Vector3 point, float distance, float degrees, Vector3 axis)
        {
            float radians = MathHelper.DegreesToRadians(degrees % 360);
            Matrix4.CreateTranslation(point, out translationPointMatrix);

            rotationMatrix = HelperMatrix.CreateRotationMatrixForAxisAngle(ref axis, ref radians);
            Vector3 cross = Vector3.NormalizeFast(Vector3.Cross(axis, KWEngine.WorldUp));
            Matrix4.CreateTranslation(-cross.X * distance, -cross.Y * distance, -cross.Z * distance, out translationMatrix);
            
            Matrix4.Mult(translationMatrix, rotationMatrix, out tempMatrix);
            Matrix4.Mult(tempMatrix, translationPointMatrix, out spinMatrix);
            Vector3.TransformPosition(zeroVector, spinMatrix, out finalTranslationPoint);

            return finalTranslationPoint;
        }
        

        /// <summary>
        /// Erfragt die Rotation, die nötig wäre, damit eine Quelle zu einem Ziel guckt
        /// </summary>
        /// <param name="source">Quellposition</param>
        /// <param name="target">Zielposition</param>
        /// <returns>Rotation</returns>
        public static Quaternion GetRotationForPoint(Vector3 source, Vector3 target)
        {
            target.X += 0.000001f;
            Matrix3 lookAt = new Matrix3(Matrix4.LookAt(target, source, KWEngine.WorldUp));
            lookAt.Transpose();
            lookAt.Invert();

            return Quaternion.FromMatrix(lookAt);
        }

       

        /// <summary>
        /// Berechnet ein Quaternion aus den übergebenen Achsenrotationen (in Grad).
        /// (Die Rotationsreihenfolge ist Z -> Y -> X)
        /// </summary>
        /// <param name="x">x-Achsenrotation (in Grad)</param>
        /// <param name="y">y-Achsenrotation (in Grad)</param>
        /// <param name="z">z-Achsenrotation (in Grad)</param>
        /// <returns>Kombinierte Rotation als Quaternion-Instanz</returns>
        public static Quaternion GetQuaternionForEulerDegrees(float x, float y, float z)
        {
            Quaternion tmpRotateX = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x));
            Quaternion tmpRotateY = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y));
            Quaternion tmpRotateZ = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z));
            return (tmpRotateZ * tmpRotateY * tmpRotateX);
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
            return HelperVector.GetRotationToMatchSurfaceNormal(lav, surfaceNormal);
        }

        #region internals
        internal static float CalculateRadiansFromDegrees(float degrees)
        {
            return MathHelper.DegreesToRadians(degrees);
        }

        internal static float CalculateDegreesFromRadians(float radiant)
        {
            return MathHelper.RadiansToDegrees(radiant);
        }

        /// <summary>
        /// Gibt die Rotation für ein Objekt an, die es benötigt, um in Richtung der Kamera ausgerichtet zu sein (Billboarding)
        /// </summary>
        /// <returns>Rotation</returns>
        internal static Quaternion GetRotationTowardsCamera()
        {
            Matrix3 vmti = new Matrix3(Matrix4.Invert(Matrix4.Transpose(KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewMatrix)));
            return Quaternion.FromMatrix(vmti);
        }
        #endregion
    }
}
