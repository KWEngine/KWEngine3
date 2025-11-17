using glTFLoader.Schema;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse zur Anzeige von Bildern im HUD
    /// </summary>
    public class HUDObjectImage : HUDObject
    {
        #region Internals
        internal int _textureId = KWEngine.TextureWhite;
        internal string _textureName = "";
        internal Vector2 _textureRepeat = new Vector2(1f, 1f);
        #endregion

        /// <summary>
        /// Standardkonstruktor für bildbasierte HUD-Objekte, der noch keine Bilddatei festlegt (Standardfarbe: weiß)
        /// </summary>
        public HUDObjectImage()
        {
            _textureId = KWEngine.TextureWhite;
        }

        /// <summary>
        /// Standardkonstruktor für bildbasierte HUD-Objekte
        /// </summary>
        /// <param name="filename">Name der Bilddatei</param>
        public HUDObjectImage(string filename)
        {
            SetTexture(filename);
        }

        /// <summary>
        /// Prüft, ob diese Instanz aktuell auf dem Bildschirm zu sehen ist
        /// </summary>
        /// <returns>true, wenn das Objekt zu sehen ist</returns>
        public override bool IsInsideScreenSpace()
        {
            float left, right, top, bottom;
            left = Position.X - _scale.X * 0.5f;
            right = Position.X + _scale.X * 0.5f;
            top = Position.Y - _scale.Y * 0.5f;
            bottom = Position.Y + _scale.Y * 0.5f;
            return !(right < 0 || left > KWEngine.Window.Width || bottom < 0 || top > KWEngine.Window.Height);
        }

        /// <summary>
        /// Gibt den aktuellen Texturwiederholungsfaktor an
        /// </summary>
        public Vector2 TextureRepeat
        {
            get
            {
                return _textureRepeat;
            }
        }

        /// <summary>
        /// Gibt an, wie oft die gewählte Textur im HUDObject wiederholt werden soll
        /// </summary>
        /// <param name="x">Wiederholungsfaktor in x-Richtung</param>
        /// <param name="y">Wiederholungsfaktor in y-Richtung</param>
        public void SetTextureRepeat(float x, float y)
        {
            _textureRepeat.X = x;
            _textureRepeat.Y = y;
        }

        /// <summary>
        /// Gibt die aktuelle Skalierung in Pixeln zurück
        /// </summary>
        public new Vector2i Scale
        {
            get
            {
                int left, right, top, bottom;
                left = (int)(Position.X - _scale.X * 0.5f);
                right = (int)(Position.X + _scale.X * 0.5f);
                top = (int)(Position.Y - _scale.Y * 0.5f);
                bottom = (int)(Position.Y + _scale.Y * 0.5f);

                return new Vector2i(right - left, bottom - top);
            }
        }

        /// <summary>
        /// Setzt die Größe (Breite und Höhe gleichermaßen)
        /// </summary>
        /// <param name="scale">Größe des Bilds (in Pixeln)</param>
        public void SetScale(float scale)
        {
            _scale.X = HelperGeneral.Clamp(scale, 0.001f, 4096f);
            _scale.Y = _scale.X;
            _scale.Z = 1;
            UpdateMVP();
        }

        /// <summary>
        /// Setzt die Größe des Bildes in Pixeln
        /// </summary>
        /// <param name="width">Breite (gültige Werte zwischen 0.001 und 4096)</param>
        /// <param name="height">Höhe (gültige Werte zwischen 0.001 und 4096)</param>
        public void SetScale(float width, float height)
        {
            _scale.X = HelperGeneral.Clamp(width, 0.001f, 4096f);
            _scale.Y = HelperGeneral.Clamp(height, 0.001f, 4096f);
            _scale.Z = 1;
            UpdateMVP();
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger über dem Bildobjekt liegt
        /// </summary>
        /// <returns>true, wenn dies der Fall ist</returns>
        public override bool IsMouseCursorOnMe()
        {
            if (KWEngine.Window.IsMouseInWindow && IsInWorld)
            {
                Vector2 mouseCoords = KWEngine.Window.MouseState.Position;
                float left, right, top, bottom;
                left = Position.X - _scale.X * 0.5f;
                right = Position.X + _scale.X * 0.5f;
                top = Position.Y - _scale.Y * 0.5f;
                bottom = Position.Y + _scale.Y * 0.5f;

                return (mouseCoords.X >= left && mouseCoords.X <= right && mouseCoords.Y >= top && mouseCoords.Y <= bottom);
            }
            return false;
        }

        internal void Reset()
        {
            SetTextureForMap(null);
        }

        internal void SetTextureForMap(string filename)
        {
            if (filename == null)
            {
                _textureId = KWEngine.TextureWhite;
                return;
            }

            filename = filename.Trim();
            if (File.Exists(filename))
            {
                if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
                {
                    _textureId = KWEngine.CurrentWorld._customTextures[filename].ID;
                }
                else
                {
                    _textureId = HelperTexture.LoadTextureForBackgroundExternal(filename, out int mipMapLevels);
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(_textureId, OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D));
                }
                if (HelperTexture.GetTextureDimensions(_textureId, out int width, out int height))
                {
                    _textureName = filename;
                    SetScale(width, height);
                }
            }
            else
            {
                KWEngine.LogWriteLine("[Map] Texture file not found");
                _textureId = KWEngine.TextureWhite;
            }
        }

        /// <summary>
        /// Setzt das anzuzeigende Bild (als Textur) und skaliert das Objekt auf die Originalgröße dieses Bildes
        /// </summary>
        /// <param name="filename">Name der Bilddatei</param>
        public void SetTexture(string filename)
        {
            if (filename == null)
            {
                KWEngine.LogWriteLine("[HUDObject] Texture file not found");
                _textureId = KWEngine.TextureAlpha;
                return;
            }

            filename = filename.Trim();
            if (File.Exists(filename))
            {
                if(KWEngine.CurrentWorld == null)
                {
                    KWEngine.LogWriteLine("[HUDObject] No current world found, cannot load texture");
                    _textureId = KWEngine.TextureAlpha;
                    return;
                }

                if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
                {
                    _textureId = KWEngine.CurrentWorld._customTextures[filename].ID;
                }
                else
                {
                    _textureId = HelperTexture.LoadTextureForBackgroundExternal(filename, out int mipMapLevels);
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(_textureId, OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D));
                }
                if (HelperTexture.GetTextureDimensions(_textureId, out int width, out int height))
                {
                    _textureName = filename;
                    SetScale(width, height);
                }
            }
            else
            {
                KWEngine.LogWriteLine("[HUDObject] Texture file not found");
                _textureId = KWEngine.TextureAlpha;
            }
        }
    }
}
