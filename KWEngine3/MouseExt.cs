﻿using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3
{
    /// <summary>
    /// Mouse-Klasse für Mauseingaben (mit Engine-Anpassungen)
    /// </summary>
    public class MouseExt
    {
        internal Dictionary<MouseButton, MouseExtState> _buttonsPressed = new Dictionary<MouseButton, MouseExtState>();
        internal void DeleteButtons()
        {
            _buttonsPressed.Clear();
        }

        internal Vector2 _mousePositionFromGLFW = new Vector2(0, 0);

        /// <summary>
        /// Gibt die absolute Scrollradposition der Maus wieder
        /// </summary>
        public int MouseScrollPosition { get { return KWEngine.Window._mouseScrollPosition; } }

        /// <summary>
        /// Gibt den Unterschied der Scrollradposition der Maus im Vergleich zur zuletzt gemessenen Position wieder
        /// </summary>
        public int MouseScrollDelta { get { return KWEngine.Window._mouseScrollDelta; } }

        /// <summary>
        /// Gibt die aktuelle Mauscursorposition an
        /// </summary>
        public Vector2 Position { get { return _mousePositionFromGLFW; } }
        //public Vector2 Position { get { return KWEngine.Window.MouseState.Position; } }

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
                bool result = _buttonsPressed.TryGetValue(button, out MouseExtState t);
                if (result)
                {
                    if (KWEngine.WorldTime > t.Time || t.OldWorld)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    MouseExtState s = new MouseExtState() { Time = KWEngine.WorldTime, OldWorld = false };
                    _buttonsPressed.Add(button, s);
                    return true;
                }
            }
            else
                return false;
        }

        internal void ChangeToOldWorld(MouseButton b)
        {
            _buttonsPressed[b].SwitchToOldWorld();
        }

        /// <summary>
        /// Gibt die Mausbewegung als Delta-Wert für beide Achsen zurück (Unterschied der Mausposition zum letzten Frame)
        /// </summary>
        /// <returns>Delta-Wert der Mauszeigerbewegung (Unterschied zum letzten Frame)</returns>
        public Vector2 GetDelta()
        {
            return KWEngine.Window._mouseDeltaToUse;
        }
    }
}
