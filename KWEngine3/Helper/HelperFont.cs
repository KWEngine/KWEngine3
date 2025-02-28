namespace KWEngine3.Helper
{
    internal static class HelperFont
    {
        public static string GetNameForInternalFontID(int id)
        {
            if (id == 0)
                return "Anonymous";
            else if (id == 1)
                return "MajorMonoDisplay";
            else if (id == 2)
                return "NovaMono";
            else if (id == 3)
                return "XanhMono";
            else if (id == 4)
                return "OpenSans";
            else
                return "Anonymous";
        }
    }
}
