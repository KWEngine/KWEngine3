using KWEngine3.FontGenerator;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse zur Darstellung von einfachen Textzeilen
    /// </summary>
    public class HUDObjectText : HUDObject
    {
        /// <summary>
        /// Anzahl der maximal je Instanz gleichzeitig verwendbarer Zeichen
        /// </summary>
        public const int MAX_CHARS = 127;

        /// <summary>
        /// Wenn 'true', werden gleiche Abstände zwischen allen Zeichen erzwungen (Standard: false)
        /// </summary>
        public bool ForceMonospace { get; set; } = false;

        /// <summary>
        /// Standardkonstruktor der HUDObjectText-Instanz
        /// </summary>
        /// <param name="text">Darzustellender Text (maximal 256 Zeichen)</param>
        public HUDObjectText(string text)
        {
            SetFont(FontFace.Anonymous);
            SetText(text);
        }

        /// <summary>
        /// Verwendeter Schriftartname des HUD-Objekts
        /// </summary>
        public string Font { get; internal set; } = "Anonymous";

        /// <summary>
        /// Setzt die Schriftart der Instanz
        /// </summary>
        /// <param name="fontFace">zu nutzende Schriftart</param>
        public void SetFont(FontFace fontFace)
        {
            Font = HelperFont.GetNameForInternalFontID((int)fontFace);
            if(KWEngine.FontDictionary.TryGetValue(Font, out KWFont fontTemp))
            {
                _font = KWEngine.FontDictionary[Font];
                CreateBuffers();
                UpdateOffsetList();
            }
            else
            {
                KWEngine.LogWriteLine("[HUDObjectText] Font unknown or invalid");
                _font = KWEngine.FontDictionary["Anonymous"];
            }
        }

        /// <summary>
        /// Setzt die Schriftart anhand des Namens
        /// </summary>
        /// <remarks>Schriftart muss zuvor via KWEngine.LoadFont() importiert worden sein</remarks>
        /// <param name="fontname">Name der Schriftart</param>
        public void SetFont(string fontname)
        {
            if(KWEngine.FontDictionary.TryGetValue(fontname, out KWFont font))
            {
                _font = font;
                UpdateOffsetList();
            }
            else
            {
                KWEngine.LogWriteLine("[HUDObjectText] Font unknown or invalid");
            }
        }

        /// <summary>
        /// Aktueller Modus der Textanordnung
        /// </summary>
        public TextAlignMode TextAlignment { get; internal set; } = TextAlignMode.Left;

        /// <summary>
        /// Setzt den Modus der Textanordnung
        /// </summary>
        /// <param name="textAlignment">neuer Modus</param>
        public void SetTextAlignment(TextAlignMode textAlignment)
        {
            TextAlignment = textAlignment;
        }

        /// <summary>
        /// Erfragt den aktuell für das Objekt festgelegten Text
        /// </summary>
        public string Text { get { return _text; } }

        /// <summary>
        /// Setzt den Text (maximal 127 Zeichen)
        /// </summary>
        /// <param name="text">zu setzender Text</param>
        /// <param name="trim">Leerzeichen zu Beginn und am Ende der Zeichenkette werden abgeschnitten (Standardwert: true)</param>
        public void SetText(string text, bool trim = true)
        {
            if (text != null)
            {
                if (trim)
                    _text = text.Trim();
                else
                    _text = text;

                if(_text.Length > MAX_CHARS + 1)
                {
                    _text = _text.Substring(0, MAX_CHARS + 1);
                }
            }
            else
            {
                _text = "";
            }
            UpdateOffsetList();
        }

        /// <summary>
        /// Verringert oder erhöht die Distanz zwischen Zeichen (Standardwert: 1, gültige Werte von -100 bis +100)
        /// </summary>
        /// <param name="distanceFactor">Distanzfaktor</param>
        public void SetCharacterDistanceFactor(float distanceFactor)
        {
            _spread = Math.Clamp(distanceFactor, -100f, 100f);
            UpdateOffsetList();
        }

        /// <summary>
        /// Gibt den aktuellen Distanzfaktor für die Abstände zwischen Zeichen an
        /// </summary>
        public float CharacterDistanceFactor
        {
            get { return _spread; }
        }

        /// <summary>
        /// Setzt die (optionale) Outline des Texts
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 2)</param>
        /// <param name="g">Grünanteil (0 bis 2)</param>
        /// <param name="b">Blauanteil (0 bis 2)</param>
        public void SetColorOutline(float r, float g, float b)
        {
            _colorOutline.X = Math.Clamp(r, 0f, 2f);
            _colorOutline.Y = Math.Clamp(g, 0f, 2f);
            _colorOutline.Z = Math.Clamp(b, 0f, 2f);
        }

        /// <summary>
        /// Setzt die (optionale) Outline des Texts
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 2)</param>
        /// <param name="g">Grünanteil (0 bis 2)</param>
        /// <param name="b">Blauanteil (0 bis 2)</param>
        /// <param name="width">Outline-Breite (0 bis 1)</param>
        public void SetColorOutline(float r, float g, float b, float width)
        {
            _colorOutline.X = Math.Clamp(r, 0f, 2f);
            _colorOutline.Y = Math.Clamp(g, 0f, 2f);
            _colorOutline.Z = Math.Clamp(b, 0f, 2f);
            _colorOutline.W = Math.Clamp(width * 0.5f, 0f, 0.5f);
        }

        /// <summary>
        /// Gibt die aktuell gesetzte Farbe für die Outline an
        /// </summary>
        public Vector3 ColorOutline
        {
            get
            {
                return new Vector3(_colorOutline.X, _colorOutline.Y, _colorOutline.Z);
            }
        }

        /// <summary>
        /// Gibt die aktuelle Breite für die Outline an
        /// </summary>
        public float ColorOutlineWidth
        {
            get
            {
                return _colorOutline.W > 0f ? _colorOutline.W * 2f : 0f;
            }
        }


        /// <summary>
        /// Setzt die Größe pro Zeichen in Pixeln (Standardwert: 32)
        /// </summary>
        /// <param name="scale">Größe (in Pixeln, Maximalwert: 512)</param>
        public void SetScale(float scale)
        {
            _scale.X = HelperGeneral.Clamp(scale, 0.001f, 512f);
            _scale.Y = _scale.X;
            _scale.Z = 1;
            UpdateOffsetList();
            UpdateMVP();
        }

        /// <summary>
        /// Prüft, ob diese Instanz aktuell auf dem Bildschirm zu sehen ist
        /// </summary>
        /// <returns>true, wenn das Objekt zu sehen ist</returns>
        public override bool IsInsideScreenSpace()
        {
            float left, right, top, bottom;
            top = Position.Y - _scale.Y * 0.5f - _font.Descent * _scale.Y;
            bottom = Position.Y + _scale.Y * 0.5f + _font.Descent * _scale.Y;
            if (TextAlignment == TextAlignMode.Left)
            {
                left = Position.X;
                right = left + _width;
            }
            else if (TextAlignment == TextAlignMode.Center)
            {
                left = Position.X - _width * 0.5f;
                right = left + _width;
            }
            else
            {
                left = Position.X - _width;
                right = left + _width;
            }

            return !(right < 0 || left > KWEngine.Window.Width || bottom < 0 || top > KWEngine.Window.Height);
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger auf dem Textobjekt liegt
        /// </summary>
        /// <returns>true, wenn Mauszeiger über dem Textobjekt liegt</returns>
        public override bool IsMouseCursorOnMe()
        {
            if (KWEngine.Window.IsMouseInWindow && IsInWorld)
            {
                Vector2 mouseCoords = GLWindow.Mouse.Position;
                float left, right, top, bottom;
                top = Position.Y - _scale.Y * 0.5f;
                bottom = Position.Y + _scale.Y * 0.5f;

                if (TextAlignment == TextAlignMode.Left)
                {
                    left = Position.X;
                    right = left + _width;
                }
                else if(TextAlignment == TextAlignMode.Center)
                {
                    left = Position.X - _width * 0.5f;
                    right = left + _width;
                }
                else
                {
                    left = Position.X - _width;
                    right = left + _width;
                }
                return (mouseCoords.X >= left && mouseCoords.X <= right && mouseCoords.Y >= top && mouseCoords.Y <= bottom);
            }
            return false;
        }

        #region Internals
        internal float _spread = 1f;
        internal float[] _uvOffsets = new float[(MAX_CHARS + 1) * 4];
        internal int _textureBuffer = -1;
        internal int _textureBufferTex = -1;
        internal float[] _glyphInfo = new float[(MAX_CHARS + 1) * 4];
        internal float[] _advances = new float[MAX_CHARS + 1];
        internal Vector4 _colorOutline = new Vector4(0f, 0f, 0f, 0f);
        internal string _text = "";
        internal KWFont _font;
        internal float _width = 0f;
        internal float _widthNormalised = 0f;


        internal HUDObjectText()
        {

        }

        internal void UpdateOffsetList()
        {
            //Console.WriteLine("updating for word: " + _text);
            KWFontGlyph space = _font.GetGlyphForCodepoint('_');
            for (int i = 0, j = 0; i < _text.Length; i++, j+=4)
            {
                KWFontGlyph glyph = _font.GetGlyphForCodepoint(_text[i]);
               
                _uvOffsets[j + 0] = glyph.UCoordinate.X;
                _uvOffsets[j + 1] = glyph.UCoordinate.Y;
                _uvOffsets[j + 2] = glyph.UCoordinate.Z;
                _uvOffsets[j + 3] = glyph.UCoordinate.W;

                if(ForceMonospace)
                {
                    float posNext = _advances[i] + space.Advance + ((space.Right - space.Left) * (_spread - 1f));
                    _advances[i + 1] = posNext;
                }
                else
                {
                    //Console.WriteLine(glyph.ToString() + " ADV: " + glyph.Advance);
                    float kerning = 0f;
                    if(i < _text.Length - 1)
                    {
                        kerning = glyph.Kerning[_text[i + 1]];
                    }
                    float posNext = _advances[i] + glyph.Advance + kerning + ((space.Right - space.Left) * (_spread - 1f));
                    _advances[i + 1] = posNext;
                }
                _glyphInfo[i * 4 + 0] = glyph.Left;
                _glyphInfo[i * 4 + 1] = glyph.Right;
                _glyphInfo[i * 4 + 2] = glyph.Top - _font.Descent * 0.5f;
                _glyphInfo[i * 4 + 3] = glyph.Bottom - _font.Descent * 0.5f;

            }
            
            if (_text.Length > 0)
            {
                _widthNormalised = _advances[_text.Length - 1] + (_font.GetGlyphForCodepoint(_text[_text.Length - 1]).Right - _font.GetGlyphForCodepoint(_text[_text.Length - 1]).Left);
                _width = _widthNormalised * _scale.X;
            }
            else
            {
                _widthNormalised = 0f;
                _width = 0f;
            }

            // update tbo:
            GL.BindBuffer(BufferTarget.TextureBuffer, _textureBuffer);
            IntPtr ptr = GL.MapBuffer(BufferTarget.TextureBuffer, BufferAccess.WriteOnly);

            unsafe
            {
                float* dest = (float*)ptr.ToPointer();

                for (int i = 0; i < _text.Length * 4; i++)
                    dest[i] = _glyphInfo[i];
            }

            GL.UnmapBuffer(BufferTarget.TextureBuffer);
            GL.BindBuffer(BufferTarget.TextureBuffer, 0);

            if (this is HUDObjectTextInput)
            {
                (this as HUDObjectTextInput).UpdateCursorOffset();
            }
        }

        internal void CreateBuffers()
        {
            if (_textureBuffer < 0)
            {
                _textureBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.TextureBuffer, _textureBuffer);
                GL.BufferData(BufferTarget.TextureBuffer, 4 * sizeof(float) * (MAX_CHARS + 1), IntPtr.Zero, BufferUsageHint.DynamicDraw);

                _textureBufferTex = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureBuffer, _textureBufferTex);
                GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rgba32f, _textureBuffer);
            }
            UpdateOffsetList();
        }
        internal void DeleteBuffers()
        {
            if (KWEngine.Window._disposed == GLWindow.DisposeStatus.None)
            {
                GL.DeleteBuffers(1, new int[] { _textureBuffer });
                GL.DeleteTextures(1, new int[] { _textureBufferTex });
                _textureBuffer = -1;
                _textureBufferTex = -1;
            }
        }
        #endregion
    }
}
