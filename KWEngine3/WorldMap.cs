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
        internal HUDObjectImage[] _items;
        internal int _indexFree;
        internal Matrix4 _viewProjection;
        internal ProjectionDirection _direction;
        internal float _radius;
        internal float _near;
        internal float _far;
        internal static Vector4[] _AABBProjectionList;
        internal const int MAXITEMCOUNT = 1024;

        internal WorldMap()
        {
            _AABBProjectionList = new Vector4[8];
            _items = new HUDObjectImage[MAXITEMCOUNT];
            for(int i = 0; i < MAXITEMCOUNT; i++)
            {
                _items[i] = new HUDObjectImage();
            }
            _indexFree = 0;
            SetCamera(0, 100, 0, ProjectionDirection.NegativeY, 50, 1, 100);
        }

        internal void Reset()
        {
            _indexFree = 0;
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen für die Map-Kamera
        /// </summary>
        /// <param name="position">Position der Kamera</param>
        /// <param name="direction">Blickrichtung der Kamera</param>
        /// <param name="radius">Sichtradius der Kamera</param>
        /// <param name="near">Naheinstellgrenze</param>
        /// <param name="far">Ferneinstellgrenze</param>
        public void SetCamera(Vector3 position, ProjectionDirection direction, float radius, float near, float far)
        {
            _direction = direction;
            _far = Math.Min(10000, Math.Abs(far));
            _near = Math.Clamp(Math.Abs(near), 1f, far);
            _radius = Math.Clamp(radius, 1f, 10000f);

            Matrix4 projection = Matrix4.CreateOrthographic((KWEngine.Window.Width / (float)KWEngine.Window.Height) * (_radius * 2f), (_radius * 2f), _near, _far);
            Matrix4 view = Matrix4.LookAt(
                position,
                position + (direction == ProjectionDirection.NegativeY ? -Vector3.UnitY : _direction == ProjectionDirection.NegativeZ ? -Vector3.UnitZ : Vector3.UnitZ),
                _direction == ProjectionDirection.NegativeY ? -Vector3.UnitZ : Vector3.UnitY
                );
            _viewProjection = view * projection;
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen für die Map-Kamera
        /// </summary>
        /// <param name="x">Kameraposition auf der X-Achse</param>
        /// <param name="y">Kameraposition auf der Y-Achse</param>
        /// <param name="z">Kameraposition auf der Z-Achse</param>
        /// <param name="direction">Blickrichtung der Kamera</param>
        /// <param name="radius">Sichtradius der Kamera</param>
        /// <param name="near">Naheinstellgrenze</param>
        /// <param name="far">Ferneinstellgrenze</param>
        public void SetCamera(float x, float y, float z, ProjectionDirection direction, float radius, float near, float far)
        {
            SetCamera(new Vector3(x, y, z), direction, radius, near, far);
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen (hier nur: Position) für die Map-Kamera
        /// </summary>
        /// <param name="x">Kameraposition auf der X-Achse</param>
        /// <param name="y">Kameraposition auf der Y-Achse</param>
        /// <param name="z">Kameraposition auf der Z-Achse</param>
        public void SetCamera(float x, float y, float z)
        {
            SetCamera(x, y, z, _direction, _radius, _near, _far);
        }

        /// <summary>
        /// Setzt die Kameraeinstellungen (hier nur: Position) für die Map-Kamera
        /// </summary>
        /// <param name="position">Kameraposition</param>
        public void SetCamera(Vector3 position)
        {
            SetCamera(position.X, position.Y, position.Z);
        }

        /// <summary>
        /// Fügt einen neuen Karteneintrag hinzu, der dann im aktuellen Frame als Overlay gezeichnet wird
        /// </summary>
        /// <param name="position">Position auf dem Bildschirm</param>
        /// <param name="scaleX">Breite</param>
        /// <param name="scaleY">Höhe</param>
        /// <param name="zIndex">Z-Index</param>
        /// <param name="r">Rotfärbung (zwischen 0f und 1f)</param>
        /// <param name="g">Grünfärbung (zwischen 0f und 1f)</param>
        /// <param name="b">Blaufärbung (zwischen 0f und 1f)</param>
        /// <param name="alpha">Sichtbarkeit (zwischen 0f und 1f)</param>
        /// <param name="emissiveR">Rote Leuchtfärbung (zwischen 0f und 1f)</param>
        /// <param name="emissiveG">Grüne Leuchtfärbung (zwischen 0f und 1f)</param>
        /// <param name="emissiveB">Blaue Leuchtfärbung (zwischen 0f und 1f)</param>
        /// <param name="emissiveIntensity">Leuchtintensität (zwischen 0f und 1f)</param>
        /// <param name="texture">Texturdateiname (darf null sein, wenn keine Textur verwendet werden soll)</param>
        /// <param name="textureRepeatX">Texturwiederholung in X-Richtung (Standard: 1f)</param>
        /// <param name="textureRepeatY">Texturwiederholung in Y-Richtung (Standard: 1f)</param>
        public void Add(Vector2i position, int scaleX, int scaleY, float zIndex, float r = 1f, float g = 1f, float b = 1f, float alpha = 1f, float emissiveR = 1f, float emissiveG = 1f, float emissiveB = 1f, float emissiveIntensity = 0f, string texture = null, float textureRepeatX = 1f, float textureRepeatY = 1f)
        {
            if(_indexFree >= _items.Length)
            {
                KWEngine.LogWriteLine("[Map] Map item could not be added - too many items already");
                return;
            }
            HUDObjectImage h = _items[_indexFree++];
            h.SetTextureForMap(texture);
            h.SetTextureRepeat(textureRepeatX, textureRepeatY);
            h.SetScale(scaleX, scaleY);
            h.SetPosition(position.X, position.Y);
            h.SetColor(r, g, b);
            h.SetOpacity(alpha);
            h.SetColorEmissive(emissiveR, emissiveG, emissiveB);
            h.SetColorEmissiveIntensity(emissiveIntensity);
            h._zIndex = Math.Clamp(zIndex, -100f, 100f);
        }

        /// <summary>
        /// Fügt einen neuen Karteneintrag hinzu, der dann im aktuellen Frame als Overlay gezeichnet wird
        /// </summary>
        /// <param name="m"></param>
        public void Add(MapEntry m)
        {
            Add(
                m.Position,
                m.Scale.X,
                m.Scale.Y,
                m.ZIndex,
                m.Color.X,
                m.Color.Y,
                m.Color.Z,
                m.Opacity,
                m.ColorEmissive.X,
                m.ColorEmissive.Y,
                m.ColorEmissive.Z, 
                m.ColorEmissiveIntensity, 
                m.Texture, 
                m.TextureRepeatX, 
                m.TextureRepeatY
                );
        }

        /// <summary>
        /// Erzeugt eine neue MapObject-Instanz
        /// </summary>
        /// <returns>neue Instanz</returns>
        public MapEntry GetNewMapObject()
        {
            return new MapEntry();
        }

        /// <summary>
        /// Ermittelt die Grenzen einer AABB-Hitbox zu der angegebenen Position in normalisierten (frei wählbaren) Bildschirmkoordinaten 
        /// </summary>
        /// <param name="p">zu projizierende Position</param>
        /// <param name="width">Breite des zu projezierenden Kubus</param>
        /// <param name="height">Höhe des zu projezierenden Kubus</param>
        /// <param name="depth">Tiefe des zu projezierenden Kubus</param>
        /// <returns>Grenzwerte des projizierten Objekts in normalisierter Form</returns>
        public ProjectionBounds GetScreenCoordinatesNormalizedFor(Vector3 p, float width, float height, float depth)
        {
            ProjectionBounds screenCoordinates = new ProjectionBounds();

            // get all 8 boundaries of the GameObjects AABB:
            _AABBProjectionList[0] = new Vector4(p.X - width / 2f, p.Y + height / 2f, p.Z - depth / 2f, 1.0f); // left top back
            _AABBProjectionList[1] = new Vector4(p.X + width / 2f, p.Y + height / 2f, p.Z - depth / 2f, 1.0f); // right top back
            _AABBProjectionList[2] = new Vector4(p.X - width / 2f, p.Y - height / 2f, p.Z - depth / 2f, 1.0f); // left bottom back
            _AABBProjectionList[3] = new Vector4(p.X + width / 2f, p.Y - height / 2f, p.Z - depth / 2f, 1.0f); // right bottom back
            _AABBProjectionList[4] = new Vector4(p.X - width / 2f, p.Y + height / 2f, p.Z + depth / 2f, 1.0f); // left top front
            _AABBProjectionList[5] = new Vector4(p.X + width / 2f, p.Y + height / 2f, p.Z + depth / 2f, 1.0f); // right top front
            _AABBProjectionList[6] = new Vector4(p.X - width / 2f, p.Y - height / 2f, p.Z + depth / 2f, 1.0f); // left bottom front
            _AABBProjectionList[7] = new Vector4(p.X + width / 2f, p.Y - height / 2f, p.Z + depth / 2f, 1.0f); // right bottom front

            float top = float.MinValue;
            float bottom = float.MaxValue;
            float left = float.MaxValue;
            float right = float.MinValue;
            float back = float.MinValue;
            float front = float.MaxValue;

            for (int i = 0; i < _AABBProjectionList.Length; i++)
            {
                _AABBProjectionList[i] = Vector4.TransformRow(_AABBProjectionList[i], _viewProjection);
                if (_AABBProjectionList[i].X < left)
                    left = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].X > right)
                    right = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].Y > top)
                    top = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Y < bottom)
                    bottom = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Z < front)
                    front = _AABBProjectionList[i].Z;
                if (_AABBProjectionList[i].Z > back)
                    back = _AABBProjectionList[i].Z;
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
        /// Ermittelt die Grenzen der AABB-Hitbox eines Objekts in normalisierten (frei wählbaren) Bildschirmkoordinaten 
        /// </summary>
        /// <param name="t">zu projizierendes Terrain-Objekt</param>
        /// <returns>Projizierte Grenzwerte in normalisierten Bildschirmkoordinaten</returns>
        public ProjectionBounds GetScreenCoordinatesNormalizedFor(TerrainObject t)
        {
            ProjectionBounds screenCoordinates = new ProjectionBounds();

            // get all 4 boundaries of the TerrainObject AABB:
            _AABBProjectionList[0] = new Vector4(t._stateCurrent._position.X - t.Width / 2, t._stateCurrent._position.Y, t._stateCurrent._position.Z - t.Depth / 2, 1.0f); // left back
            _AABBProjectionList[1] = new Vector4(t._stateCurrent._position.X + t.Width / 2, t._stateCurrent._position.Y, t._stateCurrent._position.Z - t.Depth / 2, 1.0f); // right back
            _AABBProjectionList[2] = new Vector4(t._stateCurrent._position.X - t.Width / 2, t._stateCurrent._position.Y, t._stateCurrent._position.Z + t.Depth / 2, 1.0f); // left front
            _AABBProjectionList[3] = new Vector4(t._stateCurrent._position.X + t.Width / 2, t._stateCurrent._position.Y, t._stateCurrent._position.Z + t.Depth / 2, 1.0f); // right front

            float top = float.MinValue;
            float bottom = float.MaxValue;
            float left = float.MaxValue;
            float right = float.MinValue;
            float back = float.MinValue;
            float front = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                _AABBProjectionList[i] = Vector4.TransformRow(_AABBProjectionList[i], _viewProjection);
                if (_AABBProjectionList[i].X < left)
                    left = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].X > right)
                    right = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].Y > top)
                    top = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Y < bottom)
                    bottom = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Z < front)
                    front = _AABBProjectionList[i].Z;
                if (_AABBProjectionList[i].Z > back)
                    back = _AABBProjectionList[i].Z;
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
        /// Ermittelt die Grenzen der AABB-Hitbox eines Objekts in normalisierten Bildschirmkoordinaten 
        /// </summary>
        /// <param name="g">zu projizierendes Objekt</param>
        /// <returns>Grenzwerte des projizierten Objekts in normalisierter Form</returns>
        public ProjectionBounds GetScreenCoordinatesNormalizedFor(GameObject g)
        {
            ProjectionBounds screenCoordinates = new ProjectionBounds();

            // get all 8 boundaries of the GameObjects AABB:
            _AABBProjectionList[0] = new Vector4(g.AABBLeft, g.AABBHigh, g.AABBBack, 1.0f); // left top back
            _AABBProjectionList[1] = new Vector4(g.AABBRight, g.AABBHigh, g.AABBBack, 1.0f); // right top back
            _AABBProjectionList[2] = new Vector4(g.AABBLeft, g.AABBLow, g.AABBBack, 1.0f); // left bottom back
            _AABBProjectionList[3] = new Vector4(g.AABBRight, g.AABBLow, g.AABBBack, 1.0f); // right bottom back
            _AABBProjectionList[4] = new Vector4(g.AABBLeft, g.AABBHigh, g.AABBFront, 1.0f); // left top front
            _AABBProjectionList[5] = new Vector4(g.AABBRight, g.AABBHigh, g.AABBFront, 1.0f); // right top front
            _AABBProjectionList[6] = new Vector4(g.AABBLeft, g.AABBLow, g.AABBFront, 1.0f); // left bottom front
            _AABBProjectionList[7] = new Vector4(g.AABBRight, g.AABBLow, g.AABBFront, 1.0f); // right bottom front

            float top = float.MinValue;
            float bottom = float.MaxValue;
            float left = float.MaxValue;
            float right = float.MinValue;
            float back = float.MinValue;
            float front = float.MaxValue;

            for (int i = 0; i < _AABBProjectionList.Length; i++)
            {
                _AABBProjectionList[i] = Vector4.TransformRow(_AABBProjectionList[i], _viewProjection);
                if (_AABBProjectionList[i].X < left)
                    left = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].X > right)
                    right = _AABBProjectionList[i].X;
                if (_AABBProjectionList[i].Y > top)
                    top = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Y < bottom)
                    bottom = _AABBProjectionList[i].Y;
                if (_AABBProjectionList[i].Z < front)
                    front = _AABBProjectionList[i].Z;
                if (_AABBProjectionList[i].Z > back)
                    back = _AABBProjectionList[i].Z;
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
        /// Konvertiert normalisierte Projektionsgrenzwerte zu tatsächlichen Bildschirmkoordinaten
        /// </summary>
        /// <param name="bounds">normalisierte Projektionswerte</param>
        /// <param name="scale">Skalierfaktor für die Konvertierung (Standard: 1f)</param>
        /// <param name="offsetX">Verchiebung in X-Richtung</param>
        /// <param name="offsetY">Verchiebung in Y-Richtung</param>
        /// <returns>Pixelkoordinaten</returns>
        public ProjectionBoundsScreen ConvertNormalizedCoordinatesToScreenSpace(ProjectionBounds bounds, float scale = 1f, int offsetX = 0, int offsetY = 0)
        {
            ProjectionBoundsScreen pbs = new ProjectionBoundsScreen(bounds, scale, offsetX, offsetY);
            return pbs;
        }
    }
}
