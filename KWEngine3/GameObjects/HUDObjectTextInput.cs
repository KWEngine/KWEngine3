

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Oberklasse für alle Eingabe-HUDObject-Klassen
    /// </summary>
    public class HUDObjectTextInput : HUDObjectText
    {
        /// <summary>
        /// Regelt das Blinkverhalten des Eingabecursors
        /// </summary>
        public KeyboardCursorBehaviour CursorBehaviour { get; set; } = KeyboardCursorBehaviour.Blink;

        /// <summary>
        /// Gibt an, ob die Instanz gerade den Eingabefokus besitzt
        /// </summary>
        public bool HasFocus { get; internal set; } = false;

        /// <summary>
        /// Sorgt dafür, dass die Instanz den Eingabefokus erhält
        /// </summary>
        public void GetFocus()
        {
            KWEngine.CurrentWorld._hudObjectInputWithFocus = null;

            foreach (HUDObject h in KWEngine.CurrentWorld._hudObjects)
            {
                if(h is HUDObjectTextInput)
                {
                    if (h != this)
                    {
                        (h as HUDObjectTextInput).HasFocus = false;
                    }
                }
            }
            HasFocus = true;
            KWEngine.CurrentWorld._hudObjectInputWithFocus = this;
        }

        /// <summary>
        /// Setzt den Text für die Instanz
        /// </summary>
        /// <param name="text">zu setzender Text</param>
        /// <param name="trim">wenn true, werden Leerzeichen zu Beginn und Ende des Texts automatisch abgeschnitten</param>
        public new void SetText(string text, bool trim = true)
        {
            if (text != null)
            {
                if (trim)
                    _text = text.Trim();
                else
                    _text = text;

                if (_text.Length > 256)
                {
                    _text = _text.Substring(0, 256);
                }
            }
            else
            {
                _text = "";
            }
            if(CursorPosition > text.Length)
            {
                CursorPosition = text.Length;
            }
            UpdateOffsetList();
        }


        internal int[] _offsetsCursor = null;
        internal void UpdateCursorOffsetList()
        {
            _offsetsCursor = new int[_text.Length + 1];
            for (int i = 0; i < _cursorPos; i++)
            {
                _offsetsCursor[i] = 0;
            }
            _offsetsCursor[_cursorPos] = CursorType == KeyboardCursorType.Pipe ? '|' - 32 : '_' - 32;
            for (int i = _cursorPos + 1; i < _text.Length + 1; i++)
            {
                _offsetsCursor[i] = 0;
            }
        }


        /// <summary>
        /// Gibt den Eingabefokus wieder frei
        /// </summary>
        public void ReleaseFocus()
        {
            HasFocus = false;
            WorldEvent e = new WorldEvent(KWEngine.WorldTime, "[HUDObjectTextInput]", this);
            e.GeneratedByInputFocusLost = true;
            KWEngine.CurrentWorld.AddWorldEvent(e);
        }


        /// <summary>
        /// Erfragt oder setzt die Position des Eingabecursors
        /// </summary>
        public int CursorPosition
        {
            get
            {
                return _cursorPos;
            }
            set
            {
                _cursorPos = Math.Clamp(value, 0, Text.Length);
                UpdateCursorOffsetList();
            }
        }

        /// <summary>
        /// Erstellt eine Instanz der Klasse mit einem Text
        /// </summary>
        /// <param name="text">Anzuzeigender Text</param>
        public HUDObjectTextInput(string text) : base(text)
        {
            SetText(text);
        }

        /// <summary>
        /// Regelt den Anzeigemodus für den Eingabecursor bei Keyboardeingaben (Standard: Pipe)
        /// </summary>
        public KeyboardCursorType CursorType 
        {
            get
            {
                return _cursorType;
            }
            set
            {
                _cursorType = value;
                UpdateCursorOffsetList();
            }
        }

        /// <summary>
        /// Gibt die Blinkgeschwindigkeit des Eingabecursors an
        /// </summary>
        public float CursorBlinkSpeed
        {
            get { return _cursorBlinkSpeed; }
            set
            {
                _cursorBlinkSpeed = Math.Max(0.001f, value);
            }
        }

        #region Internals
        internal int _cursorPos = 0;
        /// <summary>
        /// Fügt die angegebenen Zeichen dem Text ab der Cursorposition hinzu
        /// </summary>
        /// <param name="characters">Hinzuzufügende Zeichen</param>
        internal void AddCharacters(string characters)
        {
            if (characters != null)
            {
                if (_cursorPos >= Text.Length)
                {
                    SetText(Text + characters, false);
                    CursorPosition = Text.Length;
                }
                else
                {
                    string substringFront = Text.Substring(0, _cursorPos);
                    string substringBack = Text.Substring(_cursorPos);
                    SetText(substringFront + characters + substringBack, false);
                    CursorPosition += characters.Length;
                }
            }
        }

        internal void Backspace()
        {
            if (_cursorPos > 0)
            {
                string substringFront = Text.Substring(0, _cursorPos - 1);
                string substringBack = Text.Substring(_cursorPos);
                SetText(substringFront + substringBack);
                if (CursorPosition != Text.Length)
                {
                    MoveCursor(-1);
                }
                UpdateCursorOffsetList();
            }
        }

        internal KeyboardCursorType _cursorType = KeyboardCursorType.Pipe;

        internal float _cursorBlinkSpeed = 1f;

        internal void Delete()
        {
            if (_cursorPos < Text.Length)
            {
                string substringFront = Text.Substring(0, _cursorPos);
                string substringBack = Text.Substring(_cursorPos + 1);
                SetText(substringFront + substringBack);
                UpdateCursorOffsetList();
            }
        }

        /// <summary>
        /// Bewegt den Cursor relativ zu seiner jetztigen Position
        /// </summary>
        /// <param name="offset"></param>
        internal void MoveCursor(int offset)
        {
            CursorPosition = CursorPosition + offset;
        }

        internal void MoveCursorToEnd()
        {
            CursorPosition = Text.Length;
        }

        internal void MoveCursorToStart()
        {
            CursorPosition = 0;
        }
        #endregion
    }
}
