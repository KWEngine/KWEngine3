using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KWEngine3.FontGenerator
{
    internal static class HelperGlyph
    {
        internal static readonly string GLYPHS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]_abcdefghijklmnopqrstuvwxyz{|}~°€ÜÖÄüöäß·";

        public static KWFont LoadFontSDF_Internal(string imagefile, string jsonfile, string fontname)
        {
            try
            {
                int textureId = HelperTexture.LoadSDFTextureFromAssembly(imagefile);
                AtlasRoot textureAtlas = HelperSDF.GenerateFontDictionaryFromAssembly(jsonfile);

                KWFont kwfont = new KWFont();
                kwfont.Texture = textureId;
                kwfont.ConvertSDFAtlas(textureAtlas);
                kwfont.Name = fontname;
                return kwfont;
            }
            catch (Exception ex)
            {
                KWEngine.LogWriteLine("[Font] Invalid sdf font file: " + ex.Message);
                return new KWFont() { IsValid = false };
            }
        }

        public static KWFont LoadFontSDF_External(string imagefile, string jsonfile, string fontname)
        {
            if (imagefile == null || jsonfile == null || !File.Exists(imagefile) || !File.Exists(jsonfile))
                return new KWFont() { IsValid = false };

            try
            {
                int textureId = HelperTexture.LoadSDFTextureFromDisk(imagefile);
                AtlasRoot textureAtlas = HelperSDF.GenerateFontDictionaryFromDisk(jsonfile);

                KWFont kwfont = new KWFont();
                kwfont.Texture = textureId;
                kwfont.ConvertSDFAtlas(textureAtlas);
                kwfont.Name = fontname;
                return kwfont;
            }
            catch(Exception ex)
            {
                KWEngine.LogWriteLine("[Font] Invalid sdf font file: " + ex.Message);
                return new KWFont() { IsValid = false };
            }
        }
    }
}
