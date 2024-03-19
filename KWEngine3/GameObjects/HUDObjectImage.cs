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
        internal int _textureId = KWEngine.TextureAlpha;
        internal string _textureName = "";
        #endregion

        /// <summary>
        /// Standardkonstruktor für bildbasierte HUD-Objekte, der noch keine Bilddatei festlegt
        /// </summary>
        public HUDObjectImage()
        {

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
        /// Prüft, ob der Mauszeiger über dem Bildobjekt liegt
        /// </summary>
        /// <returns>true, wenn dies der Fall ist</returns>
        public override bool IsMouseCursorOnMe()
        {
            if (KWEngine.Window.IsMouseInWindow)
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
                if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
                {
                    _textureId = KWEngine.CurrentWorld._customTextures[filename];
                }
                else
                {
                    _textureId = HelperTexture.LoadTextureForBackgroundExternal(filename, out int mipMapLevels);
                    KWEngine.CurrentWorld._customTextures.Add(filename, _textureId);
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
