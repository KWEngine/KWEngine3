namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Oberklasse für alle Eingabe-HUDObject-Klassen
    /// </summary>
    public class HUDObjectTextInput : HUDObjectText
    {
        /// <summary>
        /// Gibt an, ob die Instanz gerade den Eingabefokus besitzt
        /// </summary>
        public bool HasFocus { get; internal set; } = false;

        /// <summary>
        /// Sorgt dafür, dass die Instanz den Eingabefokus erhält
        /// </summary>
        public void GetFocus()
        {
            foreach(HUDObject h in KWEngine.CurrentWorld._hudObjects)
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
            if(_cursorPos > text.Length)
            {
                _cursorPos = text.Length;
            }
            UpdateOffsetList();
        }
        

        /// <summary>
        /// Gibt den Eingabefokus wieder frei
        /// </summary>
        public void ReleaseFocus()
        {
            HasFocus = false;
        }


        /// <summary>
        /// Erfragt oder setzt die Position des Eingabecursors
        /// </summary>
        public int CursorPos
        {
            get
            {
                return _cursorPos;
            }
            set
            {
                _cursorPos = Math.Clamp(value, 0, Text.Length);
            }
        }

        /// <summary>
        /// Fügt die angegebenen Zeichen dem Text ab der Cursorposition hinzu
        /// </summary>
        /// <param name="characters">Hinzuzufügende Zeichen</param>
        public void AddCharacters(string characters)
        {
            if (characters != null)
            {
                if (_cursorPos >= Text.Length)
                {
                    SetText(Text + characters, false);
                }
                else
                {
                    string substringFront = Text.Substring(0, _cursorPos);
                    string substringBack = Text.Substring(_cursorPos);
                    SetText(substringFront + characters + substringBack, false);
                }
            }
        }

        /// <summary>
        /// Erstellt eine Instanz der Klasse mit einem Text
        /// </summary>
        /// <param name="text"></param>
        public HUDObjectTextInput(string text) : base(text)
        {
        }

        #region Internals
        internal int _cursorPos = 0;
        #endregion
    }
}
