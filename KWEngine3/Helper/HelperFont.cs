using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection;

namespace KWEngine3.Helper
{
    internal static class HelperFont
    {
        public static Dictionary<char, int> TextToOffsetDict = new()
        {
            {'Ä', 95 },
            {'Ö', 96 },
            {'Ü', 97 },
            {'ß', 98 },
            {'ä', 99 },
            {'ö', 100 },
            {'ü', 101 },
        };

        public static int GetOffsetForKey(Keys key, bool shift)
        {
            Console.WriteLine(key + " (" + (shift ? "SHIFT" : "NO SHIFT") + ")");

            if(shift)
            {

            }
            else
            {
                if (key == Keys.Backslash) // Hashtag
                    return 50; 
            }

            return 0;
        }

        public static int GenerateTexture(string resourceName, Assembly a)
        {
            Stream resourceStream = a.GetManifestResourceStream(resourceName);
            return HelperTexture.LoadTextureCompressedNoMipMap(resourceStream);
        }

    }
}
