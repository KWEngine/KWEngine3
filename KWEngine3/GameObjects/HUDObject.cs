using OpenTK.Mathematics;
using KWEngine3.Helper;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Art des HUD-Objekts
    /// </summary>
    public enum HUDObjectType { 
        /// <summary>
        /// Bild
        /// </summary>
        Image, 
        /// <summary>
        /// Text
        /// </summary>
        Text }

    /// <summary>
    /// HUD-Klasse
    /// </summary>
    public sealed class HUDObject
    {
        /// <summary>
        /// Gibt an, ob das HUDObject sichtbar ist oder nicht (Standard: true)
        /// </summary>
        public bool IsVisible { get; set; } = true;
        internal Vector2 _absolute = new Vector2(0, 0);
        internal Vector4 _tint = new Vector4(1, 1, 1, 1);
        internal Vector4 _glow = new Vector4(0, 0, 0, 1);
        internal HUDObjectType _type = HUDObjectType.Image;
        internal int _textureId = KWEngine.TextureAlpha;
        internal int[] _offsets = new int[] { 0 };
        internal string _textureName = "";
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; internal set; } = Vector3.Zero;
        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        public World CurrentWorld { get; internal set; } = null;
        internal Vector3 _scale = new Vector3(32f, 32f, 1f);
        internal Quaternion _rotation = Quaternion.Identity;
        internal Matrix4 _rotationMatrix = Matrix4.Identity;
        internal List<Vector3> _positions = new List<Vector3>();
        internal List<Matrix4> _modelMatrices = new List<Matrix4>();
        internal Matrix4 _scaleMatrix = Matrix4.CreateScale(32f,32f, 1f);
        internal string _text = null;
        internal int _count = 1;

        /// <summary>
        /// Name der Instanz
        /// </summary>
        public string Name { get; set; } = "undefined HUD object.";

        /// <summary>
        /// Schriftart des HUD-Objekts
        /// </summary>
        public FontFace Font { get; private set; } = FontFace.Anonymous;

        internal float _spread = 26f;
        /// <summary>
        /// Laufweite der Buchstaben (Standard: 26)
        /// </summary>
        public float CharacterSpreadFactor
        {
            get
            {
                return _spread;
            }
            set
            {
                _spread = HelperGeneral.Clamp(value, 1f, 1024f);
                UpdatePositions();
            }
        }

        /// <summary>
        /// Setzt die Rotation des HUD-Objekts
        /// </summary>
        /// <param name="x">X-Rotation in Grad</param>
        /// <param name="y">Y-Rotation in Grad</param>
        public void SetRotation(float x, float y)
        {
            _rotation = HelperRotation.GetQuaternionForEulerDegrees(x, y, 0);
            _rotationMatrix = Matrix4.CreateFromQuaternion(_rotation);
            UpdatePositions();
        }

        /// <summary>
        /// Färbung des Objekts
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        public void SetColor(float red, float green, float blue)
        {
            _tint.X = HelperGeneral.Clamp(red, 0, 1);
            _tint.Y = HelperGeneral.Clamp(green, 0, 1);
            _tint.Z = HelperGeneral.Clamp(blue, 0, 1);
            _tint.W = 1.0f;
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Objekts
        /// </summary>
        /// <param name="o">Sichtbarkeit (zwischen 0 und 1)</param>
        public void SetOpacity(float o)
        {
            _tint.W = HelperGeneral.Clamp(o, 0, 1);
        }

        /// <summary>
        /// Setzt die Glühfarbe des Objekts
        /// </summary>
        /// <param name="red">Rot (zwischen 0 und 1)</param>
        /// <param name="green">Grün (zwischen 0 und 1)</param>
        /// <param name="blue">Blau (zwischen 0 und 1)</param>
        
        public void SetColorGlow(float red, float green, float blue)
        {
            _glow.X = HelperGeneral.Clamp(red, 0, 1);
            _glow.Y = HelperGeneral.Clamp(green, 0, 1);
            _glow.Z = HelperGeneral.Clamp(blue, 0, 1);
        }

        /// <summary>
        /// Setzt die Glühintensität des Objekts
        /// </summary>
        /// <param name="intensity">Intensität (zwischen 0 und 1)</param>
        public void SetColorGlowIntensity(float intensity)
        {
            _glow.W = HelperGeneral.Clamp(intensity, 0, 1);
        }

        private void UpdateTextures()
        {
            _textureId = KWEngine.FontTextureArray[(int)Font];
            for (int i = 0; i < _count; i++)
            {
                int offset = (int)_text[i] - 32;
                _offsets[i] = offset;
            }
        }

        /// <summary>
        /// Setzt die Größe
        /// </summary>
        /// <param name="width">Breite</param>
        /// <param name="height">Höhe</param>
        public void SetScale(float width, float height)
        {
            _scale.X = HelperGeneral.Clamp(width, 0.001f, float.MaxValue);
            _scale.Y = HelperGeneral.Clamp(height, 0.001f, float.MaxValue);
            _scale.Z = 1;
            _scaleMatrix = Matrix4.CreateScale(_scale);
            UpdatePositions();
        }

        /// <summary>
        /// Setzt den Text
        /// </summary>
        /// <param name="text">Text</param>
        public void SetText(string text)
        {
            if(_type == HUDObjectType.Text && text != null)
            {
                _text = text.Trim();
                _count = _text.Length;
                _offsets = new int[_count];
                UpdatePositions();
                UpdateTextures();
                _textureName = "";
            }
            else
            {
                KWEngine.LogWriteLine("SetText() invalid for HUD images");
                return;
            }
        }

        /// <summary>
        /// Setzt die Textur
        /// </summary>
        /// <param name="filename">Bilddatei</param>
        public void SetTexture(string filename)
        {
            if(filename == null)
            {
                KWEngine.LogWriteLine("[HUDObject] Texture file not found");
                return;
            }
            if (File.Exists(filename) && _type == HUDObjectType.Image)
            {
                if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
                {
                    _textureId = KWEngine.CurrentWorld._customTextures[filename];
                }
                else
                {
                    _textureId = HelperTexture.LoadTextureForBackgroundExternal(filename, out int mipMapLevels);
                    KWEngine.CurrentWorld._customTextures.Add(filename, _textureId);
                }
                _count = 1;
                _textureName = filename.Trim();
            }
            else
            {
                KWEngine.LogWriteLine("SetTexture() invalid for HUD texts");
                return;
            }
        }

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        /// <param name="type">Art des Objekts </param>
        /// <param name="x">Breitenposition</param>
        /// <param name="y">Höhenposition</param>
        public HUDObject(HUDObjectType type, float x, float y)
        {
            _type = type;
            Position = new Vector3(x - KWEngine.Window.ClientSize.X / 2, KWEngine.Window.ClientSize.Y - y - KWEngine.Window.ClientSize.Y / 2, 0);
            _absolute.X = x;
            _absolute.Y = y;
            
            UpdatePositions();
        }

        /// <summary>
        /// Setzt die Position
        /// </summary>
        /// <param name="x">Breite in Pixeln</param>
        /// <param name="y">Höhe in Pixeln</param>
        public void SetPosition(float x, float y)
        {
            Position = new Vector3(x - KWEngine.Window.Size.X / 2, KWEngine.Window.Size.Y - y - KWEngine.Window.Size.Y / 2, 0);
            _absolute.X = x;
            _absolute.Y = y;
            UpdatePositions();
        }

        private void SetPosition(int index, Vector3 pos)
        {
            pos.X = pos.X + (CharacterSpreadFactor * index);
            lock (_positions)
            {
                _positions.Add(pos);
            }
            lock (_modelMatrices)
            {
                _modelMatrices.Add(_scaleMatrix * _rotationMatrix * Matrix4.CreateTranslation(pos));
            }
        }

        internal void UpdatePositions()
        {
            lock (_positions)
            {
                lock (_modelMatrices)
                {
                    _positions.Clear();
                    _modelMatrices.Clear();
                    for (int i = 0; i < _count; i++)
                    {
                        SetPosition(i, Position);
                    }
                }
            }
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger auf dem HUD-Objekt ist
        /// </summary>
        /// <returns>true, wenn die Maus auf dem HUD-Objekt ist</returns>
        public bool IsMouseCursorOnMe()
        {
            if (KWEngine.Window.IsMouseInWindow){
                Vector2 mouseCoords = KWEngine.Window.MousePosition;
                float left, right, top, bottom;

                if(_type == HUDObjectType.Image)
                {
                    left = _absolute.X - _scale.X * 0.5f;
                    right = _absolute.X + _scale.X * 0.5f;

                    top = _absolute.Y - _scale.Y * 0.5f;
                    bottom = _absolute.Y + _scale.Y * 0.5f;
                }
                else
                {
                    left = _absolute.X - _scale.X * 0.5f;
                    right = _absolute.X + ((_count - 1) * _spread) + _scale.X * 0.5f;

                    top = _absolute.Y - _scale.Y * 0.5f;
                    bottom = _absolute.Y + _scale.Y * 0.5f;
                }

                if(mouseCoords.X >= left && mouseCoords.X <= right && mouseCoords.Y >= top && mouseCoords.Y <= bottom)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Setzt die Schriftart der Instanz
        /// </summary>
        /// <param name="fontFace">zu nutzende Schriftart</param>
        public void SetFont(FontFace fontFace)
        {
            Font = fontFace;
        }
    }
}
