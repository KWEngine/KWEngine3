using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Keyboard-Klasse für Tastatureingaben (mit Engine-Anpassungen)
    /// </summary>
    public class KeyboardExt
    {
        internal Dictionary<Keys, KeyboardExtState> _keysPressed = new Dictionary<Keys, KeyboardExtState>();
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
            if (KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;
            return KWEngine.Window.KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="key">zu prüfende Taste</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsKeyPressed(Keys key)
        {
            if (KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;

            bool down = KWEngine.Window.KeyboardState.IsKeyDown(key);
            if (down)
            {
                bool result = _keysPressed.TryGetValue(key, out KeyboardExtState t);
                if (result)
                {
                    if (KWEngine.WorldTime > t.Time || t.OldWorld)
                        return false;
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    _keysPressed.Add(key, new KeyboardExtState() { Time = KWEngine.WorldTime, OldWorld = false });
                    return true;
                }
            }
            return false;
        }

        internal void ChangeToOldWorld(Keys k)
        {
            _keysPressed[k].SwitchToOldWorld();
        }
    }
}
