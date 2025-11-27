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

            return GLWindow._keyboard._keysPressed.ContainsKey(key);
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

            float wt = KWEngine.WorldTime;

            bool result = _keysPressed.TryGetValue(key, out KeyboardExtState t);
            if (result)
            {
                //Console.WriteLine("key press for key " + key + " queried, and key is in hashtable:");

                // is this input from a previous world? if so, always return false:
                if (t.OldWorld)
                {
                    //Console.WriteLine("\treturning false because of OldWorld = true in key state...");
                    return false;
                }
                
                // has this input already been consumed by any object?
                if (t.FrameConsumed.HasValue == true)
                {
                    if (limitToOneConsumption == false)
                    {
                        //Console.WriteLine("\tFrame: " + t.Frame + " | FrameConsumed: " + t.FrameConsumed.Value + " | " + "WorldTime: " + wt + " | t.Time: " + t.Time + " (t.OldWorld = " + t.OldWorld + ")");
                        //Console.WriteLine("\treturning false...");
                        return false;
                    }
                    else
                    {
                        return t.FrameConsumed.Value == GLWindow._frame;
                    }
                }
                else if(t.FrameConsumed.HasValue == false)
                {
                    //Console.WriteLine("\tFrame: " + t.Frame + " | FrameConsumed: " + "not consumed yet" + " | " + "WorldTime: " + wt + " | t.Time: " + t.Time + " (t.OldWorld = " + t.OldWorld + ")");
                    //Console.WriteLine("\treturning true...");
                    t.FrameConsumed = GLWindow._frame;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal void ChangeToOldWorld(Keys k)
        {
            _keysPressed[k].SwitchToOldWorld();
        }
    }
}
