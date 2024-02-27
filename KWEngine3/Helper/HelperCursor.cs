using OpenTK.Windowing.Common.Input;
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

        public static void Import(string name, string filename)
        {
            filename = HelperGeneral.EqualizePathDividers(filename);
            if(File.Exists(filename) && !_cursorDict.ContainsKey(name))
            {
                MouseCursor mc = new MouseCursor(0, 0, 0, 0, null);
                _cursorDict.Add(name, mc);
            }
            else
            {
                KWEngine.LogWriteLine("[Cursor] ");
            }
        }

        public static void GetCursor(string filename)
        {

        }

    }
}
