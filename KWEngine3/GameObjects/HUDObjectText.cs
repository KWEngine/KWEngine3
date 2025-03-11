using KWEngine3.FontGenerator;
using KWEngine3.Helper;
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
                if(fontTemp.IsValid)
                {
                    _font = KWEngine.FontDictionary[Font];
                    UpdateOffsetList();
                }
                else
                {
                    KWEngine.LogWriteLine("[HUDObjectText] Font unknown or invalid");
                    _font = KWEngine.FontDictionary["Anonymous"];
                    UpdateOffsetList();
                }
                
            }
            else
            {
                KWEngine.LogWriteLine("[HUDObjectText] Font unknown or invalid");
                _font = KWEngine.FontDictionary["Anonymous"];
                UpdateOffsetList();
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
                Vector2 mouseCoords = KWEngine.Window.Mouse.Position;
                float left, right, top, bottom;
                top = Position.Y - _scale.Y * 0.5f - _font.Descent * _scale.Y;
                bottom = Position.Y + _scale.Y * 0.5f + _font.Descent * _scale.Y;

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
        internal float[] _uvOffsets = new float[(MAX_CHARS + 1) * 2];
        internal float[] _glyphWidths = new float[MAX_CHARS + 1];
        internal float[] _advances = new float[MAX_CHARS + 1];
        internal string _text = "";
        internal KWFont _font;
        internal float _width = 0f;
        internal float _widthNormalised = 0f;

        internal HUDObjectText()
        {

        }

        internal void UpdateOffsetList()
        {
            KWFontGlyph space = _font.GetGlyphForCodepoint('_');
            for (int i = 0, j = 0; i < _text.Length; i++, j+=2)
            {
                KWFontGlyph glyph = _font.GetGlyphForCodepoint(_text[i]);
               
                _uvOffsets[j + 0] = glyph.UCoordinate.X;
                _uvOffsets[j + 1] = glyph.UCoordinate.Y;

                float previousBearing = i == 0 ? 0 : _font.GetGlyphForCodepoint(_text[i - 1]).Bearing;
                float previousAdvance = i == 0 ? 0 : _font.GetGlyphForCodepoint(_text[i - 1]).Advance;

                _glyphWidths[i] = glyph.Width;
                if(ForceMonospace)
                {
                    _advances[i + 1] = _advances[i] + space.Advance - space.Bearing + (space.Width * (_spread - 1f));
                }
                else
                {
                    _advances[i + 1] = _advances[i] + glyph.Advance - glyph.Bearing + (space.Width * (_spread - 1f));
                }
            }
            
            if (_text.Length > 0)
            {
                _widthNormalised = _advances[_text.Length - 1] + _font.GetGlyphForCodepoint(_text[_text.Length - 1]).Width;
                _width = _widthNormalised * _scale.X;
            }
            else
            {
                _widthNormalised = 0f;
                _width = 0f;
            }

            if(this is HUDObjectTextInput)
            {
                (this as HUDObjectTextInput).UpdateCursorOffset();
            }
        }
        #endregion
    }
}
