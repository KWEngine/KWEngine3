using System.Reflection;

namespace KWEngine3.Helper
{
    internal static class HelperFont
    {
        public static int GenerateTexture(string resourceName, Assembly a)
        {
            Stream resourceStream = a.GetManifestResourceStream(resourceName);
            return HelperTexture.LoadTextureCompressedNoMipMap(resourceStream);
        }

    }
}
