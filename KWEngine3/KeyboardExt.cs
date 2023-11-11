using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Keyboard-Klasse für Tastatureingaben (mit Engine-Anpassungen)
    /// </summary>
    public class KeyboardExt
    {
        internal bool _firstSimFrame = false;

        internal void UpdateFirstFrameStatus(bool firstSimulationFrame)
        {
            _firstSimFrame = firstSimulationFrame;

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
            bool opentkPressed = KWEngine.Window.KeyboardState.IsKeyPressed(key);
            if (opentkPressed && _firstSimFrame)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
