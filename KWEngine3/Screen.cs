using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace KWEngine3
{
    internal struct Screen
    {
        public bool IsPrimary { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public string Name { get; internal set; }
        public Vector2 DPI { get; internal set; }
        public IntPtr Handle { get; internal set; }
    }
}
