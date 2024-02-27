using OpenTK.Windowing.Common.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal static class HelperCursor
    {
        public static Dictionary<string, MouseCursor> _cursorDict = new();

        public static bool Import(string name, string filename, float tipX, float tipY)
        {
            filename = HelperGeneral.EqualizePathDividers(filename);
            if(File.Exists(filename) && !_cursorDict.ContainsKey(name))
            {
                SKBitmap image = SKBitmap.Decode(filename);
                if (image == null)
                {
                    return false;
                }

                byte[] data = image.Bytes;
                int width = image.Width;
                int height = image.Height;
                if (image.ColorType == SKColorType.Rgba8888)
                {
                    
                }
                else if (image.ColorType == SKColorType.Bgra8888)
                {
                   
                }
                else
                {
                    KWEngine.LogWriteLine("[Cursor] Cursor has wrong color format (needs alpha channel)");
                    image.Dispose();
                    return false;
                }
                

                if (tipX < 0 || tipX > 1)
                    tipX = 0.5f;
                if (tipY < 0 || tipY > 1)
                    tipY = 0.5f;

                int hotX = (int)(tipX * width);
                int hotY = (int)(tipY * height);
                MouseCursor mc = new MouseCursor(hotX, hotY, width, height, data);
                _cursorDict.Add(name, mc);
                image.Dispose();
            }
            else
            {
                return false;
            }
            return true;
        }

        public static MouseCursor GetCursor(string name)
        {
            if(name != null && _cursorDict.ContainsKey(name))
            {
                return _cursorDict[name];
            }
            else
            {
                return null;
            }
        }

    }
}
