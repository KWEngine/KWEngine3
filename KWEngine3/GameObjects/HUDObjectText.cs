using KWEngine3.Helper;
using OpenTK.Mathematics;
using System.Security.Cryptography;

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
        /// Schriftart des HUD-Objekts
        /// </summary>
        public FontFace Font { get; internal set; } = FontFace.Anonymous;

        /// <summary>
        /// Setzt die Schriftart der Instanz
        /// </summary>
        /// <param name="fontFace">zu nutzende Schriftart</param>
        public void SetFont(FontFace fontFace)
        {
            Font = fontFace;
            _textureId = KWEngine.FontTextureArray[(int)Font];
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
        /// Setzt den Text (maximal 256 Zeichen)
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

                if(_text.Length > 256)
                {
                    _text = _text.Substring(0, 256);
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
            if (KWEngine.Window.IsMouseInWindow)
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
        internal int[] _offsets = null;
        internal float _spread = 1f;
        internal string _text = "";
        

        internal void UpdateOffsetList()
        {
            _offsets = new int[_text.Length];
            for (int i = 0; i < _text.Length; i++)
            {
                int offset;
                if (HelperFont.TextToOffsetDict.TryGetValue(_text[i], out offset))
                {
                    // ;-)
                }
                else
                    offset = _text[i] - 32;
                _offsets[i] = offset;
            }
        }
        #endregion
    }
}
