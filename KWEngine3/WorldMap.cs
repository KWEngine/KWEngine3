using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System.Buffers;


namespace KWEngine3
{
    /// <summary>
    /// Klasse zur Organisation eines Map-Overlays (kann maximal 1024 Gegenstände anzeigen)
    /// </summary>
    public class WorldMap
    {
        internal HUDObjectMap[] _items;
        internal int _indexFree;
        internal Matrix4 _camViewProjection;
        internal Matrix4 _camView;
        internal Matrix4 _camProjection;
        internal Matrix4 _camPreRotation;
        internal ProjectionDirection _direction;
        internal Vector2 _cameraDimensions;
        internal float _near;
        internal float _far;
        internal static Vector4[] _AABBProjectionList;
        internal const int MAXITEMCOUNT = 1024;
        internal bool _drawAsCircle = false;
        internal Vector2i _targetCenter;
        internal Vector2i _targetDimensions;
        internal HUDObjectMap _background;

        internal WorldMap()
        {
            _AABBProjectionList = new Vector4[8];
            _items = new HUDObjectMap[MAXITEMCOUNT];
            for(int i = 0; i < MAXITEMCOUNT; i++)
            {
                _items[i] = new HUDObjectMap();
            }
            _indexFree = 0;
            SetCamera(0, 100, 0, ProjectionDirection.NegativeY, 50, 50, 1, 100);
            SetViewport(KWEngine.Window.Center, 128, 128);
            _camPreRotation = Matrix4.Identity;
            _background = null;
        }

        internal void Reset()
        {
            _indexFree = 0;
        }

        /// <summary>
        /// Löscht das ggf. gesetzte Hintergrundobjekt der Map
        /// </summary>
        public void ResetBackground()
        {
            _background = null;
        }

        /// <summary>
        /// Setzt das Hintergrundbild der Map
        /// </summary>
        /// <param name="filename">Dateiname (inkl. relativem Pfad)</param>
        /// <param name="width">Weite der Map (in Weltkoordinaten)</param>
        /// <param name="height">Höhe der Map (in Weltkoordinaten)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="textureRepeatX">Texturwiederholung in x-Richtung</param>
        /// <param name="textureRepeatY">Texturwiederholung in y-Richtung</param>
        public void SetBackground(string filename, float width, float height, float opacity = 1f, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            _background = CreateHUDObjectMapForBackground(Vector3.Zero, Math.Max(1f, width), Math.Max(1f, height), 0f, Vector3.One, opacity, filename, textureRepeatX, textureRepeatY);
        }

        /// <summary>
        /// Gibt die Position und Größe der Map an
        /// </summary>
        /// <param name="center">Mittelpunkt der Map in Fensterpixelkoordinaten</param>
        /// <param name="width">Breite der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="height">Höhe der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="drawAsCircle">Zeichnet die Map kreisrund statt quadratisch</param>
        public void SetViewport(Vector2i center, int width, int height, bool drawAsCircle = false)
        {
            _drawAsCircle = drawAsCircle;
            _targetCenter = center;
            _targetDimensions = new Vector2i(width, height);
        }

        /// <summary>
        /// Gibt die Position und Größe der Map an
        /// </summary>
        /// <param name="center">Mittelpunkt der Map in Fensterpixelkoordinaten</param>
        /// <param name="width">Breite der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="height">Höhe der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="drawAsCircle">Zeichnet die Map kreisrund statt quadratisch</param>
        public void SetViewport(Vector2 center, int width, int height, bool drawAsCircle = false)
        {
            _drawAsCircle = drawAsCircle;
            _targetCenter = new Vector2i((int)center.X, (int)center.Y);
            _targetDimensions = new Vector2i(width, height);
        }

        /// <summary>
        /// Gibt die Position und Größe der Map an
        /// </summary>
        /// <param name="centerX">Mittelpunkt (x-Achse) der Map in Fensterpixelkoordinaten</param>
        /// <param name="centerY">Mittelpunkt (y-Achse) der Map in Fensterpixelkoordinaten</param>
        /// <param name="width">Breite der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="height">Höhe der Map auf dem Bildschirm (in Pixeln, Mindestwert: 32)</param>
        /// <param name="drawAsCircle">Zeichnet die Map kreisrund statt quadratisch</param>
        public void SetViewport(int centerX, int centerY, int width, int height, bool drawAsCircle = false)
        {
            SetViewport(new Vector2i(centerX, centerY), width, height, drawAsCircle);
        }


        /// <summary>
        /// Setzt die Kameraeinstellungen für die Map-Kamera
        /// </summary>
        /// <param name="position">Position der Kamera</param>
        /// <param name="direction">Blickrichtung der Kamera</param>
        /// <param name="width">Sichtbreite der Kamera (Mindestwert: 1f, Maximalwert: 10000f)</param>
        /// <param name="height">Sichthöhe der Kamera (Mindestwert: 1f, Maximalwert: 10000f)</param>
        /// <param name="near">Naheinstellgrenze (Mindestwert: 1f)</param>
        /// <param name="far">Ferneinstellgrenze (Maximalwert: 10000f)</param>
        public void SetCamera(Vector3 position, ProjectionDirection direction, float width, float height, float near, float far)
        {
            _direction = direction;
            _far = Math.Min(10000, Math.Abs(far));
            _near = Math.Clamp(Math.Abs(near), 1f, far);
            _cameraDimensions = new Vector2(Math.Clamp(width, 1f, 10000f), Math.Clamp(height, 1f, 10000f));
            _camProjection = Matrix4.CreateOrthographic(_cameraDimensions.X, _cameraDimensions.Y, _near, _far);
            _camView = Matrix4.LookAt(
                position,
                position + (direction == ProjectionDirection.NegativeY ? -Vector3.UnitY : _direction == ProjectionDirection.NegativeZ ? -Vector3.UnitZ : Vector3.UnitZ),
                _direction == ProjectionDirection.NegativeY ? -Vector3.UnitZ : Vector3.UnitY
                );
            _camViewProjection = _camView * _camProjection;
        }

        /// <summary>
        /// Rotiert die Kamera um ihre Blickachse gemäß des angegebenen Look-At-Vector
        /// </summary>
        /// <param name="lookAtVector">Look-At-Vector der die Rotation enthält</param>
        public void UpdateCameraRotation(Vector3 lookAtVector)
        {
            if (_direction == ProjectionDirection.NegativeY)
            {
                float dot = Vector3.Dot(Vector3.UnitX, lookAtVector);
                float rotation = (lookAtVector.Z <= 0)
                                 ? (dot) * (MathF.PI * 0.25f)
                                 : (1f - dot) * (MathF.PI * 0.25f);
                Console.WriteLine(rotation);
                _camPreRotation = Matrix4.CreateRotationZ(-rotation);
            }
            else
            {

            }
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen für die Map-Kamera
        /// </summary>
        /// <param name="x">Kameraposition auf der X-Achse</param>
        /// <param name="y">Kameraposition auf der Y-Achse</param>
        /// <param name="z">Kameraposition auf der Z-Achse</param>
        /// <param name="direction">Blickrichtung der Kamera</param>
        /// <param name="width">Sichtbreite der Kamera (Mindestwert: 1f, Maximalwert: 10000f)</param>
        /// <param name="height">Sichtbreite der Kamera (Mindestwert: 1f, Maximalwert: 10000f)</param>
        /// <param name="near">Naheinstellgrenze</param>
        /// <param name="far">Ferneinstellgrenze</param>
        public void SetCamera(float x, float y, float z, ProjectionDirection direction, float width, float height, float near, float far)
        {
            SetCamera(new Vector3(x, y, z), direction, width, height, near, far);
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen (hier nur: Position) für die Map-Kamera
        /// </summary>
        /// <param name="x">Kameraposition auf der X-Achse</param>
        /// <param name="y">Kameraposition auf der Y-Achse</param>
        /// <param name="z">Kameraposition auf der Z-Achse</param>
        public void UpdateCamera(float x, float y, float z)
        {
            SetCamera(x, y, z, _direction, _cameraDimensions.X, _cameraDimensions.Y, _near, _far);
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen (hier nur: Position) für die Map-Kamera
        /// </summary>
        /// <param name="position">Kameraposition</param>
        public void UpdateCamera(Vector3 position)
        {
            UpdateCamera(position.X, position.Y, position.Z);
        }

        /// <summary>
        /// Fügt die aktuelle Projektion der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="go">Zu zeichnendes Objekt</param>
        /// <param name="zIndex">Z-Index für das Objekt</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="colorEmissive">Leuchtfärbung jeder Wert zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="scaleOverride">Wenn > 0, wird die Skalierung des eigentlichen Objekts mit dem angegebenen Wert überschrieben</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(GameObject go, float zIndex, Vector3 color, Vector3 colorEmissive, float emissiveIntensity = 0f, float opacity = 1f, float scaleOverride = 0f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }
            foreach (GameObjectHitbox hb in go._colliderModel._hitboxes)
            {
                if (hb._colliderType == ColliderType.ConvexHull)
                {
                    HUDObjectMap h = _items[_indexFree++];
                    h._scaleOverride = Math.Max(0f, scaleOverride);
                    if(h._scaleOverride > 0f)
                    {
                        Vector3 tmpS = hb.Owner._stateRender._scale;
                        Matrix4 tmp = HelperMatrix.CreateModelMatrix(ref tmpS, ref hb.Owner._stateRender._rotation, ref hb.Owner._stateRender._position);
                        h._modelMatrix = hb._mesh._mapPreTransform * tmp;
                    }
                    else
                    {
                        h._modelMatrix = hb._mesh._mapPreTransform * hb._modelMatrixFinal;
                    }
                    
                    h.SetTextureForMap(texture);
                    h.SetTextureRepeat(textureRepeatX, textureRepeatY);
                    h.SetColor(color.X, color.Y, color.Z, opacity);
                    h.SetColorEmissive(colorEmissive.X, colorEmissive.Y, colorEmissive.Z, emissiveIntensity);
                    h.SetZIndex(Math.Clamp(zIndex, -100f, 100f));
                    
                }
            }
        }

        /// <summary>
        /// Fügt die aktuelle Projektion der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="position">Position des zu zeichnenden Objekts</param>
        /// <param name="width">Breite des zu zeichnenden Objekts</param>
        /// <param name="height">Breite des zu zeichnenden Objekts</param>
        /// <param name="zIndex">Z-Index für das Objekt</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="colorEmissive">Leuchtfärbung jeder Wert zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(Vector3 position, float width, float height, float zIndex, Vector3 color, Vector3 colorEmissive, float emissiveIntensity = 0f, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }

            HUDObjectMap h = _items[_indexFree++];
            if(_direction == ProjectionDirection.NegativeY)
            {
                h._modelMatrix = HelperMatrix.CreateModelMatrixForHUD(position.X, position.Y, position.Z, width, 1f, height);
            }
            else
            {
                h._modelMatrix = HelperMatrix.CreateModelMatrixForHUD(position.X, position.Y, position.Z, width, height, 1f);
            }
            
            h.SetTextureForMap(texture);
            h.SetTextureRepeat(textureRepeatX, textureRepeatY);
            h.SetColor(color.X, color.Y, color.Z, opacity);
            h.SetColorEmissive(colorEmissive.X, colorEmissive.Y, colorEmissive.Z, emissiveIntensity);
            h.SetZIndex(Math.Clamp(zIndex, -100f, 100f));
            h._scaleOverride = 0f;

        }

        internal HUDObjectMap CreateHUDObjectMapForBackground(Vector3 position, float width, float height, float zIndex, Vector3 color, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            HUDObjectMap h = new HUDObjectMap();
            if (_direction == ProjectionDirection.NegativeY)
            {
                h._modelMatrix = HelperMatrix.CreateModelMatrixForHUD(width, 1f, height, position.X, position.Y, position.Z);
            }
            else
            {
                h._modelMatrix = HelperMatrix.CreateModelMatrixForHUD(width, height, 1f, position.X, position.Y, position.Z);
            }

            h.SetTextureForMap(texture);
            h.SetTextureRepeat(textureRepeatX, textureRepeatY);
            h.SetColor(color.X, color.Y, color.Z, opacity);
            h.SetColorEmissive(0f, 0f, 0f, 0f);
            h.SetZIndex(-99f);
            h._scaleOverride = 0f;
            return h;
        }
    }
}
