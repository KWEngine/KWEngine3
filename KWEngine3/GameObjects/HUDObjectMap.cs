using KWEngine3.Helper;
using OpenTK.Mathematics;
using System.ComponentModel.Design;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse zur Anzeige von Bildern im HUD
    /// </summary>
    internal class HUDObjectMap : IComparable<HUDObjectMap>
    {
        #region Internals
        internal int _textureId = KWEngine.TextureWhite;
        internal Vector2 _textureRepeat = new Vector2(1f, 1f);
        internal Vector4 _color;
        internal Vector4 _colorE;
        internal float _zIndex = 1f;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Quaternion _rotation = Quaternion.Identity;
        internal float _scaleOverride = 0f;
        
        #endregion

        /// <summary>
        /// Standardkonstruktor für bildbasierte HUD-Objekte, der noch keine Bilddatei festlegt (Standardfarbe: weiß)
        /// </summary>
        public HUDObjectMap()
        {
            _textureId = KWEngine.TextureWhite;
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

        public void SetColor(float r, float g, float b, float a)
        {
            _color = new Vector4(
                Math.Clamp(r, 0, 1),
                Math.Clamp(g, 0, 1),
                Math.Clamp(b, 0, 1),
                Math.Clamp(a, 0, 1)
                );
        }

        public void SetColorEmissive(float r, float g, float b, float i)
        {
            _colorE = new Vector4(
                Math.Clamp(r, 0, 1),
                Math.Clamp(g, 0, 1),
                Math.Clamp(b, 0, 1),
                Math.Clamp(i, 0, 1)
                );
        }

        public void SetZIndex(float z)
        {
            _zIndex = z;
        }

        public void Reset()
        {
            SetTextureForMap(null);
        }

        public void SetTextureForMap(string filename)
        {
            if (filename == null)
            {
                _textureId = KWEngine.TextureWhite;
                return;
            }

            filename = filename.Trim();
            if (KWEngine.CurrentWorld._customTextures.TryGetValue(filename, out KWTexture tex))
            {
                _textureId = tex.ID;
            }
            else
            {
                if (File.Exists(filename))
                { 
                    _textureId = HelperTexture.LoadTextureForBackgroundExternal(filename, out int mipMapLevels);
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(_textureId, OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D));
                }
                else
                {
                    KWEngine.LogWriteLine("[Map] Texture file not found");
                    _textureId = KWEngine.TextureWhite;
                }
            }
        }

        public int CompareTo(HUDObjectMap other)
        {
            return _zIndex.CompareTo(other._zIndex);
        }
    }
}
