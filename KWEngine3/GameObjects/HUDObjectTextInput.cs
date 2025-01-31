using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Oberklasse für alle Eingabe-HUDObject-Klassen
    /// </summary>
    public class HUDObjectTextInput : HUDObjectText
    {

        /// <summary>
        /// Erfragt oder setzt die Zeit, in der nach dem Fokusverlust des Eingabefeldes keine Tastaturbefehle entgegengenommen werden sollen (Standardwert: 0.5f)
        /// </summary>
        public static float TimeoutDuration
        {
            get
            {
                return _timeout;
            }
            set
            {
                if(_timeout >= 0)
                {
                    _timeout = value;
                }
            }
        }

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
        /// <param name="moveCursorToEnd">gibt an, ob der Cursor beim Erlangen des Fokus an das Ende der Zeichenkette gesetzt werden soll (Standard: true)</param> 
        public void GetFocus(bool moveCursorToEnd = true)
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
            _textBeforeAbort = Text;
            if(moveCursorToEnd) MoveCursorToEnd();
            KWEngine.CurrentWorld._hudObjectInputWithFocus = this;
        }

        /// <summary>
        /// Veranlasst die Instanz, den erzwungenen Keyboard-Eingabefokus sofort zu verlassen - ohne ein Event zu generieren
        /// </summary>
        public void ReleaseFocus()
        {
            if (KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
            {
                KWEngine.CurrentWorld._hudObjectInputWithFocus.HasFocus = false;
                KWEngine.CurrentWorld._hudObjectInputWithFocus = null;
            }
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

        internal static float _timeout = 0.5f;
        internal int[] _offsetsCursor = null;
        internal void UpdateCursorOffsetList()
        {
            _offsetsCursor = new int[_text.Length + 1];
            for (int i = 0; i < _cursorPos; i++)
            {
                _offsetsCursor[i] = 0;
            }
            if(CursorType == KeyboardCursorType.Pipe)
            {
                _offsetsCursor[_cursorPos] = '|' - 32;
            }
            else if(CursorType == KeyboardCursorType.Underscore)
            {
                _offsetsCursor[_cursorPos] = '_' - 32;
            }
            else
            {
                _offsetsCursor[_cursorPos] = 102;
            }
            for (int i = _cursorPos + 1; i < _text.Length + 1; i++)
            {
                _offsetsCursor[i] = 0;
            }
        }


        /// <summary>
        /// Gibt den Eingabefokus für den kommenden Simulatiosschritt wieder frei
        /// </summary>
        /// <param name="reason">Gibt optional den Grund für das Verlassen des Fokus an. Dieser wird der Event-Beschreibung angehängt.</param>
        internal void ReleaseFocus(string reason = "")
        {
            WorldEvent e = new WorldEvent(KWEngine.WorldTime, "[HUDObjectTextInput|" + reason + "]", this);
            e.GeneratedByInputFocusLost = true;
            KWEngine.CurrentWorld.AddWorldEvent(e);
        }

        internal void KeepENTERESCKeyPressedForOneSimulationStep()
        {
            if(KWEngine.Window._keyboard._keysPressed.ContainsKey(Keys.Enter))
            {
                KWEngine.Window._keyboard._keysPressed[Keys.Enter].Time = KWEngine.WorldTime + 1.0f;
            }
            else
            {
                KWEngine.Window._keyboard._keysPressed.Add(Keys.Enter, new KeyboardExtState() { OldWorld = false, Time = KWEngine.WorldTime + 1.0f });
            }

            if (KWEngine.Window._keyboard._keysPressed.ContainsKey(Keys.Escape))
            {
                KWEngine.Window._keyboard._keysPressed[Keys.Escape].Time = KWEngine.WorldTime + 1.0f;
            }
            else
            {
                KWEngine.Window._keyboard._keysPressed.Add(Keys.Escape, new KeyboardExtState() { OldWorld = false, Time = KWEngine.WorldTime + 1.0f });
            }
        }

        /// <summary>
        /// Bestätigt die aktuelle Eingabe und erzeugt ein WorldEvent, das in der jeweiligen OnWorldEvent()-Methode der Welt-Klasse abgefangen werden kann
        /// </summary>
        internal void ConfirmAndRaiseWorldEvent()
        {
            ReleaseFocus("CONFIRM");
        }

        /// <summary>
        /// Bricht die aktuelle Eingabe ab und erzeugt ein WorldEvent, das in der jeweiligen OnWorldEvent()-Methode der Welt-Klasse abgefangen werden kann
        /// </summary>
        internal void AbortAndRaiseWorldEvent()
        {
            SetText(_textBeforeAbort);
            ReleaseFocus("ABORT");
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
        /// Bewegt den Cursor an den Anfang des Texts
        /// </summary>
        public void MoveCursorToEnd()
        {
            CursorPosition = Text.Length;
        }

        /// <summary>
        /// Bewegt den Cursor an den Anfang des Texts
        /// </summary>
        public void MoveCursorToStart()
        {
            CursorPosition = 0;
        }

        /// <summary>
        /// Erstellt eine Instanz der Klasse mit einem Text
        /// </summary>
        /// <param name="text">Anzuzeigender Text</param>
        public HUDObjectTextInput(string text) : base(text)
        {
            SetText(text);
            CursorPosition = Text.Length;
            _textBeforeAbort = text;
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

        /// <summary>
        /// Setzt oder erfragt die Anzahl maximal einzugebender Zeichen im Bereich [1, 128] (Standardwert: 128)
        /// </summary>
        public int MaxInputLength
        {
            get
            {
                return _maxInputLength;
            }
            set
            {
                _maxInputLength = Math.Clamp(value, 1, 128);
            }
        }

        #region Internals
        internal int _cursorPos = 0;
        internal int _maxInputLength = 128;
        internal string _textBeforeAbort = "";
        /// <summary>
        /// Fügt die angegebenen Zeichen dem Text ab der Cursorposition hinzu
        /// </summary>
        /// <param name="characters">Hinzuzufügende Zeichen</param>
        internal void AddCharacters(string characters)
        {
            if (characters != null && Text.Length < MaxInputLength)
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
                SetText(substringFront + substringBack, false);
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
                SetText(substringFront + substringBack, false);
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
        #endregion
    }
}
