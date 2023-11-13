using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Mouse-Klasse für Mauseingaben (mit Engine-Anpassungen)
    /// </summary>
    public class MouseExt
    {
        internal Dictionary<MouseButton, float> _buttonsPressed = new Dictionary<MouseButton, float>();
        internal void DeleteButtons()
        {
            _buttonsPressed.Clear();
        }

        /// <summary>
        /// Gibt die aktuelle Mauscursorposition an
        /// </summary>
        public Vector2 Position { get { return KWEngine.Window.MouseState.Position; } }

        /// <summary>
        /// Prüft, ob die angegebene Maustaste gerade gedrückt wird
        /// </summary>
        /// <param name="button">zu prüfende Taste</param>
        /// <returns>true, wenn Taste gedrückt wird</returns>
        public bool IsButtonDown(MouseButton button)
        {
            return KWEngine.Window.MouseState.IsButtonDown(button);
        }

        /// <summary>
        /// Prüft, ob die angegebene Maustaste gerade gedrückt wird und im vorherigen Frame nicht gedrückt wurde
        /// </summary>
        /// <param name="button">zu prüfende Taste</param>
        /// <returns>true, wenn die Taste gedrückt und im vorherigen Frame nicht gedrückt wurde</returns>
        public bool IsButtonPressed(MouseButton button)
        {
            bool down = KWEngine.Window.MouseState.IsButtonDown(button);
            if (down)
            {
                bool result = _buttonsPressed.TryGetValue(button, out float t);
                if (result)
                {
                    if (KWEngine.WorldTime > t)
                        return false;
                    else
                        return true;
                }
                else
                {
                    _buttonsPressed.Add(button, KWEngine.WorldTime);
                    return true;
                }
            }
            else
            {
                bool result = _buttonsPressed.TryGetValue(button, out float t);
                if (result)
                    _buttonsPressed.Remove(button);
                return false;
            }
        }
    }
}
