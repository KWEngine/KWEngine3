using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Helper
{
    internal static class HelperDDS2
    {
        private const int DDS_HEADER_SIZE = 128;
        private const int DDSD_MIPMAPCOUNT = 0x00020000;

        private static int ReadInt32LE(byte[] data, int offset)
        {
            return data[offset]
                | (data[offset + 1] << 8)
                | (data[offset + 2] << 16)
                | (data[offset + 3] << 24);
        }

        private static int GetEffectiveMipCount(byte[] header, int width, int height)
        {
            int flags = ReadInt32LE(header, 8);
            int mipMapCountRaw = ReadInt32LE(header, 28);
            int mipMapCount = ((flags & DDSD_MIPMAPCOUNT) != 0 && mipMapCountRaw > 0) ? mipMapCountRaw : 1;

            int maxPossibleMips = 0;
            for (int w = width, h = height; w > 0 && h > 0; w /= 2, h /= 2)
                maxPossibleMips++;

            return Math.Max(1, Math.Min(mipMapCount, maxPossibleMips));
        }

        public static bool TryLoadDDSCubeMap(Stream fsSource, bool SRGB, out int textureID, out int mipCountTotal)
        {
            int width = -1;
            int height = -1;
            textureID = -1;
            var header = new byte[DDS_HEADER_SIZE];
            fsSource.Read(header, 0, DDS_HEADER_SIZE);
            mipCountTotal = 0;

            // first 4 bytes should be 'DDS '
            // 84-87 bytes should be 'DXTn'
            // BC4U/BC4S/ATI2/BC55/R8G8_B8G8/G8R8_G8B8/UYVY-packed/YUY2-packed formats are unsupported
            if (!(header[0] == 'D' && header[1] == 'D' && header[2] == 'S' && header[3] == ' ' && header[84] == 'D'))
            {
                goto exit;
            }

            height = ReadInt32LE(header, 12);
            width = ReadInt32LE(header, 16);
            var mipMapCount = GetEffectiveMipCount(header, width, height);
            mipCountTotal = mipMapCount;

            InternalFormat format;
            int blockSize;
            switch ((char)header[87])
            {
                case '1': // DXT1
                    format = SRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
                    blockSize = 8;
                    break;
                case '3': // DXT3
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt3Ext;
                    blockSize = 16;
                    break;
                case '5': // DXT5
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    blockSize = 16;
                    break;
                default: goto exit;
            }

            // int32 should be enough
            var fileSize = (int)fsSource.Length;
            var buffer = new byte[fileSize - DDS_HEADER_SIZE];
            fsSource.Read(buffer, 0, fileSize - DDS_HEADER_SIZE);

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    var ptr = (IntPtr)p;
                    int offset = 0;
                    int loadedMipLevels = 0;

                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        int levelsThisFace = 0;
                        for (int i = 0, w = width, h = height; i < mipMapCount && w > 0 && h > 0; ++i, w /= 2, h /= 2)
                        {
                            var size = ((w + 3) / 4) * ((h + 3) / 4) * blockSize;
                            GL.CompressedTexImage2D(TextureTarget.TextureCubeMapPositiveX + faceIndex, i, format, w, h, 0, size, ptr + offset);
                            offset += size;
                            levelsThisFace++;
                        }

                        if (faceIndex == 0)
                            loadedMipLevels = levelsThisFace;
                    }

                    mipCountTotal = loadedMipLevels;
                }
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMaxLevel, Math.Max(0, mipCountTotal - 1));
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

        exit:
            fsSource.Close();
            // false if file is not DDS with DXTn compression
            return textureID != -1;
        }

        public static bool TryLoadDDS(string path, bool SRGB, out int textureID, out int width, out int height, bool isSky = false)
        {
            textureID = width = height = -1;
            var fsSource = new FileStream(path, FileMode.Open, FileAccess.Read);
            var header = new byte[DDS_HEADER_SIZE];
            fsSource.Read(header, 0, DDS_HEADER_SIZE);

            // first 4 bytes should be 'DDS '
            // 84-87 bytes should be 'DXTn'
            // BC4U/BC4S/ATI2/BC55/R8G8_B8G8/G8R8_G8B8/UYVY-packed/YUY2-packed formats are unsupported
            if (!(header[0] == 'D' && header[1] == 'D' && header[2] == 'S' && header[3] == ' ' && header[84] == 'D'))
            {
                goto exit;
            }

            height = ReadInt32LE(header, 12);
            width = ReadInt32LE(header, 16);
            var mipMapCount = GetEffectiveMipCount(header, width, height);

            InternalFormat format;
            int blockSize;
            switch ((char)header[87])
            {
                case '1': // DXT1
                    format = SRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
                    blockSize = 8;
                    break;
                case '3': // DXT3
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt3Ext;
                    blockSize = 16;
                    break;
                case '5': // DXT5
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    blockSize = 16;
                    break;
                default: goto exit;
            }

            // int32 should be enough
            var fileSize = (int)fsSource.Length;
            var buffer = new byte[fileSize - DDS_HEADER_SIZE];
            fsSource.Read(buffer, 0, fileSize - DDS_HEADER_SIZE);

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    var ptr = (IntPtr)p;
                    int loadedMipLevels = 0;

                    for (int i = 0, w = width, h = height, offset = 0; i < mipMapCount && w > 0 && h > 0; ++i, w /= 2, h /= 2)
                    {
                        var size = ((w + 3) / 4) * ((h + 3) / 4) * blockSize;
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, format, w, h, 0, size, ptr + offset);
                        offset += size;
                        loadedMipLevels++;
                    }

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, loadedMipLevels - 1));
                }
            }

            //if (!isSky)
            //    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.CurrentWindow.AnisotropicFiltering);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

        exit:
            fsSource.Close();
            // false if file is not DDS with DXTn compression
            return textureID != -1;
        }

        public static bool TryLoadDDS(byte[] data, bool SRGB, out int textureID, out int width, out int height)
        {
            textureID = width = height = -1;
            var fsSource = new MemoryStream(data, false);
            var header = new byte[DDS_HEADER_SIZE];
            fsSource.Read(header, 0, DDS_HEADER_SIZE);

            // first 4 bytes should be 'DDS '
            // 84-87 bytes should be 'DXTn'
            // BC4U/BC4S/ATI2/BC55/R8G8_B8G8/G8R8_G8B8/UYVY-packed/YUY2-packed formats are unsupported
            if (!(header[0] == 'D' && header[1] == 'D' && header[2] == 'S' && header[3] == ' ' && header[84] == 'D'))
            {
                goto exit;
            }

            height = ReadInt32LE(header, 12);
            width = ReadInt32LE(header, 16);
            var mipMapCount = GetEffectiveMipCount(header, width, height);

            InternalFormat format;
            int blockSize;
            switch ((char)header[87])
            {
                case '1': // DXT1
                    format = SRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
                    blockSize = 8;
                    break;
                case '3': // DXT3
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt3Ext;
                    blockSize = 16;
                    break;
                case '5': // DXT5
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    blockSize = 16;
                    break;
                default: goto exit;
            }

            // int32 should be enough
            var fileSize = (int)fsSource.Length;
            var buffer = new byte[fileSize - DDS_HEADER_SIZE];
            fsSource.Read(buffer, 0, fileSize - DDS_HEADER_SIZE);

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    var ptr = (IntPtr)p;
                    int loadedMipLevels = 0;

                    for (int i = 0, w = width, h = height, offset = 0; i < mipMapCount && w > 0 && h > 0; ++i, w /= 2, h /= 2)
                    {
                        var size = ((w + 3) / 4) * ((h + 3) / 4) * blockSize;
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, format, w, h, 0, size, ptr + offset);
                        offset += size;
                        loadedMipLevels++;
                    }

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, loadedMipLevels - 1));
                }
            }

            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

        exit:
            fsSource.Close();
            // false if file is not DDS with DXTn compression
            return textureID != -1;
        }

        public static bool TryLoadDDS(Stream fsSource, bool SRGB, out int textureID, out int width, out int height, bool isSky = false)
        {
            textureID = width = height = -1;
            //var fsSource = new MemoryStream(data, false);
            var header = new byte[DDS_HEADER_SIZE];
            fsSource.Read(header, 0, DDS_HEADER_SIZE);

            // first 4 bytes should be 'DDS '
            // 84-87 bytes should be 'DXTn'
            // BC4U/BC4S/ATI2/BC55/R8G8_B8G8/G8R8_G8B8/UYVY-packed/YUY2-packed formats are unsupported
            if (!(header[0] == 'D' && header[1] == 'D' && header[2] == 'S' && header[3] == ' ' && header[84] == 'D'))
            {
                goto exit;
            }

            height = ReadInt32LE(header, 12);
            width = ReadInt32LE(header, 16);
            var mipMapCount = GetEffectiveMipCount(header, width, height);

            InternalFormat format;
            int blockSize;
            switch ((char)header[87])
            {
                case '1': // DXT1
                    format = SRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
                    blockSize = 8;
                    break;
                case '3': // DXT3
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt3Ext;
                    blockSize = 16;
                    break;
                case '5': // DXT5
                    format = SRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    blockSize = 16;
                    break;
                default: goto exit;
            }

            // int32 should be enough
            var fileSize = (int)fsSource.Length;
            var buffer = new byte[fileSize - DDS_HEADER_SIZE];
            fsSource.Read(buffer, 0, fileSize - DDS_HEADER_SIZE);

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    var ptr = (IntPtr)p;
                    int loadedMipLevels = 0;

                    for (int i = 0, w = width, h = height, offset = 0; i < mipMapCount && w > 0 && h > 0; ++i, w /= 2, h /= 2)
                    {
                        var size = ((w + 3) / 4) * ((h + 3) / 4) * blockSize;
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, i, format, w, h, 0, size, ptr + offset);
                        offset += size;
                        loadedMipLevels++;
                    }

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, loadedMipLevels - 1));
                }
            }

            if (!isSky)
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

        exit:
            fsSource.Close();
            // false if file is not DDS with DXTn compression
            return textureID != -1;
        }
    }
}
