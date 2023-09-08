using System;
using System.Collections.Generic;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse zur Instanziierung von 3D-Textobjekten in der Welt
    /// </summary>
    public class TextObject : IComparable<TextObject>
    {
        /// <summary>
        /// Name der Instanz
        /// </summary>
        public string Name { 
            get 
            { 
                return _name; 
            } 
            set
            {
                if(value != null && value.Trim().Length > 0)
                {
                    _name = value.Trim();
                }
            }
        }

        /// <summary>
        /// Aktueller Text der Instanz
        /// </summary>
        public string Text { get { return new string(_text.ToArray()); } }

        /// <summary>
        /// Erzeugt eine neue Instanz der Klasse TextObject zur Darstellung von 3D-Text
        /// </summary>
        public TextObject()
        {
            InitStates();
            SetText("o_O");
        }

        /// <summary>
        /// Erzeugt eine neue Instanz der Klasse TextObject zur Darstellung von 3D-Text
        /// </summary>
        /// <param name="text">Anzuzeigender Text</param>
        public TextObject(string text)
        {
            InitStates();
            SetText(text);
        }

        /// <summary>
        /// Setzt den anzuzeigenden Text der Instanz
        /// </summary>
        /// <param name="text">Anzuzeigender Text</param>
        public void SetText(string text)
        {
            if (text == null)
                return;

            text = text.Trim();
            if (text.Length > 128)
                text = text.Substring(0, 128);

            _text = text.ToList();
            GenerateOffsetsForText();
        }

        /// <summary>
        /// Gibt an, wie weit die Textzeichen voneinander entfernt liegen sollen (Standard: 1)
        /// </summary>
        /// <param name="f">Entfernungsfaktor (Werte zwischen 0.75f und 2f)</param>
        public void SetCharacterSpreadFactor(float f)
        {
            _stateCurrent._spreadFactor = Math.Clamp(f, 0.75f, 2f);
            UpdateWidth();
        }

        /// <summary>
        /// Setzt die Position der Instanz
        /// </summary>
        /// <param name="x">x-Koordinate</param>
        /// <param name="y">y-Koordinate</param>
        /// <param name="z">z-Koordinate</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Position der Instanz
        /// </summary>
        /// <param name="p">xyz-Koordinaten</param>
        public void SetPosition(Vector3 p)
        {
            _stateCurrent._position = p;
        }

        /// <summary>
        /// Erfragt die aktuelle Position der Instanz
        /// </summary>
        public Vector3 Position { get { return _stateCurrent._position; } }

        /// <summary>
        /// Setzt die Orientierung der Instanz
        /// </summary>
        /// <param name="x">Drehung um lokale x-Achse (in Grad)</param>
        /// <param name="y">Drehung um lokale y-Achse (in Grad)</param>
        /// <param name="z">Drehung um lokale z-Achse (in Grad)</param>
        public void SetRotation(float x, float y, float z)
        {
            SetRotation(HelperRotation.GetQuaternionForEulerDegrees(x, y, z));
        }

        /// <summary>
        /// Setzt die Orientierung der Instanz
        /// </summary>
        /// <param name="q">3-Achsen-Rotation in Form eines Quaternion</param>
        public void SetRotation(Quaternion q)
        {
            _stateCurrent._rotation = q;
        }

        /// <summary>
        /// Setzt die Größe der Instanz (Standard: 1)
        /// </summary>
        /// <param name="s">Größenangabe</param>
        public void SetScale(float s)
        {
            if (s <= 0)
                s = 1;
            _stateCurrent._scale = s;
            UpdateWidth();
        }

        /// <summary>
        /// Setzt die anzuzeigende Schriftart
        /// </summary>
        /// <param name="font">Name der zu benutzenden Schriftart</param>
        public void SetFont(FontFace font)
        {
            _fontFace = font;
        }

        /// <summary>
        /// Erfragt die aktuell gewählte Farbe der Instanz
        /// </summary>
        public Vector3 Color { get { return _stateCurrent._color.Xyz; } }

        /// <summary>
        /// Setzt die Farbe der Instanz
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._color.X = Math.Clamp(r, 0f, 1f);
            _stateCurrent._color.Y = Math.Clamp(g, 0f, 1f);
            _stateCurrent._color.Z = Math.Clamp(b, 0f, 1f);
        }

        /// <summary>
        /// Steuert die Transparenz der Instanz
        /// </summary>
        /// <param name="opacity">Transparenzwert (0 = voll transparent, 1 = voll sichtbar)</param>
        public void SetOpacity(float opacity)
        {
            _stateCurrent._color.W = Math.Clamp(opacity, 0f, 1f);
        }

        /// <summary>
        /// Erfragt die aktuelle selbstleuchtende Farbe der Instanz
        /// </summary>
        public Vector4 ColorEmissive { get { return _stateCurrent._colorEmissive; } }

        /// <summary>
        /// Setzt die selbstleuchtende Farbe der Instanz
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        /// <param name="intensity">Intensität (zwischen 0 und 2)</param>
        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive.X = Math.Clamp(r, 0f, 1f);
            _stateCurrent._colorEmissive.Y = Math.Clamp(g, 0f, 1f);
            _stateCurrent._colorEmissive.Z = Math.Clamp(b, 0f, 1f);
            _stateCurrent._colorEmissive.W = Math.Clamp(intensity, 0f, 2f);
        }

        /// <summary>
        /// Vergleicht das Objekt bzgl. seiner Entfernung zur Kamera mit einem anderen Objekt
        /// </summary>
        /// <param name="other">anderes Objekt</param>
        /// <returns>1, wenn das aufrufende Objekt näher an der Kamera ist, sonst -1</returns>
        public int CompareTo(TextObject other)
        {
            Vector3 camPos = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateCurrent._position : KWEngine.CurrentWorld.CameraPosition;

            float distanceToCameraThis = (this._stateCurrent._position - camPos).LengthSquared;
            float distanceToCameraOther = (other._stateCurrent._position - camPos).LengthSquared;
            return distanceToCameraOther > distanceToCameraThis ? 1 : -1;
        }

        /// <summary>
        /// Methode, die von der Engine automatisch aufgerufen wird, um das Objekt zu aktualisieren
        /// </summary>
        public virtual void Act()
        {

        }

        #region internals
        internal TextObjectState _stateCurrent;
        internal TextObjectState _statePrevious;
        internal TextObjectState _stateRender;

        internal List<char> _text = new List<char>();
        internal List<int> _offsets = new List<int>();
        internal FontFace _fontFace = FontFace.Anonymous;
        internal string _name = "undefined text object";

        internal void InitStates()
        {
            _stateCurrent = new TextObjectState(this);
            _stateRender = new TextObjectState(this);
            _statePrevious = _stateCurrent;
        }

        internal void GenerateOffsetsForText()
        {
            _offsets.Clear();
            foreach(char c in _text)
            {
                _offsets.Add(Math.Max(0, c - 32));
            }
            UpdateWidth();
        }

        internal void UpdateWidth()
        {
            _stateCurrent._width = _stateCurrent._scale * _offsets.Count * _stateCurrent._spreadFactor;
        }
        #endregion
    }
}
