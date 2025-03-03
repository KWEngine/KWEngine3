using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererForwardInstanced
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNormalMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UColorMaterial { get; private set; } = -1;
        public static int UMetallicRoughness { get; private set; } = -1;
        public static int UUseTexturesAlbedoNormalEmissive { get; private set; } = -1;
        public static int UUseTexturesMetallicRoughness { get; private set; } = -1;
        public static int UTextureMetallicRoughnessCombined { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureMetallic { get; private set; } = -1;
        public static int UTextureRoughness { get; private set; } = -1;
        public static int UTextureEmissive { get; private set; } = -1;
        public static int UTextureTransparency { get; private set; } = -1;
        public static int UTextureTransform { get; private set; } = -1;
        public static int UUseAnimations { get; private set; } = -1;
        public static int UBoneTransforms { get; private set; } = -1;
        public static int UShadowMap { get; private set; } = -1;
        public static int UShadowMapCube { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;
        public static int ULights { get; private set; } = -1;
        public static int ULightCount { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UMetallicType { get; private set; } = -1;
        public static int UTextureSkybox { get; private set; } = -1;
        public static int UTextureBackground { get; private set; } = -1;
        public static int UUseTextureReflection { get; private set; } = -1;
        public static int UTextureSkyboxRotation { get; private set; } = -1;
        public static int UShadowCaster { get; private set; } = -1;
        public static int UTextureClip { get; private set; } = -1;
        public static int UBlockIndex { get; private set; } = -1;


        private const int TEXTUREOFFSET = 0;        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.lighting.forwardinstanced.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.lighting.forwardinstanced.frag";

                int vertexShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                UBlockIndex = GL.GetUniformBlockIndex(ProgramID, "uInstanceBlock");
                GL.UniformBlockBinding(ProgramID, UBlockIndex, 0);

                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorMaterial = GL.GetUniformLocation(ProgramID, "uColorMaterial");
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");
                UMetallicRoughness = GL.GetUniformLocation(ProgramID, "uMetallicRoughness");
                UUseTexturesAlbedoNormalEmissive = GL.GetUniformLocation(ProgramID, "uUseTexturesAlbedoNormalEmissive");
                UUseTexturesMetallicRoughness = GL.GetUniformLocation(ProgramID, "uUseTexturesMetallicRoughness");

                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                ULightCount = GL.GetUniformLocation(ProgramID, "uLightCount");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UTextureMetallic = GL.GetUniformLocation(ProgramID, "uTextureMetallic");
                UTextureRoughness = GL.GetUniformLocation(ProgramID, "uTextureRoughness");
                UTextureEmissive = GL.GetUniformLocation(ProgramID, "uTextureEmissive");
                UTextureTransparency = GL.GetUniformLocation(ProgramID, "uTextureTransparency");
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UTextureMetallicRoughnessCombined = GL.GetUniformLocation(ProgramID, "uTextureIsMetallicRoughnessCombined");
                UMetallicType = GL.GetUniformLocation(ProgramID, "uMetallicType");
                UUseAnimations = GL.GetUniformLocation(ProgramID, "uUseAnimations");
                UBoneTransforms = GL.GetUniformLocation(ProgramID, "uBoneTransforms");

                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");

                UTextureSkybox = GL.GetUniformLocation(ProgramID, "uTextureSkybox");
                UTextureBackground = GL.GetUniformLocation(ProgramID, "uTextureBackground");
                UUseTextureReflection = GL.GetUniformLocation(ProgramID, "uUseTextureReflectionQuality");
                UTextureSkyboxRotation = GL.GetUniformLocation(ProgramID, "uTextureSkyboxRotation");

                UShadowCaster = GL.GetUniformLocation(ProgramID, "uShadowCaster");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            // lights array:
            GL.Uniform1(ULights, KWEngine.CurrentWorld._preparedLightsCount * KWEngine.LIGHTINDEXDIVIDER, KWEngine.CurrentWorld._preparedLightsArray);
            GL.Uniform1(ULightCount, KWEngine.CurrentWorld._preparedLightsCount);
            GL.Uniform3(UColorAmbient, KWEngine.CurrentWorld._colorAmbient);

            // camera pos:
            if (KWEngine.Mode == EngineMode.Play)
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraGame._stateRender._position);
            else
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraEditor._stateRender._position);

            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);

            TextureUnit currentTextureUnit = TextureUnit.Texture6;
            int currentTextureNumber = 6;
            // upload shadow maps (tex2d):
            int i;
            for (i = 0; i < KWEngine.CurrentWorld._preparedTex2DIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedTex2DIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2D, l._fbShadowMap.Attachments[0].ID);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[0]);
            }
            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureWhite);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref KWEngine.Identity);
            }

            // upload cube maps, so reset counter to 0:
            for (i = 0; i < KWEngine.CurrentWorld._preparedCubeMapIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedCubeMapIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.TextureCubeMap, l._fbShadowMap.Attachments[0].ID);
                GL.Uniform1(UShadowMapCube + i, currentTextureNumber);
            }

            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.TextureCubemapEmpty);
                GL.Uniform1(UShadowMapCube + i, currentTextureNumber);
            }

            // standard 2d background upload:
            GL.ActiveTexture(currentTextureUnit++);
            if (KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular)
            {
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.CurrentWorld._background._skyboxId);
            }
            else if (KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard)
            {
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.CurrentWorld._background._standardId);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
            }
            GL.Uniform1(UTextureBackground, currentTextureNumber++);

            // cubemap background upload:
            GL.ActiveTexture(currentTextureUnit++);
            if (KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular)
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.TextureCubemapEmpty);
            }
            else if (KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard)
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.TextureCubemapEmpty);
            }
            else if (KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.None)
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.TextureCubemapEmpty);
            }
            else
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.CurrentWorld._background._skyboxId);
            }
            GL.Uniform1(UTextureSkybox, currentTextureNumber++);

            Vector4i reflectionStats = new Vector4i(
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular ? 2 :
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.CubeMap ? 1 :
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard ? -1 : 0,
                KWEngine.CurrentWorld._background._mipMapLevels,
                (int)(KWEngine.CurrentWorld._background._brightnessMultiplier * 1000),
                (int)KWEngine.Window._ppQuality > 1 ? 1 : 0);
            GL.Uniform4(UUseTextureReflection, reflectionStats);
            if (KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular && KWEngine.CurrentWorld._background.Type == BackgroundType.Skybox)
            {
                GL.UniformMatrix3(UTextureSkyboxRotation, false, ref KWEngine.CurrentWorld._background._rotationReflection);
            }
            else
            {
                GL.UniformMatrix3(UTextureSkyboxRotation, false, ref KWEngine.CurrentWorld._background._rotation);
            }
        }

        private static void SortByZ(List<GameObject> transparentObjects)
        {
            transparentObjects.Sort();
        }

        private static void SortByZ(List<RenderObject> transparentObjects)
        {
            transparentObjects.Sort();
        }

        public static void RenderScene(List<RenderObject> transparentObjects)
        {
            if (KWEngine.CurrentWorld != null)
            {
                SortByZ(transparentObjects);
                GL.Enable(EnableCap.Blend);
                foreach (RenderObject r in transparentObjects)
                {
                    if ((!r.SkipRender && r.IsInsideScreenSpace) || KWEngine.Mode == EngineMode.Edit)
                        Draw(r);
                }
                GL.Disable(EnableCap.Blend);
            }
        }

        public static void Draw(RenderObject r)
        {
            if (r._stateRender._opacity == 0)
                return;

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBlockIndex, r._ubo);

            GL.Uniform4(UColorTint, new Vector4(r._stateRender._colorTint, r._stateRender._opacity));
            GL.Uniform4(UColorEmissive, r._stateRender._colorEmissive);
            GL.Uniform1(UMetallicType, (int)r._model._metallicType);

            int val = r.IsShadowCaster ? 1 : -1;
            val *= r.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(UShadowCaster, val);

            GeoMesh[] meshes = r._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = r._model.Material[i];
                if (material.ColorAlbedo.W <= 0)
                    continue;


                if (r.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    for (int j = 0; j < r._stateRender._boneTranslationMatrices[mesh.Name].Length; j++)
                    {
                        Matrix4 tmp = r._stateRender._boneTranslationMatrices[mesh.Name][j];
                        GL.UniformMatrix4(UBoneTransforms + j * KWEngine._uniformOffsetMultiplier, false, ref tmp);
                    }
                }
                else
                {
                    GL.Uniform1(UUseAnimations, 0);
                }

                GL.UniformMatrix4(UModelMatrix, false, ref r._stateRender._modelMatrices[i]);
                GL.UniformMatrix4(UNormalMatrix, false, ref r._stateRender._normalMatrices[i]);
                GL.Uniform2(UMetallicRoughness, new Vector2(material.Metallic, material.Roughness));
                GL.Uniform4(UColorMaterial, material.ColorAlbedo);
                GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * r._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * r._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + r._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + r._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, r._stateRender._uvClip);

                Vector3i useTexturesAlbedoNormalEmissive = new Vector3i(
                    material.TextureAlbedo.IsTextureSet ? 1 : 0,
                    material.TextureNormal.IsTextureSet ? 1 : 0,
                    material.TextureEmissive.IsTextureSet ? 1 : 0
                    );
                Vector3i useTexturesMetallicRoughness = new Vector3i(
                    material.TextureMetallic.IsTextureSet ? 1 : 0,
                    material.TextureRoughness.IsTextureSet ? 1 : 0,
                    material.TextureRoughnessIsSpecular ? 1 : 0
                    );
                GL.Uniform3(UUseTexturesAlbedoNormalEmissive, useTexturesAlbedoNormalEmissive);
                GL.Uniform3(UUseTexturesMetallicRoughness, useTexturesMetallicRoughness);

                UploadTextures(ref material, r);

                if (material.RenderBackFace && r.DisableBackfaceCulling)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                if (r.IsDepthTesting == false)
                {
                    GL.Disable(EnableCap.DepthTest);
                }
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, r.InstanceCount);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);

                if (material.RenderBackFace && r.DisableBackfaceCulling)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                if (r.IsDepthTesting == false)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
            }
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBlockIndex, 0);
        }

        private static void UploadTextures(ref GeoMaterial material, EngineObject g)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
            GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W));

            // Normal
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 1);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureNormal.IsTextureSet ? material.TextureNormal.OpenGLID : KWEngine.TextureNormalEmpty);
            GL.Uniform1(UTextureNormal, TEXTUREOFFSET + 1);

            // Emissive
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 2);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureEmissive.IsTextureSet ? material.TextureEmissive.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureEmissive, TEXTUREOFFSET + 2);

            // Transparency
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 3);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureTranparency.IsTextureSet ? material.TextureTranparency.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureTransparency, TEXTUREOFFSET + 3);

            // Metallic/Roughness
            GL.Uniform1(UTextureMetallicRoughnessCombined, material.TextureRoughnessInMetallic ? 1 : 0);
            if(material.TextureRoughnessInMetallic)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 4);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureMetallic.IsTextureSet ? material.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
                GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 4);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 4);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureMetallic.IsTextureSet ? material.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
                GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 4);

                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 5);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureRoughness.IsTextureSet ? material.TextureRoughness.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureRoughness, TEXTUREOFFSET + 5);
            }
        }
    }
}
