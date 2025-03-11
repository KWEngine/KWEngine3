using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

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
        internal Vector3 _cameraPosition;
        internal float _near;
        internal float _far;
        internal static Vector4[] _AABBProjectionList;
        internal const int MAXITEMCOUNT = 1024;
        internal bool _drawAsCircle = false;
        internal Vector2i _targetCenter;
        internal Vector2i _targetDimensions;
        internal HUDObjectMap _background;
        internal bool _setupComplete = false;

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

        internal bool CheckIfInsideFrustum(GameObject g)
        {
            float _camDiameter = _cameraDimensions.LengthFast;
            if(_direction == ProjectionDirection.NegativeY)
            {
                float camLeft = _cameraPosition.X - _camDiameter;
                float camRight = _cameraPosition.X + _camDiameter;
                float camTop = _cameraPosition.Z - _camDiameter;
                float camBottom = _cameraPosition.Z + _camDiameter;

                return !(g.AABBRight < camLeft || g.AABBLeft > camRight || g.AABBFront < camTop || g.AABBBack > camBottom);
            }
            else
            {
                float camLeft = _cameraPosition.X - _camDiameter;
                float camRight = _cameraPosition.X + _camDiameter;
                float camTop = _cameraPosition.Y + _camDiameter;
                float camBottom = _cameraPosition.Y - _camDiameter;
                return !(g.AABBRight < camLeft || g.AABBLeft > camRight || g.AABBHigh < camBottom || g.AABBLow > camTop);
            }
        }

        internal bool CheckIfInsideFrustum(TerrainObject t)
        {
            float _camDiameter = _cameraDimensions.LengthFast;
            if (_direction == ProjectionDirection.NegativeY)
            {
                float camLeft = _cameraPosition.X - _camDiameter;
                float camRight = _cameraPosition.X + _camDiameter;
                float camTop = _cameraPosition.Z - _camDiameter;
                float camBottom = _cameraPosition.Z + _camDiameter;
                return !(t._stateCurrent._position.X + t.Width / 2f < camLeft || t._stateCurrent._position.X - t.Width / 2f > camRight || t._stateCurrent._position.Z - t.Depth / 2f < camTop || t._stateCurrent._position.Z + t.Depth / 2f > camBottom);
            }
            else
            {
                return false;
            }
        }

        internal bool CheckIfInsideFrustum(Vector3 p, float x, float y)
        {
            float _camDiameter = _cameraDimensions.LengthFast;
            if (_direction == ProjectionDirection.NegativeY)
            {
                float camLeft = _cameraPosition.X - _camDiameter;
                float camRight = _cameraPosition.X + _camDiameter;
                float camTop = _cameraPosition.Z - _camDiameter;
                float camBottom = _cameraPosition.Z + _camDiameter;
                return !(p.X + x / 2f < camLeft || p.X - x / 2f > camRight || p.Z + y / 2f < camTop || p.Z - y / 2f > camBottom);
            }
            else
            {
                float camLeft = _cameraPosition.X - _camDiameter;
                float camRight = _cameraPosition.X + _camDiameter;
                float camTop = _cameraPosition.Y + _camDiameter;
                float camBottom = _cameraPosition.Y - _camDiameter;
                return !(p.X + x / 2f < camLeft || p.X - x / 2f > camRight || p.Y - y / 2f > camTop || p.Z + y / 2f < camBottom);
            }
        }

        /// <summary>
        /// Aktiviert/Deaktiviert das Overlay der Map
        /// </summary>
        public bool Enabled { get; set; } = false;

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
            _cameraPosition = position;
            _camProjection = Matrix4.CreateOrthographic(_cameraDimensions.X, _cameraDimensions.Y, _near, _far);
            _camView = Matrix4.LookAt(
                position,
                position + (direction == ProjectionDirection.NegativeY ? -Vector3.UnitY : _direction == ProjectionDirection.NegativeZ ? -Vector3.UnitZ : Vector3.UnitZ),
                _direction == ProjectionDirection.NegativeY ? -Vector3.UnitZ : Vector3.UnitY
                );
            _camViewProjection = _camView * _camProjection;
            _setupComplete = true;
        }

        /// <summary>
        /// Rotiert die Kamera um ihre Blickachse gemäß des angegebenen Look-At-Vector
        /// </summary>
        /// <param name="lookAtVector">Look-At-Vector der die Rotation enthält</param>
        public void UpdateCameraRotation(Vector3 lookAtVector)
        {
            if(!_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Please setup map camera first before changing rotation");
                return;
            }

            if (_direction == ProjectionDirection.NegativeY)
            {
                if(lookAtVector.Z <= 0)
                {
                    float rotation = Vector3.Dot(Vector3.UnitX, lookAtVector) * (MathF.PI * 0.5f);
                    _camPreRotation = Matrix4.CreateRotationZ(rotation);
                }
                else
                {
                    float rotation = MathF.PI + Vector3.Dot(-Vector3.UnitX, lookAtVector) * (MathF.PI * 0.5f);
                    _camPreRotation = Matrix4.CreateRotationZ(rotation);
                }
            }
            else
            {
                KWEngine.LogWriteLine("[Map] Camera rotation update not supported for -Z direction");
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
            UpdateCamera(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen (hier nur: Position) für die Map-Kamera
        /// </summary>
        /// <param name="position">Kameraposition</param>
        public void UpdateCamera(Vector3 position)
        {
            if (!_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Please setup map camera first before setting its position");
                return;
            }

            _camView = Matrix4.LookAt(
                position,
                position + (_direction == ProjectionDirection.NegativeY ? -Vector3.UnitY : -Vector3.UnitZ),
                _direction == ProjectionDirection.NegativeY ? -Vector3.UnitZ : Vector3.UnitY
                );
            _camViewProjection = _camView * _camProjection;
        }

        /// <summary>
        /// Fügt die aktuelle Projektion der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="go">Zu zeichnendes Objekt</param>
        /// <param name="zIndex">Z-Index für das Objekt (darf zwischen -2f und +2f liegen)</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="colorEmissive">Leuchtfärbung jeder Wert zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="scaleOverride">Wenn > 0, wird die Skalierung des eigentlichen Objekts mit dem angegebenen Wert überschrieben (deaktiviert zusätzlich die Rotation des Objekts)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(GameObject go, float zIndex, Vector3 color, Vector3 colorEmissive, float emissiveIntensity = 0f, float opacity = 1f, float scaleOverride = 0f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (!Enabled || !_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Map is disabled or setup is incomplete");
                return;
            }

            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }

            if (KWEngine.Mode != EngineMode.Play || !CheckIfInsideFrustum(go))
            {
                return;
            }
            if (scaleOverride > 0f)
            {
                Vector3 pos = go.Center;
                Vector3 scale = new Vector3(scaleOverride);

                HUDObjectMap h = _items[_indexFree++];
                h._modelMatrix =  HelperMatrix.CreateModelMatrixForHUD(ref scale, ref pos);
                h.SetTextureForMap(texture);
                h._go = null;
                h.SetTextureRepeat(textureRepeatX, textureRepeatY);
                h.SetColor(color.X, color.Y, color.Z, opacity);
                h.SetColorEmissive(colorEmissive.X, colorEmissive.Y, colorEmissive.Z, emissiveIntensity);
                h.SetZIndex(Math.Clamp(zIndex, -1.99999f, 1.99999f));
                h._scaleOverride = Math.Max(0f, scaleOverride);
            }
            else
            {
                Vector3 pos = go.Center;
                Vector3 scale;
                if(_direction == ProjectionDirection.NegativeY)
                {
                   scale = new Vector3(go.AABBRight - go.AABBLeft, 1f, go.AABBFront - go.AABBBack);
                }
                else
                {
                    scale = new Vector3(go.AABBRight - go.AABBLeft, go.AABBHigh - go.AABBLow, 1f);
                }

                HUDObjectMap h = _items[_indexFree++];
                h._scaleOverride = Math.Max(0f, scaleOverride);
                h._modelMatrix = HelperMatrix.CreateModelMatrixForHUD(ref scale, ref pos);
                h._go = null;
                h.SetTextureForMap(texture);
                h.SetTextureRepeat(textureRepeatX, textureRepeatY);
                h.SetColor(color.X, color.Y, color.Z, opacity);
                h.SetColorEmissive(colorEmissive.X, colorEmissive.Y, colorEmissive.Z, emissiveIntensity);
                h.SetZIndex(Math.Clamp(zIndex, -1.99999f, 1.99999f));

            }
        }

        /// <summary>
        /// Fügt die aktuelle Projektion der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="go">Zu zeichnendes Objekt</param>
        /// <param name="zIndex">Z-Index für das Objekt (darf zwischen -2f und +2f liegen)</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="colorEmissive">Leuchtfärbung jeder Wert zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void AddAsRealModel(GameObject go, float zIndex, Vector3 color, Vector3 colorEmissive, float emissiveIntensity = 0f, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (!Enabled || !_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Map is disabled or setup not incomplete");
                return;
            }

            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }

            if (KWEngine.Mode != EngineMode.Play || !CheckIfInsideFrustum(go))
            {
                return;
            }

            Vector3 pos = go.Center;
            Vector3 scale;
            if (_direction == ProjectionDirection.NegativeY)
            {
                scale = new Vector3(go.AABBRight - go.AABBLeft, 1f, go.AABBFront - go.AABBBack);
            }
            else
            {
                scale = new Vector3(go.AABBRight - go.AABBLeft, go.AABBHigh - go.AABBLow, 1f);
            }

            HUDObjectMap h = _items[_indexFree++];
            h._scaleOverride = 0f;
            h._go = go;
            h._modelMatrix = go._stateCurrent._modelMatrix;
            h.SetTextureForMap(texture);
            h.SetTextureRepeat(textureRepeatX, textureRepeatY);
            h.SetColor(color.X, color.Y, color.Z, opacity);
            h.SetColorEmissive(colorEmissive.X, colorEmissive.Y, colorEmissive.Z, emissiveIntensity);
            h.SetZIndex(Math.Clamp(zIndex, -1.99999f, 1.99999f));
        }

        /// <summary>
        /// Fügt die Instanz des TerrainObject der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="t">Zu zeichnendes Terrain</param>
        /// <param name="zIndex">Z-Index für das Objekt (darf zwischen -2f und +2f liegen)</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(TerrainObject t, float zIndex, Vector3 color, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (!Enabled || !_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Map is disabled or setup not incomplete");
                return;
            }

            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }

            if (KWEngine.Mode != EngineMode.Play || !CheckIfInsideFrustum(t))
                return;

            HUDObjectMap h = _items[_indexFree++];
            h._scaleOverride = 0f;
            h._go = null;
            h._modelMatrix = HelperMatrix.CreateModelMatrix(t.Width, 1f, t.Depth, ref HelperVector.QIdentity, t._stateCurrent._position.X, t._stateCurrent._position.Y, t._stateCurrent._position.Z);
            h.SetTextureForMap(texture);
            h.SetTextureRepeat(textureRepeatX, textureRepeatY);
            h.SetColor(color.X, color.Y, color.Z, opacity);
            h.SetColorEmissive(0f, 0f, 0f, 0f);
            h.SetZIndex(Math.Clamp(zIndex, -1.99999f, 1.99999f));
        }

        /// <summary>
        /// Fügt die aktuelle Projektion der Map für den aktuellen Frame hinzu
        /// </summary>
        /// <param name="position">Position des zu zeichnenden Objekts</param>
        /// <param name="width">Breite des zu zeichnenden Objekts</param>
        /// <param name="height">Breite des zu zeichnenden Objekts</param>
        /// <param name="zIndex">Z-Index für das Objekt (darf zwischen -2f und +2f liegen)</param>
        /// <param name="color">Färbung (jeder Wert zwischen 0f und 1f)</param>
        /// <param name="colorEmissive">Leuchtfärbung jeder Wert zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="opacity">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(Vector3 position, float width, float height, float zIndex, Vector3 color, Vector3 colorEmissive, float emissiveIntensity = 0f, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (!Enabled || !_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Map is disabled or setup not incomplete");
                return;
            }

            if (_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }

            if (KWEngine.Mode != EngineMode.Play || !CheckIfInsideFrustum(position, width, height))
                return;

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
            h.SetZIndex(Math.Clamp(zIndex, -1.99999f, 1.99999f));
            h._scaleOverride = 0f;
            h._go = null;
        }

        internal HUDObjectMap CreateHUDObjectMapForBackground(Vector3 position, float width, float height, float zIndex, Vector3 color, float opacity = 1f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if (!_setupComplete)
            {
                KWEngine.LogWriteLine("[Map] Map setup is incomplete - cannot create background");
                return null;
            }

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
            h.SetZIndex(-99.9f);
            h._go = null;
            h._scaleOverride = 0f;
            return h;
        }
    }
}
