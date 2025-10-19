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
        /// Spiegelt einen zweidimensionalen Punkt an einer durch zwei zweidimensionale Punkte definierten Geraden
        /// </summary>
        /// <param name="pX">X-Koordinate des zu spiegelnden Punkts</param>
        /// <param name="pY">Y-Koordinate des zu spiegelnden Punkts</param>
        /// <param name="linepoint1X">X-Koordinate von Punkt #1 auf der Geraden</param>
        /// <param name="linepoint1Y">Y-Koordinate von Punkt #1 auf der Geraden</param>
        /// <param name="linepoint2X">X-Koordinate von Punkt #2 auf der Geraden</param>
        /// <param name="linepoint2Y">Y-Koordinate von Punkt #2 auf der Geraden</param>
        /// <returns>Gespiegelter Punkt als Vector2-Instanz</returns>
        public static Vector2 Mirror2DPointOverLine(float pX, float pY, float linepoint1X, float linepoint1Y, float linepoint2X, float linepoint2Y)
        {
            float vx = linepoint2X - linepoint1X;
            float vy = linepoint2Y - linepoint1Y;
            float x = linepoint1X - pX;
            float y = linepoint1Y - pY;
            float r = 1f / (vx * vx + vy * vy);

            float mirrorX = pX + 2f * (x - x * vx * vx * r - y * vx * vy * r);
            float mirrorY = pY + 2f * (y - y * vy * vy * r - x * vx * vy * r);

            return new Vector2(mirrorX, mirrorY);
        }

        /// <summary>
        /// Spiegelt einen zweidimensionalen Punkt an einer durch zwei zweidimensionale Punkte definierten Geraden
        /// </summary>
        /// <param name="p">zu spiegelnder Punkt</param>
        /// <param name="linepoint1">Punkt #1 der Geraden</param>
        /// <param name="linepoint2">Punkt #2 der Geraden</param>
        /// <returns>Gespiegelter Punkt als Vector2-Instanz</returns>
        public static Vector2 Mirror2DPointOverLine(Vector2 p, Vector2 linepoint1, Vector2 linepoint2)
        {
            float vx = linepoint2.X - linepoint1.X;
            float vy = linepoint2.Y - linepoint1.Y;
            float x = linepoint1.X - p.X;
            float y = linepoint1.Y - p.Y;
            float r = 1f / (vx * vx + vy * vy);

            float mirrorX = p.X + 2f * (x - x * vx * vx * r - y * vx * vy * r);
            float mirrorY = p.Y + 2f * (y - y * vy * vy * r - x * vx * vy * r);

            return new Vector2(mirrorX, mirrorY);
        }

        /// <summary>
        /// Prüft, ob die Richtung dieses Vektors hauptsächlich in die positive Y-Achse (also aufwärts) zeigt 
        /// </summary>
        /// <param name="v">zu prüfender Vektor</param>
        /// <returns>true, wenn dieser Vektor hauptsächlich nach oben zeigt</returns>
        public static bool IsVectorPointingUpward(Vector3 v)
        {
            return v.Y > 0 && Math.Abs(v.X) < v.Y && Math.Abs(v.Z) < v.Y;
        }

        /// <summary>
        /// Prüft, ob die Richtung dieses Vektors hauptsächlich in die negative Y-Achse (also abwärts) zeigt 
        /// </summary>
        /// <param name="v">zu prüfender Vektor</param>
        /// <returns>true, wenn dieser Vektor hauptsächlich nach oben zeigt</returns>
        public static bool IsVectorPointingDownward(Vector3 v)
        {
            return v.Y < 0 && Math.Abs(v.X) < -v.Y && Math.Abs(v.Z) < -v.Y;
        }

        /// <summary>
        /// Berechnet den Vektor, der vom angegebenen Startpunkt zum angegebenen Endpunkt zeigt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Delta-Vektor</returns>
        public static Vector3 GetDeltaBetweenVectors( Vector3 start, Vector3 end )
        {
            return end - start;
        }

        /// <summary>
        /// Berechnet den Vektor auf XZ-Ebene, der vom angegebenen Startpunkt zum angegebenen Endpunkt zeigt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Delta-Vektor</returns>
        public static Vector3 GetDeltaBetweenVectorsXZ(Vector3 start, Vector3 end)
        {
            return new Vector3(end.X, 0f, end.Z) - new Vector3(start.X, 0f, start.Z);
        }

        /// <summary>
        /// Berechnet den Vektor auf XY-Ebene, der vom angegebenen Startpunkt zum angegebenen Endpunkt zeigt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Delta-Vektor</returns>
        public static Vector3 GetDeltaBetweenVectorsXY(Vector3 start, Vector3 end)
        {
            return new Vector3(end.X, end.Y, 0f) - new Vector3(start.X, start.Y, 0f);
        }

        /// <summary>
        /// Berechnet die Distanz zwischen zwei Vektoren
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Distanz</returns>
        public static float GetDistanceBetweenVectors( Vector3 start, Vector3 end )
        {
            return GetDeltaBetweenVectors(start, end).LengthFast;
        }

        /// <summary>
        /// Berechnet die Distanz zwischen zwei Vektoren auf der XZ-Ebene
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Distanz</returns>
        public static float GetDistanceBetweenVectorsXZ(Vector3 start, Vector3 end)
        {
            return GetDeltaBetweenVectorsXZ(start, end).LengthFast;
        }

        /// <summary>
        /// Berechnet die Distanz zwischen zwei Vektoren auf der XY-Ebene
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Distanz</returns>
        public static float GetDistanceBetweenVectorsXY(Vector3 start, Vector3 end)
        {
            return GetDeltaBetweenVectorsXY(start, end).LengthFast;
        }

        /// <summary>
        /// Berechnet den normalisierten Richtungsvektor, der vom Startpunkt zum Endpunkt führt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Richtungsvektor (normalisiert)</returns>
        public static Vector3 GetDirectionFromVectorToVector( Vector3 start, Vector3 end )
        {
            return Vector3.NormalizeFast(GetDeltaBetweenVectors(start, end));
        }

        /// <summary>
        /// Berechnet den normalisierten Richtungsvektor, der auf XZ-Ebene vom Startpunkt zum Endpunkt führt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Richtungsvektor (normalisiert)</returns>
        public static Vector3 GetDirectionFromVectorToVectorXZ(Vector3 start, Vector3 end)
        {
            return Vector3.NormalizeFast(GetDeltaBetweenVectorsXZ(start, end));
        }

        /// <summary>
        /// Berechnet den normalisierten Richtungsvektor, der auf XY-Ebene vom Startpunkt zum Endpunkt führt
        /// </summary>
        /// <param name="start">Startpunkt</param>
        /// <param name="end">Endpunkt</param>
        /// <returns>Richtungsvektor (normalisiert)</returns>
        public static Vector3 GetDirectionFromVectorToVectorXY(Vector3 start, Vector3 end)
        {
            return Vector3.NormalizeFast(GetDeltaBetweenVectorsXY(start, end));
        }

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
        /// <param name="surfaceNormal">Ebenenvektor (muss normalisiert sein!)</param>
        /// <returns>Neue zur Ebene passende Rotation (als Quaternion)</returns>
        public static Quaternion GetRotationToMatchSurfaceNormal(Vector3 lav, Vector3 surfaceNormal)
        {
            Vector3 playerRight = Vector3.Cross(-lav, KWEngine.WorldUp);
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
            Quaternion.Invert(rotation, out Quaternion qri);
            Quaternion sourceQ = new(source, 0);
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
        /// Berechnet die normalisierten 2D-Bildschirmkoordinaten für die übergebene GameObject-Instanz.
        /// </summary>
        /// <param name="g">GameObject-Instanz</param>
        /// <returns>2D-Bildschirmkoordinaten als Vector2-Instanz</returns>
        public static Vector2 GetScreenCoordinatesNormalizedFor(GameObject g)
        {
            return GetScreenCoordinatesNormalizedFor(g.Center);
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
            Vector2i screenCoordinates = new();

            Vector4 posClipSpace = Vector4.TransformRow(new Vector4(position, 1f), KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewProjectionMatrix);
            posClipSpace.X /= posClipSpace.W;
            posClipSpace.Y /= posClipSpace.W;

            screenCoordinates.X = (int)((posClipSpace.X * 0.5f + 0.5f) * KWEngine.Window.ClientSize.X);
            screenCoordinates.Y = KWEngine.Window.ClientSize.Y - (int)((posClipSpace.Y * 0.5f + 0.5f) * KWEngine.Window.ClientSize.Y);

            return screenCoordinates;
        }

        /// <summary>
        /// Berechnet die 2D-Bildschirmkoordinaten für die übergebene 3D-Position.
        /// </summary>
        /// <param name="position">Positionsangabe in 3D</param>
        /// <returns>Normalisierte 2D-Bildschirmkoordinaten als Vector2-Instanz</returns>
        public static Vector2 GetScreenCoordinatesNormalizedFor(Vector3 position)
        {
            Vector2 screenCoordinates = new();

            Vector4 posClipSpace = Vector4.TransformRow(new(position, 1f), KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewProjectionMatrix);
            posClipSpace.X /= posClipSpace.W;
            posClipSpace.Y /= posClipSpace.W;

            screenCoordinates.X = posClipSpace.X;
            screenCoordinates.Y = posClipSpace.Y;

            return screenCoordinates;
        }

        /// <summary>
        /// Konvertiert die normalisierten Projektionsgrenzwerte zu tatsächlichen Bildschirmkoordinaten
        /// </summary>
        /// <param name="bounds">normalisierte Projektionswerte</param>
        /// <param name="scale">Skalierfaktor für die Konvertierung (Standard: 1f)</param>
        /// <param name="offsetX">Verchiebung in X-Richtung</param>
        /// <param name="offsetY">Verchiebung in Y-Richtung</param>
        /// <returns>Pixelkoordinaten</returns>
        public static ProjectionBoundsScreen ConvertProjectionBoundsToScreenSpace(ProjectionBounds bounds, float scale = 1f, int offsetX = 0, int offsetY = 0)
        {
            ProjectionBoundsScreen pbs = new ProjectionBoundsScreen(bounds, scale, offsetX, offsetY);
            return pbs;
        }

        /// <summary>
        /// Ermittelt die Grenzen einer AABB-Hitbox zu der angegebenen Position in normalisierten (frei wählbaren) Bildschirmkoordinaten 
        /// </summary>
        /// <param name="p">zu projizierende Position</param>
        /// <param name="width">Breite des zu projezierenden Kubus</param>
        /// <param name="height">Höhe des zu projezierenden Kubus</param>
        /// <param name="depth">Tiefe des zu projezierenden Kubus</param>
        /// <param name="cameraPosition">Kameraposition für die Projektion</param>
        /// <param name="direction">Blickrichtung der Kamera</param>
        /// <param name="radius">Radius der Blickweite (ausgehend von der Kameraposition, gültiger Bereich [1;10000])</param>
        /// <param name="near">Naheinstellgrenze der Kamera (Standard: 1f, gültiger Bereich [1;far - 1])</param>
        /// <param name="far">Blickweite der Kamera in die Ferne (Standard: 100f, gültiger Bereich [near;10000])</param>
        /// <returns>Grenzwerte des projizierten Objekts in normalisierter Form</returns>
        public static ProjectionBounds GetScreenCoordinatesNormalizedFor(Vector3 p, float width, float height, float depth, Vector3 cameraPosition, ProjectionDirection direction, float radius, float near = 1f, float far = 100f)
        {
            far = Math.Min(10000, Math.Abs(far));
            near = Math.Clamp(Math.Abs(near), 1f, far);
            radius = Math.Clamp(radius, 1f, 10000f);
            width = Math.Max(0.01f, width);
            height = Math.Max(0.01f, height);
            depth = Math.Max(0.01f, depth);
            Matrix4 projection = Matrix4.CreateOrthographic((KWEngine.Window.Width / (float)KWEngine.Window.Height) * radius, radius, near, far);
            Matrix4 view = Matrix4.LookAt(
                cameraPosition,
                cameraPosition + (direction == ProjectionDirection.NegativeY ? -Vector3.UnitY : direction == ProjectionDirection.NegativeZ ? -Vector3.UnitZ : Vector3.UnitZ),
                direction == ProjectionDirection.NegativeY ? -Vector3.UnitZ : Vector3.UnitY
                );
            Matrix4 viewProjection = view * projection;

            ProjectionBounds screenCoordinates = new ProjectionBounds();

            // get all 8 boundaries of the GameObjects AABB:
            AABBProjectionList[0] = new Vector4(p.X - width / 2f, p.Y + height / 2f, p.Z - depth / 2f, 1.0f); // left top back
            AABBProjectionList[1] = new Vector4(p.X + width / 2f, p.Y + height / 2f, p.Z - depth / 2f, 1.0f); // right top back
            AABBProjectionList[2] = new Vector4(p.X - width / 2f, p.Y - height / 2f, p.Z - depth / 2f, 1.0f); // left bottom back
            AABBProjectionList[3] = new Vector4(p.X + width / 2f, p.Y - height / 2f, p.Z - depth / 2f, 1.0f); // right bottom back
            AABBProjectionList[4] = new Vector4(p.X - width / 2f, p.Y + height / 2f, p.Z + depth / 2f, 1.0f); // left top front
            AABBProjectionList[5] = new Vector4(p.X + width / 2f, p.Y + height / 2f, p.Z + depth / 2f, 1.0f); // right top front
            AABBProjectionList[6] = new Vector4(p.X - width / 2f, p.Y - height / 2f, p.Z + depth / 2f, 1.0f); // left bottom front
            AABBProjectionList[7] = new Vector4(p.X + width / 2f, p.Y - height / 2f, p.Z + depth / 2f, 1.0f); // right bottom front

            float top = float.MinValue;
            float bottom = float.MaxValue;
            float left = float.MaxValue;
            float right = float.MinValue;
            float back = float.MinValue;
            float front = float.MaxValue;

            for (int i = 0; i < AABBProjectionList.Length; i++)
            {
                AABBProjectionList[i] = Vector4.TransformRow(AABBProjectionList[i], viewProjection);
                if (AABBProjectionList[i].X < left)
                    left = AABBProjectionList[i].X;
                if (AABBProjectionList[i].X > right)
                    right = AABBProjectionList[i].X;
                if (AABBProjectionList[i].Y > top)
                    top = AABBProjectionList[i].Y;
                if (AABBProjectionList[i].Y < bottom)
                    bottom = AABBProjectionList[i].Y;
                if (AABBProjectionList[i].Z < front)
                    front = AABBProjectionList[i].Z;
                if (AABBProjectionList[i].Z > back)
                    back = AABBProjectionList[i].Z;
            }

            screenCoordinates.Top = top;
            screenCoordinates.Bottom = bottom;
            screenCoordinates.Left = left;
            screenCoordinates.Right = right;
            screenCoordinates.Back = back;
            screenCoordinates.Front = front;
            screenCoordinates.Center = new Vector2((left + right) * 0.5f, (top + bottom) * 0.5f);

            return screenCoordinates;
        }

        /// <summary>
        /// Erzeugt einen weichen Übergang des Wertes wenn er innerhalb der Unter-/Obergrenze liegt
        /// </summary>
        /// <param name="lowerBound">Wertuntergrenze</param>
        /// <param name="upperBound">Wertobergrenze</param>
        /// <param name="value">Zu überblendender Wert</param>
        /// <returns>Weichgezeichneter Wert</returns>
        public static float SmoothStep(float lowerBound, float upperBound, float value)
        {
            value = MathHelper.Clamp((value - lowerBound) / (upperBound - lowerBound), 0f, 1f);
            return value * value * (3.0f - 2.0f * value);
        }

        /// <summary>
        /// Erzeugt einen noch weicheren Übergang des Wertes wenn er innerhalb der Unter-/Obergrenze liegt
        /// </summary>
        /// <param name="lowerBound">Wertuntergrenze</param>
        /// <param name="upperBound">Wertobergrenze</param>
        /// <param name="value">Zu überblendender Wert</param>
        /// <returns>Weichgezeichneter Wert</returns>
        public static float SmootherStep(float lowerBound, float upperBound, float value)
        {
            value = MathHelper.Clamp((value - lowerBound) / (upperBound - lowerBound), 0f, 1f);
            return value * value * value * (value * (6.0f * value - 15.0f) + 10.0f);
        }

        /// <summary>
        /// Erzeugt einen weichen Übergang des Wertes wenn er innerhalb der Unter-/Obergrenze liegt
        /// </summary>
        /// <param name="lowerBound">Wertuntergrenze</param>
        /// <param name="upperBound">Wertobergrenze</param>
        /// <param name="value">Zu überblendender Wert</param>
        /// <returns>Weichgezeichneter Wert</returns>
        public static Vector3 SmoothStep(Vector3 lowerBound, Vector3 upperBound, float value)
        {
            return new Vector3(
                SmoothStep(lowerBound.X, upperBound.X, value),
                SmoothStep(lowerBound.Y, upperBound.Y, value),
                SmoothStep(lowerBound.Z, upperBound.Z, value)
                );
        }

        /// <summary>
        /// Erzeugt einen noch weicheren Übergang des Wertes wenn er innerhalb der Unter-/Obergrenze liegt
        /// </summary>
        /// <param name="lowerBound">Wertuntergrenze</param>
        /// <param name="upperBound">Wertobergrenze</param>
        /// <param name="value">Zu überblendender Wert</param>
        /// <returns>Weichgezeichneter Wert</returns>
        public static Vector3 SmootherStep(Vector3 lowerBound, Vector3 upperBound, float value)
        {
            return new Vector3(
                SmootherStep(lowerBound.X, upperBound.X, value),
                SmootherStep(lowerBound.Y, upperBound.Y, value),
                SmootherStep(lowerBound.Z, upperBound.Z, value)
                );
        }


        internal static Vector3 VectorZero = Vector3.Zero;
        internal static Vector4[] AABBProjectionList = new Vector4[8];
        internal static Quaternion QIdentity = Quaternion.Identity;
        internal static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
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

        internal static Vector3 ConvertVector3DToOpenTK(Assimp.Vector3D src)
        {
            return new Vector3(src.X, src.Y, src.Z);
        }

        internal static Vector2 ConvertVector2DToOpenTK(Assimp.Vector2D src)
        {
            return new Vector2(src.X, src.Y);
        }
    }
}
