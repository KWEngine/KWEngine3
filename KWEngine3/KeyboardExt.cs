using KWEngine3.GameObjects;
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
            if (!KWEngine.Window.IsFocused || KWEngine.CurrentWorld == null || KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;

            return KWEngine.Window.KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="key">zu prüfende Taste</param>
        /// <param name="limitToOneConsumption">Wenn true, wird die Taste nur vom ersten Objekt fragenden Objekt als gedrückt erkannt (sollte nur in absoluten Sonderfällen verwendet werden)</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsKeyPressed(Keys key, bool limitToOneConsumption = false)
        {
            if (!KWEngine.Window.IsFocused || KWEngine.CurrentWorld == null || KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;

            if (KWEngine.CurrentWorld.WorldTime - KWEngine.CurrentWorld._textinputLostFocusTimout < HUDObjectTextInput.TimeoutDuration)
            {
                return false;
            }

            bool down = KWEngine.Window.KeyboardState.IsKeyDown(key);
            if (down)
            {
                bool keyIsInHashtable = _keysPressed.TryGetValue(key, out KeyboardExtState t);
                if (keyIsInHashtable)
                {
                    if (t.OldWorld)
                        return false;

                    if (t.FrameConsumed.HasValue == false)
                    {
                        t.FrameConsumed = GLWindow._frame;
                        return true;
                    }
                    else
                    {
                        if (limitToOneConsumption)
                        {
                            return false;

                        }
                        else
                        {
                            return t.FrameConsumed == GLWindow._frame;
                        }
                    }
                }
                else
                {
                    if (_keysPressed.ContainsKey(key) == false)
                    {
                        _keysPressed.Add(key, new KeyboardExtState() { Frame = GLWindow._frame, FrameConsumed = GLWindow._frame, Time = KWEngine.WorldTime, OldWorld = false });
                    }
                    return true;
                }
            }
            else
            {
                _keysPressed.Remove(key);
                return false;
            }
        }

        internal void ChangeToOldWorld(Keys k)
        {
            _keysPressed[k].SwitchToOldWorld();
        }
    }
}
