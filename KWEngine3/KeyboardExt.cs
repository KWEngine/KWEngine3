using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Keyboard-Klasse für Tastatureingaben (mit Engine-Anpassungen)
    /// </summary>
    public class KeyboardExt
    {
        internal Dictionary<Keys, float> _keysPressed = new Dictionary<Keys, float>();
        internal void DeleteKeys()
        {
            _keysPressed.Clear();
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird
        /// </summary>
        /// <param name="key">zu prüfende Taste</param>
        /// <returns>true, wenn Taste gedrückt wird</returns>
        public bool IsKeyDown(Keys key)
        {
            return KWEngine.Window.KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="key">zu prüfende Taste</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsKeyPressed(Keys key)
        {
            bool down = KWEngine.Window.KeyboardState.IsKeyDown(key);
            if (down)
            {
                bool result = _keysPressed.TryGetValue(key, out float t);
                if (result)
                {
                    if (KWEngine.WorldTime > t)
                        return false;
                    else
                        return true;
                }
                else
                {
                    _keysPressed.Add(key, KWEngine.WorldTime);
                    return true;
                }
            }
            else
            {
                bool result = _keysPressed.TryGetValue(key, out float t);
                if (result)
                    _keysPressed.Remove(key);
                return false;
            }
        }
    }
}
