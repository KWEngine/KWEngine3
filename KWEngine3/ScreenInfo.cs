using OpenTK.Mathematics;

namespace KWEngine3
{
    /// <summary>
    /// Informationen über die angeschlossenen Monitore
    /// </summary>
    public struct ScreenInfo
    {
        internal Screen[] _screens;
        internal int _primaryIndex;

        /// <summary>
        /// Anzahl der angeschlossenen Monitore
        /// </summary>
        public int Count { get { return _screens.Length; } }

        /// <summary>
        /// Erfragt Informationen über den primären Bildschirm
        /// </summary>
        internal Screen PrimaryScreen 
        { 
            get
            {
                return _screens[_primaryIndex];
            }
        }

        /// <summary>
        /// Gibt die horizontale Auflösung des primären Monitors an
        /// </summary>
        public int PrimaryScreenWidth
        {
            get
            {
                return PrimaryScreen.Width;
            }
        }

        /// <summary>
        /// Gibt die vertikale Auflösung des primären Monitors an
        /// </summary>
        public int PrimaryScreenHeight
        {
            get
            {
                return PrimaryScreen.Height;
            }
        }

        /// <summary>
        /// Gibt das window handle des primären Monitors an
        /// </summary>
        public IntPtr PrimaryScreenHandle
        {
            get
            {
                return PrimaryScreen.Handle;
            }
        }

        /// <summary>
        /// Erfragt den Namen des Monitors mit dem angegebenen Index
        /// </summary>
        /// <param name="index">Index des Monitors (erlaubte Werte >= 0)</param>
        /// <returns>Name des Monitors</returns>
        public string GetScreenName(int index = -1)
        {
            if(index < 0 || index >= Count)
            {
                return PrimaryScreen.Name;
            }
            else
            {
                return _screens[index].Name;
            }
        }

        /// <summary>
        /// Erfragt die horizontale u. vertikale Auflösung des Monitors mit dem angegebenen Index
        /// </summary>
        /// <param name="index">Index des Monitors (erlaubte Werte >= 0)</param>
        /// <returns>Auflösung des Monitors</returns>
        public Vector2i GetScreenResolution(int index = -1)
        {
            if (index < 0 || index >= Count)
            {
                return new Vector2i(PrimaryScreen.Width, PrimaryScreen.Height);
            }
            else
            {
                return new Vector2i(_screens[index].Width, _screens[index].Height);
            }
        }

        /// <summary>
        /// Erfragt die DPI des Monitors mit dem angegebenen Index
        /// </summary>
        /// <param name="index">Index des Monitors (erlaubte Werte >= 0)</param>
        /// <returns>DPI-Werte (horizontal u. vertikal)</returns>
        public Vector2 GetScreenDPI(int index = -1)
        {
            if (index < 0 || index >= Count)
            {
                return PrimaryScreen.DPI;
            }
            else
            {
                return _screens[index].DPI;
            }
        }

        /// <summary>
        /// Erfragt das im Betriebssystem für den angegebenen Monitor hinterlegte Handle (Pointer)
        /// </summary>
        /// <param name="index">Index des Monitors (erlaubte Werte >= 0)</param>
        /// <returns>Handle (Pointer)</returns>
        public IntPtr GetScreenHandle(int index = -1)
        {
            if (index < 0 || index >= Count)
            {
                return PrimaryScreen.Handle;
            }
            else
            {
                return _screens[index].Handle;
            }
        }
    }
}
