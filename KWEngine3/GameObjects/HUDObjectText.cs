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
            _font = KWEngine.FontDictionary[Font];
            _textureId = _font.Texture;
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
        /// Setzt den Text (maximal 128 Zeichen)
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

                if(_text.Length > 128)
                {
                    _text = _text.Substring(0, 128);
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
        }

        /// <summary>
        /// Gibt den aktuellen Distanzfaktor für die Abstände zwischen Zeichen an
        /// </summary>
        public float CharacterDistanceFactor
        {
            get { return _spread; }
        }

        /// <summary>
        /// Setzt die Größe pro Zeichen (in Pixeln)
        /// </summary>
        /// <param name="scale">Größe (in Pixeln)</param>
        public void SetScale(float scale)
        {
            _scale.X = HelperGeneral.Clamp(scale, 0.001f, 4096f);
            _scale.Y = _scale.X;
            _scale.Z = 1;
            UpdateMVP();
        }

        /// <summary>
        /// Prüft, ob diese Instanz aktuell auf dem Bildschirm zu sehen ist
        /// </summary>
        /// <returns>true, wenn das Objekt zu sehen ist</returns>
        public override bool IsInsideScreenSpace()
        {
            float left, right, top, bottom;
            float characterWidth = _scale.X;

            left = Position.X;
            right = Position.X + (_text.Length - 1) * Math.Abs(_spread) * characterWidth + characterWidth;
            float diff = right - left;

            if (TextAlignment == TextAlignMode.Left)
            {
                if (_spread < 0)
                {
                    left -= (diff - characterWidth);
                    right -= (diff - characterWidth);
                }
            }
            if (TextAlignment == TextAlignMode.Center)
            {
                left -= diff / 2f;
                right -= diff / 2f;
            }
            else if (TextAlignment == TextAlignMode.Right)
            {
                if (_spread >= 0)
                {
                    right -= diff;
                    left -= diff;
                }
                else
                {
                    right -= characterWidth;
                    left -= characterWidth;
                }
            }
            top = Position.Y - _scale.Y * 0.5f;
            bottom = Position.Y + _scale.Y * 0.5f;

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
                float characterWidth = _scale.X;

                left = Position.X;
                right = Position.X + (_text.Length - 1) * Math.Abs(_spread) * characterWidth + characterWidth;
                float diff = right - left;
               
                if(TextAlignment == TextAlignMode.Left)
                {
                    if(_spread < 0)
                    {
                        left -= (diff - characterWidth);
                        right -= (diff - characterWidth);
                    }
                }
                if (TextAlignment == TextAlignMode.Center)
                {
                    left -= diff / 2f;
                    right -= diff / 2f;
                }
                else if(TextAlignment == TextAlignMode.Right)
                {
                    if (_spread >= 0)
                    {
                        right -= diff;
                        left -= diff;
                    }
                    else
                    {
                        right -= characterWidth;
                        left -= characterWidth;
                    }
                }
                top = Position.Y - _scale.Y * 0.5f;
                bottom = Position.Y + _scale.Y * 0.5f;

                return (mouseCoords.X >= left && mouseCoords.X <= right && mouseCoords.Y >= top && mouseCoords.Y <= bottom);
            }
            
            return false;
        }

        #region Internals
        internal int _textureId = (int)FontFace.Anonymous;
        internal float[] _uvOffsets = new float[128 * 2];
        internal float[] _advances = new float[128];
        internal float _spread = 1f;
        internal string _text = "";
        internal KWFont _font;
        

        internal void UpdateOffsetList()
        {
            for (int i = 0, j = 0; i < _text.Length; i++, j+=2)
            {
                KWFontGlyph glyph = _font.GetGlyphForCodepoint(_text[i]);
               
                _uvOffsets[j + 0] = glyph.UCoordinate.X;
                _uvOffsets[j + 1] = glyph.UCoordinate.Y;

                if(i < 128 - 1)
                {
                    _advances[i + 1] = _advances[i] + glyph.Advance.X + glyph.Width;
                }
            }

        }
        #endregion
    }
}
