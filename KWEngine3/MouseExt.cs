using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Keyboard-Klasse für Tastatureingaben (mit Engine-Anpassungen)
    /// </summary>
    public class MouseExt
    {
        internal bool _firstSimFrame = false;

        internal void UpdateFirstFrameStatus(bool firstSimulationFrame)
        {
            _firstSimFrame = firstSimulationFrame;

        }

        /// <summary>
        /// Gibt die aktuell gemessene Mauszeigerposition an
        /// </summary>
        public Vector2 Position 
        { 
            get
            {
                return KWEngine.Window.MouseState.Position;
            }
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird
        /// </summary>
        /// <param name="button">zu prüfende Taste</param>
        /// <returns>true, wenn Taste gedrückt wird</returns>
        public bool IsButtonDown(MouseButton button)
        {
            return KWEngine.Window.MouseState.IsButtonDown(button);
        }

        /// <summary>
        /// Prüft, ob die angegebene Taste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="button">zu prüfende Taste</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsButtonPressed(MouseButton button)
        {
            bool opentkPressed = KWEngine.Window.MouseState.IsButtonPressed(button);
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
