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
        public bool HasNestedList;
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
        public float FValue;

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Helferklasse für Texturen
    /// </summary>
    public static class HelperTexture
    {
        internal static readonly Regex rxNonDigits = new Regex(@"[^\d]+");
        internal static readonly string replaceSymbol = "|KWEngine|";

        internal static bool LoadBitmapForWindowIcon(string file, out int width, out int height, out byte[] data)
        {
            width = -1;
            height = -1;
            data = null;
            string tmp = file.ToLower().Trim();
            if (!tmp.EndsWith(".png") && !tmp.EndsWith(".jpeg") && !tmp.EndsWith(".jpg"))
            {
                KWEngine.LogWriteLine("[HelperTexture] Invalid window icon file type - please use JPG or PNG");
                return false;
            }

            SKBitmap bitmap;
            try
            {
                bitmap = SKBitmap.Decode(file);
                if (bitmap != null && bitmap.Width >= 16 && bitmap.Height >= 16 && bitmap.Width <= 64 && bitmap.Height <= 64 && bitmap.Width == bitmap.Height)
                {
                    width = bitmap.Width;
                    height = bitmap.Height;
                    data = new byte[bitmap.Bytes.Length];
                    SKBitmap bitmapRGBA = bitmap.Copy(SKColorType.Rgba8888);
                    Array.Copy(bitmapRGBA.Bytes, data, bitmapRGBA.Bytes.Length);
                    bitmap.Dispose();
                    bitmapRGBA.Dispose();
                    return true;
                }
                else
                {
                    KWEngine.LogWriteLine("[HelperTexture] Invalid window icon - width and height need to be between 16-64px and of equal dimension length");
                    return false;
                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Window] Invalid window icon - unknown error occured");
                return false;
            }
        }

        internal static string GetFileEnding(string path)
        {
            int index = path.LastIndexOf('.');
            if (index >= 0)
            {
                return path.Substring(index + 1).Trim().ToLower();
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
            else if (type == TextureType.Transparency)
                return "TRANSPARENCY";
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
                case TextureType.Transparency: return material.TextureOpacity.UVIndex;
                default: return material.TextureDiffuse.UVIndex;
            }
        }
       
        internal static int GetTextureSlotIndex(Assimp.Material material, TextureType ttype, bool isBumpInsteadOfNormalMap = false)
        {
            if(isBumpInsteadOfNormalMap && material.HasTextureHeight)
            {
                return material.TextureHeight.TextureIndex;
            }

            switch (ttype)
            {
                case TextureType.Albedo: return material.TextureDiffuse.TextureIndex;
                case TextureType.Normal: return material.TextureNormal.TextureIndex;
                case TextureType.Roughness: return material.TextureSpecular.TextureIndex;
                case TextureType.Emissive: return material.TextureEmissive.TextureIndex;
                case TextureType.Metallic: return -1; // TODO
                case TextureType.Transparency: return material.TextureOpacity.TextureIndex;
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
                case TextureType.Transparency: return KWEngine.TextureWhite;
                default: return KWEngine.TextureDefault;
            }
        }

        internal static Assimp.TextureType GetAssimpTextureType(TextureType ttype, bool isBumpInsteadOfNormal = false)
        {
            if(isBumpInsteadOfNormal)
            {
                return Assimp.TextureType.Height;
            }
            switch(ttype)
            {
                case TextureType.Albedo: return Assimp.TextureType.Diffuse;
                case TextureType.Normal: return Assimp.TextureType.Normals;
                case TextureType.Roughness: return Assimp.TextureType.Specular;
                case TextureType.Emissive: return Assimp.TextureType.Emissive;
                case TextureType.Metallic: return Assimp.TextureType.Unknown;
                case TextureType.Transparency: return Assimp.TextureType.Opacity;
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

        internal static long GetMaterialIdFor(string materialName, FBXNode root)
        {
            for (int i = 0; i < root.Siblings.Count; i++)
            {
                if (root.Siblings[i].Name == "Objects")
                {
                    FBXNode objectsGroup = root.Siblings[i];
                    foreach (FBXNode geometryObject in objectsGroup.Children)
                    {
                        foreach (FBXNode geometryObjectDetail in geometryObject.Siblings)
                        {
                            if (geometryObjectDetail.Name == "Material")
                            {
                                foreach (FBXProperty property in geometryObjectDetail.Properties)
                                {
                                    if (property.Type == "S")
                                    {
                                        int idx = property.Name.IndexOf(replaceSymbol);
                                        if (idx > 0)
                                        {
                                            string matName = property.Name.Substring(0, idx);
                                            if (matName == materialName)
                                            {
                                                return geometryObjectDetail.Properties[0].ID;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }

        internal static bool GetMetallicRoughnessValues(long materialId, FBXNode root, out float r, out float m)
        {
            r = 1.0f;
            m = 0.0f;

            bool mFound = false;
            bool rFound = false;

            for (int i = 0; i < root.Siblings.Count; i++)
            {
                if (root.Siblings[i].Name == "Objects")
                {
                    FBXNode objectsGroup = root.Siblings[i];
                    foreach (FBXNode geometryObject in objectsGroup.Children)
                    {
                        foreach (FBXNode geometryObjectDetail in geometryObject.Siblings)
                        {
                            if (geometryObjectDetail.Name == "Material")
                            {
                                foreach (FBXProperty property in geometryObjectDetail.Properties)
                                {
                                    if (property.Type == "L" && property.ID == materialId)
                                    {
                                        foreach(FBXNode innerChild in geometryObjectDetail.Children)
                                        {
                                            foreach(FBXNode innerSibling in innerChild.Siblings)
                                            {
                                                if(innerSibling.Name == "Properties70" && innerSibling.Children.Count > 0)
                                                {
                                                    FBXNode propertyNode = innerSibling.Children[0];
                                                    foreach(FBXNode propertyNodeSibling in propertyNode.Siblings)
                                                    {
                                                        foreach(FBXProperty propertyNodeSiblingProp in propertyNodeSibling.Properties)
                                                        {
                                                            if(propertyNodeSiblingProp.Name == "ReflectionFactor")
                                                            {
                                                                m = propertyNodeSibling.Properties.Last().FValue;
                                                                mFound = true;
                                                            }
                                                            if(propertyNodeSiblingProp.Name == "SpecularFactor")
                                                            {
                                                                r = 1f - propertyNodeSibling.Properties.Last().FValue;
                                                                rFound = true;
                                                            }
                                                        }
                                                        if (mFound && rFound) break;
                                                    }
                                                }
                                                if (mFound && rFound) break;
                                            }
                                            if (mFound && rFound) break;
                                        }
                                    }
                                    if (mFound && rFound) break;
                                }
                            }
                            if (mFound && rFound) break;
                        }
                        if (mFound && rFound) break;
                    }
                }
                if (mFound && rFound) break;
            }

            return mFound && rFound;
        }

        internal static bool GetTextureMetallic(long textureId, FBXNode root, out string metallicFilename, out byte[] metallicData)
        {
            bool result = false;
            metallicFilename = "";
            metallicData = null;
            bool textureIdApproved = false;
            for (int i = 0; i < root.Siblings.Count; i++)
            {
                if (root.Siblings[i].Name == "Objects")
                {
                    FBXNode objectsGroup = root.Siblings[i];
                    foreach (FBXNode geometryObject in objectsGroup.Children)
                    {
                        foreach (FBXNode geometryObjectDetail in geometryObject.Siblings)
                        {
                            if (geometryObjectDetail.Name == "Texture")
                            {
                                bool found = false;
                                foreach (FBXProperty property in geometryObjectDetail.Properties)
                                {
                                    if (property.Type == "S" && property.Name.ToLower().Contains("metallic_texture"))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if(found)
                                {
                                    long maybeId = geometryObjectDetail.Properties[0].ID;
                                    if (maybeId == textureId)
                                    {
                                        textureIdApproved = true;
                                        foreach(FBXNode sibling in geometryObjectDetail.Children[0].Siblings)
                                        {
                                            if(sibling.Name == "FileName" || sibling.Name == "RelativeFilename")
                                            {
                                                metallicFilename = sibling.Properties[0].Name;
                                                int index = metallicFilename.IndexOf(replaceSymbol);
                                                int limit = index > 0 ? index : metallicFilename.Length;
                                                metallicFilename = metallicFilename.Substring(0, limit);
                                                metallicFilename = SceneImporter.StripPathFromFile(metallicFilename);
                                                result = true;
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            if (textureIdApproved)
                            {
                                if (geometryObjectDetail.Name == "Video")
                                {
                                    FBXNode firstChild = geometryObjectDetail.Children[0];
                                    bool sourceFound = false;
                                    foreach(FBXNode firstChildChild in firstChild.Siblings)
                                    {
                                        if(firstChildChild.Name == "RelativeFilename")
                                        {
                                            if (firstChildChild.Properties[0].Name.Contains(metallicFilename))
                                            {
                                                sourceFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if(sourceFound)
                                    {
                                        foreach (FBXNode firstChildChild in firstChild.Siblings)
                                        {
                                            if (firstChildChild.Name == "Content")
                                            {
                                                metallicData = new byte[firstChildChild.Properties[0].RawData.Length];
                                                Array.Copy(firstChildChild.Properties[0].RawData, metallicData, metallicData.Length);
                                                result = true;
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                        if (textureIdApproved) break;
                    }
                }
                if (textureIdApproved) break;
            }

            return result;
        }

        internal static bool GetTextureRoughness(long textureId, FBXNode root, out string roughnessFilename, out byte[] roughnessData)
        {
            bool result = false;
            roughnessFilename = "";
            roughnessData = null;
            bool textureIdApproved = false;
            for (int i = 0; i < root.Siblings.Count; i++)
            {
                if (root.Siblings[i].Name == "Objects")
                {
                    FBXNode objectsGroup = root.Siblings[i];
                    foreach (FBXNode geometryObject in objectsGroup.Children)
                    {
                        foreach (FBXNode geometryObjectDetail in geometryObject.Siblings)
                        {
                            if (geometryObjectDetail.Name == "Texture")
                            {
                                bool found = false;
                                foreach (FBXProperty property in geometryObjectDetail.Properties)
                                {
                                    if (property.Type == "S" && property.Name.ToLower().Contains("roughness_texture"))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                {
                                    long maybeId = geometryObjectDetail.Properties[0].ID;
                                    if (maybeId == textureId)
                                    {
                                        textureIdApproved = true;
                                        foreach (FBXNode sibling in geometryObjectDetail.Children[0].Siblings)
                                        {
                                            if (sibling.Name == "FileName" || sibling.Name == "RelativeFilename")
                                            {
                                                roughnessFilename = sibling.Properties[0].Name;
                                                int index = roughnessFilename.IndexOf(replaceSymbol);
                                                int limit = index > 0 ? index : roughnessFilename.Length;
                                                roughnessFilename = roughnessFilename.Substring(0, limit);
                                                roughnessFilename = SceneImporter.StripPathFromFile(roughnessFilename);
                                                result = true;
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            if (textureIdApproved)
                            {
                                if (geometryObjectDetail.Name == "Video")
                                {
                                    FBXNode firstChild = geometryObjectDetail.Children[0];
                                    bool sourceFound = false;
                                    foreach (FBXNode firstChildChild in firstChild.Siblings)
                                    {
                                        if (firstChildChild.Name == "RelativeFilename")
                                        {
                                            if (firstChildChild.Properties[0].Name.Contains(roughnessFilename))
                                            {
                                                sourceFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (sourceFound)
                                    {
                                        foreach (FBXNode firstChildChild in firstChild.Siblings)
                                        {
                                            if (firstChildChild.Name == "Content")
                                            {
                                                roughnessData = new byte[firstChildChild.Properties[0].RawData.Length];
                                                Array.Copy(firstChildChild.Properties[0].RawData, roughnessData, roughnessData.Length);
                                                result = true;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        if (textureIdApproved) break;
                    }
                }
                if (textureIdApproved) break;
            }

            return result;
        }

        internal static void GetMetallicRoughnessForMaterialID(long materialId, FBXNode root, out string roughnessFilename, out byte[] roughnessData, out string metallicFilename, out byte[] metallicData, out float roughness, out float metallic)
        {
            roughness = 1.0f;
            roughnessData = null;
            roughnessFilename = "";
            metallic = 0.0f;
            metallicData = null;
            metallicFilename = "";

            foreach(FBXNode sibling in root.Siblings)
            {
                if(sibling.Name == "Connections")
                {
                    FBXNode connList = sibling.Children[0];
                    foreach(FBXNode connListSibling in connList.Siblings)
                    {
                        if(connListSibling.Name == "C")
                        {
                            long id = connListSibling.Properties[2].ID;
                            if(id ==  materialId)
                            {
                                if(GetMetallicRoughnessValues(materialId, root, out float tRoughness, out float tMetallic))
                                {
                                    roughness = tRoughness;
                                    metallic = tMetallic;
                                }

                                long textureId = connListSibling.Properties[1].ID;
                                if(GetTextureMetallic(textureId, root, out string mFilename, out byte[] mData))
                                {
                                    metallicFilename = mFilename;
                                    metallicData = mData;
                                }
                                if (GetTextureRoughness(textureId, root, out string mFilenameR, out byte[] mDataR))
                                {
                                    roughnessFilename = mFilenameR;
                                    roughnessData = mDataR;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static GeoTexture GetFBXTextureFilenamesAndData(string filename, string materialName, TextureType type, ref GeoModel model, out float roughness, out float metallic)
        {
            roughness = 1.0f;
            metallic = 0.0f;
            GeoTexture tex = new GeoTexture();
            if (model.AssemblyMode == SceneImporter.AssemblyMode.File)
            {
                byte[] filedata = File.ReadAllBytes(filename);
                if (IsFBXASCII(filedata))
                {
                    // TODO: check if there are old FBX ASCII files that include references to roughness/metallic textures and values...
                }
                else
                {
                    uint version = BitConverter.ToUInt32(filedata, 23);
                    FBXNode root;
                    if (version <= 7400)
                    {
                        root = ReadFBXNodeStructureOld(filedata, 27);
                    }
                    else
                    {
                        root = ReadFBXNodeStructure(filedata, 27);
                    }
                    
                    long materialIdFromFBX = GetMaterialIdFor(materialName, root);
                    if (materialIdFromFBX > 0)
                    {
                        GetMetallicRoughnessForMaterialID(materialIdFromFBX, root, out string roughFile, out byte[] roughData, out string metalFile, out byte[] metalData, out roughness, out metallic);
                        if (type == TextureType.Roughness && roughFile.Length > 0)
                        {
                            string tFilename = roughFile;

                            tex.Type = TextureType.Roughness;
                            tex.UVTransform = new Vector4(1, 1, 0, 0);
                            tex.IsEmbedded = roughData != null;
                            tex.UVMapIndex = 0;
                            if (tex.IsEmbedded)
                            {
                                tFilename = model.Name + "_" + materialName + "_" + GetTextureTypeString(type) + "-EMBEDDED_" + "X" + "." + GetFileEnding(tFilename);
                                if (!ConvertEmbeddedToTemporaryFile(roughData, tFilename, model.Path))
                                {
                                    KWEngine.LogWriteLine("[Import] Temporary image file " + roughFile + " could not be written to disk");
                                }
                            }
                            tex.Filename = tFilename;

                            if (model.Textures.ContainsKey(tFilename))
                            {
                                tex.OpenGLID = model.Textures[tFilename].OpenGLID;
                            }
                            else if (SceneImporter.CheckIfOtherModelsShareTexture(tFilename, model.Path, out GeoTexture sharedTexture))
                            {
                                tex = sharedTexture;
                            }
                            else
                            {
                                tex.OpenGLID = LoadTextureForModelExternal(
                                        SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(tFilename), model.Path), out int mipMaps
                                    );
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
                            }
                        }
                        else if (type == TextureType.Metallic && metalFile.Length > 0)
                        {
                            string tFilename = metalFile;

                            tex.Type = TextureType.Metallic;
                            tex.UVTransform = new Vector4(1, 1, 0, 0);
                            tex.IsEmbedded = metalData != null;
                            tex.UVMapIndex = 0;
                            if (tex.IsEmbedded)
                            {
                                tFilename = model.Name + "_" + materialName + "_" + GetTextureTypeString(type) + "-EMBEDDED_" + "X" + "." + GetFileEnding(tFilename);
                                if (!ConvertEmbeddedToTemporaryFile(metalData, tFilename, model.Path))
                                {
                                    KWEngine.LogWriteLine("[Import] Temporary image file " + roughFile + " could not be written to disk");
                                }
                            }
                            tex.Filename = tFilename;

                            if (model.Textures.ContainsKey(tFilename))
                            {
                                tex.OpenGLID = model.Textures[tFilename].OpenGLID;
                            }
                            else if (SceneImporter.CheckIfOtherModelsShareTexture(tFilename, model.Path, out GeoTexture sharedTexture))
                            {
                                tex = sharedTexture;
                            }
                            else
                            {
                                tex.OpenGLID = LoadTextureForModelExternal(
                                        SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(tFilename), model.Path), out int mipMaps
                                    );
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
                            }
                        }
                    }
                    
                }
            }
            return tex;
        }

        internal static bool CheckFBXNestedListNew(byte[] data, uint endoffset)
        {
            if (endoffset == 0)
                return false;
            // check for 25-NUL-Byte ending of node:
            int endIndex = (int)(endoffset - 1);
            char[] nestedListTerminator = new char[25];
            bool hasNestedList = true;
            for (int i = 0; i < nestedListTerminator.Length; i++)
            {
                nestedListTerminator[i] = (char)data[endIndex - i];
                if (nestedListTerminator[i] != 0)
                {
                    hasNestedList = false;
                }
            }
            return hasNestedList;
        }

        internal static bool CheckFBXNestedList(byte[] data, uint endoffset)
        {
            if (endoffset == 0)
                return false;
            // check for 13-NUL-Byte ending of node:
            int endIndex = (int)(endoffset - 1);
            char[] nestedListTerminator = new char[13];
            bool hasNestedList = true;
            for (int i = 0; i < nestedListTerminator.Length; i++)
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
            for (int i = 0; i < nestingLevel; i++)
            {
                TABS += "\t";
            }
            FBXNode n = new FBXNode();

            int idx = (int)startIndex;
            uint endoffset = (uint)BitConverter.ToUInt64(data, idx);
            n.EndOffset = endoffset;
            bool hasNestedList = CheckFBXNestedListNew(data, endoffset);
            n.HasNestedList = hasNestedList;
            n.Parent = parent;

            idx += 8;
            uint numproperties = (uint)BitConverter.ToUInt64(data, idx);
            n.NumProperties = numproperties;

            idx += 8;
            uint proplistlen = (uint)BitConverter.ToUInt64(data, idx);
            n.PropertyListLen = proplistlen;

            idx += 8;
            byte namelen = data[idx];
            n.NameLen = namelen;

            idx += 1;
            string name = "";
            int start = idx;
            for (int i = idx; i < start + namelen; i++)
            {
                name += (char)data[i];
                idx++;
            }
            n.Name = name;

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
                    idx += 2;
                }
                else if (propertyTypeCode == 'C')
                {
                    // bool in byte (lsbit = 1/0)
                    byte val = data[idx];
                    idx += 1;
                }
                else if (propertyTypeCode == 'I')
                {
                    // int
                    int val = BitConverter.ToInt32(data, idx);
                    idx += 4;
                }
                else if (propertyTypeCode == 'F')
                {
                    // float
                    float val = BitConverter.ToSingle(data, idx);
                    prop.FValue = val;
                    idx += 4;
                }
                else if (propertyTypeCode == 'D')
                {
                    // double
                    double val = BitConverter.ToDouble(data, idx);
                    prop.FValue = Convert.ToSingle(val);
                    idx += 8;
                }
                else if (propertyTypeCode == 'L')
                {
                    //long (signed)
                    long val = BitConverter.ToInt64(data, idx);
                    prop.ID = val;
                    idx += 8;
                }
                else if (propertyTypeCode == 'f')
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
                }
                else if (propertyTypeCode == 'S')
                {
                    uint length = BitConverter.ToUInt32(data, idx);

                    idx += 4;
                    string s = "";
                    for (int j = idx; j < idx + (int)length; j++)
                    {
                        s += (char)data[j];
                    }
                    s = s.Replace("\0", replaceSymbol);
                    prop.Name = s;
                    idx += (int)length;
                }
                else if (propertyTypeCode == 'R')
                {
                    uint length = BitConverter.ToUInt32(data, idx);

                    byte[] arraydata = new byte[length];
                    Array.Copy(data, idx, arraydata, 0, length);
                    prop.RawData = arraydata;
                    idx += (int)length;
                }
                n.Properties.Add(prop);
            }

            // if it has a nested list, save the offset to this list in an offset variable:
            if (hasNestedList)
            {
                n.Children.Add(ReadFBXNodeStructure(data, (uint)idx, n, nestingLevel + 1, (int)n.EndOffset));
            }


            idx = (int)n.EndOffset;

            int end = nestingLevelEndOffset > 0 ? nestingLevelEndOffset - 13 : data.Length;
            if (isSiblingsRead == false)
            {
                while (idx > 0 && idx < end)
                {
                    FBXNode sibling = ReadFBXNodeStructure(data, (uint)idx, parent, nestingLevel, -1, true);
                    n.Siblings.Add(sibling);
                    idx = (int)sibling.EndOffset;
                }
            }
            return n;
        }

        internal static FBXNode ReadFBXNodeStructureOld(byte[] data, uint startIndex, FBXNode parent = null, int nestingLevel = 0, int nestingLevelEndOffset = 0, bool isSiblingsRead = false)
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
                    idx += 2;
                }
                else if(propertyTypeCode == 'C')
                {
                    // bool in byte (lsbit = 1/0)
                    byte val = data[idx];
                    idx += 1;
                }
                else if( propertyTypeCode == 'I')
                {
                    // int
                    int val = BitConverter.ToInt32(data, idx);
                    idx += 4;
                }
                else if (propertyTypeCode == 'F')
                {
                    // float
                    float val = BitConverter.ToSingle(data, idx);
                    prop.FValue = val;
                    idx += 4;
                }
                else if (propertyTypeCode == 'D')
                {
                    // double
                    double val = BitConverter.ToDouble(data, idx);
                    prop.FValue = Convert.ToSingle(val);
                    idx += 8;
                }
                else if(propertyTypeCode == 'L')
                {
                    //long (signed)
                    long val = BitConverter.ToInt64(data, idx);
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
                    s = s.Replace("\0", replaceSymbol);
                    prop.Name = s;
                    idx += (int)length;
                }
                else if (propertyTypeCode == 'R')
                {
                    uint length = BitConverter.ToUInt32(data, idx);

                    byte[] arraydata = new byte[length];
                    Array.Copy(data, idx, arraydata, 0, length);
                    prop.RawData = arraydata;
                    idx += (int)length;
                }
                n.Properties.Add(prop);
            }

            // if it has a nested list, save the offset to this list in an offset variable:
            if (hasNestedList)
            {
                n.Children.Add(ReadFBXNodeStructureOld(data, (uint)idx, n, nestingLevel + 1, (int)n.EndOffset));
            }
            

            idx = (int)n.EndOffset;

            int end = nestingLevelEndOffset > 0 ? nestingLevelEndOffset - 13 : data.Length;
            if (isSiblingsRead == false)
            {
                while (idx > 0 && idx < end)
                {
                    FBXNode sibling = ReadFBXNodeStructureOld(data, (uint)idx, parent, nestingLevel, -1, true);
                    n.Siblings.Add(sibling);
                    idx = (int)sibling.EndOffset;
                }
            }
            return n;
        }

        internal static GeoTexture ProcessTextureForAssimpPBRMaterial(Assimp.Material material, TextureType ttype, ref GeoModel model, out float roughnessVal, out float metallicVal)
        {
            roughnessVal = 1f;
            metallicVal = 0f;
            string roughnessTexture = "";
            string metallicTexture = "";

            if(model.AssemblyMode == SceneImporter.AssemblyMode.Internal)
                return new GeoTexture();

            string filetype = GetFileEnding(model.Filename);
                
            if (filetype == "fbx")
            {
                try
                {
                    return GetFBXTextureFilenamesAndData(model.Filename, material.Name, ttype, ref model, out roughnessVal, out metallicVal);
                }
                catch(Exception)
                {
                    KWEngine.LogWriteLine("[Import] Error reading PBR textures in model " + model.Filename);
                    return new GeoTexture();
                }
            }
            else if(filetype == "obj")
            {
                string[] lines = File.ReadAllLines(model.Filename);
                if(lines != null && lines.Length > 0)
                {
                    foreach(string line in lines)
                    {
                        if(line.StartsWith("mtllib"))
                        {
                            string matfilename = line.Replace("mtllib ", "").Trim();
                            string[] mtlLines = null;
                            try
                            {
                                mtlLines = File.ReadAllLines(SceneImporter.StripFileNameFromPath(model.Filename) + matfilename);
                                int mtOffset = 0;
                                int mtEnd = -1;
                                bool mtFound = false;
                                foreach(string mtline in mtlLines)
                                {
                                    if(mtline == "newmtl " + material.Name)
                                    {
                                        mtFound = true;
                                        break;
                                    }
                                    mtOffset++;
                                }
                                if(mtFound)
                                {
                                    mtEnd = mtlLines.Length - 1;
                                    for (int i = mtOffset + 1; i < mtlLines.Length; i++)
                                    {
                                        if (mtlLines[i] == "" || mtlLines[i].ToLower().StartsWith("newmtl"))
                                        {
                                            mtEnd = i;
                                            break;
                                        }
                                    }

                                    if(mtEnd > mtOffset + 1)
                                    {
                                        for(int i = mtOffset + 1; i < mtEnd; i++)
                                        {
                                            if (mtlLines[i].StartsWith("Pr "))
                                            {
                                                string tmp = mtlLines[i].Substring(2);
                                                float.TryParse(tmp, out roughnessVal);
                                            }
                                            else if (mtlLines[i].StartsWith("Pm "))
                                            {
                                                string tmp = mtlLines[i].Substring(2);
                                                float.TryParse(tmp, out metallicVal);
                                            }
                                            else if (mtlLines[i].StartsWith("map_Pr "))
                                            {
                                                roughnessTexture = mtlLines[i].Substring(6).Trim();
                                            }
                                            else if (mtlLines[i].StartsWith("map_Pm "))
                                            {
                                                metallicTexture = mtlLines[i].Substring(6).Trim();
                                            }
                                        }
                                    }

                                    if(ttype == TextureType.Roughness && roughnessTexture.Length > 0)
                                    {
                                        GeoTexture tex = new GeoTexture();
                                        tex.UVTransform = new Vector4(1, 1, 0, 0);
                                        tex.Filename = HelperGeneral.EqualizePathDividers(roughnessTexture);
                                        tex.IsEmbedded = false;
                                        tex.UVMapIndex = 0;
                                        tex.Type = ttype;

                                        if (model.Textures.ContainsKey(roughnessTexture))
                                        {
                                            tex.OpenGLID = model.Textures[roughnessTexture].OpenGLID;
                                        }
                                        else if (SceneImporter.CheckIfOtherModelsShareTexture(roughnessTexture, model.Path, out GeoTexture sharedTexture))
                                        {
                                            tex = sharedTexture;
                                        }
                                        else
                                        {
                                            tex.OpenGLID = LoadTextureForModelExternal(
                                                    SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(roughnessTexture), model.Path), out int mipMaps
                                                );
                                            tex.MipMaps = mipMaps;

                                            if (tex.OpenGLID > 0)
                                            {
                                                if (GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                                                {
                                                    tex.Width = width;
                                                    tex.Height = height;
                                                }
                                            }
                                        }
                                        return tex;
                                    }
                                    else if(ttype == TextureType.Metallic && metallicTexture.Length > 0)
                                    {
                                        GeoTexture tex = new GeoTexture();
                                        tex.UVTransform = new Vector4(1, 1, 0, 0);
                                        tex.Filename = HelperGeneral.EqualizePathDividers(metallicTexture);
                                        tex.IsEmbedded = false;
                                        tex.UVMapIndex = 0;
                                        tex.Type = ttype;

                                        if (model.Textures.ContainsKey(metallicTexture))
                                        {
                                            tex.OpenGLID = model.Textures[metallicTexture].OpenGLID;
                                        }
                                        else if (SceneImporter.CheckIfOtherModelsShareTexture(metallicTexture, model.Path, out GeoTexture sharedTexture))
                                        {
                                            tex = sharedTexture;
                                        }
                                        else
                                        {
                                            tex.OpenGLID = LoadTextureForModelExternal(
                                                    SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(metallicTexture), model.Path), out int mipMaps
                                                );
                                            tex.MipMaps = mipMaps;

                                            if (tex.OpenGLID > 0)
                                            {
                                                if (GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                                                {
                                                    tex.Width = width;
                                                    tex.Height = height;
                                                }
                                            }
                                        }
                                        return tex;
                                    }
                                }
                            }
                            catch(Exception)
                            {
                                KWEngine.LogWriteLine("[Import] Error reading PBR textures in model " + model.Filename);
                                return new GeoTexture();
                            }
                            break;
                        }
                    }
                }
            }
            return new GeoTexture();
        }

        internal static GeoTexture ProcessTextureForAssimpMaterial(Assimp.Material material, TextureType ttype, Assimp.Scene scene, ref GeoModel model, bool isBumpInsteadOfNormalMap = false)
        {
            GeoTexture tex = new GeoTexture();
            tex.UVTransform = new Vector4(1, 1, 0, 0);
            tex.UVMapIndex = GetUVIndex(material, ttype);
            tex.Type = ttype;

            Assimp.TextureType att = GetAssimpTextureType(ttype, isBumpInsteadOfNormalMap);
            if (material.GetMaterialTexture(att, GetTextureSlotIndex(material, ttype, isBumpInsteadOfNormalMap), out Assimp.TextureSlot texSlot))
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
                                tFilename = model.Name + "_" + material.Name + "_" + GetTextureTypeString(tex.Type) + "-EMBEDDED_" + texSlot.TextureIndex + "_" + index + "." + embedded.CompressedFormatHint;
                                tex.IsEmbedded = true;
                                tex.Filename = tFilename;
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
                        tex = sharedTexture;
                    }
                    else
                    {
                        tex.OpenGLID = LoadTextureForModelExternal(
                                SceneImporter.FindTextureInSubs(SceneImporter.StripPathFromFile(tFilename), model.Path), out int mipMaps
                            );
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

            if (tex.IsEmbedded && tex.Filename.Length > 0)
            {
                bool deleted = DeleteTemporaryFile(tex.Filename, model.Path);
            }

            return tex;
        }

        internal static GeoTexture ProcessTextureForMaterial(TextureType textureType, Assimp.Material material, Assimp.Scene scene, ref GeoModel model, bool isSpecularAsRoughness, out float roughnessVal, out float metallicVal)
        {
            if(isSpecularAsRoughness == false && (textureType == TextureType.Roughness || textureType == TextureType.Metallic))
            {
                return ProcessTextureForAssimpPBRMaterial(material, textureType, ref model, out roughnessVal, out metallicVal);
            }
            else
            {
                roughnessVal = 1.0f;
                metallicVal = 0.0f;
                return ProcessTextureForAssimpMaterial(material, textureType, scene, ref model);
            }
        }

        internal static GeoTexture ProcessTextureForMaterialNormalBump(Assimp.Material material, Assimp.Scene scene, ref GeoModel model, bool isSpecularAsRoughness, out float roughnessVal, out float metallicVal)
        {
            roughnessVal = 1.0f;
            metallicVal = 0.0f;
            return ProcessTextureForAssimpMaterial(material, TextureType.Normal, scene, ref model, true);
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

        internal static void SaveTextureToBitmapR8(int texId, int width, int height, string name = null)
        {
            SKBitmap b = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
            byte[] data = new byte[width * height];
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            int x = 0, y = height - 1;
            for (int i = 0; i < data.Length; i++)
            {
                byte red = (byte)(data[i] * 100);
                b.SetPixel(x, y, new SKColor(red, red, red));
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
        internal static int CreateEmptyCubemapTexture(bool is3d = false)
        {
            int texID = GL.GenTexture();
            if (is3d)
            {
                GL.BindTexture(TextureTarget.TextureCubeMapArray, texID);
                byte[] pxColor = new byte[] { 0, 0, 0 };

                GL.TexImage3D(TextureTarget.TextureCubeMapArray, 0, PixelInternalFormat.Rgb, 1, 1, 6, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);

                GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            }
            else
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, texID);
                byte[] pxColor = new byte[] { 0, 0, 0 };

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgb, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pxColor);

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

                GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            }
            return texID;
        }

        internal static int CreateEmptyCubemapDepthTexture()
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

        internal static int CreateEmptyDepthTexture()
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

        internal static int LoadTextureCompressedWithMipMaps(string filename)
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

        internal static int LoadTextureCompressedWithMipMaps(Stream stream)
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

        internal static int LoadFontTextureCompressedNoMipMap(Stream s)
        {
            int texID = -1;
            bool error = false;
            using (HelperDDS dds = new HelperDDS(s))
            {
                if (dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 || dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT5)
                {
                    texID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texID);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT1 ?  InternalFormat.CompressedRgbaS3tcDxt1Ext : dds.DDSPixelFormat == HelperDDS.PixelFormat.DXT3 ? InternalFormat.CompressedRgbaS3tcDxt3Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext, dds.BitmapImage.Width, dds.BitmapImage.Height, 0, dds.Data.Length, dds.Data);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
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


        internal static int LoadTextureCompressedNoMipMap(string fileName)
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

        internal static int LoadTextureCompressedWithMipMapInternal(string fileName)
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

        /// <summary>
        /// Rundet eine Ganzzahl auf, so dass sie der nächsthöheren Zweierpotenz entspricht
        /// </summary>
        /// <param name="value">aufzurundende Zahl</param>
        /// <returns>nächsthöhere Zweierpotenz</returns>
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

        /// <summary>
        /// Erfragt die Auflösung einer Bilddatei in Pixeln
        /// </summary>
        /// <remarks>Ist das Bild ungültig, wird eine Auflösung von 0 Pixeln zurückgegeben</remarks>
        /// <param name="filename">Bilddateiname</param>
        /// <returns>Auflösung in Pixeln</returns>
        public static Vector2i GetResolutionFromImage(string filename)
        {
            Vector2i res = new();

            if (filename == null)
            {
                KWEngine.LogWriteLine("[Texture] Invalid file: '" + filename + "'");
            }
            else
            {
                using (SKBitmap image = SKBitmap.Decode(HelperGeneral.EqualizePathDividers(filename)))
                {
                    if (image == null)
                    {
                        KWEngine.LogWriteLine("[Texture] Invalid image data");
                    }
                    else
                    {
                        res.X = image.Width;
                        res.Y = image.Height;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Rundet eine Ganzzahl ab, so dass sie der nächstniedrigeren Zweierpotenz entspricht
        /// </summary>
        /// <param name="value">abzurundende Zahl</param>
        /// <returns>nächstniedrigere Zweierpotenz</returns>
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

                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
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

        internal static int LoadTextureFromAssembly(string resourceName, Assembly assembly, bool noFiltering = false)
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

                int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                if (noFiltering)
                {
                    //GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    mipMapCount = 0;
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }
                
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            return texID;
        }

        internal static int LoadTextureFromAssembly3D(string resourceName, Assembly assembly)
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
                GL.BindTexture(TextureTarget.Texture2DArray, texID);
                byte[] data = image.Bytes;
                if (image.ColorType == SKColorType.Rgba8888)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 2, 0,
                     PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Gray8)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R8, image.Width, image.Height, 2, 0,
                     PixelFormat.Red, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Rgb888x)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 2, 0,
                     PixelFormat.Rgb, PixelType.UnsignedByte, data);
                }
                else if (image.ColorType == SKColorType.Bgra8888)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 2, 0,
                     PixelFormat.Bgra, PixelType.UnsignedByte, data);
                }
                else
                {
                    data = image.Bytes;
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 2, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, data);
                }

                GL.TexParameter(TextureTarget.Texture2DArray, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
                int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
            }
            return texID;
        }

        internal static int LoadTextureInternal(string filename, bool noFiltering = false)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "KWEngine3.Assets.Textures." + filename;
            return LoadTextureFromAssembly(resourceName, assembly, noFiltering);
        }

        internal static int LoadTextureInternal3D(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "KWEngine3.Assets.Textures." + filename;
            return LoadTextureFromAssembly3D(resourceName, assembly);
        }

        internal static int LoadTextureForHeightMap(SKBitmap image, out int mipMaps)
        {
            int texID = GL.GenTexture();
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
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                 PixelFormat.Bgra, PixelType.UnsignedByte, data);
            }

            //GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            //int mipMapCount = GetMaxMipMapLevels(image.Width, image.Height);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Math.Max(0, mipMapCount - 2));
            mipMaps = 0;
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texID;
        }

        internal static int LoadTextureForModelExternal(string filename, out int mipMaps)
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

                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
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

        internal static int LoadTextureForModelGLB(byte[] rawTextureData, out int mipMaps)
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
                    
                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
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

        internal static int LoadTextureForModelInternalExecutingAssembly(string filename, out int mipMaps)
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
                    if (image.ColorType == SKColorType.Rgba8888 || image.ColorType == SKColorType.Bgra8888)
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

                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
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

        internal static int LoadTextureForModelInternal(string filename, out int mipMaps)
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

                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, KWEngine.Window.AnisotropicFilteringLevel);
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

        internal static int LoadTextureForBackgroundExternal(string filename, out int mipMapLevels)
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
