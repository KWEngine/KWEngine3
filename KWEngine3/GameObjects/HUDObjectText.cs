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
            SetPosition(0, 0);
            SetScale(24, 24);
            SetFont("OpenSans");
            SetText(text);
        }

        /// <summary>
        /// Schriftart des HUD-Objekts
        /// </summary>
        public string Font { get; internal set; } = "undefined font name";

        /// <summary>
        /// Setzt die Schriftart der Instanz
        /// </summary>
        /// <param name="font">zu nutzende Schriftart</param>
        public void SetFont(string font)
        {
            if(KWEngine._fonts.TryGetValue(font, out KWFont kwfont))
            {
                _currentFont = kwfont;
                Font = _currentFont.Name;
            }
            else
            {
                KWEngine.LogWriteLine("[HUDObjectText] Font '" + font + "' invalid");
                _currentFont = KWEngine._fonts["OpenSans"];
                Font = _currentFont.Name;
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
            Update();
        }

        /// <summary>
        /// Verringert oder erhöht die Distanz zwischen Zeichen (Standardwert: 1, gültige Werte von -100 bis +100)
        /// </summary>
        /// <param name="distanceFactor">Distanzfaktor</param>
        public void SetCharacterDistanceFactor(float distanceFactor)
        {
            _charDistanceFactor = Math.Clamp(distanceFactor, -100f, 100f);
        }

        /// <summary>
        /// Gibt den aktuellen Distanzfaktor für die Abstände zwischen Zeichen an
        /// </summary>
        public float CharacterDistanceFactor
        {
            get { return _charDistanceFactor; }
        }

        /// <summary>
        /// Prüft, ob diese Instanz aktuell auf dem Bildschirm zu sehen ist
        /// </summary>
        /// <returns>true, wenn das Objekt zu sehen ist</returns>
        public override bool IsInsideScreenSpace()
        {
            // TODO

            return false;
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger auf dem Textobjekt liegt
        /// </summary>
        /// <returns>true, wenn Mauszeiger über dem Textobjekt liegt</returns>
        public override bool IsMouseCursorOnMe()
        {
            if (KWEngine.Window.IsMouseInWindow && IsInWorld)
            {
                // TODO
            }
            
            return false;
        }

        /// <summary>
        /// Gibt die aktuelle Breite der Instanz in Pixeln an
        /// </summary>
        public float Width { get { return _hitbox.Max.X * _scale.X;  } }

        /// <summary>
        /// Gibt die aktuelle Höhe der Instanz in Pixeln an
        /// </summary>
        public float Height { get { return _hitbox.Max.Y * _scale.Y; } }

        #region Internals

        internal float _charDistanceFactor = 1f;
        internal string _text = "";
        internal int[] _vaos_step1 = new int[256];
        internal int[] _vaos_step2 = new int[256];
        internal KWFont _currentFont;
        internal Box2 _hitbox;
        internal int _vaoIndex = 0;

        internal void Update()
        {
            float width = 0f;
            float height = 0f;
            _vaoIndex = 0;
            foreach(char c in _text)
            {
                KWFontGlyph g = _currentFont.GetGlyphForCodepoint(c);
                if(!g.IsValid)
                {
                    g = _currentFont.GetGlyphForCodepoint(' ');
                }

                width += g.Width + g.Advance.X * _charDistanceFactor;
                if (g.Height > height)
                    height = g.Height;

                _vaos_step1[_vaoIndex] = g.VAO_Step1;
                _vaos_step2[_vaoIndex] = g.VAO_Step2;

                _vaoIndex++;
            }

            _hitbox = new Box2(0f, 0f, width, height);
        }
        #endregion
    }
}
