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
            if (KWEngine.CurrentWorld == null || KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;
            return GLWindow._keyboard._keysPressed.ContainsKey(key);
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="key">zu prüfende Taste</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsKeyPressed(Keys key)
        {
            if (KWEngine.CurrentWorld == null || KWEngine.CurrentWorld.HasObjectWithActiveInputFocus)
                return false;

            if (KWEngine.CurrentWorld.WorldTime - KWEngine.CurrentWorld._textinputLostFocusTimout < HUDObjectTextInput.TimeoutDuration)
            {
                return false;
            }


            float wt = KWEngine.WorldTime;

            bool result = _keysPressed.TryGetValue(key, out KeyboardExtState t);
            if (result)
            {
                Console.WriteLine("keypress for key " + key + " queried, and key is in hashtable:");
                if (wt > t.Time || t.OldWorld)
                {
                    Console.WriteLine("\tWorldTime: " + wt + " | t.Time: " + t.Time + " (t.OldWorld = " + t.OldWorld + ")");
                    Console.WriteLine("\treturning false...");
                    return false;
                }
                else
                {
                    Console.WriteLine("\tWorldTime: " + wt + " | t.Time: " + t.Time + " (t.OldWorld = " + t.OldWorld + ")");
                    Console.WriteLine("\treturning true...");
                    return true;
                }
            }
            else
            {
                /*
                Console.WriteLine("keypress for key " + key + " queried, and key is NOT in hashtable:");
                Console.WriteLine("\tAdding new Entry to Key DB at WorldTime: " + wt);
                lock (_keysPressed)
                {
                    if(_keysPressed.ContainsKey(key) == false)
                        _keysPressed.Add(key, new KeyboardExtState() { Time = wt, OldWorld = false });
                }
                return true;
                */
                return false;
            }
        }

        internal void ChangeToOldWorld(Keys k)
        {
            _keysPressed[k].SwitchToOldWorld();
        }
    }
}
