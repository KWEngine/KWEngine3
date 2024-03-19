using System.Buffers.Binary;
using System.Reflection;
using System.Text.RegularExpressions;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;

namespace KWEngine3.Helper
{
    internal class FBXNode
    {
        public uint EndOffset;
        public uint NumProperties;
        public uint PropertyListLen;
        public byte NameLen;
        public string Name;
        public string Rest;
        public bool HasNestedList;
        public uint NestedListOffset;
        public List<FBXNode> Children = new List<FBXNode>();
        public List<FBXProperty> Properties = new List<FBXProperty>();
        public List<FBXNode> Siblings = new List<FBXNode>();
        public FBXNode Parent;

        public override string ToString()
        {
            return Name;
        }
    }

    internal class FBXProperty
    {
        public string Type;
        public string Name;
        public byte[] RawData;
        public long ID;

        public override string ToString()
        {
            return Name;
        }
    }

    internal static class HelperTexture
    {
        private static readonly Regex rxNonDigits = new Regex(@"[^\d]+");



        internal static string GetFileEnding(string path)
        {
            int index = path.LastIndexOf('.');
            if (index >= 0)
            {
                return path.Substring(index + 1).ToLower();
            }
            else
                return "";
        }

        internal static int LeaveOnlyDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) 
                return -1;
            string cleaned = rxNonDigits.Replace(input, "");

            if (cleaned.Length == 0) 
                return -1;

            return Convert.ToInt32(cleaned);
        }

        internal static bool ConvertEmbeddedToTemporaryFile(byte[] data, string filename, string path)
        {
            File.WriteAllBytes(path + "/" + filename, data);
            return File.Exists(path + "/" + filename);
        }

        internal static bool DeleteTemporaryFile(string filename, string path)
        {
            File.Delete(path + "/" + filename);
            return !File.Exists(path + "/" + filename);
        }

        internal static string GetTextureTypeString(TextureType type)
        {
            if (type == TextureType.Albedo)
                return "ALBEDO";
            else if (type == TextureType.Normal)
                return "NORMAL";
            else if (type == TextureType.Metallic)
                return "METALLIC";
            else if (type == TextureType.Roughness)
                return "ROUGHNESS";
            else if (type == TextureType.Emissive)
                return "EMISSIVE";
            else
                return "UNKNOWN";
        }

        internal static int GetUVIndex(Assimp.Material material, TextureType ttype)
        {
            switch(ttype)
            {
                case TextureType.Albedo: return material.TextureDiffuse.UVIndex;
                case TextureType.Normal: return material.TextureNormal.UVIndex;
                case TextureType.Roughness: return material.TextureDiffuse.UVIndex;
                case TextureType.Emissive: return material.TextureEmissive.UVIndex;
                case TextureType.Metallic: return material.TextureDiffuse.UVIndex;
                default: return material.TextureDiffuse.UVIndex;
            }
        }
       
        internal static int GetTextureSlotIndex(Assimp.Material material, TextureType ttype)
        {
            switch (ttype)
            {
                case TextureType.Albedo: return material.TextureDiffuse.TextureIndex;
                case TextureType.Normal: return material.TextureNormal.TextureIndex;
                case TextureType.Roughness: return -1; // TODO
                case TextureType.Emissive: return material.TextureEmissive.TextureIndex;
                case TextureType.Metallic: return -1; // TODO
                default: return material.TextureDiffuse.TextureIndex;
            }
        }

        internal static int GetDefaultTexture(TextureType ttype)
        {
            switch (ttype)
            {
                case TextureType.Albedo: return KWEngine.TextureDefault;
                case TextureType.Normal: return KWEngine.TextureNormalEmpty;
                case TextureType.Roughness: return KWEngine.TextureWhite;
                case TextureType.Emissive: return KWEngine.TextureBlack;
                case TextureType.Metallic: return KWEngine.TextureBlack;
                default: return KWEngine.TextureDefault;
            }
        }

        internal static Assimp.TextureType GetAssimpTextureType(TextureType ttype)
        {
            switch(ttype)
            {
                case TextureType.Albedo: return Assimp.TextureType.Diffuse;
                case TextureType.Normal: return Assimp.TextureType.Normals;
                case TextureType.Roughness: return Assimp.TextureType.Specular;
                case TextureType.Emissive: return Assimp.TextureType.Emissive;
                case TextureType.Metallic: return Assimp.TextureType.Unknown;
                default: return Assimp.TextureType.Diffuse;
            }
        }

        internal static bool IsFBXASCII(byte[] data)
        {
            string fileinfo = "";
            for(int i = 0; i < 20; i++)
            {
                fileinfo += (char)data[i];
            }
            return fileinfo != "Kaydara FBX Binary  ";
        }

        internal static string GetFBXTextureFilename(TextureType ttype, string filename, string modelName)
        {
            byte[] filedata = File.ReadAllBytes(filename);
            if(IsFBXASCII(filedata))
            {

            }
            else
            {
                uint version = BitConverter.ToUInt32(filedata, 23);
                FBXNode root = ReadFBXNodeStructure(filedata, 27);

                for(int i = 0; i < root.Siblings.Count;i++)
                {
                    if (root.Siblings[i].Name == "Objects")
                    {
                        FBXNode objectsGroup = root.Siblings[i];
                        foreach(FBXNode geometryObject in objectsGroup.Children)
                        {
                            foreach(FBXNode geometryObjectDetail in geometryObject.Siblings)
                            {
                                int indexModel = -1;
                                for(int j = 0; j < geometryObjectDetail.Properties.Count; j++)
                                {
                                    if (geometryObjectDetail.Properties[j].Name.StartsWith(modelName))
                                    {
                                        indexModel = j;
                                        break;
                                    }
                                }
                                
                            }
                        }

                    }
                }
                //CycleFBXNodes(filedata, 27);
            }
            return "";
        }

        internal static bool CheckFBXNestedList(byte[] data, uint endoffset)
        {
            if (endoffset == 0)
                return false;
            // check for 13-NUL-Byte ending of node:
            int endIndex = (int)(endoffset - 1);
            char[] nestedListTerminator = new char[13];
            bool hasNestedList = true;
            for (int i = 0; i < 13; i++)
            {
                nestedListTerminator[i] = (char)data[endIndex - i];
                if (nestedListTerminator[i] != 0)
                {
                    hasNestedList = false;
                }
            }
            return hasNestedList;
        }

        internal static FBXNode ReadFBXNodeStructure(byte[] data, uint startIndex, FBXNode parent = null, int nestingLevel = 0, int nestingLevelEndOffset = 0, bool isSiblingsRead = false)
        {
            string TABS = "";
            for(int i = 0; i < nestingLevel; i++)
            {
                TABS += "\t";
            }
            FBXNode n = new FBXNode();

            int idx = (int)startIndex;
            uint endoffset = BitConverter.ToUInt32(data, idx);
            n.EndOffset = endoffset;
            bool hasNestedList = CheckFBXNestedList(data, endoffset);
            n.HasNestedList = hasNestedList;
            n.Parent = parent;

            idx += 4;
            uint numproperties = BitConverter.ToUInt32(data, idx);
            n.NumProperties = numproperties;

            idx += 4;
            uint proplistlen = BitConverter.ToUInt32(data, idx);
            n.PropertyListLen = proplistlen;

            idx += 4;
            byte namelen = data[idx];
            n.NameLen = namelen;

            idx += 1;
            string name = "";
            int start = idx;
            for(int i = idx; i < start + namelen; i++)
            {
                name += (char)data[i];
                idx++;
            }
            n.Name = name;
            Console.WriteLine(TABS + "Reading node: "  + n.Name);
            if (n.Name == "SceneInfo")
            {

            }

            int idxBeforePropertyLoop = idx;
            // Loop through properties:
            for (int i = 0; i < numproperties; i++)
            {
                FBXProperty prop = new FBXProperty();
                char propertyTypeCode = (char)data[idx];
                prop.Type = propertyTypeCode.ToString();
                idx += 1;
                if (propertyTypeCode == 'Y')
                {
                    // short
                    
                    short val = BitConverter.ToInt16(data, idx);
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    idx += 2;
                }
                else if(propertyTypeCode == 'C')
                {
                    // bool in byte (lsbit = 1/0)
                    byte val = data[idx];
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    idx += 1;
                }
                else if( propertyTypeCode == 'I')
                {
                    // int
                    int val = BitConverter.ToInt32(data, idx);
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    idx += 4;
                }
                else if (propertyTypeCode == 'F')
                {
                    // float
                    float val = BitConverter.ToSingle(data, idx);
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    idx += 4;
                }
                else if (propertyTypeCode == 'D')
                {
                    // double
                    double val = BitConverter.ToDouble(data, idx);
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    idx += 8;
                }
                else if(propertyTypeCode == 'L')
                {
                    //long (signed)
                    long val = BitConverter.ToInt64(data, idx);
                    Console.WriteLine(TABS + "\t" + propertyTypeCode.ToString() + ": " + val);
                    prop.ID = val;
                    idx += 8;
                }
                else if(propertyTypeCode == 'f')
                {
                    uint arrayLength = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint encoding = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint compressedLength = BitConverter.ToUInt32(data, idx);
                    
                    if(encoding > 0)
                    {
                        idx += (int)compressedLength;
                    }
                    else
                    {
                        idx += (int)arrayLength * 4;
                    }
                    Console.WriteLine(TABS + "\t" + "float[]");
                }
                else if (propertyTypeCode == 'd')
                {
                    uint arrayLength = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint encoding = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint compressedLength = BitConverter.ToUInt32(data, idx);

                    if (encoding > 0)
                    {
                        idx += (int)compressedLength;
                    }
                    else
                    {
                        idx += (int)arrayLength * 8;
                    }
                    Console.WriteLine(TABS + "\t" + "double[]");
                }
                else if (propertyTypeCode == 'l')
                {
                    uint arrayLength = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint encoding = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint compressedLength = BitConverter.ToUInt32(data, idx);

                    if (encoding > 0)
                    {
                        idx += (int)compressedLength;
                    }
                    else
                    {
                        idx += (int)arrayLength * 8;
                    }
                    Console.WriteLine(TABS + "\t" + "long[]");
                }
                else if (propertyTypeCode == 'i')
                {
                    uint arrayLength = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint encoding = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint compressedLength = BitConverter.ToUInt32(data, idx);

                    if (encoding > 0)
                    {
                        idx += (int)compressedLength;
                    }
                    else
                    {
                        idx += (int)arrayLength * 4;
                    }
                    Console.WriteLine(TABS + "\t" + "int[]");
                }
                else if (propertyTypeCode == 'b')
                {
                    uint arrayLength = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint encoding = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    uint compressedLength = BitConverter.ToUInt32(data, idx);

                    if (encoding > 0)
                    {
                        idx += (int)compressedLength;
                    }
                    else
                    {
                        idx += (int)arrayLength * 1;
                    }
                    Console.WriteLine(TABS + "\t" + "byte[]");
                }
                else if(propertyTypeCode == 'S')
                {
                    uint length = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    string s = "";
                    for(int j = idx; j < idx + (int)length; j++)
                    {
                        s += (char)data[j];
                    }
                    s = s.Replace("\0", "|");
                    Console.WriteLine(TABS + "\t" + "string: " + s);
                    prop.Name = s;
                    idx += (int)length;
                }
                else if (propertyTypeCode == 'R')
                {
                    uint length = BitConverter.ToUInt32(data, idx);

                    byte[] arraydata = new byte[length];
                    Array.Copy(data, idx, arraydata, 0, length);
                    Console.WriteLine(TABS + "\t" + "RAW DATA");
                    prop.RawData = arraydata;
                    idx += (int)length;
                }
                n.Properties.Add(prop);
            }

            // if it has a nested list, save the offset to this list in an offset variable:
            if (hasNestedList)
            {
                Console.WriteLine(TABS + " START READING OF CHILDREN FOR " + n.Name);
                n.Children.Add(ReadFBXNodeStructure(data, (uint)idx, n, nestingLevel + 1, (int)n.EndOffset));
                Console.WriteLine(TABS + " END READING OF CHILDREN FOR " + n.Name);
            }
            

            idx = (int)n.EndOffset;

            int end = nestingLevelEndOffset > 0 ? nestingLevelEndOffset - 13 : data.Length;
            if (isSiblingsRead == false)
            {
                Console.WriteLine(TABS + "START READING SIBLINGS FOR " + (parent == null ? "ROOT" : parent.Name));
                while (idx > 0 && idx < end)
                {
                    FBXNode sibling = ReadFBXNodeStructure(data, (uint)idx, parent, nestingLevel, -1, true);
                    n.Siblings.Add(sibling);
                    idx = (int)sibling.EndOffset;
                }
                Console.WriteLine(TABS + " END READING SIBLINGS FOR " + (parent == null ? "ROOT" : parent.Name));
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(idx);
            Console.ForegroundColor = ConsoleColor.White;
            return n;
        }

        internal static void CycleFBXNodes(byte[] data, int startIndex)
        {
            while(startIndex > 0)
            {
                FBXNode n = ReadFBXNodeStructure(data, (uint)startIndex, null);
                Console.WriteLine(n.Name + " (at index: " + startIndex + ")");
                startIndex = (int)n.EndOffset;
            }
        }

        internal static GeoTexture ProcessTextureForAssimpPBRMaterial(Assimp.Material material, TextureType ttype, Assimp.Scene scene, ref GeoModel model)
        {
            if (material.Name.ToLower() == "materialpbr")
            {
                string filetype = GetFileEnding(model.Filename);
                string textureFilename = GetFBXTextureFilename(ttype, model.Filename, model.Name);
                if (filetype == "fbx")
                {

                }
                else if(filetype == "obj")
                {

                }
                else if(filetype == "dae")
                {

                }
                else
                {

                }
            }
            return new GeoTexture();
        }

        internal static GeoTexture ProcessTextureForAssimpMaterial(Assimp.Material material, TextureType ttype, Assimp.Scene scene, ref GeoModel model)
        {
            GeoTexture tex = new GeoTexture();
            tex.UVTransform = new Vector4(1, 1, 0, 0);
            tex.UVMapIndex = GetUVIndex(material, ttype);
            tex.Type = ttype;

            if (material.GetMaterialTexture(GetAssimpTextureType(ttype), GetTextureSlotIndex(material, ttype), out Assimp.TextureSlot texSlot))
            {
                string tFilename;
                if (texSlot.FilePath != null)
                {
                    tFilename = texSlot.FilePath;
                    if (tFilename.Length == 0)
                    {
                        // try to find a texture with the model's filename in the same directory or in the subdirectories:
                        tFilename = SceneImporter.FindTextureInSubs(model.Name, model.Path, true);
                    }
                    if (tFilename.Length == 0)
                    {
                        tex.IsEmbedded = true;
                    }
                    else if(tFilename.Contains("*"))
                    {
                        //embedded
                        int index = LeaveOnlyDigits(tFilename);
                        if (scene.Textures.Count > index)
                        {
                            Assimp.EmbeddedTexture embedded = scene.Textures[index];
                            if(embedded.HasCompressedData)
                            {
                                tFilename = model.Name + "_" + material.Name + "_" + GetTextureTypeString(tex.Type) + "-EMBEDDED_" + index + "." + embedded.CompressedFormatHint;
                                tex.IsEmbedded = true;
                                if(!ConvertEmbeddedToTemporaryFile(embedded.CompressedData, tFilename, model.Path))
                                {
                                    KWEngine.LogWriteLine("[Import] Temporary image file " + tFilename + " could not be written to disk");
                                    tFilename = "";
                                }
                            }
                            else
                            {
                                KWEngine.LogWriteLine("[Import] Unsupported embedded image format for " + model.Name + " - skipped import");
                                tFilename = "";
                            }
                        }
                        else
                            tFilename = "";
                    }
                    else
                    {
                        // load file from disk
                        tFilename = HelperGeneral.EqualizePathDividers(tFilename);
                    }

                    if (model.Textures.ContainsKey(tFilename))
                    {
                        tex.OpenGLID = model.Textures[tFilename].OpenGLID;
                    }
                    else if (SceneImporter.CheckIfOtherModelsShareTexture(tFilename, model.Path, out GeoTexture sharedTexture))
                    {
                        return sharedTexture;
                    }
                    else
                    {
                        tex.OpenGLID = LoadTextureForModelExternal(
                                SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(tFilename), model.Path), out int mipMaps
                            );
                        if(tex.IsEmbedded)
                        {
                            bool deleted = DeleteTemporaryFile(tFilename, model.Path);
                            if (!deleted)
                            {
                                KWEngine.LogWriteLine("[Import] Temporary image file " + tFilename + " could not be deleted");
                            }
                        }
                        tex.MipMaps = mipMaps;

                        if (tex.OpenGLID > 0)
                        {
                            if (GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                            {
                                tex.Width = width;
                                tex.Height = height;
                            }
                            tex.Filename = tFilename;
                        }
                        else
                        {
                            tex.OpenGLID = GetDefaultTexture(ttype);
                            if (GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                            {
                                tex.Width = width;
                                tex.Height = height;
                            }
                            tex.UVTransform = new Vector4(1, 1, 0, 0);
                            tex.MipMaps = 0;
                            tex.IsKWEngineTexture = true;
                        }
                    }
                }
                else
                {
                    tex.OpenGLID = GetDefaultTexture(ttype);
                    if (GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                    {
                        tex.Width = width;
                        tex.Height = height;
                    }
                    tex.UVTransform = new Vector4(1, 1, 0, 0);
                    tex.MipMaps = 0;
                    tex.IsKWEngineTexture = true;

                }
            }
            return tex;
        }

        internal static GeoTexture ProcessTextureForMaterial(TextureType textureType, Assimp.Material material, Assimp.Scene scene, ref GeoModel model)
        {
            if(textureType == TextureType.Roughness || textureType == TextureType.Metallic)
            {
                return ProcessTextureForAssimpPBRMaterial(material, textureType, scene, ref model);
            }
            else
            {
                return ProcessTextureForAssimpMaterial(material, textureType, scene, ref model);
            }
        }
        internal static void SaveTextureToBitmap(int texId, int width, int height, string name = null)
        {
            SKBitmap b = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
            byte[] data = new byte[width * height * 4];
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            int x = 0, y = height - 1;
            for (int i = 0; i < data.Length; i += 4)
            {
                byte red = data[i];
                byte green = data[i + 1];
                byte blue = data[i + 2];
                b.SetPixel(x, y, new SKColor(red, green, blue));
                int prevX = x;
                x = (x + 1) % width;
                if (prevX > 0 && x == 0)
                {
                    y--;
                }
            }
            SKWStream s = SKFileWStream.OpenStream(name == null ? "texture2d_rgb.bmp" : name);
            b.Encode(s, SKEncodedImageFormat.Bmp, 1);
            s.Dispose();
            b.Dispose();
        }

        internal static void SaveTextureToBitmap16(int texId, string name = null)
        {
            GL.Flush();
            GL.Finish();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.GetTextureLevelParameter(texId, 0, GetTextureParameter.TextureWidth, out int width);
            GL.GetTextureLevelParameter(texId, 0, GetTextureParameter.TextureHeight, out int height);
            SKBitmap b = new SKBitmap(width, height, SKColorType.Rgba16161616, SKAlphaType.Opaque);
            float[] data = new float[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            int x = 0, y = height - 1;
            for (int i = 0; i < data.Length; i += 4)
            {
                float rd = HelperGeneral.Clamp(data[i] * byte.MaxValue, 0, 255);
                float gn = HelperGeneral.Clamp(data[i + 1] * byte.MaxValue, 0, 255);
                float bl = HelperGeneral.Clamp(data[i + 2] * byte.MaxValue, 0, 255);
                float al = HelperGeneral.Clamp(data[i + 3] * byte.MaxValue, 0, 255);


                byte red = (byte)rd;
                byte green = (byte)gn;
                byte blue = (byte)bl;
                byte alpha = (byte)al;
                b.SetPixel(x, y, new SKColor(red, green, blue, alpha));
                int prevX = x;
                x = (x + 1) % width;
                if (prevX > 0 && x == 0)
                {
                    y--;
                }
            }
            using (SKFileWStream ws = new SKFileWStream(name == null ? "texture2d_16_tonemapped_rgba.png" : name))
            {
                b.Encode(ws, SKEncodedImageFormat.Png, 100);
            }
            b.Dispose();
        }

        /*
        internal static void SaveDepthMapToBitmap(int texId)
        {
            SKBitmap b = new SKBitmap(KWEngine.ShadowMapSize, KWEngine.ShadowMapSize, SKColorType.Rgb888x, SKAlphaType.Opaque);
            float[] depthData = new float[KWEngine.ShadowMapSize * KWEngine.ShadowMapSize];
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, depthData);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            HelperGeneral.ScaleToRange(0, 255, depthData);
            int x = 0, y = KWEngine.ShadowMapSize - 1;
            for(int i = 0; i < depthData.Length; i++)
            {
                int rgb = (int)(depthData[i]);
                b.SetPixel(x, y, new SKColor((byte)rgb, (byte)rgb, (byte)rgb));
                int prevX = x;
                x = (x + 1) % KWEngine.ShadowMapSize;
                if(prevX > 0 && x == 0)
                {
                    y--;
                }
            }
            using (SKFileWStream ws = new SKFileWStream("texture2d_depth.bmp"))
            {
                b.Encode(ws, SKEncodedImageFormat.Bmp, 100);
            }
            b.Dispose();
        }
        */
        /*
        internal static void SaveDepthCubeMapToBitmap(TextureTarget target, int texId)
        {
            SKBitmap b = new SKBitmap(KWEngine.ShadowMapSize, KWEngine.ShadowMapSize, SKColorType.Rgb888x, SKAlphaType.Opaque);
            float[] depthData = new float[KWEngine.ShadowMapSize * KWEngine.ShadowMapSize];
            GL.BindTexture(TextureTarget.TextureCubeMap, texId);
            GL.GetTexImage(target, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, depthData);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            HelperGeneral.ScaleToRange(0, 255, depthData);
            int x = 0, y = KWEngine.ShadowMapSize - 1;
            for (int i = 0; i < depthData.Length; i++)
            {
                int rgb = (int)(depthData[i]);
                b.SetPixel(x, y, new SKColor((byte)rgb, (byte)rgb, (byte)rgb));
                int prevX = x;
                x = (x + 1) % KWEngine.ShadowMapSize;
                if (prevX > 0 && x == 0)
                {
                    y--;
                }
            }
            using (SKFileWStream ws = new SKFileWStream("cube_"+target.ToString()))
            {
                b.Encode(ws, SKEncodedImageFormat.Bmp, 100);
            }
            b.Dispose();
        }
        */
        public static int CreateEmptyCubemapTexture()
        {
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texID);
            byte[] pxColor = new byte[] { 0, 0, 0 };

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgb, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return texID;
        }

        public static int CreateEmptyCubemapDepthTexture()
        {
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texID);
            float[] pxColor = new float[] { 1 };

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, pxColor);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return texID;
        }

        public static int CreateEmptyDepthTexture()
        {
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 1, 1, 0, 
                OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.Float, new float[] { 1, 1 });
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texID;
        }

        public static int LoadTextureCompressedWithMipMaps(string filename)
        {
            bool success = HelperDDS2.TryLoadDDS(filename, false, out int texID, out int width, out int height);
            if (!success)
            {
                //HelperGeneral.ShowErrorAndQuit("HelperTexture::LoadTextureCompressedWithMipMaps()", "Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
                KWEngine.LogWriteLine("[Texture] Invalid texture file " + (filename == null ? "" : filename.Trim()));
                texID = -1;
                throw new Exception("Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
            }
                
            return texID;
        }

        public static int LoadTextureCompressedWithMipMaps(Stream stream)
        {
            bool success = HelperDDS2.TryLoadDDS(stream, false, out int texID, out int width, out int height);
            if (!success)
            {
                KWEngine.LogWriteLine("[Texture] Invalid DDS texture");
                texID = -1;
                throw new Exception("Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
            }

            return texID;
        }

        public static int LoadTextureCompressedNoMipMap(Stream s)
        {
            int texID = -1;
            bool error = false;
            using (HelperDDS dds = new HelperDDS(s))
            {
                if (dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT5)
                {

                    texID = GL.GenTexture();

                    GL.BindTexture(TextureTarget.Texture2D, texID);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 ?  InternalFormat.CompressedRgbaS3tcDxt1Ext : dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 ? InternalFormat.CompressedRgbaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext, dds.BitmapImage.Width, dds.BitmapImage.Height, 0, dds.Data.Length, dds.Data);

                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
                else
                {
                    error = true;
                }
            }
            if (error)
                throw new Exception("Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
            return texID;
        }


        public static int LoadTextureCompressedNoMipMap(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "KWEngine3.Assets.Textures." + fileName;

            int texID = -1;
            bool error = false;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                using(HelperDDS dds = new HelperDDS(s))
                {
                    if (dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT5)
                    {
                        texID = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, texID);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 ? InternalFormat.CompressedRgbaS3tcDxt1Ext : dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 ? InternalFormat.CompressedRgbaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext, dds.BitmapImage.Width, dds.BitmapImage.Height, 0, dds.Data.Length, dds.Data);

                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                    else
                    {
                        error = true;
                    }
                }
                if (error)
                    throw new Exception("Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
            }
            return texID;
        }

        public static int LoadTextureCompressedWithMipMapInternal(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "KWEngine3.Assets.Textures." + fileName;

            int texID = -1;
            bool error = false;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                using (HelperDDS dds = new HelperDDS(s))
                {
                    if (dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT5)
                    {
                        texID = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, texID);
                        //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                        //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 ? InternalFormat.CompressedRgbaS3tcDxt1Ext : dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 ? InternalFormat.CompressedRgbaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext, dds.BitmapImage.Width, dds.BitmapImage.Height, 0, dds.Data.Length, dds.Data);

                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                    else
                    {
                        error = true;
                    }
                }
                if (error)
                    throw new Exception("Unsupported compressed texture format: only DXT1, DXT3 and DXT5 are supported.");
            }
            return texID;
        }

        public static int RoundUpToPowerOf2(int value)
        {
            if (value < 0)
            {
                throw new Exception("Negative values are not allowed.");
            }

            uint v = (uint)value;

            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return (int)v;
        }

        public static int RoundDownToPowerOf2(int value)
        {
            if (value == 0)
            {
                return 0;
            }

            uint v = (uint)value;
           
            v |= (v >> 1);
            v |= (v >> 2);
            v |= (v >> 4);
            v |= (v >> 8);
            v |= (v >> 16);
            v = v - (v >> 1);

            return (int)v;
        }

        internal static int LoadTextureFromByteArray(byte[] imagedata)
        {
            int texID = -1;
            using (MemoryStream s = new MemoryStream(imagedata, false))
            {
                SKBitmap image = SKBitmap.Decode(s);
                if (image == null)
                {
                    KWEngine.LogWriteLine("[Texture] Invalid image data");
                    return -1;
                }
                texID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texID);
                byte[] data;

                if (image.ColorType == SKColorType.Rgba8888)
                {
                    data = image.Bytes;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data);
                }
                else
                {
                    data = image.Bytes;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, data);
                }

                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));

                
                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            return texID;
        }

        internal static int LoadTextureFromAssembly(string resourceName, Assembly assembly)
        {
            int texID = -1;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                SKBitmap image = SKBitmap.Decode(s);
                if (image == null)
                {
                    KWEngine.LogWriteLine("[Texture] File " + (resourceName == null ? "" : resourceName.Trim()) + " invalid");
                    return -1;
                }
                texID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texID);
                byte[] data = image.Bytes;
                if (image.ColorType == SKColorType.Rgba8888)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                     PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Gray8)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, image.Width, image.Height, 0,
                     PixelFormat.Red, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Rgb888x)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                     PixelFormat.Rgb, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Bgra8888)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                     PixelFormat.Bgra, PixelType.UnsignedByte, data);
                }
                else
                {
                    data = image.Bytes;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, data);
                }

                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            return texID;
        }

        internal static int LoadTextureInternal(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "KWEngine3.Assets.Textures." + filename;
            return LoadTextureFromAssembly(resourceName, assembly);
        }

        public static int LoadTextureForModelExternal(string filename, out int mipMaps)
        {
            mipMaps = 0;
            if (!File.Exists(filename))
            {
                return -1;
            }
            if(filename.EndsWith("dds"))
            {
                return LoadTextureCompressedWithMipMaps(filename);
            }

            int texID;
            try
            {
                SKBitmap image = SKBitmap.Decode(filename);
                if (image == null)
                {
                    KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                    return -1;
                }
                texID =  GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texID);

                byte[] data = image.Bytes;
                if (image.ColorType == SKColorType.Rgba8888)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                     PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
                else if(image.ColorType == SKColorType.Gray8)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, image.Width, image.Height, 0,
                     PixelFormat.Red, PixelType.UnsignedByte, data);
                }
                else if(image.ColorType == SKColorType.Rgb888x)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                     PixelFormat.Rgb, PixelType.UnsignedByte, data);
                }
                else if(image.ColorType == SKColorType.Bgra8888)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                     PixelFormat.Bgra, PixelType.UnsignedByte, data);
                }
                else
                {
                    KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                    return -1;
                }

                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
                mipMaps = Math.Max(0, mipMapCount - 2);
                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);

            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                return -1;
            }
            return texID;
        }

        public static int LoadTextureForModelGLB(byte[] rawTextureData, out int mipMaps)
        {
            int texID;
            mipMaps = 0;
            if (rawTextureData[0] == 0x44 && rawTextureData[1] == 0x44 && rawTextureData[2] == 0x53)
            {
                HelperDDS2.TryLoadDDS(rawTextureData, false, out texID, out int width, out int height);
                return texID;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(rawTextureData))
                {
                    SKBitmap image = SKBitmap.Decode(ms);
                    if (image == null)
                    {
                        KWEngine.LogWriteLine("[Texture] File inside GLB model is invalid");
                        return -1;
                    }
                    texID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texID);
                    
                    byte[] data = image.Bytes;
                    if (image.ColorType == SKColorType.Rgba8888)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                         PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                    else if (image.ColorType == SKColorType.Gray8)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, image.Width, image.Height, 0,
                         PixelFormat.Red, PixelType.UnsignedByte, data);
                    }
                    else if (image.ColorType == SKColorType.Rgb888x)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                         PixelFormat.Rgb, PixelType.UnsignedByte, data);
                    }
                    else if (image.ColorType == SKColorType.Bgra8888)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                         PixelFormat.Bgra, PixelType.UnsignedByte, data);
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Texture] GLB Texture source invalid");
                        return -1;
                    }
                    
                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
                    mipMaps = Math.Max(0, mipMapCount - 2);
                    image.Dispose();
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Texture] GLB model file invalid");
                return -1;
            }
            return texID;
        }

        public static int LoadTextureForModelInternalExecutingAssembly(string filename, out int mipMaps)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            int texID;
            mipMaps = 0;
            if (filename.EndsWith("dds"))
            {
                string assPath = a.GetName().Name + ".Assets.Textures." + filename;
                using (Stream s = a.GetManifestResourceStream(assPath))
                    texID = LoadTextureCompressedWithMipMaps(s);

                return texID;
            }

            try
            {
                string assPath = a.GetName().Name + "." + filename;
                using (Stream s = a.GetManifestResourceStream(assPath))
                {
                    SKBitmap image = SKBitmap.Decode(s);
                    if (image == null)
                    {
                        KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                        return -1;
                    }
                    texID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texID);
                    byte[] data;
                    if (image.ColorType == SKColorType.Rgba8888)
                    {
                        data = image.Bytes;
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                         PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                    else
                    {
                        data = image.Bytes;
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                         PixelFormat.Rgb, PixelType.UnsignedByte, data);
                    }

                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
                    mipMaps = Math.Max(0, mipMapCount - 2);
                    image.Dispose();
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                return -1;
            }
            return texID;
        }

        public static int LoadTextureForModelInternal(string filename, out int mipMaps)
        {
            Assembly a = Assembly.GetEntryAssembly();
            int texID;
            mipMaps = 0;
            if (filename.EndsWith("dds"))
            {
                string assPath = a.GetName().Name + "." + filename;
                using (Stream s = a.GetManifestResourceStream(assPath))
                    texID = LoadTextureCompressedWithMipMaps(s);

                return texID;
            }

            try
            {
                string assPath = a.GetName().Name + "." + filename;
                using (Stream s = a.GetManifestResourceStream(assPath))
                {
                    SKBitmap image = SKBitmap.Decode(s);
                    if (image == null)
                    {
                        KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                        return -1;
                    }
                    texID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texID);
                    byte[] data;
                    if (image.ColorType == SKColorType.Rgba8888)
                    {
                        data = image.Bytes;
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                         PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                    else
                    {
                        data = image.Bytes;
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                         PixelFormat.Rgb, PixelType.UnsignedByte, data);
                    }

                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFiltering);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
                    mipMaps = Math.Max(0, mipMapCount - 2);
                    image.Dispose();
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                return -1;
            }
            return texID;
        }

        public static int LoadTextureForBackgroundExternal(string filename, out int mipMapLevels)
        {
            if (!File.Exists(filename))
            {
                mipMapLevels = 0;
                return -1;
            }
            int texID;
            if(filename.ToLower().EndsWith(".dds"))
            {
                HelperDDS2.TryLoadDDS(filename, false, out texID, out int width, out int height, true);
                mipMapLevels = 0;
                return texID;
            }
            try
            {
                SKBitmap image = SKBitmap.Decode(filename);
                if (image == null)
                {
                    
                    throw new Exception("File " + filename + " is not a valid image file.");
                }
                texID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, GetPixelInternalFormatForSKColorType(image.ColorType), image.Width, image.Height, 0,
                    GetPixelFormatForSKColorType(image.ColorType), PixelType.UnsignedByte, image.Bytes);
               
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                mipMapLevels = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapLevels - 2));

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);

            }
            catch (Exception ex)
            {
                KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                throw new Exception("Could not load image file " + filename + "! Make sure to copy it to the correct output directory. " + "[" + ex.Message + "]");
            }
            return texID;
        }

        internal static int LoadTextureSkyboxEquirectangular(string filename, out int mipMapLevels, out int w, out int h)
        {
            if (!File.Exists(filename))
            {
                mipMapLevels = 0;
                w = -1;
                h = -1;
                return -1;
            }
            int texID;
            if (filename.ToLower().EndsWith(".dds"))
            {
                HelperDDS2.TryLoadDDS(filename, false, out texID, out int width, out int height, true);
                mipMapLevels = 0;
                w = width;
                h = height;
                return texID;
            }
            try
            {
                SKBitmap image = SKBitmap.Decode(filename);
                if (image == null)
                {

                    throw new Exception("File " + filename + " is not a valid image file.");
                }
                texID = GL.GenTexture();
                w = image.Width;
                h = image.Height;
                GL.BindTexture(TextureTarget.Texture2D, texID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, GetPixelInternalFormatForSKColorType(image.ColorType), image.Width, image.Height, 0,
                    GetPixelFormatForSKColorType(image.ColorType), PixelType.UnsignedByte, image.Bytes);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                mipMapLevels = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapLevels - 2));

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);

            }
            catch (Exception ex)
            {
                KWEngine.LogWriteLine("[Texture] File " + (filename == null ? "" : filename.Trim()) + " invalid");
                throw new Exception("Could not load image file " + filename + "! Make sure to copy it to the correct output directory. " + "[" + ex.Message + "]");
            }
            return texID;
        }

        internal static int LoadTextureSkybox(string filename, out int mipMapLevels)
        {
            if (!filename.ToLower().EndsWith("jpg") && !filename.ToLower().EndsWith("jpeg") && !filename.ToLower().EndsWith("png") && !filename.ToLower().EndsWith("dds"))
            {
                mipMapLevels = 0;
                return -1;
            }

            try
            {
                using (Stream s = File.Open(filename, FileMode.Open))
                {
                    if(filename.ToLower().EndsWith(".dds"))
                    {
                        HelperDDS2.TryLoadDDSCubeMap(s, false, out int textureId, out mipMapLevels);
                        return textureId;
                    }

                    SKBitmap image = SKBitmap.Decode(s);
                    int width = image.Width;
                    int height = image.Height;
                    int height_onethird = height / 3;
                    int width_onequarter = width / 4;

                    SKBitmap image_front = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);
                    SKBitmap image_back = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);
                    SKBitmap image_up = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);
                    SKBitmap image_down = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);
                    SKBitmap image_left = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);
                    SKBitmap image_right = new SKBitmap(width_onequarter, height_onethird, image.ColorType, image.AlphaType);


                    //front
                    using (SKCanvas cInner = new SKCanvas(image_front))
                    {
                        cInner.DrawBitmap(image,  new SKRect(2 * width_onequarter, height_onethird, 2 * width_onequarter + width_onequarter, height_onethird + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }
                    // back
                    using (SKCanvas cInner = new SKCanvas(image_back))
                    {
                        cInner.DrawBitmap(image, new SKRect(0, height_onethird, width_onequarter, height_onethird + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }

                    // up
                    using (SKCanvas cInner = new SKCanvas(image_up))
                    {
                        cInner.DrawBitmap(image, new SKRect(width_onequarter, 0, width_onequarter + width_onequarter, 0 + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }

                    // down
                    using (SKCanvas cInner = new SKCanvas(image_down))
                    {
                        cInner.DrawBitmap(image, new SKRect(width_onequarter, 2 * height_onethird, width_onequarter + width_onequarter, 2 * height_onethird + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }

                    // left
                    using (SKCanvas cInner = new SKCanvas(image_left))
                    {
                        cInner.DrawBitmap(image, new SKRect(width_onequarter, height_onethird, width_onequarter + width_onequarter, height_onethird + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }

                    // right
                    using (SKCanvas cInner = new SKCanvas(image_right))
                    {
                        cInner.DrawBitmap(image, new SKRect(3 * width_onequarter, height_onethird, 3 * width_onequarter + width_onequarter, height_onethird + height_onethird), new SKRect(0, 0, width_onequarter, height_onethird));
                        cInner.Flush();
                    }

                    int newTexture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.TextureCubeMap, newTexture);
                    byte[] data;

                    PixelInternalFormat iFormat = GetPixelInternalFormatForSKColorType(image.ColorType);
                    PixelFormat pxFormat = GetPixelFormatForSKColorType(image.ColorType);

                    // front
                    data = image_front.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    // back
                    data = image_back.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    // up
                    data = image_up.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    // down
                    data = image_down.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    // left
                    data = image_left.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    // right
                    data = image_right.Bytes;
                    GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, iFormat, width_onequarter, height_onethird, 0, pxFormat, PixelType.UnsignedByte, data);

                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

                    GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
                    mipMapLevels = GetMaxMipMapLevels(width_onequarter, height_onethird);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapLevels - 2));

                    image.Dispose();
                    image_front.Dispose();
                    image_back.Dispose();
                    image_up.Dispose();
                    image_down.Dispose();
                    image_left.Dispose();
                    image_right.Dispose();
                    GL.BindTexture(TextureTarget.TextureCubeMap, 0);
                    return newTexture;
                }
            }
            catch (Exception)
            {
                mipMapLevels = 0;
                return -1;
            }
        }


        internal static PixelInternalFormat GetPixelInternalFormatForSKColorType(SKColorType type)
        {
            switch(type)
            {
                case SKColorType.Bgra8888:
                    return PixelInternalFormat.Rgba8;
                case SKColorType.Rgb888x:
                    return PixelInternalFormat.Rgba8;
                case SKColorType.Gray8:
                    return PixelInternalFormat.R8;
                case SKColorType.Rgba8888:
                    return PixelInternalFormat.Rgba8;
                default:
                    return PixelInternalFormat.Rgba;
            }
        }

        internal static PixelFormat GetPixelFormatForSKColorType(SKColorType type)
        {
            switch (type)
            {
                case SKColorType.Bgra8888:
                    return PixelFormat.Bgra;
                case SKColorType.Rgb888x:
                    return PixelFormat.Rgb;
                case SKColorType.Gray8:
                    return PixelFormat.Red;
                case SKColorType.Rgba8888:
                    return PixelFormat.Rgba;
                default:
                    return PixelFormat.Rgba;
            }
        }

        internal static int GetMaxMipMapLevels(int width, int height)
        {
            return 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        }

        internal static bool GetTextureDimensions(int oglTextureId, out int width, out int height)
        {
            if(oglTextureId <= 0)
            {
                width = 0;
                height = 0;
                return false;
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, oglTextureId);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out width);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out height);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                if (width <= 0 || height <= 0)
                {
                    width = 0;
                    height = 0;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal static void DeleteTexture(int texId)
        {
            if(texId >= 0)
            {
                GL.DeleteTextures(1, new int[] { texId });
            }
        }
    }
}
