using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
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
        /// Setzt die Ausrichtung des Texts (Standard: linksbündig)
        /// </summary>
        /// <param name="mode">Art der Ausrichtung</param>
        public void SetTextAlignment(TextAlignMode mode)
        {
            if (_text == null) return;

            _text.SetTextAlignment(mode);
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
        /// Setzt den Typ des Fortschrittsanzeigers
        /// </summary>
        /// <param name="type">Bar, FilledCircle oder UnfilledCircle</param>
        public void SetProgressIndicatorType(ProgressIndicatorType type)
        {
            _progressType = type;
        }

        /// <summary>
        /// Setzt den aktuellen Fortschrittswert
        /// </summary>
        /// <param name="value">Wert zwischen 0f (leer) und 1f (voll)</param>
        public void SetProgressValue(float value)
        {
            _progressValue = Math.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Setzt die Position des Fortschrittsanzeigers in Pixeln
        /// </summary>
        /// <param name="x">Horizontale Position</param>
        /// <param name="y">Vertikale Position</param>
        public void SetProgressPosition(int x, int y)
        {
            _progressPosition = new Vector2i(x, y);
            UpdateProgressModelMatrix();
        }

        /// <summary>
        /// Zentriert den Fortschrittsanzeiger im Fenster
        /// </summary>
        public void SetProgressPositionToWindowCenter()
        {
            _progressPosition = new Vector2i(
                KWEngine.Window.ClientRectangle.HalfSize.X,
                KWEngine.Window.ClientRectangle.HalfSize.Y
            );
            UpdateProgressModelMatrix();
        }

        /// <summary>
        /// Setzt die Größe des Fortschrittsanzeigers in Pixeln
        /// </summary>
        /// <remarks>Für Kreis-Typen wird width als Durchmesser für beide Achsen verwendet</remarks>
        /// <param name="width">Breite in Pixeln</param>
        /// <param name="height">Höhe in Pixeln</param>
        public void SetProgressSize(int width, int height)
        {
            _progressSize = new Vector2i(Math.Max(1, width), Math.Max(1, height));
            UpdateProgressModelMatrix();
        }

        /// <summary>
        /// Setzt die Füllfarbe des Fortschrittsanzeigers
        /// </summary>
        /// <param name="r">Rotanteil (0f bis 2f)</param>
        /// <param name="g">Grünanteil (0f bis 2f)</param>
        /// <param name="b">Blauanteil (0f bis 2f)</param>
        /// <param name="alpha">Deckkraft (0f bis 1f)</param>
        public void SetProgressColor(float r, float g, float b, float alpha = 1f)
        {
            _progressColor = new Vector4(
                Math.Clamp(r, 0f, 2f),
                Math.Clamp(g, 0f, 2f),
                Math.Clamp(b, 0f, 2f),
                Math.Clamp(alpha, 0f, 1f));
        }

        /// <summary>
        /// Setzt die Dicke des äußeren Rands (und für UnfilledCircle: die Ringbreite)
        /// </summary>
        /// <remarks>
        /// Für Bar: Rand in Pixeln, berechnet als Anteil der kürzeren Seite.
        /// Für Kreise: Anteil des Radius (0.0f = kein Rand, 1.0f = halber Radius).
        /// Wert 0.0 deaktiviert den Rand komplett.
        /// </remarks>
        /// <param name="thickness">Dicke (0f bis 1f)</param>
        public void SetProgressBorderThickness(float thickness)
        {
            _progressBorderThickness = Math.Clamp(thickness * 0.5f, 0f, 0.49f);
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Fortschrittsanzeigers
        /// </summary>
        /// <param name="visible">Sichtbar oder nicht</param>
        public void SetProgressVisible(bool visible)
        {
            _progressVisible = visible;
        }

        /// <summary>
        /// Zeichnet eine neue Version des Loading Screen auf dem Bildschirm
        /// </summary>
        public void Update()
        {
            KWEngine.Window.RenderLoadingScreen();
        }

        /// <summary>
        /// Erfragt die aktuelle Textgröße in Pixeln
        /// </summary>
        public float TextScale
        {
            get { return _text == null ? 0f : _text.Scale.X; }
        }

        /// <summary>
        /// Erfragt den aktuell gesetzten Text
        /// </summary>
        public string Text
        {
            get { return _text == null ? "" : _text.Text; }
        }

        #region Internals
        internal Vector2i _textureIconSpriteSheetDimensions = Vector2i.One;
        internal Vector2i _textureIconSpriteSheetIndices = Vector2i.Zero;

        internal HUDObjectImage _bgFill = null;
        internal HUDObjectImage _bg = null;
        internal HUDObjectImage _icon = null;
        internal HUDObjectText _text = null;

        internal float _progressValue = 0f;
        internal ProgressIndicatorType _progressType = ProgressIndicatorType.Bar;
        internal Vector2i _progressPosition = Vector2i.Zero;
        internal Vector2i _progressSize = new Vector2i(200, 20);
        internal Vector4 _progressColor = new Vector4(1f, 1f, 1f, 1f);
        internal float _progressBorderThickness = 0.1f;
        internal bool _progressVisible = true;
        internal Matrix4 _progressModelMatrix = Matrix4.Identity;

        internal void UpdateProgressModelMatrix()
        {
            Vector3 scale = new Vector3(_progressSize.X, _progressSize.Y, 1f);
            Vector3 pos = new Vector3(_progressPosition.X, _progressPosition.Y, 0f);
            _progressModelMatrix = HelperMatrix.CreateModelMatrixForHUD(ref scale, ref pos);
        }

        internal List<string> _textureKeys;

        internal void Init()
        {
            _textureKeys = new List<string>();

            _progressPosition = new Vector2i(
                KWEngine.Window.ClientRectangle.HalfSize.X,
                KWEngine.Window.ClientRectangle.HalfSize.Y
            );
            UpdateProgressModelMatrix();

            if (_bgFill == null)
            {
                _bgFill = new HUDObjectImage();
                _bgFill.SetScale(KWEngine.Window.Width, KWEngine.Window.Height);
                _bgFill.SetColor(0f, 0f, 0f);
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
            if (_textureKeys == null || _textureKeys.Count == 0)
                return;

            // Collect all OpenGL texture IDs currently in use by the world,
            // so we only delete loading screen textures that no other consumer references.
            HashSet<int> activeIds = CollectActiveTextureIds();

            foreach (string key in _textureKeys)
            {
                if (KWEngine.CurrentWorld._customTextures.TryGetValue(key, out KWTexture tex))
                {
                    if (!activeIds.Contains(tex.ID))
                    {
                        HelperTexture.DeleteTexture(tex.ID);
                    }
                    KWEngine.CurrentWorld._customTextures.Remove(key);
                }
            }
            _textureKeys.Clear();
        }

        internal HashSet<int> CollectActiveTextureIds()
        {
            HashSet<int> ids = new HashSet<int>();
            World w = KWEngine.CurrentWorld;

            // GameObjects (and TerrainObjects which extend EngineObject)
            foreach (GameObject go in w._gameObjects)
            {
                if (go._model?.Material == null) continue;
                foreach (GeoMaterial mat in go._model.Material)
                {
                    if (mat.TextureAlbedo.IsTextureSet)   ids.Add(mat.TextureAlbedo.OpenGLID);
                    if (mat.TextureNormal.IsTextureSet)   ids.Add(mat.TextureNormal.OpenGLID);
                    if (mat.TextureEmissive.IsTextureSet) ids.Add(mat.TextureEmissive.OpenGLID);
                    if (mat.TextureMetallic.IsTextureSet) ids.Add(mat.TextureMetallic.OpenGLID);
                    if (mat.TextureRoughness.IsTextureSet) ids.Add(mat.TextureRoughness.OpenGLID);
                }
            }

            // TerrainObjects
            foreach (TerrainObject t in w._terrainObjects)
            {
                if (t._heightmap > 0) ids.Add(t._heightmap);
                if (t._gModel?.Material != null)
                {
                    foreach (GeoMaterial mat in t._gModel.Material)
                    {
                        if (mat.TextureAlbedo.IsTextureSet)    ids.Add(mat.TextureAlbedo.OpenGLID);
                        if (mat.TextureNormal.IsTextureSet)    ids.Add(mat.TextureNormal.OpenGLID);
                        if (mat.TextureEmissive.IsTextureSet)  ids.Add(mat.TextureEmissive.OpenGLID);
                        if (mat.TextureMetallic.IsTextureSet)  ids.Add(mat.TextureMetallic.OpenGLID);
                        if (mat.TextureRoughness.IsTextureSet) ids.Add(mat.TextureRoughness.OpenGLID);
                    }
                }
            }

            // FoliageObjects
            foreach (FoliageBase f in w._foliageObjects)
            {
                if (f is FoliageObjectCustom fc && fc._textureId > 0)
                    ids.Add(fc._textureId);
            }

            // HUD objects
            foreach (HUDObject h in w._hudObjects)
            {
                if (h is HUDObjectImage hi && hi._textureId > 0
                    && hi._textureId != KWEngine.TextureWhite
                    && hi._textureId != KWEngine.TextureAlpha)
                    ids.Add(hi._textureId);
            }

            // World background
            if (w._background._standardId > 0) ids.Add(w._background._standardId);
            if (w._background._skyboxId > 0)   ids.Add(w._background._skyboxId);

            // RenderObjects (separate list, not in _gameObjects)
            foreach (RenderObject ro in w._renderObjects)
            {
                if (ro._model?.Material == null) continue;
                foreach (GeoMaterial mat in ro._model.Material)
                {
                    if (mat.TextureAlbedo.IsTextureSet)    ids.Add(mat.TextureAlbedo.OpenGLID);
                    if (mat.TextureNormal.IsTextureSet)    ids.Add(mat.TextureNormal.OpenGLID);
                    if (mat.TextureEmissive.IsTextureSet)  ids.Add(mat.TextureEmissive.OpenGLID);
                    if (mat.TextureMetallic.IsTextureSet)  ids.Add(mat.TextureMetallic.OpenGLID);
                    if (mat.TextureRoughness.IsTextureSet) ids.Add(mat.TextureRoughness.OpenGLID);
                }
            }

            // All loaded models (including those not yet assigned to any GameObject)
            foreach (GeoModel model in KWEngine.Models.Values)
            {
                if (model.Textures != null)
                {
                    foreach (GeoTexture t in model.Textures.Values)
                    {
                        if (t.IsTextureSet) ids.Add(t.OpenGLID);
                    }
                }
            }

            return ids;
        }

        internal LoadingScreen()
        {
            Init();
        }
        #endregion
    }
}
