using glTFLoader;
using glTFLoader.Schema;
using KWEngine3.Helper;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Reflection;
using static KWEngine3.Model.SceneImporter;

namespace KWEngine3.Model
{
    internal static class SceneImporterGLTF
    {
        internal static List<GeoAnimation> LoadAnimations(string filename)
        {
            List<GeoAnimation> animations = new List<GeoAnimation>();

            filename = HelperGeneral.EqualizePathDividers(filename);
            try
            {
                Gltf scene = Interface.LoadModel(filename);
                GeoModel dummy = new GeoModel();
                dummy.Filename = filename;
                SetModelPaths(ref dummy, filename); // the buffers are read relative to the file
                ProcessAnimations(scene, ref dummy);
                if (dummy.Animations != null && dummy.Animations.Count > 0)
                {
                    animations.AddRange(dummy.Animations);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                ClearImportCache();
            }
            return animations;
        }

        internal static GeoModel LoadModel(string filename, bool flipTextureCoordinates = false)
        {
            filename = HelperGeneral.EqualizePathDividers(filename.Trim());
            Gltf scene;
            try
            {
                scene = Interface.LoadModel(filename);
            }
            catch (Exception)
            {
                return null;
            }

            if (scene.Scenes.Length != 1)
            {
                KWEngine.LogWriteLine("Model " + filename + " contains of more than one scene. This is not supported.");
                return null;

            }
            if (scene.Skins != null && scene.Skins.Length > 1)
            {
                KWEngine.LogWriteLine("Cannot load gltf files with more than one skeletal armature.");
                return null;
            }

            GeoModel model = ProcessScene(scene, filename);
            return model;
        }

        private static GeoModel ProcessScene(Gltf scene, string filename)
        {
            GeoModel returnModel = new GeoModel();
            returnModel.Filename = filename;
            returnModel.Name = StripPathFromFile(filename);
            SetModelPaths(ref returnModel, filename);
            returnModel.Meshes = new();
            returnModel.TransformGlobalInverse = Matrix4.Identity;
            returnModel.Textures = new Dictionary<string, GeoTexture>();
            returnModel.IsValid = false;

            bool result;
            try
            {
                GenerateNodeHierarchy(scene, ref returnModel);
                FindRootBone(scene.Scenes[0], ref scene, ref returnModel);
                result = ProcessBones(scene, ref returnModel);
                if (result)
                    result = ProcessMeshes(scene, ref returnModel);
                ProcessAnimations(scene, ref returnModel);
            }
            finally
            {
                ClearImportCache();
            }

            returnModel.IsValid = result;
            return returnModel;
        }

        private static void SetModelPaths(ref GeoModel model, string filename)
        {
            string p = HelperGeneral.EqualizePathDividers(Assembly.GetExecutingAssembly().Location);
            string pA = HelperGeneral.EqualizePathDividers(new DirectoryInfo(StripFileNameFromPath(p)).FullName);
            if (!Path.IsPathRooted(filename))
            {
                model.PathAbsolute = HelperGeneral.EqualizePathDividers(Path.Combine(pA, filename));
            }
            else
            {
                model.PathAbsolute = filename;
            }

            model.GLBOffset = GetGlbOffset(model.PathAbsolute, ref model);
            model.AssemblyMode = AssemblyMode.File;
            model.CalculatePath();
        }

        /*
        private static Node GetNode(Gltf scene, int index)
        {
            return null;
        }
        

        private static Node GetRootNode()
        {
            return null;
        }
        */

        private static void GenerateNodeHierarchy(Gltf gltf, ref GeoModel model)
        {
            Scene scene = gltf.Scenes[0];
            GeoNode root = new GeoNode();
            root.Name = "KWRootGLTF";
            root.Transform = Matrix4.Identity;
            root.Parent = null;
            model.Root = root;
            model.NodesWithoutHierarchy.Add(root);
            foreach (int childIndex in scene.Nodes)
            {
                Node child = gltf.Nodes[childIndex];
                root.Children.Add(MapNodeToNode(child, ref gltf, ref model, ref root));

            }
        }

        private static GeoNode MapNodeToNode(Node n, ref Gltf scene, ref GeoModel model, ref GeoNode callingNode)
        {

            GeoNode gNode = new GeoNode();
            gNode.Parent = callingNode;
            gNode.Transform = HelperMatrix.ConvertGLTFTRSToOpenTKMatrix(n.Scale, n.Rotation, n.Translation);
            gNode.Name = n.Name;
            gNode.NameWithoutFBXSuffix = n.Name;
            model.NodesWithoutHierarchy.Add(gNode);
            if (n.Children != null)
            {
                foreach (int childIndex in n.Children)
                {
                    Node child = scene.Nodes[childIndex];
                    gNode.Children.Add(MapNodeToNode(child, ref scene, ref model, ref gNode));
                }
            }
            return gNode;
        }

        private static Node GetNodeForBone(Node node, Skin b, ref Gltf gltf)
        {
            foreach (int nIndex in node.Children)
            {
                Node n = gltf.Nodes[nIndex];
                if (n.Name == b.Name)
                {
                    return n;
                }
                else
                {
                    Node nodeCandidate = GetNodeForBone(n, b, ref gltf);
                    if (nodeCandidate != null)
                        return nodeCandidate;
                }
            }
            return null;
        }

        private static void FindRootBone(Scene scene, ref Gltf gltf, ref GeoModel model)
        {
            if (gltf.Skins != null && gltf.Skins.Length > 0 && gltf.Skins[0].Joints.Length > 0)
            {
                string armatureNodeName = gltf.Skins[0].Name;
                foreach (GeoNode n in model.NodesWithoutHierarchy)
                {
                    if (n.Name == armatureNodeName)
                    {
                        model.Armature = n;
                        return;
                    }
                }
            }
        }

        private static void ProcessBoneStructure(Gltf scene, ref GeoModel model, ref Node currentNode)
        {
            if (currentNode.Skin == null)
            {
                // this is a bone
                GeoBone geoBone = new GeoBone();
                geoBone.Name = currentNode.Name;
                //geoBone.Index = localBoneIndex++;
                //geoBone.Offset = FindInverseBindMatrixForBone(scene, childNode, ref model);
                if (!model.BoneNames.Contains(geoBone.Name))
                {
                    model.BoneNames.Add(geoBone.Name);
                }

            }
            if (currentNode.Children != null)
            {
                foreach (int childIndex in currentNode.Children)
                    ProcessBoneStructure(scene, ref model, ref scene.Nodes[childIndex]);
            }
        }

        private static bool ProcessBones(Gltf scene, ref GeoModel model)
        {
            // TODO: Check if this is even needed anymore...
            if (model.Armature == null)
            {
                return true;
            }

            Node armature = null;
            foreach (Node n in scene.Nodes)
            {
                if (n.Name == model.Armature.Name)
                {
                    // found armature node
                    armature = n;
                    break;
                }
            }
            if (armature != null)
            {
                model.HasBones = true;
                int localBoneIndex = 0;
                foreach (int armatureChildNodeId in armature.Children)
                {
                    Node childNode = scene.Nodes[armatureChildNodeId];
                    if (childNode.Skin == null)
                    {
                        // this is a bone
                        GeoBone geoBone = new GeoBone();
                        geoBone.Name = childNode.Name;
                        geoBone.Index = localBoneIndex++;
                        geoBone.Offset = FindInverseBindMatrixForBone(scene, childNode, ref model);
                        if (!model.BoneNames.Contains(geoBone.Name))
                        {
                            model.BoneNames.Add(geoBone.Name);
                        }

                    }
                    ProcessBoneStructure(scene, ref model, ref childNode);

                }
            }

            return model.BoneNames.Count <= KWEngine.MAX_BONES;
        }

        private static Matrix4 FindInverseBindMatrixForBone(Gltf scene, Node boneNode, ref GeoModel model)
        {
            int accessorId = scene.Skins[0].InverseBindMatrices == null ? -1 : (int)scene.Skins[0].InverseBindMatrices;
            if (accessorId < 0)
                return Matrix4.Identity;

            Accessor a = scene.Accessors[accessorId];
            if (a.Type == Accessor.TypeEnum.MAT4)
            {
                GltfImportCache cache = GetImportCache(scene);
                if (cache.InverseBindMatrices == null)
                {
                    cache.InverseBindMatrices = GetInverseBindMatricesFromAccessor(a, scene, ref model);
                    cache.JointIndexByName = new Dictionary<string, int>();
                    for (int i = 0; i < scene.Skins[0].Joints.Length; i++)
                    {
                        // first joint with that name wins (like the former linear search)
                        cache.JointIndexByName.TryAdd(GetJointKey(scene.Nodes[scene.Skins[0].Joints[i]].Name), i);
                    }
                }
                if (cache.JointIndexByName.TryGetValue(GetJointKey(boneNode.Name), out int jointIndex))
                {
                    return cache.InverseBindMatrices[jointIndex];
                }
                return Matrix4.Identity;
            }
            else
            {
                return Matrix4.Identity;
            }
        }

        private static GeoBone CreateBone(Gltf scene, ref GeoModel model)
        {
            GeoBone bone = new GeoBone();
            //
            return bone;
        }

        private static Matrix4[] GetInverseBindMatricesFromAccessor(Accessor a, Gltf scene, ref GeoModel model)
        {
            Matrix4[] matrices = new Matrix4[a.Count];
            byte[] data = GetByteDataFromAccessor(scene, a, ref model);

            int offset = 0;
            // analyse data array:
            for (int i = 0; i < matrices.Length; i++)
            {
                float[] tmpArray = new float[16];
                for (int j = 0; j < tmpArray.Length; j++)
                {
                    float f = BitConverter.ToSingle(data, offset);
                    tmpArray[j] = f;
                    offset += 4;
                }
                matrices[i] = HelperMatrix.ConvertGLTFFloatArraytoOpenTKMatrix(tmpArray);
            }

            // return final matrices
            return matrices;
        }

        private static bool FindTransformForMesh(Gltf scene, Node currentNode, Mesh mesh, ref Matrix4 transform, out string nodeName, out Node node, ref Matrix4 parentTransform)
        {

            Matrix4 currentNodeTransform = parentTransform * HelperMatrix.ConvertGLTFTRSToOpenTKMatrix(currentNode.Scale, currentNode.Rotation, currentNode.Translation);
            if (currentNode.Mesh != null)
            {
                Mesh tmpMesh = scene.Meshes[(int)currentNode.Mesh];
                if (tmpMesh.Name == mesh.Name)
                {
                    transform = currentNodeTransform;
                    nodeName = currentNode.Name;
                    node = currentNode;
                    return true;
                }
            }
            if (currentNode.Children != null)
            {
                for (int i = 0; i < currentNode.Children.Length; i++)
                {
                    Node child = scene.Nodes[currentNode.Children[i]];
                    bool found = FindTransformForMesh(scene, child, mesh, ref transform, out string nName, out Node node2, ref currentNodeTransform);
                    if (found)
                    {
                        nodeName = nName;
                        node = node2;
                        return true;
                    }
                }
            }

            transform = Matrix4.Identity;
            nodeName = null;
            node = null;
            return false;
        }

        internal static string StripFileNameFromPath(string path)
        {
            path = HelperGeneral.EqualizePathDividers(path);
            int index = path.LastIndexOf(Path.AltDirectorySeparatorChar);
            if (index < 0)
            {
                return path;
            }
            else
            {
                return path.Substring(0, index + 1);
            }

        }

        internal static string StripFileNameFromAssemblyPath(string path)
        {
            int index = path.LastIndexOf('.');
            if (index < 0)
            {
                return path;
            }
            else
            {
                index = path.LastIndexOf('.', index - 1);
                if (index < 0)
                {
                    return path;
                }
                else
                {
                    return path.Substring(0, index + 1);
                }
            }
        }

        internal static string StripPathFromFile(string fileWithPath)
        {
            fileWithPath = HelperGeneral.EqualizePathDividers(fileWithPath);
            int index = fileWithPath.LastIndexOf(Path.AltDirectorySeparatorChar);
            if (index < 0)
            {
                return fileWithPath;
            }
            else
            {
                return fileWithPath.Substring(index + 1);
            }
        }

        internal static string FindTextureInSubs(string filename, string path = null)
        {
            DirectoryInfo currentDir;
            if (path == null)
            {
                string p = Assembly.GetExecutingAssembly().Location;
                currentDir = new DirectoryInfo(StripFileNameFromPath(p));
            }
            else
            {
                currentDir = new DirectoryInfo(StripFileNameFromPath(path));
            }

            foreach (FileInfo fi in currentDir.GetFiles())
            {
                if (fi.Name == StripPathFromFile(filename))
                {
                    // file found:
                    return HelperGeneral.EqualizePathDividers(fi.FullName);
                }
            }

            if (currentDir.GetDirectories().Length == 0)
            {
                Debug.WriteLine("File " + filename + " not found anywhere.");
            }
            else
            {
                foreach (DirectoryInfo di in currentDir.GetDirectories())
                {
                    return FindTextureInSubs(filename, di.FullName);
                }
            }

            return "";
        }

        private static void ProcessMaterialsForMesh(Gltf scene, Mesh mesh, MeshPrimitive currentPrimitive, ref GeoModel model, ref GeoMesh geoMesh, Dictionary<string, int> glbTextures, Dictionary<int, GeoMaterial> materialCache = null)
        {
            GeoMaterial geoMaterial = new GeoMaterial() { TextureAlbedo = new GeoTexture() };
            geoMaterial.AttachedToMesh = mesh.Name;
            int materialId = currentPrimitive.Material == null ? -1 : (int)currentPrimitive.Material;

            // Invisible meshes need their own material instance (alpha = 0), so they bypass the cache.
            bool isInvisible = (mesh.Name != null && mesh.Name.ToLower().Contains("_invisible"))
                            || (geoMesh != null && geoMesh.Name.ToLower().Contains("_invisible"));
            if (!isInvisible && materialCache != null && materialCache.TryGetValue(materialId, out GeoMaterial cached))
            {
                // the cached material was created for another mesh, so store the name of this mesh
                cached.AttachedToMesh = mesh.Name;
                geoMesh.Material = cached;
                return;
            }

            if (materialId < 0)
            {
                geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
                geoMaterial.TextureRoughnessIsSpecular = false;
                geoMaterial.Roughness = 1;
                geoMaterial.Metallic = 0;
                geoMaterial.ColorAlbedo.W = 1;
                if (
                            (mesh.Name != null && mesh.Name.ToLower().Contains("_invisible"))
                            ||
                            (geoMesh != null && geoMesh.Name.ToLower().Contains("_invisible"))
                        )
                {
                    geoMaterial.ColorAlbedo.W = 0;
                }
            }
            else
            {
                Material material = scene.Materials[materialId];
                geoMaterial.RenderBackFace = material.DoubleSided;
                geoMaterial.Name = material.Name;
                geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                geoMaterial.ColorAlbedo = new Vector4(material.PbrMetallicRoughness.BaseColorFactor[0], material.PbrMetallicRoughness.BaseColorFactor[1], material.PbrMetallicRoughness.BaseColorFactor[2], material.PbrMetallicRoughness.BaseColorFactor[3]);
                geoMaterial.ColorEmissive = new Vector4(material.EmissiveFactor[0], material.EmissiveFactor[1], material.EmissiveFactor[2], 1f);
                geoMaterial.Metallic = material.PbrMetallicRoughness.MetallicFactor;
                geoMaterial.Roughness = material.PbrMetallicRoughness.RoughnessFactor;
                if (
                    (mesh.Name != null && mesh.Name.ToLower().Contains("_invisible"))
                    ||
                    (geoMesh != null && geoMesh.Name.ToLower().Contains("_invisible"))
                )
                {
                    geoMaterial.ColorAlbedo.W = 0;
                }

                int glTexIdTemp = -1;

                // Color/Diffuse texture:
                if (material.PbrMetallicRoughness.BaseColorTexture != null)
                {
                    Texture tinfo = scene.Textures[material.PbrMetallicRoughness.BaseColorTexture.Index];
                    int sourceId = tinfo.Source != null ? (int)tinfo.Source : -1;
                    int sampleId = tinfo.Sampler != null ? (int)tinfo.Sampler : -1;
                    if (sourceId >= 0 && sourceId < scene.Images.Length)
                    {
                        Image i = scene.Images[sourceId];
                        bool texFoundInSameFile = false;
                        if (i.Name != null)
                        {
                            texFoundInSameFile = glbTextures.TryGetValue(i.Name, out glTexIdTemp);
                        }
                        else
                        {
                            i.Name = "GLTF-albmat-" + materialId;
                        }
                        GeoTexture tex = new GeoTexture();
                        bool duplicateFound = CheckIfOtherModelsShareTexture(i.Uri, model.Path, out tex);
                        if (!duplicateFound)
                        {
                            string filename = "";
                            int glTextureId;
                            if (texFoundInSameFile)
                            {
                                glTextureId = glTexIdTemp;
                            }
                            else
                            {
                                byte[] rawTextureData = GetTextureDataFromAccessor(scene, i, ref model, out filename);
                                glTextureId = HelperTexture.LoadTextureForModelGLB(rawTextureData, out int mipMaps);
                                glbTextures.Add(i.Name, glTextureId);
                            }

                            tex.UVTransform = GetTextureRepeatValues(material.PbrMetallicRoughness.BaseColorTexture);
                            tex.Filename = texFoundInSameFile ? i.Name : filename;
                            tex.UVMapIndex = material.PbrMetallicRoughness.BaseColorTexture.TexCoord;
                            tex.Type = TextureType.Albedo;
                            tex.OpenGLID = glTextureId;

                            if (HelperTexture.GetTextureDimensions(tex.OpenGLID, out int width, out int height))
                            {
                                tex.Width = width;
                                tex.Height = height;
                            }
                        }
                        geoMaterial.TextureAlbedo = tex;
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] Invalid texture index found in model, skipping texture");
                    }
                }

                // Normal map:
                if (material.NormalTexture != null)
                {
                    MaterialNormalTextureInfo tinfo = material.NormalTexture;
                    int sourceId = tinfo.Index;
                    if (sourceId >= 0 && sourceId < scene.Images.Length)
                    {
                        glTFLoader.Schema.Image i = scene.Images[sourceId];
                        bool texFoundInSameFile = false;
                        if (i.Name != null)
                        {
                            texFoundInSameFile = glbTextures.TryGetValue(i.Name, out glTexIdTemp);
                        }
                        else
                        {
                            i.Name = "GLTF-nrmmat-" + materialId;
                        }
                        GeoTexture tex = new GeoTexture();
                        bool duplicateFound = CheckIfOtherModelsShareTexture(i.Uri, model.Path, out tex);
                        if (!duplicateFound)
                        {
                            string filename = "";
                            int glTextureId;
                            if (texFoundInSameFile)
                            {
                                glTextureId = glTexIdTemp;
                            }
                            else
                            {
                                byte[] rawTextureData = GetTextureDataFromAccessor(scene, i, ref model, out filename);
                                glTextureId = HelperTexture.LoadTextureForModelGLB(rawTextureData, out int mipMaps);
                                glbTextures.Add(i.Name, glTextureId);
                            }
                            tex.UVTransform = GetTextureRepeatValues(material.PbrMetallicRoughness.BaseColorTexture);
                            tex.Filename = texFoundInSameFile ? i.Name : filename;
                            tex.UVMapIndex = material.NormalTexture.TexCoord;
                            tex.Type = TextureType.Normal;
                            tex.OpenGLID = glTextureId;
                        }
                        geoMaterial.TextureNormal = tex;
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] Invalid texture index found in model, skipping texture");
                    }
                }

                // Metallic/roughness texture:
                if (material.PbrMetallicRoughness.MetallicRoughnessTexture != null)
                {

                    geoMaterial.TextureRoughnessInMetallic = true;
                    Texture tinfo = scene.Textures[material.PbrMetallicRoughness.MetallicRoughnessTexture.Index];
                    int sourceId = tinfo.Source != null ? (int)tinfo.Source : -1;
                    int sampleId = tinfo.Sampler != null ? (int)tinfo.Sampler : -1;
                    if (sourceId >= 0 && sourceId < scene.Images.Length)
                    {
                        Image i = scene.Images[sourceId];
                        bool texFoundInSameFile = false;
                        if (i.Name != null)
                        {
                            texFoundInSameFile = glbTextures.TryGetValue(i.Name, out glTexIdTemp);
                        }
                        else
                        {
                            i.Name = "GLTF-pbrmat-" + materialId;
                        }

                        bool duplicateFound = CheckIfOtherModelsShareTexture(i.Uri, model.Path, out GeoTexture tex);
                        if (!duplicateFound)
                        {
                            string filename = "";
                            int glTextureId;
                            if (texFoundInSameFile)
                            {
                                glTextureId = glTexIdTemp;
                            }
                            else
                            {
                                byte[] rawTextureData = GetTextureDataFromAccessor(scene, i, ref model, out filename);
                                glTextureId = HelperTexture.LoadTextureForModelGLB(rawTextureData, out int mipMaps);
                                glbTextures.Add(i.Name, glTextureId);
                            }
                            tex.UVTransform = GetTextureRepeatValues(material.PbrMetallicRoughness.MetallicRoughnessTexture);
                            tex.Filename = texFoundInSameFile ? i.Name : filename;
                            tex.UVMapIndex = material.PbrMetallicRoughness.MetallicRoughnessTexture.TexCoord;
                            tex.Type = TextureType.Metallic;
                            tex.OpenGLID = glTextureId;
                        }
                        geoMaterial.TextureMetallic = tex;
                        geoMaterial.TextureRoughnessInMetallic = true;
                        geoMaterial.TextureRoughnessIsSpecular = true; // TODO: wtf was i thinking here?
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] Invalid texture index found in model, skipping texture");
                    }
                }

                // Emissive texture
                if (material.EmissiveTexture != null)
                {
                    TextureInfo tinfo = material.EmissiveTexture;
                    int sourceId = tinfo.Index;
                    if (sourceId >= 0 && sourceId < scene.Images.Length)
                    {
                        Image i = scene.Images[sourceId];
                        bool texFoundInSameFile = false;
                        if (i.Name != null)
                        {
                            texFoundInSameFile = glbTextures.TryGetValue(i.Name, out glTexIdTemp);
                        }
                        else
                        {
                            i.Name = "GLTF-emimat-" + materialId;
                        }
                        GeoTexture tex = new GeoTexture();
                        bool duplicateFound = CheckIfOtherModelsShareTexture(i.Uri, model.Path, out tex);
                        if (!duplicateFound)
                        {
                            string filename = "";
                            int glTextureId;
                            if (texFoundInSameFile)
                            {
                                glTextureId = glTexIdTemp;
                            }
                            else
                            {
                                byte[] rawTextureData = GetTextureDataFromAccessor(scene, i, ref model, out filename);
                                glTextureId = HelperTexture.LoadTextureForModelGLB(rawTextureData, out int mipMaps);
                                glbTextures.Add(i.Name, glTextureId);
                            }
                            tex.UVTransform = GetTextureRepeatValues(material.EmissiveTexture);
                            tex.Filename = texFoundInSameFile ? i.Name : filename;
                            tex.UVMapIndex = material.EmissiveTexture.TexCoord;
                            tex.Type = TextureType.Emissive;
                            tex.OpenGLID = glTextureId;
                        }
                        geoMaterial.TextureEmissive = tex;
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] Invalid texture index found in model, skipping texture");
                    }
                }
            }
            if (!isInvisible && materialCache != null && !materialCache.ContainsKey(materialId))
                materialCache[materialId] = geoMaterial;
            geoMesh.Material = geoMaterial;
        }

        private static Vector2 GetTextureRepeatValuesFromDictionary(Dictionary<string, object> dict)
        {
            if (dict != null && dict.ContainsKey("KHR_texture_transform"))
            {
                JObject transforms = (JObject)dict["KHR_texture_transform"];
                if (transforms.HasValues)
                {
                    foreach (JToken child in transforms.Children())
                    {
                        if (child.Path == "scale")
                        {
                            try
                            {
                                var uv = child.Value<JProperty>().First;
                                var x = uv.First;
                                var y = uv.Last;
                                float xf = x.Value<float>();
                                float yf = y.Value<float>();
                                return new Vector2(xf, yf);
                            }
                            catch (Exception)
                            {
                                return new Vector2(1, 1);
                            }

                        }
                    }
                }

                return new Vector2(1, 1);
            }
            else
            {
                return new Vector2(1, 1);
            }
        }

        private static Vector4 GetTextureRepeatValues(TextureInfo textureInfo)
        {
            Vector4 value = new Vector4();
            value.Xy = GetTextureRepeatValuesFromDictionary(textureInfo.Extensions);
            return value;
        }

        private static Vector2 GetTextureRepeatValues(MaterialNormalTextureInfo textureInfo)
        {
            return GetTextureRepeatValuesFromDictionary(textureInfo.Extensions);
        }

        private static bool CheckIfOtherModelsShareTexture(string texture, string path, out GeoTexture sharedTex)
        {
            sharedTex = new GeoTexture();
            foreach (string key in KWEngine.Models.Keys)
            {
                GeoModel m = KWEngine.Models[key];
                if (m.Path == path)
                {
                    foreach (string texKey in m.Textures.Keys)
                    {
                        if (texKey == texture)
                        {
                            sharedTex = m.Textures[texKey];
                            return true;
                        }
                    }
                }

            }
            return false;
        }

        private static int GetGlbOffset(string path, ref GeoMeshCollider collider)
        {
            if (!path.ToLower().EndsWith(".glb"))
            {
                return 0;
            }
            if (collider.GlbOffset >= 0)
            {
                return collider.GlbOffset;
            }
            return ReadGlbBinaryChunkOffset(path);
        }

        private static int GetGlbOffset(string pathAbsolute, ref GeoModel model)
        {
            if (model.GLBOffset >= 0)
            {
                return model.GLBOffset;
            }
            if (!pathAbsolute.ToLower().EndsWith(".glb"))
            {
                return 0;
            }
            return ReadGlbBinaryChunkOffset(pathAbsolute);
        }

        // GLB layout: 12 byte header, JSON chunk (8 byte chunk header + data), BIN chunk (8 byte chunk header + data).
        // Returns the file position of the BIN chunk data (0 if there is none).
        private static int ReadGlbBinaryChunkOffset(string path)
        {
            const uint GLB_MAGIC = 0x46546C67;      // "glTF"
            const uint CHUNK_TYPE_BIN = 0x004E4942; // "BIN\0"
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (stream.Length < 20 || reader.ReadUInt32() != GLB_MAGIC)
                {
                    return 0;
                }
                stream.Position = 12;
                long jsonChunkLength = reader.ReadUInt32();
                long binChunkHeader = 20 + jsonChunkLength;
                if (binChunkHeader + 8 > stream.Length)
                {
                    return 0;
                }
                stream.Position = binChunkHeader + 4;
                if (reader.ReadUInt32() != CHUNK_TYPE_BIN)
                {
                    return 0;
                }
                return (int)(binChunkHeader + 8);
            }
        }

        // Data of one file import: every buffer is loaded (or base64-decoded) only once.
        private sealed class GltfImportCache
        {
            public Gltf Scene;
            public byte[][] Buffers;
            public Matrix4[] InverseBindMatrices;
            public Dictionary<string, int> JointIndexByName;
        }

        private static GltfImportCache _importCache = null;

        private static GltfImportCache GetImportCache(Gltf scene)
        {
            if (_importCache == null || _importCache.Scene != scene)
            {
                _importCache = new GltfImportCache()
                {
                    Scene = scene,
                    Buffers = new byte[scene.Buffers != null ? scene.Buffers.Length : 0][]
                };
            }
            return _importCache;
        }

        private static void ClearImportCache()
        {
            _importCache = null;
        }

        private static string GetJointKey(string nodeName)
        {
            return nodeName ?? "\0"; // dictionary keys must not be null
        }

        private static bool IsBase64BufferUri(string uri)
        {
            return uri != null && uri.StartsWith("data:application/octet-stream;base64");
        }

        private static byte[] GetBufferData(Gltf scene, int bufferIndex, string filePath, string directory, int glbOffset)
        {
            GltfImportCache cache = GetImportCache(scene);
            if (cache.Buffers[bufferIndex] != null)
            {
                return cache.Buffers[bufferIndex];
            }

            byte[] data;
            glTFLoader.Schema.Buffer buffer = scene.Buffers[bufferIndex];
            if (buffer.Uri == null)
            {
                if (!filePath.ToLower().EndsWith(".glb"))
                {
                    KWEngine.LogWriteLine("[Import] GLTF buffer without uri found");
                    return null;
                }
                // binary chunk of the glb file itself
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    long length = Math.Max(0, Math.Min(buffer.ByteLength, stream.Length - glbOffset));
                    data = new byte[length];
                    stream.Position = glbOffset;
                    stream.ReadExactly(data, 0, data.Length);
                }
            }
            else if (IsBase64BufferUri(buffer.Uri))
            {
                int commapos = buffer.Uri.IndexOf(',');
                if (commapos < 0)
                {
                    KWEngine.LogWriteLine("[Import] GLTF base64 stream invalid");
                    return null;
                }
                data = Convert.FromBase64String(buffer.Uri.Substring(commapos + 1));
            }
            else
            {
                data = File.ReadAllBytes(directory + Path.AltDirectorySeparatorChar + buffer.Uri);
            }
            cache.Buffers[bufferIndex] = data;
            return data;
        }

        private static int GetComponentCount(Accessor a)
        {
            switch (a.Type)
            {
                case Accessor.TypeEnum.SCALAR: return 1;
                case Accessor.TypeEnum.VEC2: return 2;
                case Accessor.TypeEnum.VEC3: return 3;
                case Accessor.TypeEnum.VEC4: return 4;
                case Accessor.TypeEnum.MAT2: return 4;
                case Accessor.TypeEnum.MAT3: return 9;
                case Accessor.TypeEnum.MAT4: return 16;
                default: return 1;
            }
        }

        // Copies only the bytes that belong to the accessor (several accessors may share one buffer view).
        private static byte[] CopyAccessorData(byte[] buffer, BufferView bufferView, Accessor a)
        {
            if (buffer == null)
                return null;

            int start = bufferView.ByteOffset + a.ByteOffset;
            int length = a.Count * GetComponentCount(a) * GetBytesPerData(a);
            int maxLength = Math.Min(bufferView.ByteLength - a.ByteOffset, buffer.Length - start);
            if (length > maxLength)
                length = Math.Max(0, maxLength);

            byte[] data = new byte[length];
            Array.Copy(buffer, start, data, 0, length);
            return data;
        }

        private static byte[] GetTextureDataFromAccessor(Gltf scene, glTFLoader.Schema.Image i, ref GeoModel model, out string filename)
        {
            filename = null;
            if (i.MimeType != glTFLoader.Schema.Image.MimeTypeEnum.image_jpeg && i.MimeType != glTFLoader.Schema.Image.MimeTypeEnum.image_png)
            {
                KWEngine.LogWriteLine("[Import] Texture in GLTF file is not JPG/PNG");
                return null;
            }
            byte[] data = null;
            if (i.BufferView != null)
            {
                BufferView bvTexture = scene.BufferViews[(int)i.BufferView];
                if (bvTexture.ByteStride != null)
                {
                    KWEngine.LogWriteLine("[Import] GLTF stride attribute not supported");
                    return null;
                }
                bool isGlb = model.PathAbsolute.ToLower().EndsWith(".glb");
                if (!isGlb && !IsBase64BufferUri(scene.Buffers[bvTexture.Buffer].Uri))
                {
                    KWEngine.LogWriteLine("[Import] GLTF base64 stream invalid");
                    return null;
                }
                byte[] buffer = GetBufferData(scene, bvTexture.Buffer, model.PathAbsolute, model.Path, GetGlbOffset(model.PathAbsolute, ref model));
                if (buffer == null)
                    return null;
                int length = Math.Max(0, Math.Min(bvTexture.ByteLength, buffer.Length - bvTexture.ByteOffset));
                data = new byte[length];
                Array.Copy(buffer, bvTexture.ByteOffset, data, 0, length);
            }
            else
            {
                using (FileStream stream = File.Open(model.Path + Path.AltDirectorySeparatorChar + i.Uri, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    data = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(data, 0, (int)stream.Length);
                }
                filename = i.Uri;
            }
            return data;
        }

        private static byte[] GetByteDataFromAccessor(Gltf scene, Accessor a, ref GeoModel model)
        {
            if (a == null)
                return null;

            BufferView bufferView = scene.BufferViews[(int)a.BufferView];
            if (bufferView.ByteStride != null)
            {
                KWEngine.LogWriteLine("[Import] GLTF stride attribute not supported");
                return null;
            }
            byte[] buffer = GetBufferData(scene, bufferView.Buffer, model.PathAbsolute, model.Path, model.GLBOffset);
            return CopyAccessorData(buffer, bufferView, a);
        }

        private static byte[] GetByteDataFromAccessor(Gltf scene, Accessor a, ref GeoMeshCollider collider)
        {
            if (a == null)
                return null;

            BufferView bufferView = scene.BufferViews[(int)a.BufferView];
            if (bufferView.ByteStride != null)
            {
                KWEngine.LogWriteLine("[Import] GLTF stride attribute not supported");
                return null;
            }
            byte[] buffer = GetBufferData(scene, bufferView.Buffer, collider.FileName, SceneImporter.StripFileNameFromPath(collider.FileName), collider.GlbOffset);
            return CopyAccessorData(buffer, bufferView, a);
        }

        private static GeoVertex[] GetVertexDataForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoModel model, Node node, out float xmin, out float xmax, out float ymin, out float ymax, out float zmin, out float zmax, out List<Vector3> uniqueVertices, out List<int> uniqueBoneIds)
        {
            xmin = float.MaxValue;
            xmax = float.MinValue;
            ymin = float.MaxValue;
            ymax = float.MinValue;
            zmin = float.MaxValue;
            zmax = float.MinValue;
            uniqueVertices = new List<Vector3>();

            int vertexIndex = mprim.Attributes["POSITION"];
            int bonesIndex = mprim.Attributes.ContainsKey("JOINTS_0") ? mprim.Attributes["JOINTS_0"] : -1;
            int weightsIndex = mprim.Attributes.ContainsKey("WEIGHTS_0") ? mprim.Attributes["WEIGHTS_0"] : -1;
            Accessor indexAccessor = scene.Accessors[vertexIndex];
            Accessor bonesAccessor = null;
            Accessor weightsAccessor = null;
            if (bonesIndex >= 0 && weightsIndex >= 0)
            {
                bonesAccessor = scene.Accessors[bonesIndex];
                weightsAccessor = scene.Accessors[weightsIndex];
            }

            byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref model);
            byte[] dataBones = GetByteDataFromAccessor(scene, bonesAccessor, ref model);
            byte[] dataWeights = GetByteDataFromAccessor(scene, weightsAccessor, ref model);

            int bytesPerData = GetBytesPerData(indexAccessor);
            int bytesPerDataBones = GetBytesPerData(bonesAccessor);
            int bytesPerDataWeights = GetBytesPerData(weightsAccessor);
            uint[] bonesData = null;
            float[] weightsData = null;

            float[] verticesData = new float[data.Length / bytesPerData];
            if (bytesPerDataBones > 0 && bytesPerDataWeights > 0)
            {
                bonesData = new uint[dataBones.Length / bytesPerDataBones];
                weightsData = new float[dataWeights.Length / bytesPerDataWeights];
            }
            List<GeoVertex> verticesArray = new List<GeoVertex>();
            bool isFullHitbox = IsFullHitboxNode(node);
            HashSet<Vector3> uniqueVertexSet = isFullHitbox ? new HashSet<Vector3>() : null;
            for (int i = 0, j = 0, cswitch = 0; i < data.Length; i += bytesPerData)
            {
                if (bytesPerData == 1)
                {
                    verticesData[j++] = data[i];
                    //TODO: CAUTION - this method will not work if this branch is entered
                }
                else if (bytesPerData == 2)
                {
                    verticesData[j++] = BitConverter.ToUInt16(data, i);
                    //TODO: CAUTION - this method will not work if this branch is entered
                }
                else
                {
                    verticesData[j] = BitConverter.ToSingle(data, i);

                    if (cswitch == 0)
                    {
                        if (verticesData[j] < xmin)
                            xmin = verticesData[j];
                        if (verticesData[j] > xmax)
                            xmax = verticesData[j];
                    }
                    else if (cswitch == 1)
                    {
                        if (verticesData[j] < ymin)
                            ymin = verticesData[j];
                        if (verticesData[j] > ymax)
                            ymax = verticesData[j];
                    }
                    else
                    {
                        if (verticesData[j] < zmin)
                            zmin = verticesData[j];
                        if (verticesData[j] > zmax)
                            zmax = verticesData[j];

                        Vector3 tmpVertex = new Vector3(verticesData[j - 2], verticesData[j - 1], verticesData[j]);
                        verticesArray.Add(new GeoVertex(j, tmpVertex.X, tmpVertex.Y, tmpVertex.Z)); // moved from down below (testing)

                        if (isFullHitbox && uniqueVertexSet.Add(tmpVertex))
                        {
                            uniqueVertices.Add(tmpVertex);
                        }
                        //verticesArray.Add(new GeoVertex(j, tmpVertex.X, tmpVertex.Y, tmpVertex.Z));
                    }

                    cswitch = (cswitch + 1) % 3;
                    j++;
                }
            }

            uniqueBoneIds = new List<int>();
            int numberOfWeightsActuallySet = 0;
            if (bytesPerDataBones > 0 && bytesPerDataWeights > 0)
            {
                for (int i = 0; i < verticesArray.Count * 4; i++)
                {
                    // if four values have been collected, update the GeoVertex instance appropriately:
                    if (i > 0 && i % 4 == 0)
                    {
                        GeoVertex vertex = verticesArray[(i - 1) / 4];
                        int tmpWeightsSet = FillGeoVertexWithBoneIdsAndWeights(bonesData[i - 4], bonesData[i - 3], bonesData[i - 2], bonesData[i - 1], weightsData[i - 4], weightsData[i - 3], weightsData[i - 2], weightsData[i - 1], ref vertex, out List<int> bIdsLast);
                        if (tmpWeightsSet > numberOfWeightsActuallySet)
                        {
                            numberOfWeightsActuallySet = tmpWeightsSet;
                        }
                        foreach (int bId in bIdsLast)
                        {
                            if (!uniqueBoneIds.Contains(bId))
                                uniqueBoneIds.Add(bId);
                        }
                    }

                    if (bytesPerDataBones == 1)
                        bonesData[i] = dataBones[i];
                    else if (bytesPerDataBones == 2)
                    {
                        bonesData[i] = BitConverter.ToUInt16(dataBones, i * bytesPerDataBones);
                    }
                    else
                    {
                        bonesData[i] = BitConverter.ToUInt32(dataBones, i * bytesPerDataBones);
                    }

                    if (bytesPerDataWeights == 1)
                        weightsData[i] = dataWeights[i];
                    else if (bytesPerDataWeights == 2)
                    {
                        weightsData[i] = BitConverter.ToInt16(dataWeights, i * bytesPerDataWeights);

                    }
                    else
                    {
                        weightsData[i] = BitConverter.ToSingle(dataWeights, i * bytesPerDataWeights);
                    }
                }

                // Handle last vertex:
                int lastVertexIndex = verticesArray.Count - 1;
                int lastVertexIndex2 = verticesArray.Count * 4 - 1;
                GeoVertex vertexLast = verticesArray[lastVertexIndex];
                int tmpWeightsSetLast = FillGeoVertexWithBoneIdsAndWeights(bonesData[lastVertexIndex2 - 4], bonesData[lastVertexIndex2 - 3], bonesData[lastVertexIndex2 - 2], bonesData[lastVertexIndex2 - 1], weightsData[lastVertexIndex2 - 4], weightsData[lastVertexIndex2 - 3], weightsData[lastVertexIndex2 - 2], weightsData[lastVertexIndex2 - 1], ref vertexLast, out List<int> bIds);
                if (tmpWeightsSetLast > numberOfWeightsActuallySet)
                {
                    numberOfWeightsActuallySet = tmpWeightsSetLast;
                }
                foreach (int bId in bIds)
                {
                    if (!uniqueBoneIds.Contains(bId))
                        uniqueBoneIds.Add(bId);
                }

            }
            return verticesArray.ToArray();
        }

        private static List<Vector3> GetVertexDataForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoMeshCollider collider, Node node, out float xmin, out float xmax, out float ymin, out float ymax, out float zmin, out float zmax, ref List<Vector3> uniqueVertices, Dictionary<Vector3, int> uniqueVertexIndices)
        {
            xmin = float.MaxValue;
            xmax = float.MinValue;
            ymin = float.MaxValue;
            ymax = float.MinValue;
            zmin = float.MaxValue;
            zmax = float.MinValue;

            List<Vector3> verticesArray = new();

            int vertexIndex = mprim.Attributes["POSITION"];

            Accessor indexAccessor = scene.Accessors[vertexIndex];
            byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref collider);

            int bytesPerData = GetBytesPerData(indexAccessor);

            float[] verticesData = new float[data.Length / bytesPerData];
            for (int i = 0, j = 0, cswitch = 0; i < data.Length; i += bytesPerData)
            {
                if (bytesPerData == 1)
                {
                    verticesData[j++] = data[i];
                    //TODO: CAUTION - this method will not work if this branch is entered
                }
                else if (bytesPerData == 2)
                {
                    verticesData[j++] = BitConverter.ToUInt16(data, i);
                    //TODO: CAUTION - this method will not work if this branch is entered
                }
                else
                {
                    verticesData[j] = BitConverter.ToSingle(data, i);

                    if (cswitch == 0)
                    {
                        if (verticesData[j] < xmin)
                            xmin = verticesData[j];
                        if (verticesData[j] > xmax)
                            xmax = verticesData[j];
                    }
                    else if (cswitch == 1)
                    {
                        if (verticesData[j] < ymin)
                            ymin = verticesData[j];
                        if (verticesData[j] > ymax)
                            ymax = verticesData[j];
                    }
                    else
                    {
                        if (verticesData[j] < zmin)
                            zmin = verticesData[j];
                        if (verticesData[j] > zmax)
                            zmax = verticesData[j];

                        Vector3 tmpVertex = new Vector3(verticesData[j - 2], verticesData[j - 1], verticesData[j]);
                        verticesArray.Add(tmpVertex);

                        if (uniqueVertexIndices.TryAdd(tmpVertex, uniqueVertices.Count))
                        {
                            uniqueVertices.Add(tmpVertex);
                        }
                    }

                    cswitch = (cswitch + 1) % 3;
                    j++;
                }
            }
            return verticesArray;
        }

        private static int FillGeoVertexWithBoneIdsAndWeights(uint b0, uint b1, uint b2, uint b3, float w0, float w1, float w2, float w3, ref GeoVertex vertex, out List<int> boneIds)
        {
            Vector3 weight = new Vector3(w0, w1, w2);
            boneIds = new List<int>();

            vertex.BoneIDs[0] = b0;
            vertex.Weights[0] = weight.X;
            vertex.BoneIDs[1] = b1;
            vertex.Weights[1] = weight.Y;
            vertex.BoneIDs[2] = b2;
            vertex.Weights[2] = weight.Z;
            if (weight.X > 0)
            {
                vertex.WeightSet++;
                boneIds.Add((int)b0);
            }
            if (weight.Y > 0)
            {
                vertex.WeightSet++;
                boneIds.Add((int)b1);
            }
            if (weight.Z > 0)
            {
                vertex.WeightSet++;
                boneIds.Add((int)b2);
            }

            return vertex.WeightSet;
        }

        private static float[] GetNormalDataForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoModel model, Node node, out List<Vector3> normalDataUnique)
        {
            int vertexIndex = (int)mprim.Attributes["NORMAL"];
            Accessor indexAccessor = scene.Accessors[vertexIndex];

            byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref model);
            int bytesPerData = GetBytesPerData(indexAccessor);
            float[] normalData = new float[data.Length / bytesPerData];
            normalDataUnique = new List<Vector3>();
            for (int i = 0, j = 0; i < data.Length; i += bytesPerData)
            {
                if (bytesPerData == 1)
                {
                    normalData[j++] = data[i];
                }
                else if (bytesPerData == 2)
                {
                    normalData[j++] = BitConverter.ToUInt16(data, i);
                }
                else
                {
                    normalData[j++] = BitConverter.ToSingle(data, i);
                }
            }

            if (IsFullHitboxNode(node))
            {
                HashSet<Vector3> normalSet = new HashSet<Vector3>();
                for (int i = 0; i < normalData.Length; i += 3)
                {
                    Vector3 n = new Vector3(normalData[i], normalData[i + 1], normalData[i + 2]);
                    if (normalSet.Add(n))
                    {
                        normalDataUnique.Add(n);
                    }
                }
            }

            return normalData;
        }

        private static float[] GetTangentDataForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoModel model)
        {
            if (mprim.Attributes.ContainsKey("TANGENT"))
            {
                int vertexIndex = (int)mprim.Attributes["TANGENT"];
                Accessor indexAccessor = scene.Accessors[vertexIndex];

                byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref model);

                int bytesPerData = GetBytesPerData(indexAccessor);
                float[] tangentData = new float[data.Length / bytesPerData];
                for (int i = 0, j = 0; i < data.Length; i += bytesPerData)
                {
                    if (bytesPerData == 1)
                    {
                        tangentData[j++] = data[i];
                    }
                    else if (bytesPerData == 2)
                    {
                        tangentData[j++] = BitConverter.ToUInt16(data, i);
                    }
                    else
                    {
                        tangentData[j++] = BitConverter.ToSingle(data, i);
                    }
                }
                return tangentData;
            }
            else
                return null;

        }

        private static float[] GetUVDataForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoModel model, int index = 0)
        {
            if (mprim.Attributes.ContainsKey("TEXCOORD_" + index))
            {
                int vertexIndex = (int)mprim.Attributes["TEXCOORD_" + index];
                Accessor indexAccessor = scene.Accessors[vertexIndex];

                byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref model);

                int bytesPerData = GetBytesPerData(indexAccessor);
                float[] uvData = new float[data.Length / bytesPerData];
                for (int i = 0, j = 0; i < data.Length; i += bytesPerData)
                {
                    if (bytesPerData == 1)
                    {
                        uvData[j++] = data[i];
                    }
                    else if (bytesPerData == 2)
                    {
                        uvData[j++] = BitConverter.ToUInt16(data, i);
                    }
                    else
                    {
                        uvData[j++] = BitConverter.ToSingle(data, i);
                    }
                }
                return uvData;
            }
            else
                return null;
        }


        private static List<uint> GetIndicesForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoMeshCollider collider)
        {
            Accessor indexAccessor = scene.Accessors[(int)mprim.Indices];
            byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref collider);
            if (data == null)
                return null;
            return ConvertIndexData(data, GetBytesPerData(indexAccessor)).ToList();
        }

        private static uint[] GetIndicesForMeshPrimitive(Gltf scene, MeshPrimitive mprim, ref GeoModel model)
        {
            Accessor indexAccessor = scene.Accessors[(int)mprim.Indices];
            byte[] data = GetByteDataFromAccessor(scene, indexAccessor, ref model);
            if (data == null)
                return null;
            return ConvertIndexData(data, GetBytesPerData(indexAccessor));
        }

        private static uint[] ConvertIndexData(byte[] data, int bytesPerData)
        {
            uint[] indicesData = new uint[data.Length / bytesPerData];
            for (int i = 0, j = 0; i < data.Length; i += bytesPerData)
            {
                if (bytesPerData == 1)
                {
                    indicesData[j++] = data[i];
                }
                else if (bytesPerData == 2)
                {
                    indicesData[j++] = BitConverter.ToUInt16(data, i);
                }
                else
                {
                    indicesData[j++] = BitConverter.ToUInt32(data, i);
                }
            }
            return indicesData;
        }

        private static int GetBytesPerData(Accessor a)
        {
            if (a != null)
            {
                if (a.ComponentType == Accessor.ComponentTypeEnum.BYTE)
                    return 1;
                else if (a.ComponentType == Accessor.ComponentTypeEnum.SHORT || a.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
                    return 2;
                else if (a.ComponentType == Accessor.ComponentTypeEnum.FLOAT || a.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_INT)
                    return 4;
                else
                    return 1;
            }
            else
                return -1;
        }

        private static bool ProcessMeshes(Gltf scene, ref GeoModel model)
        {
            int glbOffset = -1;
            Dictionary<string, int> previouslyImportedGLBTextures = new Dictionary<string, int>();
            Dictionary<int, GeoMaterial> materialCache = new Dictionary<int, GeoMaterial>();

            model.MeshCollider.MeshHitboxes = new List<GeoMeshHitbox>();
            string currentNodeName = null;
            Matrix4 currentNodeTransform = Matrix4.Identity;
            GeoMeshHitbox meshHitBox = null;
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            Matrix4 nodeTransform = Matrix4.Identity;
            Mesh mesh = null;
            for (int m = 0; m < scene.Meshes.Length; m++)
            {
                Node currentNode = null;
                mesh = scene.Meshes[m];
                if (mesh.Primitives[0].Mode != MeshPrimitive.ModeEnum.TRIANGLES)
                {
                    return false;
                }
                Matrix4 parentTransform = Matrix4.Identity;
                bool transformFound = false;
                string nodeName = "";
                foreach (Node node in scene.Nodes)
                {
                    transformFound = FindTransformForMesh(scene, node, mesh, ref nodeTransform, out nodeName, out Node targetNode, ref parentTransform);
                    if (transformFound)
                    {
                        currentNode = targetNode;
                        break;
                    }
                }
                if (!transformFound)
                {
                    KWEngine.LogWriteLine("[Import] Mesh '" + mesh.Name + "' in model " + model.Name + " is not used by any node - skipped");
                    continue;
                }

                minX = float.MaxValue;
                minY = float.MaxValue;
                minZ = float.MaxValue;

                maxX = float.MinValue;
                maxY = float.MinValue;
                maxZ = float.MinValue;

                List<Vector3> uniqueVerticesForWholeMesh = new List<Vector3>();
                List<Vector3> uniqueNormalsForWholeMesh = new List<Vector3>();
                Dictionary<Vector3, int> uniqueVertexIndicesForWholeMesh = new Dictionary<Vector3, int>();
                HashSet<Vector3> uniqueNormalSetForWholeMesh = new HashSet<Vector3>();
                List<GeoMeshFaceHelper> facesForWholeMesh = new List<GeoMeshFaceHelper>();

                for (int m2 = 0; m2 < mesh.Primitives.Length; m2++)
                {
                    MeshPrimitive mprim = mesh.Primitives[m2];
                    GeoMesh geoMesh = new GeoMesh();
                    uint[] indices = GetIndicesForMeshPrimitive(scene, mprim, ref model);
                    if (glbOffset >= 0)
                    {
                        model.GLBOffset = glbOffset;
                    }

                    geoMesh.Vertices = GetVertexDataForMeshPrimitive(scene, mprim, ref model, currentNode, out float xmin, out float xmax, out float ymin, out float ymax, out float zmin, out float zmax, out List<Vector3> uniqueVertices, out List<int> boneIds);
                    if (xmin <= xmax && ymin <= ymax && zmin <= zmax)
                        geoMesh.CenterLocal = new Vector3((xmin + xmax) * 0.5f, (ymin + ymax) * 0.5f, (zmin + zmax) * 0.5f);

                    float[] normals = GetNormalDataForMeshPrimitive(scene, mprim, ref model, currentNode, out List<Vector3> uniqueNormals);

                    float[] tangents = GetTangentDataForMeshPrimitive(scene, mprim, ref model);

                    float[] uvs = GetUVDataForMeshPrimitive(scene, mprim, ref model, 0);

                    float[] uvs2 = GetUVDataForMeshPrimitive(scene, mprim, ref model, 1);

                    if (tangents == null || tangents.Length == 0)
                    {
                        if (uvs != null && normals != null && indices != null)
                        {
                            tangents = GenerateTangentsFrom(geoMesh.Vertices, normals, uvs, indices);
                        }
                        else
                        {
                            KWEngine.LogWriteLine("[Import] Could not rebuild tangent vectors for model '" + model.Name + "'. Normal maps won't work.");

                        }
                    }

                    if (xmin < minX)
                        minX = xmin;
                    if (xmax > maxX)
                        maxX = xmax;
                    if (ymin < minY)
                        minY = ymin;
                    if (ymax > maxY)
                        maxY = ymax;
                    if (zmin < minZ)
                        minZ = zmin;
                    if (zmax > maxZ)
                        maxZ = zmax;

                    geoMesh.Transform = nodeTransform;
                    geoMesh.Terrain = null;

                    geoMesh.Name = mesh.Name + " #" + m2.ToString().PadLeft(4, '0') + " (Node: " + nodeName + ")";
                    currentNodeName = nodeName;
                    currentNodeTransform = nodeTransform;
                    geoMesh.NameOrg = mesh.Name;
                    geoMesh.Primitive = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;
                    geoMesh.IndexCount = indices.Length;

                    if (model.HasBones)
                    {
                        geoMesh.BoneTranslationMatrixCount = boneIds.Count;
                        ProcessMeshBones(scene, currentNode, ref model, ref geoMesh, boneIds);
                    }

                    geoMesh.VAOGenerateAndBind();
                    geoMesh.VBOGenerateIndices(indices);
                    geoMesh.VBOGenerateVerticesAndBones(model.HasBones);
                    geoMesh.VBOGenerateNormals(normals);
                    geoMesh.VBOGenerateTangents(normals, tangents);
                    geoMesh.VBOGenerateTextureCoords1(uvs);

                    ProcessMaterialsForMesh(scene, mesh, mprim, ref model, ref geoMesh, previouslyImportedGLBTextures, materialCache);

                    geoMesh.VAOUnbind();
                    model.Meshes.Add(geoMesh.Name, geoMesh);

                    if (currentNodeName.ToLower().Contains("_fullhitbox"))
                    {
                        List<GeoMeshFaceHelper> meshFaces = GenerateFacesForExtendedHitbox(indices, geoMesh.Vertices);
                        //geoMesh.Vertices = null; // not needed anymore, let the GC clear it!
                        facesForWholeMesh.AddRange(meshFaces);

                        foreach (Vector3 normal in uniqueNormals)
                        {
                            if (uniqueNormalSetForWholeMesh.Add(normal))
                            {
                                uniqueNormalsForWholeMesh.Add(normal);
                            }
                        }
                        foreach (Vector3 vertex in uniqueVertices)
                        {
                            if (uniqueVertexIndicesForWholeMesh.TryAdd(vertex, uniqueVerticesForWholeMesh.Count))
                            {
                                uniqueVerticesForWholeMesh.Add(vertex);
                            }
                        }
                    }
                    geoMesh.Vertices = null; // not needed anymore, let the GC clear it!
                }

                List<GeoMeshFace> facesForHitbox = null;
                if (currentNodeName.ToLower().Contains("_fullhitbox"))
                {
                    facesForHitbox = new List<GeoMeshFace>();
                    List<Vector3> faceNormals = new List<Vector3>();
                    foreach (GeoMeshFaceHelper face in facesForWholeMesh)
                    {
                        FindIndicesForFace(ref face.Vertices[0], ref face.Vertices[1], ref face.Vertices[2], uniqueVertexIndicesForWholeMesh, uniqueNormalsForWholeMesh, out int indexVertex1, out int indexVertex2, out int indexVertex3, out int indexNormal);
                        Vector3 normal = uniqueNormalsForWholeMesh[indexNormal];
                        //if (!faceNormals.Contains(normal))
                        {
                            faceNormals.Add(normal);
                            GeoMeshFace tmpFace = new GeoMeshFace(indexNormal, false, indexVertex1, indexVertex2, indexVertex3);
                            facesForHitbox.Add(tmpFace);
                        }
                    }
                }

                // Generate hitbox for the mesh:
                meshHitBox = new GeoMeshHitbox(maxX, maxY, maxZ, minX, minY, minZ, currentNodeName.ToLower().Contains("_fullhitbox") ? uniqueNormalsForWholeMesh : null, currentNodeName.ToLower().Contains("_fullhitbox") ? uniqueVerticesForWholeMesh : null, facesForHitbox);
                meshHitBox.Model = model;
                meshHitBox.Name = mesh.Name;
                meshHitBox.Transform = currentNodeTransform;
                meshHitBox.TransformInverse = Matrix4.Invert(currentNodeTransform);
                if (!currentNodeName.ToLower().Contains("_nohitbox"))
                    model.MeshCollider.MeshHitboxes.Add(meshHitBox);
            }
            return true;
        }

        private static List<GeoMeshFaceHelper> GenerateFacesForExtendedHitbox(uint[] meshIndices, GeoVertex[] meshVertices)
        {
            List<GeoMeshFaceHelper> faces = new List<GeoMeshFaceHelper>();

            for (int i = 0; i < meshIndices.Length; i += 3)
            {
                GeoVertex v1 = meshVertices[(int)meshIndices[i + 0]];
                GeoVertex v2 = meshVertices[(int)meshIndices[i + 1]];
                GeoVertex v3 = meshVertices[(int)meshIndices[i + 2]];

                GeoMeshFaceHelper tmpFace = new GeoMeshFaceHelper(v1, v2, v3);
                faces.Add(tmpFace);
            }
            return faces;
        }

        private static bool IsFullHitboxNode(Node node)
        {
            return node != null && node.Name != null && node.Name.ToLower().Contains("_fullhitbox");
        }

        // Same result as GeoHelper.FindIndexOfVertexInList(), but the vertex indices come from a dictionary instead of a linear search.
        private static void FindIndicesForFace(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, Dictionary<Vector3, int> vertexIndices, List<Vector3> normalsForWholeMesh, out int iv1, out int iv2, out int iv3, out int in1)
        {
            iv1 = vertexIndices.TryGetValue(v1, out int index1) ? index1 : -1;
            iv2 = vertexIndices.TryGetValue(v2, out int index2) ? index2 : -1;
            iv3 = vertexIndices.TryGetValue(v3, out int index3) ? index3 : -1;
            in1 = -1;

            Vector3 n = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
            for (int i = 0; i < normalsForWholeMesh.Count; i++)
            {
                float dot = Vector3.Dot(n, normalsForWholeMesh[i]);
                if (dot >= (1 - GeoHelper.ERROR_MARGIN) && dot <= 1 + GeoHelper.ERROR_MARGIN)
                {
                    in1 = i;
                    break;
                }
            }

            if (in1 < 0 || iv1 < 0 || iv2 < 0 || iv3 < 0)
            {
                throw new Exception("Could not find appropriate face normal index in hitbox' normal array.");
            }
        }

        private static void ProcessMeshBones(Gltf scene, Node n, ref GeoModel model, ref GeoMesh geoMesh, List<int> uniqueBoneIds)
        {

            if (n.Mesh != null && n.Skin != null && scene.Meshes[(int)n.Mesh].Name == geoMesh.NameOrg)
            {
                Skin s = scene.Skins[(int)n.Skin];
                if (s.Joints != null)
                {
                    foreach (int index in s.Joints)
                    {
                        geoMesh.BoneNames.Add(scene.Nodes[index].Name);
                        geoMesh.BoneIndices.Add(index);
                        Matrix4 boneOffsetMatrix = FindInverseBindMatrixForBone(scene, scene.Nodes[index], ref model);
                        geoMesh.BoneOffset.Add(boneOffsetMatrix);
                        geoMesh.BoneOffsetInverse.Add(Matrix4.Invert(boneOffsetMatrix));
                    }
                }
            }

        }

        private static Vector3[] GetTranslationOrScaleValues(Gltf scene, Accessor a, ref GeoModel model)
        {
            List<Vector3> validValues = new List<Vector3>();
            byte[] data = GetByteDataFromAccessor(scene, a, ref model);

            for (int i = 0; i < data.Length; i += 12)
            {
                float tx = BitConverter.ToSingle(data, i);
                float ty = BitConverter.ToSingle(data, i + 4);
                float tz = BitConverter.ToSingle(data, i + 8);
                Vector3 t = new Vector3(tx, ty, tz);
                validValues.Add(t);
            }

            return validValues.ToArray();
        }

        private static Quaternion[] GetRotationValues(Gltf scene, Accessor a, ref GeoModel model)
        {
            List<Quaternion> validQuaternions = new List<Quaternion>();
            //Quaternion[] values = new Quaternion[a.Count];
            byte[] data = GetByteDataFromAccessor(scene, a, ref model);

            for (int i = 0; i < data.Length; i += 16)
            {
                float rx = BitConverter.ToSingle(data, i);
                float ry = BitConverter.ToSingle(data, i + 4);
                float rz = BitConverter.ToSingle(data, i + 8);
                float rw = BitConverter.ToSingle(data, i + 12);
                Quaternion r = new Quaternion(rx, ry, rz, rw);
                if (r.LengthSquared > 0)
                    validQuaternions.Add(r);
            }

            return validQuaternions.ToArray();
        }

        private static float[] GetTimestampValues(Gltf scene, Accessor a, ref GeoModel model, out float duration)
        {
            float[] values = new float[a.Count];
            byte[] data = GetByteDataFromAccessor(scene, a, ref model);
            duration = BitConverter.ToSingle(data, data.Length - 4);
            for (int i = 0, counter = 0; i < data.Length; i += 4)
            {
                float ts = BitConverter.ToSingle(data, i);
                values[counter++] = ts;
            }
            return values;
        }

        private static void ProcessAnimations(Gltf scene, ref GeoModel model)
        {
            if (scene.Animations != null)
            {
                model.Animations = new List<GeoAnimation>();

                foreach (Animation a in scene.Animations)
                {
                    GeoAnimation ga = new GeoAnimation();
                    ga.Name = a.Name;
                    ga.AnimationChannels = new Dictionary<string, GeoNodeAnimationChannel>();
                    if (a.Channels != null)
                    {
                        Dictionary<string, GeoNodeAnimationChannel> channels = new Dictionary<string, GeoNodeAnimationChannel>();
                        List<GeoAnimationKeyframe> rotationKeys = new List<GeoAnimationKeyframe>();
                        List<GeoAnimationKeyframe> scaleKeys = new List<GeoAnimationKeyframe>();
                        List<GeoAnimationKeyframe> translationKeys = new List<GeoAnimationKeyframe>();

                        foreach (AnimationChannel nac in a.Channels)
                        {
                            string targetBoneName = scene.Nodes[(int)nac.Target.Node].Name;
                            GeoNodeAnimationChannel gnac = null;
                            // Translation:
                            if (nac.Target.Path == AnimationChannelTarget.PathEnum.translation)
                            {
                                AnimationSampler sampler = a.Samplers[nac.Sampler];
                                Accessor accInputTimestamps = scene.Accessors[sampler.Input];
                                Accessor accOutputValues = scene.Accessors[sampler.Output];
                                Vector3[] translationKeyValues = GetTranslationOrScaleValues(scene, accOutputValues, ref model);
                                float[] translationKeyTimestamps = GetTimestampValues(scene, accInputTimestamps, ref model, out float duration);
                                if (ga.DurationInTicks == 0)
                                    ga.DurationInTicks = duration;
                                List<GeoAnimationKeyframe> kframes = new List<GeoAnimationKeyframe>();

                                for (int i = 0; i < translationKeyTimestamps.Length; i++)
                                {
                                    GeoAnimationKeyframe keyframe = new GeoAnimationKeyframe();
                                    keyframe.Time = translationKeyTimestamps[i];
                                    keyframe.Translation = translationKeyValues[i];
                                    keyframe.Type = GeoKeyframeType.Translation;
                                    kframes.Add(keyframe);
                                }

                                channels.TryGetValue(targetBoneName, out gnac);
                                if (gnac == null)
                                {
                                    gnac = new GeoNodeAnimationChannel();
                                    gnac.NodeName = targetBoneName;
                                    channels.Add(targetBoneName, gnac);
                                }
                                gnac.TranslationKeys = kframes;

                            }

                            // Rotation:
                            if (nac.Target.Path == AnimationChannelTarget.PathEnum.rotation)
                            {
                                AnimationSampler sampler = a.Samplers[nac.Sampler];
                                Accessor accInputTimestamps = scene.Accessors[sampler.Input];
                                Accessor accOutputValues = scene.Accessors[sampler.Output];
                                Quaternion[] rotationKeyValues = GetRotationValues(scene, accOutputValues, ref model);
                                float[] rotationKeyTimestamps = GetTimestampValues(scene, accInputTimestamps, ref model, out float duration);
                                if (ga.DurationInTicks == 0)
                                    ga.DurationInTicks = duration;
                                List<GeoAnimationKeyframe> kframes = new List<GeoAnimationKeyframe>();

                                for (int i = 0; i < rotationKeyTimestamps.Length; i++)
                                {
                                    GeoAnimationKeyframe keyframe = new GeoAnimationKeyframe();
                                    keyframe.Time = rotationKeyTimestamps[i];
                                    keyframe.Rotation = rotationKeyValues[i];
                                    keyframe.Type = GeoKeyframeType.Rotation;
                                    kframes.Add(keyframe);
                                }

                                channels.TryGetValue(targetBoneName, out gnac);
                                if (gnac == null)
                                {
                                    gnac = new GeoNodeAnimationChannel();
                                    gnac.NodeName = targetBoneName;
                                    channels.Add(targetBoneName, gnac);
                                }
                                gnac.RotationKeys = kframes;
                            }

                            // Scale:
                            if (nac.Target.Path == AnimationChannelTarget.PathEnum.scale)
                            {
                                AnimationSampler sampler = a.Samplers[nac.Sampler];
                                Accessor accInputTimestamps = scene.Accessors[sampler.Input];
                                Accessor accOutputValues = scene.Accessors[sampler.Output];
                                Vector3[] scaleKeyValues = GetTranslationOrScaleValues(scene, accOutputValues, ref model);
                                float[] scaleKeyTimestamps = GetTimestampValues(scene, accInputTimestamps, ref model, out float duration);
                                if (ga.DurationInTicks == 0)
                                    ga.DurationInTicks = duration;
                                List<GeoAnimationKeyframe> kframes = new List<GeoAnimationKeyframe>();

                                for (int i = 0; i < scaleKeyTimestamps.Length; i++)
                                {
                                    GeoAnimationKeyframe keyframe = new GeoAnimationKeyframe();
                                    keyframe.Time = scaleKeyTimestamps[i];
                                    keyframe.Scale = scaleKeyValues[i];
                                    keyframe.Type = GeoKeyframeType.Scale;
                                    kframes.Add(keyframe);
                                }

                                channels.TryGetValue(targetBoneName, out gnac);
                                if (gnac == null)
                                {
                                    gnac = new GeoNodeAnimationChannel();
                                    gnac.NodeName = targetBoneName;
                                    channels.Add(targetBoneName, gnac);
                                }
                                gnac.ScaleKeys = kframes;
                            }
                        }
                        foreach (GeoNodeAnimationChannel channel in channels.Values)
                        {
                            ga.AnimationChannels.Add(channel.NodeName, channel);
                        }

                    }
                    model.Animations.Add(ga);
                }
            }
        }
        private static float[] GenerateTangentsFrom(GeoVertex[] vertices, float[] normals, float[] uvs, uint[] indices)
        {
            float[] tangents = new float[normals.Length];

            for (int indexCounter = 0; indexCounter < indices.Length; indexCounter += 3)
            {
                Vector3 v0 = new(vertices[indices[indexCounter + 0]].X, vertices[indices[indexCounter + 0]].Y, vertices[indices[indexCounter + 0]].Z);
                Vector3 v1 = new(vertices[indices[indexCounter + 1]].X, vertices[indices[indexCounter + 1]].Y, vertices[indices[indexCounter + 1]].Z);
                Vector3 v2 = new(vertices[indices[indexCounter + 2]].X, vertices[indices[indexCounter + 2]].Y, vertices[indices[indexCounter + 2]].Z);

                Vector2 uv0 = new(uvs[indices[indexCounter + 0] * 2 + 0], uvs[indices[indexCounter + 0] * 2 + 1]);
                Vector2 uv1 = new(uvs[indices[indexCounter + 1] * 2 + 0], uvs[indices[indexCounter + 1] * 2 + 1]);
                Vector2 uv2 = new(uvs[indices[indexCounter + 2] * 2 + 0], uvs[indices[indexCounter + 2] * 2 + 1]);

                // Edges of the triangle : position delta
                Vector3 deltaPos1 = v1 - v0;
                Vector3 deltaPos2 = v2 - v0;

                // UV delta
                Vector2 deltaUV1 = uv1 - uv0;
                Vector2 deltaUV2 = uv2 - uv0;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                tangent.Normalize();

                tangents[indices[indexCounter + 0] * 3 + 0] = tangent.X;
                tangents[indices[indexCounter + 0] * 3 + 1] = tangent.Y;
                tangents[indices[indexCounter + 0] * 3 + 2] = tangent.Z;

                tangents[indices[indexCounter + 1] * 3 + 0] = tangent.X;
                tangents[indices[indexCounter + 1] * 3 + 1] = tangent.Y;
                tangents[indices[indexCounter + 1] * 3 + 2] = tangent.Z;

                tangents[indices[indexCounter + 2] * 3 + 0] = tangent.X;
                tangents[indices[indexCounter + 2] * 3 + 1] = tangent.Y;
                tangents[indices[indexCounter + 2] * 3 + 2] = tangent.Z;
            }

            return tangents;
        }

        public static GeoMeshCollider LoadCollider(string filename, ColliderType colliderType)
        {
            GeoMeshCollider collider = new GeoMeshCollider();
            filename = HelperGeneral.EqualizePathDividers(filename);
            if (!File.Exists(filename))
            {
                KWEngine.LogWriteLine("[Import] Cannot find collider model " + filename);
                return collider;
            }
            Gltf wholeScene;
            try
            {
                wholeScene = Interface.LoadModel(filename);
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Import] Invalid collider model file: " + filename);
                return null;
            }
            if (wholeScene.Scenes.Length != 1)
            {
                KWEngine.LogWriteLine("[Import] Collider file " + filename + " contains of more than one scene. This is not supported.");
                return collider;
            }
            collider.FileName = filename;
            collider.Name = StripPathFromFile(filename);
            collider.GlbOffset = GetGlbOffset(filename, ref collider);

            try
            {
                GenerateNodeHierarchy(wholeScene, ref collider);
                foreach (GeoNode child in collider.Root.Children)
                {
                    GenerateMeshHitboxes(wholeScene, child, ref collider, colliderType);
                }
            }
            finally
            {
                ClearImportCache();
            }
            return collider;
        }

        private static void GenerateMeshHitboxes(Gltf wholeScene, GeoNode geoNode, ref GeoMeshCollider collider, ColliderType colliderType)
        {
            FindGLTFNodeForGeoNode(wholeScene, geoNode, out Node n);
            if (n != null)
            {
                GeoMeshHitbox hb = GenerateMeshHitboxForNode(wholeScene, n, ref collider, colliderType);
                if (hb != null)
                {
                    hb.Name = n.Name;
                    hb.Transform = geoNode.Transform;
                    hb.TransformInverse = Matrix4.Invert(hb.Transform);
                    collider.MeshHitboxes.Add(hb);
                }
                foreach (GeoNode child in geoNode.Children)
                {
                    GenerateMeshHitboxes(wholeScene, child, ref collider, colliderType);
                }
            }
        }

        private static void GenerateNodeHierarchy(Gltf gltf, ref GeoMeshCollider collider)
        {
            Scene scene = gltf.Scenes[0];
            GeoNode root = new GeoNode();
            root.Name = "KWRootGLTF";
            root.Transform = Matrix4.Identity;
            root.Parent = null;
            collider.Root = root;
            foreach (int childIndex in scene.Nodes)
            {
                Node child = gltf.Nodes[childIndex];
                root.Children.Add(MapNodeToNode(child, ref gltf, ref collider, ref root));
            }
        }

        private static GeoMeshHitbox GenerateMeshHitboxForNode(Gltf scene, Node gltfNode, ref GeoMeshCollider collider, ColliderType colliderType)
        {
            List<GeoMeshFace> faces = new();
            List<Vector3> uniqueVertices = new();
            List<Vector3> uniqueNormals = new();
            List<Vector3> allVertices = new();
            List<uint> allIndices = new();
            Dictionary<Vector3, int> uniqueVertexIndices = new();
            Dictionary<Vector3, int> uniqueNormalIndices = new();

            if (gltfNode.Mesh.HasValue)
            {
                Mesh mesh = scene.Meshes[gltfNode.Mesh.Value];
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float minZ = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float maxZ = float.MinValue;
                foreach (MeshPrimitive mp in mesh.Primitives)
                {
                    if (mp.Indices.HasValue && mp.Mode == MeshPrimitive.ModeEnum.TRIANGLES)
                    {
                        List<uint> indicesOfPrimitive = GetIndicesForMeshPrimitive(scene, mp, ref collider);
                        if (indicesOfPrimitive == null)
                            return null;
                        // indices of each primitive start at 0, but allVertices holds the vertices of all primitives
                        uint vertexOffset = (uint)allVertices.Count;
                        foreach (uint index in indicesOfPrimitive)
                        {
                            allIndices.Add(index + vertexOffset);
                        }
                        List<Vector3> verticesOfPrimitive = GetVertexDataForMeshPrimitive(scene, mp, ref collider, gltfNode, out float xmin, out float xmax, out float ymin, out float ymax, out float zmin, out float zmax, ref uniqueVertices, uniqueVertexIndices);
                        if (xmin < minX)
                            minX = xmin;
                        if (ymin < minY)
                            minY = ymin;
                        if (zmin < minZ)
                            minZ = zmin;
                        if (xmax > maxX)
                            maxX = xmax;
                        if (ymax > maxY)
                            maxY = ymax;
                        if (zmax > maxZ)
                            maxZ = zmax;
                        allVertices.AddRange(verticesOfPrimitive);
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] GLTF file " + collider.FileName + " does not have vertex indices for a certain mesh or is not composed of triangles. This is not supported.");
                        return null;
                    }
                }
                // Generate faces and their normals:
                for (int i = 0; i < allIndices.Count; i += 3)
                {
                    int index1 = uniqueVertexIndices.TryGetValue(allVertices[(int)allIndices[i + 0]], out int tmpIndex1) ? tmpIndex1 : -1;
                    int index2 = uniqueVertexIndices.TryGetValue(allVertices[(int)allIndices[i + 1]], out int tmpIndex2) ? tmpIndex2 : -1;
                    int index3 = uniqueVertexIndices.TryGetValue(allVertices[(int)allIndices[i + 2]], out int tmpIndex3) ? tmpIndex3 : -1;
                    if (index1 < 0 || index2 < 0 || index3 < 0)
                    {
                        KWEngine.LogWriteLine("[Import] GLTF file " + collider.FileName + " has invalid vertex data");
                        return null;
                    }

                    Vector3 a = uniqueVertices[index1];
                    Vector3 b = uniqueVertices[index2];
                    Vector3 c = uniqueVertices[index3];

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(c - b, a - b));
                    if (!uniqueNormalIndices.TryGetValue(normal, out int normalIndex))
                    {
                        normalIndex = uniqueNormals.Count;
                        uniqueNormalIndices.Add(normal, normalIndex);
                        uniqueNormals.Add(normal);
                    }


                    GeoMeshFace face = new GeoMeshFace(normalIndex, false, index1, index2, index3);
                    faces.Add(face);
                }

                // Generate final hitbox:
                GeoMeshHitbox hb = new GeoMeshHitbox(maxX, maxY, maxZ, minX, minY, minZ, uniqueNormals, uniqueVertices, faces);
                hb._colliderType = colliderType;
                return hb;
            }
            else
            {
                return null;
            }
        }

        private static void FindGLTFNodeForGeoNode(Gltf gltf, GeoNode node, out Node output)
        {
            foreach (Node n in gltf.Nodes)
            {
                if (n.Name == node.Name)
                {
                    output = n;
                    return;
                }
            }
            output = null;
        }

        private static GeoNode MapNodeToNode(Node n, ref Gltf scene, ref GeoMeshCollider collider, ref GeoNode callingNode)
        {
            GeoNode gNode = new GeoNode();
            gNode.Parent = callingNode;
            gNode.Transform = callingNode.Transform * HelperMatrix.ConvertGLTFTRSToOpenTKMatrix(n.Scale, n.Rotation, n.Translation);
            gNode.Name = n.Name;
            gNode.NameWithoutFBXSuffix = n.Name;
            if (n.Children != null)
            {
                foreach (int childIndex in n.Children)
                {
                    Node child = scene.Nodes[childIndex];
                    gNode.Children.Add(MapNodeToNode(child, ref scene, ref collider, ref gNode));
                }
            }
            return gNode;
        }

    }
}