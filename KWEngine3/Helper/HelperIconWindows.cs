using System.Runtime.InteropServices;
internal class HelperIconWindows
{
    [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint ExtractIconExW(
        string lpszFile,
        int nIconIndex,
        IntPtr[] phiconLarge,
        IntPtr[] phiconSmall,
        uint nIcons
    );

    

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern int GetDIBits(
        IntPtr hdc,
        IntPtr hBitmap,
        uint uStartScan,
        uint cScanLines,
        IntPtr lpvBits,
        ref BITMAPINFO lpbi,
        uint uUsage
    );

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;               // Gibt an, ob es sich um ein Icon oder einen Cursor handelt
        public int xHotspot;             // x-Position des Hotspots
        public int yHotspot;             // y-Position des Hotspots
        public IntPtr hbmMask;           // Handle zum Masken-Bitmap
        public IntPtr hbmColor;          // Handle zum farbigen Bitmap
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO pIconInfo);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public uint[] bmiColors;
    }

    public static byte[] GetSmallIconAsByteArray(out int width, out int height)
    {
        width = 0;
        height = 0;
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        IntPtr[] largeHandles = new IntPtr[1];
        uint numIcons = ExtractIconExW(exePath, 0, largeHandles, null, 1);

        byte[] iconData = null;

        if (numIcons == 0 || largeHandles[0] == IntPtr.Zero)
            return iconData;

        if (GetIconInfo(largeHandles[0], out ICONINFO iconinfo))
        {
            iconData = GetIconPixelData(iconinfo.hbmColor, out width, out height);
            DestroyIcon(largeHandles[0]);
        }

        return iconData;
    }

    private static byte[] GetIconPixelData(IntPtr hIcon, out int width, out int height)
    {
        width = 0; height = 0;

        IntPtr hdc = CreateCompatibleDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero)
            return null;

        BITMAPINFO bmpInfo = new BITMAPINFO();
        bmpInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));

        int result = GetDIBits(hdc, hIcon, 0, 0, IntPtr.Zero, ref bmpInfo, 0);
        if (result == 0)
            return null;

        width = bmpInfo.bmiHeader.biWidth;
        height = Math.Abs(bmpInfo.bmiHeader.biHeight);

        int bytesPerPixel = bmpInfo.bmiHeader.biBitCount / 8;
        int stride = width * bytesPerPixel;
        int imageSize = stride * height;

        byte[] pixelData = new byte[imageSize];
        GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();

            result = GetDIBits(hdc, hIcon, 0, (uint)height, ptr, ref bmpInfo, 0);
            if (result == 0)
                pixelData = null;
        }
        finally
        {
            handle.Free();
            DeleteDC(hdc);
        }

        return pixelData;
    }
}