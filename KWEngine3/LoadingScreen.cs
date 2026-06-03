using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3
{
    /// <summary>
    /// Klasse, die für die Darstellung des Loading Screens zuständig ist
    /// </summary>
    /// <remarks>Die Verwendung des Loading Screens ist optional</remarks>
    public sealed class LoadingScreen
    {
        /// <summary>
        /// Setzt die Hintergrundfarbe des Ladebildschirms
        /// </summary>
        /// <param name="r">Rotanteil (0f bis 1f)</param>
        /// <param name="g">Grünanteil (0f bis 1f)</param>
        /// <param name="b">Blauanteil (0f bis 1f)</param>
        public void SetBackgroundFillColor(float r, float g, float b)
        {
            _bgFill.SetColor(r, g, b);
        }

        /// <summary>
        /// Setzt die Hintergrundtextur
        /// </summary>
        /// <param name="filename">Dateiname</param>
        /// <param name="repeatX">Horizontale Wiederholungen der Textur</param>
        /// <param name="repeatY">Vertikale Wiederholungen der Textur</param>
        public void SetBackgroundTexture(string filename, float repeatX = 1f, float repeatY = 1f)
        {
            if (_bg == null) return;

            _bg.SetTexture(filename);
            if (!_textureKeys.Contains(filename))
                _textureKeys.Add(filename);

            int sX = _bg.Scale.X;
            int sY = _bg.Scale.Y;
            int wWidth = KWEngine.Window.Width;
            int wHeight = KWEngine.Window.Height;

            float scaleX = (float)wWidth / sX;
            float scaleY = (float)wHeight / sY;
            float scale = Math.Max(scaleX, scaleY);

            _bg.SetScale(sX * scale, sY * scale);
            _bg.SetTextureRepeat(repeatX, repeatY);
        }

        /// <summary>
        /// Setzt die Textur für das Ladebildschirmzeichen
        /// </summary>
        /// <param name="filename">Dateiname</param>
        /// <param name="columns">Falls es ein Spritesheet ist, Anzahl der Spalten</param>
        /// <param name="rows">Falls es ein Spritesheet ist, Anzahl der Reihen</param>
        public void SetIconTexture(string filename, int columns = 1, int rows = 1)
        {
            if (_icon == null) return;

            _icon.SetTexture(filename);

            if (!_textureKeys.Contains(filename))
                _textureKeys.Add(filename);

            _textureIconSpriteSheetDimensions.X = Math.Max(columns, 1);
            _textureIconSpriteSheetDimensions.Y = Math.Max(rows, 1);
            _icon.SetTextureRepeat(1f / _textureIconSpriteSheetDimensions.X, 1f / _textureIconSpriteSheetDimensions.Y);

            _icon.SetScale(_icon.Scale.X * 1f / _textureIconSpriteSheetDimensions.X, _icon.Scale.Y * 1f / _textureIconSpriteSheetDimensions.Y);
        }

        /// <summary>
        /// Setzt die Position des Zeichens
        /// </summary>
        /// <param name="x">Horizontaler Offset in Pixeln</param>
        /// <param name="y">Verzikaler Offset in Pixeln</param>
        public void SetIconPosition(int x, int y)
        {
            if (_icon == null) return;

            _icon.SetPosition(x, y);
        }

        /// <summary>
        /// Setzt den Skalierungsfaktor des Zeichens
        /// </summary>
        /// <param name="s">Faktor (z.B. 2.0f für zweifache Vergrößerung)</param>
        public void SetIconScaleFactor(float s)
        {
            if (_icon == null) return;

            _icon.SetScale(_icon.Scale.X * s, _icon.Scale.Y * s);
        }

        /// <summary>
        /// Zentriert das Zeichen auf dem Bildschirm
        /// </summary>
        public void SetIconPositionToWindowCenter()
        {
            if (_icon == null) return;

            _icon.CenterOnScreen();
        }

        /// <summary>
        /// Falls das Ladebildschirmzeichen eine Spritesheet-Textur hat, kann man hier angeben, welcher Index gerade gerendert werden soll
        /// </summary>
        /// <param name="column">Nullbasierter Spaltenindex</param>
        /// <param name="row">Nullbasierter Reihenindex</param>
        public void SetIconSpriteSheetIndex(int column, int row)
        {
            if(_icon._textureId >= 0 && 
                _icon._textureId != KWEngine.TextureAlpha &&
                _icon._textureId != KWEngine.TextureWhite &&
                column < _textureIconSpriteSheetDimensions.X &&
                row < _textureIconSpriteSheetDimensions.Y
            )
            {
                _textureIconSpriteSheetIndices.X = Math.Clamp(column, 0, _textureIconSpriteSheetDimensions.X - 1);
                _textureIconSpriteSheetIndices.Y = Math.Clamp(row, 0, _textureIconSpriteSheetDimensions.Y - 1);
            }
            else
            {
                KWEngine.LogWriteLine("[LoadingScreen] Invalid sprite sheet indices");
            }
        }

        /// <summary>
        /// Setzt den Text des Ladebildschirms
        /// </summary>
        /// <param name="txt">Text</param>
        public void SetText(string txt)
        {
            if (_text == null) return;

            _text.SetText(txt);
        }

        /// <summary>
        /// Setzt die Schriftart des Ladebildschirmtexts
        /// </summary>
        /// <param name="fontFace">Schriftart</param>
        public void SetTextFont(FontFace fontFace)
        {
            if (_text == null) return;

            _text.SetFont(fontFace);
        }

        /// <summary>
        /// Setzt die Schriftart des Ladebildschirmtexts
        /// </summary>
        /// <param name="fontName">Schriftart</param>
        public void SetTextFont(string fontName)
        {
            if (_text == null) return;

            _text.SetFont(fontName);
        }

        /// <summary>
        /// Setzt die Position der Ladebildschirmschrift
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetTextPosition(int x, int y)
        {
            if (_text == null) return;

            _text.SetPosition(x, y);
        }

        /// <summary>
        /// Zentriert die Ladebildschirmschrift im Fenster
        /// </summary>
        public void SetTextPositionToWindowCenter()
        {
            if (_text == null) return;

            _text.CenterOnScreen();
        }

        /// <summary>
        /// Setzt die Skalierung (in Pixeln) des Texts
        /// </summary>
        /// <param name="s">Skalierung (in Pixeln)</param>
        public void SetTextScale(float s)
        {
            if (_text == null) return;

            _text.SetScale(s);
        }

        /// <summary>
        /// Setzt die Textfarbe
        /// </summary>
        /// <param name="r">Rotanteil (0f bis 1f)</param>
        /// <param name="g">Grünanteil (0f bis 1f)</param>
        /// <param name="b">Blauanteil (0f bis 1f)</param>
        public void SetTextColor(float r, float g, float b)
        {
            if (_text == null) return;

            _text.SetColor(r, g, b);
        }


        /// <summary>
        /// Setzt die Textfarbe der Textumrandung
        /// </summary>
        /// <param name="r">Rotanteil (0f bis 1f)</param>
        /// <param name="g">Grünanteil (0f bis 1f)</param>
        /// <param name="b">Blauanteil (0f bis 1f)</param>
        /// <param name="width">Normalisierte Breite der Umrandung (zwischen 0f und 1f)</param>
        public void SetTextColorOutline(float r, float g, float b, float width)
        {
            if (_text == null) return;

            _text.SetColorOutline(r, g, b, width);
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Texts
        /// </summary>
        /// <param name="o">Sichtbarkeitsfaktor (zwischen 0f und 1f)</param>
        public void SetTextOpacity(float o)
        {
            if (_text == null) return;

            _text.SetOpacity(o);
        }

        /// <summary>
        /// Zeichnet eine neue Version des Loading Screen auf dem Bildschirm
        /// </summary>
        public void Update()
        {
            KWEngine.Window.RenderLoadingScreen();
        }

        #region Internals
        internal Vector2i _textureIconSpriteSheetDimensions = Vector2i.One;
        internal Vector2i _textureIconSpriteSheetIndices = Vector2i.Zero;

        internal HUDObjectImage _bgFill = null;
        internal HUDObjectImage _bg = null;
        internal HUDObjectImage _icon = null;
        internal HUDObjectText _text = null;

        internal List<string> _textureKeys;

        internal void Init()
        {
            _textureKeys = new List<string>();

            if (_bgFill == null)
            {
                _bgFill = new HUDObjectImage();
                _bgFill.SetScale(KWEngine.Window.Width, KWEngine.Window.Height);
                _bgFill.CenterOnScreen();
            }

            if (_bg == null)
            {
                _bg = new HUDObjectImage();
                _bg.SetInvisible();
                _bg.SetScale(0, 0);
                _bg.CenterOnScreen();
            }
            if(_icon == null)
            {
                _icon = new HUDObjectImage();
                _icon.SetInvisible();
                _icon.SetScale(0, 0);
                _bg.CenterOnScreen();

                _textureIconSpriteSheetIndices = Vector2i.Zero;
                _textureIconSpriteSheetDimensions = Vector2i.One;
            }
            if(_text == null)
            {
                _text = new HUDObjectText("");
                _text.SetTextAlignment(TextAlignMode.Center);
                _text.CenterOnScreen();
            }
        }

        internal void Dispose()
        {
            // dispose all custom textures:
            foreach(KeyValuePair<string, KWTexture> pair in KWEngine.CurrentWorld._customTextures)
            {
                if(_textureKeys != null && _textureKeys.Contains(pair.Key))
                {
                    HelperTexture.DeleteTexture(pair.Value.ID);
                }
            }
            foreach(string key in _textureKeys)
            {
                KWEngine.CurrentWorld._customTextures.Remove(key);
            }
            _textureKeys.Clear();
        }

        internal LoadingScreen()
        {
            Init();
        }
        #endregion
    }
}
