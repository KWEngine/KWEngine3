﻿using System.Reflection;
using Assimp;
using Assimp.Configs;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using SkiaSharp;

namespace KWEngine3.Model
{
    internal static class SceneImporter
    {
        private enum FileType { DirectX, Filmbox, Wavefront, GLTF, Collada, Blender, Invalid }
        private static FileType CheckFileEnding(string filename)
        {
            string ending = filename.Trim().ToLower().Substring(filename.LastIndexOf('.') + 1);
            switch (ending)
            {
                case "x":
                    return FileType.DirectX;
                case "dae":
                    return FileType.Collada;
                case "glb":
                    return FileType.GLTF;
                case "gltf":
                    return FileType.GLTF;
                case "obj":
                    return FileType.Wavefront;
                case "fbx":
                    return FileType.Filmbox;
                default:
                    return FileType.Invalid;
            }
        }

        internal enum AssemblyMode { File, Internal, User }


        internal static List<GeoAnimation> LoadAnimations(string filename)
        {
            List<GeoAnimation> animations = new List<GeoAnimation>();

            filename = HelperGeneral.EqualizePathDividers(filename);
            FileType type = CheckFileEnding(filename);
            if (type != FileType.Invalid)
            {
                PostProcessSteps steps = PostProcessSteps.None;

                AssimpContext importer = new AssimpContext();
                Scene scene = importer.ImportFile(filename, steps);

                GeoModel dummy = new GeoModel();
                ProcessAnimations(scene, ref dummy);
                if(dummy.Animations != null && dummy.Animations.Count > 0)
                {
                    animations.AddRange(dummy.Animations);
                }
            }
            return animations;
        }

        internal static GeoMeshCollider LoadColliderInternal(string filename, ColliderType colliderType)
        {
            GeoMeshCollider collider = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "KWEngine3.Assets.Models." + filename;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                AssimpContext importer = new AssimpContext();
                Scene scene = importer.ImportFileFromStream(s, PostProcessSteps.ValidateDataStructure);
                if (scene == null)
                {
                    KWEngine.LogWriteLine("[Import] Invalid collider asset: " + resourceName);
                    return null;
                }

                collider = ProcessColliderScene(scene, filename, colliderType);
                scene.Clear();
                importer.Dispose();

                return collider;
            }
        }

        internal static GeoModel LoadModel(string filename, bool flipTextureCoordinates = false, AssemblyMode am = AssemblyMode.Internal)
        {
            if (filename == null)
                filename = "";
            filename = filename.Trim();

            FileType type = CheckFileEnding(filename);
            if(type == FileType.GLTF)
            {
                GeoModel model = SceneImporterGLTF.LoadModel(filename, flipTextureCoordinates);
                return model;
            }
            else
            {
                if(type == FileType.Wavefront && am == AssemblyMode.File)
                {
                    if(File.Exists(filename))
                    {
                        string[] lines = File.ReadAllLines(filename);
                        if(lines != null && lines.Length > 2)
                        {
                            int offset = 0;
                            bool changed = false;
                            while(offset < lines.Length)
                            {
                                if (lines[offset].StartsWith("#"))
                                {
                                    lines[offset] = "";
                                    changed = true;
                                    offset++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if(changed)
                                File.WriteAllLines(filename, lines);
                        }
                    }
                }

                AssimpContext importer = new AssimpContext();
                importer.SetConfig(new VertexBoneWeightLimitConfig(KWEngine.MAX_BONE_WEIGHTS));
                importer.SetConfig(new MaxBoneCountConfig(KWEngine.MAX_BONES));
                importer.SetConfig(new FBXOptimizeEmptyAnimationCurvesConfig(false));
                importer.SetConfig(new FBXImportLightsConfig(false));
                importer.SetConfig(new FBXImportCamerasConfig(false));
                Scene scene = null;
                if (am != AssemblyMode.File)
                {
                    if (type == FileType.Invalid)
                    {
                        KWEngine.LogWriteLine("[Import] Invalid file type on " + (filename == null ? "" : filename.Trim()));
                        return null;
                    }
                    string resourceName;
                    Assembly assembly;

                    if (am == AssemblyMode.Internal)
                    {
                        assembly = Assembly.GetExecutingAssembly();
                        resourceName = "KWEngine3.Assets.Models." + filename;
                    }
                    else
                    {
                        assembly = Assembly.GetEntryAssembly();
                        resourceName = assembly.GetName().Name + "." + filename;
                    }

                    using (Stream s = assembly.GetManifestResourceStream(resourceName))
                    {
                        PostProcessSteps steps =
                              PostProcessSteps.LimitBoneWeights
                            | PostProcessSteps.Triangulate
                            | PostProcessSteps.ValidateDataStructure
                            | PostProcessSteps.GenerateUVCoords
                            | PostProcessSteps.CalculateTangentSpace;

                        if (filename != "kwcube.obj" && filename != "kwcube6.obj" && filename != "kwplatform.obj")
                            steps |= PostProcessSteps.JoinIdenticalVertices;
                        if(filename == "kwsphere.obj")
                            steps |= PostProcessSteps.GenerateSmoothNormals;
                        if (type == FileType.DirectX)
                            steps |= PostProcessSteps.FlipWindingOrder;
                        if (flipTextureCoordinates)
                            steps |= PostProcessSteps.FlipUVs;
                        scene = importer.ImportFileFromStream(s, steps);
                    }
                }
                else
                {
                    filename = HelperGeneral.EqualizePathDividers(filename);
                    if (type != FileType.Invalid)
                    {
                        PostProcessSteps steps =
                                  PostProcessSteps.LimitBoneWeights |
                                  PostProcessSteps.Triangulate
                                | PostProcessSteps.ValidateDataStructure
                                | PostProcessSteps.GenerateUVCoords
                                | PostProcessSteps.CalculateTangentSpace
                                | PostProcessSteps.SplitByBoneCount
                                ;
                        if (type == FileType.DirectX)
                            steps |= PostProcessSteps.FlipWindingOrder;
                        if (flipTextureCoordinates)
                            steps |= PostProcessSteps.FlipUVs;

                        scene = importer.ImportFile(filename, steps);
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[Import] Invalid model file type");
                        return null;
                    }
                }
                if (scene == null)
                {
                    KWEngine.LogWriteLine("[Import] Invalid model file");
                    return null;
                }

                GeoModel model = ProcessScene(scene, filename, am);
                scene.Clear();
                importer.Dispose();

                return model;
            }
        }

        private static GeoModel ProcessScene(Scene scene, string filename, AssemblyMode am)
        {
            GeoModel returnModel = new GeoModel();
            if (filename.Contains("kwcube6.obj"))
                returnModel.IsKWCube6 = true;
            returnModel.Filename = filename;
            returnModel.Name = StripPathFromFile(filename);
            returnModel.MeshCollider.Name = returnModel.Name;
            if (am == AssemblyMode.Internal)
            {
                returnModel.PathAbsolute = "";
            }
            else
            {
                if (am == AssemblyMode.User)
                {
                    returnModel.PathAbsolute = Assembly.GetEntryAssembly().GetName().Name + "." + filename;
                }
                else
                {
                    string p = Assembly.GetExecutingAssembly().Location;
                    string pA = new DirectoryInfo(StripFileNameFromPath(p)).FullName;
                    if (!Path.IsPathRooted(filename))
                    {
                        returnModel.PathAbsolute = HelperGeneral.EqualizePathDividers(Path.Combine(pA, filename));
                    }
                    else
                    {
                        returnModel.PathAbsolute = HelperGeneral.EqualizePathDividers(filename);
                    }

                    bool success = File.Exists(returnModel.PathAbsolute);
                }
            }


            returnModel.AssemblyMode = am;
            returnModel.CalculatePath();
            returnModel.Meshes = new();
            returnModel.TransformGlobalInverse = Matrix4.Invert(HelperMatrix.ConvertAssimpToOpenTKMatrix(scene.RootNode.Transform));
            returnModel.Textures = new Dictionary<string, GeoTexture>();

            GenerateNodeHierarchy(scene.RootNode, ref returnModel);
            bool result;
            result = ProcessBones(scene, ref returnModel);
            if(result)
                result = ProcessMeshes(scene, ref returnModel);
            if(result)
                ProcessAnimations(scene, ref returnModel);

            returnModel.IsValid = result;
            return returnModel;
        }



        private static void GenerateNodeHierarchy(Node node, ref GeoModel model)
        {
            GeoNode root = new GeoNode();
            root.Name = node.Name;
            root.Transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(node.Transform);
            root.Parent = null;
            model.Root = root;
            model.NodesWithoutHierarchy.Add(root);
            foreach (Node child in node.Children)
            {
                root.Children.Add(MapNodeToNode(child, ref model, ref root));
            }
        }

        private static GeoNode MapNodeToNode(Node n, ref GeoModel model, ref GeoNode callingNode)
        {
            GeoNode gNode = new GeoNode();
            gNode.Name = n.Name;
            gNode.Parent = callingNode;

            if (n.Name.Contains("_$AssimpFbx$_"))
            {
                gNode.IsAssimpFBXNode = true;
                gNode.NameWithoutFBXSuffix = n.Name.Substring(0, n.Name.IndexOf("_$AssimpFbx$_"));
            }
            else
            {
                model.NodesWithoutHierarchy.Add(gNode);
                gNode.NameWithoutFBXSuffix = n.Name;
            }
            gNode.Transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(n.Transform);
            foreach (Node child in n.Children)
            {
                GeoNode newNode = MapNodeToNode(child, ref model, ref gNode);
                gNode.Children.Add(newNode);
            }
            return gNode;
        }
        private static Node GetNodeForBone(Node node, Bone b)
        {
            foreach(Node n in node.Children)
            {
                if(n.Name == b.Name)
                {
                    return n;
                }
                else
                {
                    Node nodeCandidate = GetNodeForBone(n, b);
                    if (nodeCandidate != null)
                        return nodeCandidate;
                }
            }
            return null;
        }

        private static void FindRootBone(Scene scene, ref GeoModel model, string boneName)
        {
            int c = 0;
            while(c < scene.Meshes.Count)
            {
                Mesh m = scene.Meshes[c];
                if(m.BoneCount > 0)
                {
                    Node boneNode = GetNodeForBone(scene.RootNode, m.Bones[0]);
                    if(boneNode != null)
                    {
                        while(boneNode.Parent != scene.RootNode)
                        {
                            boneNode = boneNode.Parent;
                        }
                        foreach(GeoNode n in model.NodesWithoutHierarchy)
                        {
                            if(n.Name == boneNode.Name)
                            {
                                model.Armature = n;
                                return;
                            }
                        }

                    }
                }
                c++;
            }
        }

        private static Node ScanForParent(Scene scene, Node node)
        {
            if (node.Parent != null && node.Parent.Parent == null)
            {
                return node.Parent;
            }
            else
            {
                return ScanForParent(scene, node.Parent);
            }
        }

        private static bool ProcessBones(Scene scene, ref GeoModel model)
        {
            foreach (Mesh mesh in scene.Meshes)
            {
                if(mesh.HasBones)
                {
                    model.HasBones = true;
                    break;
                }
            }
            if(model.HasBones == false)
            {
                return true;
            }

            model.BoneNames = new List<string>();
            foreach (Mesh mesh in scene.Meshes)
            {
                if (!model.BoneDictionary.ContainsKey(mesh.Name))
                {
                    model.BoneDictionary.Add(mesh.Name, new());
                }

                int boneIndexLocal = 0;
                foreach (Bone bone in mesh.Bones)
                {
                    if(model.BoneNames.Contains(bone.Name) == false)
                    {
                        model.BoneNames.Add(bone.Name);
                    }

                    if (model.Armature == null)
                        FindRootBone(scene, ref model, bone.Name);

                    GeoBone geoBone = new GeoBone();
                    geoBone.Name = bone.Name;
                    geoBone.Index = boneIndexLocal;
                    geoBone.Offset = HelperMatrix.ConvertAssimpToOpenTKMatrix(bone.OffsetMatrix);
                    geoBone.OffsetInverse = Matrix4.Invert(geoBone.Offset);
                    boneIndexLocal++;
                    if (!model.BoneDictionary[mesh.Name].ContainsKey(bone.Name))
                    {
                        model.BoneDictionary[mesh.Name].Add(bone.Name, geoBone);
                    }
                }
            }
            return model.BoneNames.Count <= KWEngine.MAX_BONES;
        }

        private static void FindTransformForMesh(ref GeoMeshCollider collider, GeoNode node, string nodename, ref Matrix4 transform)
        {
            if(node.Name == nodename)
            {
                transform = node.Transform;
                return;
            }
            else
            {
                foreach(GeoNode child in node.Children)
                {
                    FindTransformForMesh(ref collider, child, nodename, ref transform);
                }
            }
        }

        private static bool FindTransformForMesh(Scene scene, Node currentNode, Mesh mesh, ref Matrix4 transform, out string nodeName, ref Matrix4 parentTransform)
        {
            Matrix4 currentNodeTransform = parentTransform * HelperMatrix.ConvertAssimpToOpenTKMatrix(currentNode.Transform);
            for (int i = 0; i < currentNode.MeshIndices.Count; i++)
            {
                Mesh tmpMesh = scene.Meshes[currentNode.MeshIndices[i]];
                if (tmpMesh.Name == mesh.Name)
                {
                    transform = currentNodeTransform;
                    nodeName = currentNode.Name;
                    return true;
                }
            }

            for (int i = 0; i < currentNode.ChildCount; i++)
            {
                Node child = currentNode.Children[i];
                bool found = FindTransformForMesh(scene, child, mesh, ref transform, out string nName, ref currentNodeTransform);
                if (found)
                {
                    nodeName = nName;
                    return true;
                }
            }

            transform = Matrix4.Identity;
            nodeName = null;
            return false;
        }

        internal static string StripFileNameFromPath(string path)
        {
            path = HelperGeneral.EqualizePathDividers(path);
            if (File.Exists(path))
            {
                int index = path.LastIndexOf(Path.AltDirectorySeparatorChar);
                int indexPoint = path.LastIndexOf('.');
                if (index < 0 || indexPoint < index)
                {
                    return path;
                }
                else
                {
                    return path.Substring(0, index + 1);
                }
            }
            else
                return path;
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

        internal static string StripEndingFromFile(string filename)
        {
            int index = filename.LastIndexOf(".");
            if(index > 0)
            {
                return filename.Substring(0, index);
            }
            return filename;
        }

        internal static bool IsImageFile(string filename)
        {
            string fn = filename.ToLower().Trim();
            return (fn.EndsWith(".png") || fn.EndsWith(".jpg") || fn.EndsWith(".jpeg") || fn.EndsWith(".bmp") || fn.EndsWith(".dds"));
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

        internal static string FindTextureInSubs(string filename, string path = null, bool isNotAnImageFile = false)
        {
            filename = HelperGeneral.EqualizePathDividers(filename);
            if (path != null)
                path = HelperGeneral.EqualizePathDividers(path);
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
                if (isNotAnImageFile)
                {
                    string fn = fi.Name.Trim();
                    bool fnIsImageFile = IsImageFile(fn);
                    filename = StripEndingFromFile(filename);
                    if (fn.Contains(filename) && fnIsImageFile)
                    {
                        return HelperGeneral.EqualizePathDividers(fi.FullName);
                    }
                }
                else
                {
                    if (StripPathFromFile(fi.Name.Trim()) == StripPathFromFile(filename).Trim())
                    {
                        return HelperGeneral.EqualizePathDividers(fi.FullName);
                    }
                }
            }

            if (currentDir.GetDirectories().Length == 0)
            {
                KWEngine.LogWriteLine("[Import] Image file " + filename + " cannot be found.");
            }
            else
            {
                foreach (DirectoryInfo di in currentDir.GetDirectories())
                {
                    string result = FindTextureInSubs(filename, di.FullName, isNotAnImageFile);
                    if(result != null && result.Length > 0)
                    {
                        return result;
                    }
                }
            }

            return "";
        }

        private static void ProcessMaterialsForMesh(Scene scene, Mesh mesh, ref GeoModel model, ref GeoMesh geoMesh, bool isKWCube = false)
        {
            GeoMaterial geoMaterial = new GeoMaterial();
            Material material = null;
            if (isKWCube)
            {
                if (mesh.MaterialIndex >= 0)
                {
                    material = scene.Materials[mesh.MaterialIndex];
                    geoMaterial.Name = model.Filename == "kwcube.obj" ? "KWCube" : material.Name;
                    geoMaterial.BlendMode = material.BlendMode == BlendMode.Default ? OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha : OpenTK.Graphics.OpenGL4.BlendingFactor.One; // TODO: Check if this is correct!
                    geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                    geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);

                }
                else
                {
                    geoMaterial.Name = "kw-undefined.";
                    geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                    geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                    geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
                }
                geoMaterial.Roughness = 1;
                geoMaterial.Metallic = 0;
                geoMaterial.ColorAlbedo.W = 1;
            }
            else
            {
                if (mesh.MaterialIndex >= 0)
                {
                    material = scene.Materials[mesh.MaterialIndex];
                    geoMaterial.Name = material.Name;

                    if (material.Name == "DefaultMaterial")
                    {
                        geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                        geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                        geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
                        geoMaterial.Roughness = 1;
                        geoMaterial.Metallic = 0;
                        geoMaterial.TextureRoughnessIsSpecular = false;
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
                        geoMaterial.Roughness = material.HasShininess ? (100f - material.Shininess) / 100f : 1;
                        geoMaterial.Metallic = material.HasReflectivity ? material.Reflectivity : 0;
                        geoMaterial.BlendMode = material.BlendMode == BlendMode.Default ? OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha : OpenTK.Graphics.OpenGL4.BlendingFactor.One; // TODO: Check if this is correct!
                        if (model.AssemblyMode == AssemblyMode.Internal && material.Name == "System")
                        {
                            geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                        }
                        else if (model.AssemblyMode == AssemblyMode.Internal && material.Name == "X")
                        {
                            geoMaterial.ColorAlbedo = new Vector4(1, 0, 0, 1);
                        }
                        else if (model.AssemblyMode == AssemblyMode.Internal && material.Name == "Y")
                        {
                            geoMaterial.ColorAlbedo = new Vector4(0, 1, 0, 1);
                        }
                        else if (model.AssemblyMode == AssemblyMode.Internal && material.Name == "Z")
                        {
                            geoMaterial.ColorAlbedo = new Vector4(0, 0, 1, 1);
                        }
                        else
                        {
                            geoMaterial.ColorAlbedo = material.HasColorDiffuse ? new Vector4(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B, material.ColorDiffuse.A) : new Vector4(1, 1, 1, 1);
                        }
                        geoMaterial.ColorEmissive = material.HasColorEmissive ? new Vector4(material.ColorEmissive.R, material.ColorEmissive.G, material.ColorEmissive.B, material.ColorEmissive.A) : new Vector4(0, 0, 0, 1);
                        geoMaterial.TextureRoughnessIsSpecular = false;
                        geoMaterial.ColorAlbedo.W = material.HasOpacity ? material.Opacity : 1;
                        if (
                            (mesh.Name != null && mesh.Name.ToLower().Contains("_invisible"))
                            ||
                            (geoMesh != null && geoMesh.Name.ToLower().Contains("_invisible"))
                        )
                        {
                            geoMaterial.ColorAlbedo.W = 0;
                        }
                    }


                }
                else
                {
                    geoMaterial.Name = "kw-undefined.";
                    geoMaterial.Metallic = 0;
                    geoMaterial.Roughness = 1;
                    geoMaterial.ColorAlbedo.W = 1;
                    geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                    geoMaterial.ColorAlbedo = new Vector4(1, 1, 1, 1);
                    geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
                    geoMaterial.TextureRoughnessIsSpecular = false;
                    if (
                            (mesh.Name != null && mesh.Name.ToLower().Contains("_invisible"))
                            ||
                            (geoMesh != null && geoMesh.Name.ToLower().Contains("_invisible"))
                        )
                    {
                        geoMaterial.ColorAlbedo.W = 0;
                    }
                }
            }
            GeoTexture tex;
            // Process Textures:
            if (material != null)
            {
                if (material.HasTextureDiffuse)
                {
                    tex = HelperTexture.ProcessTextureForMaterial(TextureType.Albedo, material, scene, ref model, false, out float rTmp, out float mTmp);
                    if(tex.IsTextureSet)
                    {
                        geoMaterial.TextureAlbedo = tex;
                        if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                        {
                            model.Textures.Add(tex.Filename, tex);
                        }
                    }
                }
                else
                {
                    GeoTexture texDummy = new GeoTexture();
                    geoMaterial.TextureAlbedo = texDummy;
                }

                if(material.HasTextureOpacity)
                {
                    tex = HelperTexture.ProcessTextureForMaterial(TextureType.Transparency, material, scene, ref model, false, out float rTmp, out float mTmp);
                    if (tex.IsTextureSet)
                    {
                        geoMaterial.TextureTranparency = tex;
                        if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                        {
                            model.Textures.Add(tex.Filename, tex);
                        }
                        model.HasTransparencyTexture = true;
                    }

                }

                if(material.HasTextureNormal)
                {
                    tex = HelperTexture.ProcessTextureForMaterial(TextureType.Normal, material, scene, ref model, false, out float rTmp, out float mTmp);
                    if (tex.IsTextureSet)
                    {
                        geoMaterial.TextureNormal = tex;
                        if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                        {
                            model.Textures.Add(tex.Filename, tex);
                        }
                    }
                }
                else if(material.HasTextureHeight)
                {
                    // alternative for blender wavefront exporter writing normal map to bump map
                    tex = HelperTexture.ProcessTextureForMaterialNormalBump(material, scene, ref model, false, out float rTmp, out float mTmp);
                    if (tex.IsTextureSet)
                    {
                        KWEngine.LogWriteLine("[Import] Assuming normal map in bump map property for " + model.Name + " - lighting may be wrong");
                        geoMaterial.TextureNormal = tex;
                        if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                        {
                            model.Textures.Add(tex.Filename, tex);
                        }
                    }
                }


                if (material.HasTextureEmissive)
                {
                    tex = HelperTexture.ProcessTextureForMaterial(TextureType.Emissive, material, scene, ref model, false, out float rTmp, out float mTmp);
                    if (tex.IsTextureSet)
                    {
                        geoMaterial.TextureEmissive = tex;
                        if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                        {
                            model.Textures.Add(tex.Filename, tex);
                        }
                    }
                }
                float rVal = 1.0f;
                float mVal = 0.0f;
                // Look for roughness texture:
                tex = HelperTexture.ProcessTextureForAssimpPBRMaterial(material, TextureType.Roughness, ref model, out rVal, out mVal);
                if (tex.IsTextureSet)
                {
                    geoMaterial.TextureRoughness = tex;
                    if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                    {
                        model.Textures.Add(tex.Filename, tex);
                    }
                }
                else
                {
                    // check if there is at least a specular texture available:
                    if (material.HasTextureSpecular)
                    {
                        tex = HelperTexture.ProcessTextureForMaterial(TextureType.Roughness, material, scene, ref model, true, out rVal, out mVal);
                        if (tex.IsTextureSet)
                        {
                            geoMaterial.TextureRoughness = tex;
                            geoMaterial.TextureRoughnessIsSpecular = true;
                            if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                            {
                                model.Textures.Add(tex.Filename, tex);
                            }
                        }
                    }
                }
                if(geoMaterial.TextureRoughness.IsTextureSet == false && rVal != geoMaterial.Roughness)
                {
                    geoMaterial.Roughness = rVal;
                }


                // Look for metallic texture:
                tex = HelperTexture.ProcessTextureForAssimpPBRMaterial(material, TextureType.Metallic, ref model, out rVal, out mVal);
                if (tex.IsTextureSet)
                {
                    geoMaterial.TextureMetallic = tex;
                    if (!tex.IsKWEngineTexture && model.Textures.ContainsKey(tex.Filename) == false)
                    {
                        model.Textures.Add(tex.Filename, tex);
                    }
                }
                if (geoMaterial.TextureMetallic.IsTextureSet == false && mVal != geoMaterial.Metallic)
                {
                    geoMaterial.Metallic = mVal;
                }
            }
            geoMesh.Material = geoMaterial;
        }

        internal static bool CheckIfOtherModelsShareTexture(string texture, string path, out GeoTexture sharedTex)
        {
            path = HelperGeneral.EqualizePathDividers(path);
            sharedTex = new GeoTexture();
            foreach(string key in KWEngine.Models.Keys)
            {
                GeoModel m = KWEngine.Models[key];
                if(m.Path == path)
                {
                    foreach (string texKey in m.Textures.Keys)
                    {
                        if(texKey == texture)
                        {
                            sharedTex = m.Textures[texKey];
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static float GetAvgZValue(List<Vector3D> vertices, ref Matrix4 nodeTransform)
        {
            float max = float.MinValue;
            float min = float.MaxValue;

            foreach(Vector3D V in vertices)
            {
                Vector3 v = Vector3.TransformPosition(new Vector3(V.X, V.Y, V.Z), nodeTransform);

                if (v.Z > max) max = v.Z;
                if (v.Z < min) min = v.Z;
            }
            return (max - min) * 0.5f;
        }

        private static bool ProcessMeshes(Scene scene, ref GeoModel model)
        {
            model.MeshCollider.MeshHitboxes = new List<GeoMeshHitbox>();

            string currentMeshName = null;
            string currentNodeName = null;
            Matrix4 currentNodeTransform = Matrix4.Identity;
            Mesh currentMesh = null;
            GeoMeshHitbox meshHitBox = null;
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            Matrix4 nodeTransform = Matrix4.Identity;
            Mesh mesh = null;
            List<Vector3> uniqueVerticesForWholeMesh = new List<Vector3>();
            List<Vector3> uniqueNormalsForWholeMesh = new List<Vector3>();
            List<GeoMeshFaceHelper> faceHelpersForWholeMesh = new List<GeoMeshFaceHelper>();
            for (int m = 0; m < scene.MeshCount; m++)
            {
                mesh = scene.Meshes[m];
                Matrix4 parentTransform = Matrix4.Identity;
                bool transformFound = FindTransformForMesh(scene, scene.RootNode, mesh, ref nodeTransform, out string nodeName, ref parentTransform);
                bool isNewMesh = currentMeshName != null && mesh.Name != currentMeshName && currentNodeName != null && currentNodeName != nodeName;

                if (mesh.PrimitiveType != PrimitiveType.Triangle)
                {
                    return false;
                }

                if (isNewMesh)
                {
                    if (currentMeshName != null)
                    {
                        List<GeoMeshFace> facesForHitbox = null;
                        if (currentNodeName.ToLower().Contains("_fullhitbox"))
                        {
                            facesForHitbox = new List<GeoMeshFace>();
                            List<Vector3> faceNormals = new List<Vector3>();
                            foreach (GeoMeshFaceHelper face in faceHelpersForWholeMesh)
                            {
                                GeoHelper.FindIndexOfVertexInList(ref face.Vertices[0], ref face.Vertices[1], ref face.Vertices[2], uniqueVerticesForWholeMesh, uniqueNormalsForWholeMesh, out int indexVertex1, out int indexVertex2, out int indexVertex3, out int indexNormal);
                                Vector3 normal = uniqueNormalsForWholeMesh[indexNormal];
                                //if (!faceNormals.Contains(normal))
                                //{
                                faceNormals.Add(normal);
                                GeoMeshFace tmpFace = new GeoMeshFace(indexNormal, false, indexVertex1, indexVertex2, indexVertex3);
                                facesForHitbox.Add(tmpFace);
                                //}
                            }
                        }

                        // Generate hitbox for the previous mesh:
                        meshHitBox = new GeoMeshHitbox(maxX, maxY, maxZ, minX, minY, minZ, uniqueNormalsForWholeMesh, uniqueVerticesForWholeMesh, facesForHitbox);
                        meshHitBox.Model = model;
                        meshHitBox.Name = currentNodeName;
                        meshHitBox.Transform = currentNodeTransform;
                        meshHitBox.TransformInverse = Matrix4.Invert(currentNodeTransform);
                        if(!currentNodeName.ToLower().Contains("_nohitbox"))
                            model.MeshCollider.MeshHitboxes.Add(meshHitBox);

                        faceHelpersForWholeMesh.Clear();
                        uniqueNormalsForWholeMesh.Clear();
                        uniqueVerticesForWholeMesh.Clear();
                    }
                    minX = float.MaxValue;
                    minY = float.MaxValue;
                    minZ = float.MaxValue;

                    maxX = float.MinValue;
                    maxY = float.MinValue;
                    maxZ = float.MinValue;
                }

                currentMeshName = mesh.Name;

                GeoMesh geoMesh = new GeoMesh();
                //Matrix4 parentTransform = Matrix4.Identity;
                //bool transformFound = FindTransformForMesh(scene, scene.RootNode, mesh, ref nodeTransform, out string nodeName, ref parentTransform);
                geoMesh.Transform = nodeTransform;
                geoMesh.Terrain = null;
                geoMesh.BoneTranslationMatrixCount = mesh.BoneCount;
                string affix = "";
                try
                {
                    if (scene.HasMaterials)
                    {
                        Material tmpMat = scene.Materials[mesh.MaterialIndex];
                        if(tmpMat.HasTextureOpacity)
                        {
                            // determine the max z-value for the mesh's vertices
                            if(mesh.HasVertices)
                            {
                                float z = GetAvgZValue(mesh.Vertices, ref nodeTransform);
                                z = (MathHelper.Clamp(z / 20000f, -1f, 1f) + 1f) * 0.5f * 40000;
                                ushort zInt = (ushort)Math.Round(z);
                                char affixChar = (char)zInt;
                                affix += ("z" + affixChar);

                            }
                        }
                    }
                }
                catch(Exception)
                {

                }
                geoMesh.Name = affix + mesh.Name + " #" + m.ToString().PadLeft(4, '0') + " (Node: " + nodeName + ")";
                currentNodeName = nodeName;
                currentMesh = mesh;
                currentNodeTransform = nodeTransform;
                geoMesh.NameOrg = mesh.Name;
                geoMesh.Vertices = new GeoVertex[mesh.VertexCount];
                geoMesh.Primitive = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;
                geoMesh.VAOGenerateAndBind();

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    Vector3D vertex = mesh.Vertices[i];
                    if (vertex.X > maxX)
                        maxX = vertex.X;
                    if (vertex.Y > maxY)
                        maxY = vertex.Y;
                    if (vertex.Z > maxZ)
                        maxZ = vertex.Z;
                    if (vertex.X < minX)
                        minX = vertex.X;
                    if (vertex.Y < minY)
                        minY = vertex.Y;
                    if (vertex.Z < minZ)
                        minZ = vertex.Z;

                    GeoVertex geoVertex = new GeoVertex(i, vertex.X, vertex.Y, vertex.Z);
                    Vector3 gv3 = new Vector3(vertex.X, vertex.Y, vertex.Z);
                    if (currentNodeName.ToLower().Contains("_fullhitbox") && !uniqueVerticesForWholeMesh.Contains(gv3))
                    {
                        uniqueVerticesForWholeMesh.Add(gv3);
                    }
                    geoMesh.Vertices[i] = geoVertex;
                }
                uint[] indices = mesh.GetUnsignedIndices();
                geoMesh.IndexCount = indices.Length;

                if (model.HasBones)
                {
                    for (int i = 0; i < mesh.BoneCount; i++)
                    {
                        Bone bone = mesh.Bones[i];

                        geoMesh.BoneNames.Add(bone.Name);
                        geoMesh.BoneIndices.Add(i);
                        Matrix4 boneOffsetMatrix = HelperMatrix.ConvertAssimpToOpenTKMatrix(bone.OffsetMatrix);
                        geoMesh.BoneOffset.Add(boneOffsetMatrix);
                        geoMesh.BoneOffsetInverse.Add(Matrix4.Invert(boneOffsetMatrix));

                        foreach (VertexWeight vw in bone.VertexWeights)
                        {
                            int weightIndexToBeSet = geoMesh.Vertices[vw.VertexID].WeightSet;
                            if (weightIndexToBeSet >= KWEngine.MAX_BONE_WEIGHTS)
                            {
                                return false;
                            }

                            geoMesh.Vertices[vw.VertexID].Weights[weightIndexToBeSet] = vw.Weight;
                            geoMesh.Vertices[vw.VertexID].BoneIDs[weightIndexToBeSet] = (uint)i;
                            geoMesh.Vertices[vw.VertexID].WeightSet++;
                        }

                    }
                }

                geoMesh.VBOGenerateIndices(indices);
                geoMesh.VBOGenerateVerticesAndBones(model.HasBones);
                List<Vector3> currentMeshNormals = geoMesh.VBOGenerateNormals(mesh);
                if (currentNodeName.ToLower().Contains("_fullhitbox"))
                {
                    foreach (Vector3 n in currentMeshNormals)
                    {
                        if (!uniqueNormalsForWholeMesh.Contains(n))
                        {
                            uniqueNormalsForWholeMesh.Add(n);
                        }
                    }

                    for (int i = 0; i < mesh.FaceCount; i++)
                    {
                        Face f = mesh.Faces[i];
                        Vector3 gv1 = new Vector3(mesh.Vertices[f.Indices[0]].X, mesh.Vertices[f.Indices[0]].Y, mesh.Vertices[f.Indices[0]].Z);
                        Vector3 gv2 = new Vector3(mesh.Vertices[f.Indices[1]].X, mesh.Vertices[f.Indices[1]].Y, mesh.Vertices[f.Indices[1]].Z);
                        Vector3 gv3 = new Vector3(mesh.Vertices[f.Indices[2]].X, mesh.Vertices[f.Indices[2]].Y, mesh.Vertices[f.Indices[2]].Z);
                        GeoMeshFaceHelper fh = new GeoMeshFaceHelper(gv1, gv2, gv3);
                        faceHelpersForWholeMesh.Add(fh);
                    }
                }

                geoMesh.VBOGenerateTangents(mesh);
                geoMesh.VBOGenerateTextureCoords1(mesh);
                //geoMesh.VBOGenerateTextureCoords2(mesh);

                ProcessMaterialsForMesh(scene, mesh, ref model, ref geoMesh, model.Filename == "kwcube.obj" || model.Filename == "kwcube6.obj");

                geoMesh.VAOUnbind();
                geoMesh.Vertices = null; // no longer needed, let GC get it
                model.Meshes.Add(geoMesh.Name, geoMesh);
            }

            // Generate hitbox for the last mesh:
            if (currentMeshName != null)
            {
                List<GeoMeshFace> facesForHitbox = null;
                if (currentNodeName.ToLower().Contains("_fullhitbox"))
                {
                    facesForHitbox = new List<GeoMeshFace>();
                    foreach (GeoMeshFaceHelper face in faceHelpersForWholeMesh)
                    {
                        GeoHelper.FindIndexOfVertexInList(ref face.Vertices[0], ref face.Vertices[1], ref face.Vertices[2], uniqueVerticesForWholeMesh, uniqueNormalsForWholeMesh, out int indexVertex1, out int indexVertex2, out int indexVertex3, out int indexNormal);
                        Vector3 normal = uniqueNormalsForWholeMesh[indexNormal];
                        GeoMeshFace tmpFace = new GeoMeshFace(indexNormal, false, indexVertex1, indexVertex2, indexVertex3);
                        facesForHitbox.Add(tmpFace);
                    }
                }

                if(currentMeshName.StartsWith("KWQuad"))
                {
                    maxZ = 0.5f;
                    minZ = -0.5f;
                }
                meshHitBox = new GeoMeshHitbox(maxX, maxY, maxZ, minX, minY, minZ, uniqueNormalsForWholeMesh, uniqueVerticesForWholeMesh, facesForHitbox);
                meshHitBox.Model = model;
                meshHitBox.Name = currentNodeName;
                meshHitBox.Transform = nodeTransform;
                meshHitBox.TransformInverse = Matrix4.Invert(nodeTransform);
                if(!currentNodeName.ToLower().Contains("_nohitbox"))
                    model.MeshCollider.MeshHitboxes.Add(meshHitBox);

                faceHelpersForWholeMesh.Clear();
                uniqueNormalsForWholeMesh.Clear();
                uniqueVerticesForWholeMesh.Clear();
            }
            return true;
        }

        private static void ProcessAnimations(Scene scene, ref GeoModel model)
        {
            if (scene.HasAnimations)
            {
                model.Animations = new List<GeoAnimation>();
                foreach (Animation a in scene.Animations)
                {
                    GeoAnimation ga = new GeoAnimation();
                    ga.DurationInTicks = (float)a.DurationInTicks;
                    ga.TicksPerSecond = (float)a.TicksPerSecond;
                    ga.Name = a.Name;
                    ga.AnimationChannels = new Dictionary<string, GeoNodeAnimationChannel>();
                    foreach (NodeAnimationChannel nac in a.NodeAnimationChannels)
                    {
                        GeoNodeAnimationChannel ganc;
                        string nodename = nac.NodeName;
                        if(ga.AnimationChannels.ContainsKey(nodename))
                        {
                            ganc = ga.AnimationChannels[nodename];
                        }
                        else
                        {
                            ganc = new GeoNodeAnimationChannel();
                        }

                        // Rotation:
                        ganc.RotationKeys = new List<GeoAnimationKeyframe>();
                        foreach (QuaternionKey key in nac.RotationKeys)
                        {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Mathematics.Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W);
                            akf.Translation = new Vector3(0, 0, 0);
                            akf.Scale = new Vector3(1, 1, 1);
                            akf.Type = GeoKeyframeType.Rotation;
                            ganc.RotationKeys.Add(akf);
                        }

                        // Scale:
                        ganc.ScaleKeys = new List<GeoAnimationKeyframe>();
                        foreach (VectorKey key in nac.ScalingKeys)
                        {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Mathematics.Quaternion(0, 0, 0, 1);
                            akf.Translation = new Vector3(0, 0, 0);
                            akf.Scale = new Vector3(key.Value.X, key.Value.Y, key.Value.Z);
                            akf.Type = GeoKeyframeType.Scale;
                            ganc.ScaleKeys.Add(akf);
                        }

                        // Translation:
                        ganc.TranslationKeys = new List<GeoAnimationKeyframe>();
                        foreach (VectorKey key in nac.PositionKeys)
                        {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Mathematics.Quaternion(0, 0, 0, 1);
                            akf.Translation = new Vector3(key.Value.X, key.Value.Y, key.Value.Z);
                            akf.Scale = new Vector3(1, 1, 1);
                            akf.Type = GeoKeyframeType.Translation;
                            ganc.TranslationKeys.Add(akf);
                        }

                        ga.AnimationChannels.Add(nodename, ganc);
                    }
                    model.Animations.Add(ga);
                }
            }
        }

        public static GeoMeshCollider LoadCollider(string filename, ColliderType colliderType)
        {
            GeoMeshCollider collider = null;
            filename = filename == null ? "" : filename.Trim();
            if(!File.Exists(filename))
            {
                return collider;
            }

            FileType type = CheckFileEnding(filename);
            if (type == FileType.Wavefront)
            {
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    if (lines != null && lines.Length > 2)
                    {
                        int offset = 0;
                        bool changed = false;
                        while (offset < lines.Length)
                        {
                            if (lines[offset].StartsWith("#"))
                            {
                                lines[offset] = "";
                                changed = true;
                                offset++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (changed)
                            File.WriteAllLines(filename, lines);
                    }
                }
            }

            if (type == FileType.GLTF)
            {
                collider = SceneImporterGLTF.LoadCollider(filename, colliderType);
                return collider;
            }
            else
            {
                AssimpContext importer = new AssimpContext();
                Scene scene = null;

                filename = HelperGeneral.EqualizePathDividers(filename);
                if (type != FileType.Invalid)
                {
                    scene = importer.ImportFile(filename, PostProcessSteps.ValidateDataStructure);
                }
                else
                {
                    KWEngine.LogWriteLine("[Import] Invalid collider file type");
                    return null;
                }

                if (scene == null)
                {
                    KWEngine.LogWriteLine("[Import] Invalid collider file");
                    return null;
                }

                collider = ProcessColliderScene(scene, filename, colliderType);
                scene.Clear();
                importer.Dispose();

                return collider;
            }
        }

        public static GeoMeshCollider ProcessColliderScene(Scene scene, string filename, ColliderType colliderType)
        {
            if (scene.HasMeshes)
            {
                GeoMeshCollider collider = new GeoMeshCollider();
                collider.Name = scene.RootNode.Name;
                GenerateNodeHierarchy(scene.RootNode, ref collider);
                ProcessMeshes(scene, scene.RootNode, ref collider, colliderType);

                return collider;
            }
            else
            {
                return null;
            }
        }

        private static void ProcessMeshes(Scene scene, Node currentNode, ref GeoMeshCollider collider, ColliderType colliderType)
        {
            if(currentNode.HasMeshes)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float minZ = float.MaxValue;

                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float maxZ = float.MinValue;

                List<Vector3> uniqueVertices = new();
                List<Vector3> uniqueNormals = new();
                List<GeoMeshFace> faces = new();

                foreach (int meshIndex in currentNode.MeshIndices)
                {
                    Mesh m = scene.Meshes[meshIndex];
                    foreach(Face f in m.Faces)
                    {
                        List<Vector3> faceVerticesTmp = new();
                        GeoMeshFace face = new GeoMeshFace(f.Indices.Count);
                        foreach (int index in f.Indices)
                        {
                            Vector3 vertex = new Vector3(m.Vertices[index].X, m.Vertices[index].Y, m.Vertices[index].Z);
                            if (vertex.X < minX)
                                minX = vertex.X;
                            if (vertex.Y < minY)
                                minY = vertex.Y;
                            if (vertex.Z < minZ)
                                minZ = vertex.Z;
                            if (vertex.X > maxX)
                                maxX = vertex.X;
                            if (vertex.Y > maxY)
                                maxY = vertex.Y;
                            if (vertex.Z > maxZ)
                                maxZ = vertex.Z;

                            faceVerticesTmp.Add(vertex);
                            int indexOfVertexInList = uniqueVertices.IndexOf(vertex);
                            if(indexOfVertexInList >= 0)
                            {
                                face.AddVertex(indexOfVertexInList);
                            }
                            else
                            {
                                uniqueVertices.Add(vertex);
                                face.AddVertex(uniqueVertices.Count - 1);
                            }
                        }
                        // determine face normal
                        Vector3 n = CalculateMeshFaceNormal(faceVerticesTmp);
                        int indexOfNormal = uniqueNormals.IndexOf(n);
                        if(indexOfNormal >= 0)
                        {
                            face.SetNormal(indexOfNormal);
                        }
                        else
                        {
                            uniqueNormals.Add(n);
                            face.SetNormal(uniqueNormals.Count - 1);
                        }
                        faces.Add(face);
                    }
                }
                GeoMeshHitbox hitbox = new GeoMeshHitbox(maxX, maxY, maxZ, minX, minY, minZ, uniqueNormals, uniqueVertices, faces);
                hitbox.Model = null;
                hitbox.Name = currentNode.Name;
                hitbox.Transform = Matrix4.Identity;
                hitbox._colliderType = colliderType;
                FindTransformForMesh(ref collider, collider.Root, currentNode.Name, ref hitbox.Transform);
                hitbox.TransformInverse = hitbox.Transform;
                if (!currentNode.Name.ToLower().Contains("_nohitbox"))
                    collider.MeshHitboxes.Add(hitbox);
            }
            foreach(Node child in currentNode.Children)
            {
                ProcessMeshes(scene, child, ref collider, colliderType);
            }
        }

        private static Vector3 CalculateMeshFaceNormal(List<Vector3> faceVerticesTmp)
        {
            Vector3 n = Vector3.Zero;

            for(int index = 0; index < faceVerticesTmp.Count; index++)
            {
                Vector3 current = faceVerticesTmp[index];
                Vector3 next = faceVerticesTmp[(index + 1) % faceVerticesTmp.Count];

                n.X += (current.Y - next.Y) * (current.Z + next.Z);
                n.Y += (current.Z - next.Z) * (current.X + next.X);
                n.Z += (current.X - next.X) * (current.Y + next.Y);
            }
            n.Normalize();
            return n;
        }

        private static void GenerateNodeHierarchy(Node node, ref GeoMeshCollider collider)
        {
            GeoNode root = new GeoNode();
            root.Name = node.Name;
            root.Transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(node.Transform);
            root.Parent = null;
            collider.Root = root;
            foreach (Node child in node.Children)
            {
                root.Children.Add(MapNodeToNode(child, ref collider, ref root));
            }
        }

        private static GeoNode MapNodeToNode(Node n, ref GeoMeshCollider collider, ref GeoNode callingNode)
        {
            GeoNode gNode = new GeoNode();
            gNode.Name = n.Name;
            gNode.Parent = callingNode;
            gNode.Transform = gNode.Transform * HelperMatrix.ConvertAssimpToOpenTKMatrix(n.Transform);
            foreach (Node child in n.Children)
            {
                GeoNode newNode = MapNodeToNode(child, ref collider, ref gNode);
                gNode.Children.Add(newNode);
            }
            return gNode;
        }
    }
}