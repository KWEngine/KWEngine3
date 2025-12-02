using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer.LowQuality
{
    internal class RendererForwardLQ : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UNormalMatrix { get; private set; } = -1;
        public int UColorTint { get; private set; } = -1;
        public int UColorEmissive { get; private set; } = -1;
        public int UColorMaterial { get; private set; } = -1;
        public int UMetallicRoughness { get; private set; } = -1;
        public int UUseTexturesAlbedoNormalEmissive { get; private set; } = -1;
        public int UUseTexturesMetallicRoughness { get; private set; } = -1;
        public int UTextureMetallicRoughnessCombined { get; private set; } = -1;
        public int UTextureNormal { get; private set; } = -1;
        public int UTextureAlbedo { get; private set; } = -1;
        public int UTextureMetallic { get; private set; } = -1;
        public int UTextureRoughness { get; private set; } = -1;
        public int UTextureEmissive { get; private set; } = -1;
        public int UTextureTransparency { get; private set; } = -1;
        public int UTextureTransform { get; private set; } = -1;
        public int UUseAnimations { get; private set; } = -1;
        public int UBoneTransforms { get; private set; } = -1;
        public int UShadowMap { get; private set; } = -1;
        public int UShadowMapCube { get; private set; } = -1;
        public int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public int UViewProjectionMatrixShadowMapOuter { get; private set; } = -1;
        public int UCameraPos { get; private set; } = -1;
        public int ULights { get; private set; } = -1;
        public int ULightCount { get; private set; } = -1;
        public int UColorAmbient { get; private set; } = -1;
        public int UMetallicType { get; private set; } = -1;
        public int UTextureSkybox { get; private set; } = -1;
        public int UTextureBackground { get; private set; } = -1;
        public int UUseTextureReflection { get; private set; } = -1;
        public int UTextureSkyboxRotation { get; private set; } = -1;
        public int UShadowCaster { get; private set; } = -1;
        public int UTextureClip { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;

        public void UnbindUBO(int ubo)
        {
            
        }

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.LowQuality.shader.lighting.forwardLQ.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.LowQuality.shader.lighting.forwardLQ.frag";

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
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UTextureTransparency = GL.GetUniformLocation(ProgramID, "uTextureTransparency");
                UTextureMetallicRoughnessCombined = GL.GetUniformLocation(ProgramID, "uTextureIsMetallicRoughnessCombined");
                UMetallicType = GL.GetUniformLocation(ProgramID, "uMetallicType");
                UUseAnimations = GL.GetUniformLocation(ProgramID, "uUseAnimations");
                UBoneTransforms = GL.GetUniformLocation(ProgramID, "uBoneTransforms");

                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                UViewProjectionMatrixShadowMapOuter = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMapOuter");

                UTextureSkybox = GL.GetUniformLocation(ProgramID, "uTextureSkybox");
                UTextureBackground = GL.GetUniformLocation(ProgramID, "uTextureBackground");
                UUseTextureReflection = GL.GetUniformLocation(ProgramID, "uUseTextureReflectionQuality");
                UTextureSkyboxRotation = GL.GetUniformLocation(ProgramID, "uTextureSkyboxRotation");

                UShadowCaster = GL.GetUniformLocation(ProgramID, "uShadowCaster");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");
            }
        }

        public void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public void SetGlobals()
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


            TextureUnit currentTextureUnit = TextureUnit.Texture6;
            int currentTextureNumber = 6;
            // upload shadow maps (tex2d):
            int i;
            for (i = 0; i < KWEngine.CurrentWorld._preparedTex2DIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedTex2DIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2DArray, l._fbShadowMap.Attachments[0].ID);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[0]);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMapOuter + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[1]);
            }
            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2DArray, KWEngine.TextureWhite3D);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref KWEngine.Identity);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMapOuter + i * KWEngine._uniformOffsetMultiplier, false, ref KWEngine.Identity);
            }

            // upload cube maps, so reset counter to 0:
            for (i = 0; i < KWEngine.CurrentWorld._preparedCubeMapIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedCubeMapIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.TextureCubeMapArray, l._fbShadowMap.Attachments[0].ID);
                GL.Uniform1(UShadowMapCube + i, currentTextureNumber);
            }
            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.TextureCubeMapArray, KWEngine.TextureCubemapEmpty3D);
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
            else if(KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.None)
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
                KWEngine.Window._renderQuality > RenderQualityLevel.Default ? 1 : 0);
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

        public void RenderScene(List<GameObject> transparentObjects, List<GameObject> stencilObjects)
        {
            if (KWEngine.CurrentWorld != null)
            {
                HelperMatrix.SortByZ(transparentObjects);
                GL.Enable(EnableCap.Blend);
                foreach (GameObject g in transparentObjects)
                {
                    if (KWEngine.Mode == EngineMode.Edit || (!g.SkipRender && g.IsInsideScreenSpaceForRenderPass))
                    {
                        if (g._colorHighlightMode != HighlightMode.Disabled && g._colorHighlight.W > 0f)
                        {
                            stencilObjects.Add(g);
                        }
                        Draw(g, g.IsAttachedToViewSpaceGameObject);
                    }
                }
                GL.Disable(EnableCap.Blend);
            }
        }

        public void Draw(GameObject g, bool isVSG = false)
        {
            if(isVSG)
            {
                Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrixNoShake : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            }
            else
            {
                Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            }

            GL.Uniform4(UColorTint, new Vector4(g._stateRender._colorTint, g._stateRender._opacity));
            GL.Uniform4(UColorEmissive, g._stateRender._colorEmissive);
            GL.Uniform1(UMetallicType, (int)g._model._metallicType);

            int val = g.IsShadowCaster ? 1 : -1;
            val *= g.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(UShadowCaster, val);
            
            GeoMesh[] meshes = g._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];

                GeoMaterial material = g._model.Material[i];
                if (material.ColorAlbedo.W <= 0)
                    continue;

                GL.Uniform3(UMetallicRoughness, new Vector3(material.Metallic, material.Roughness, g._hues[i]));

                if (g.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    for (int j = 0; j < g._stateRender._boneTranslationMatrices[mesh.Name].Length; j++)
                    {
                        Matrix4 tmp = g._stateRender._boneTranslationMatrices[mesh.Name][j];
                        GL.UniformMatrix4(UBoneTransforms + j * KWEngine._uniformOffsetMultiplier, false, ref tmp);
                    }
                }
                else
                {
                    GL.Uniform1(UUseAnimations, 0);
                }

                GL.UniformMatrix4(UModelMatrix, false, ref g._stateRender._modelMatrices[i]);
                GL.UniformMatrix4(UNormalMatrix, false, ref g._stateRender._normalMatrices[i]);
                GL.Uniform4(UColorMaterial, material.ColorAlbedo);

                GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, g._stateRender._uvClip);

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
                
                UploadTextures(ref material, g);

                if (material.RenderBackFace && g.DisableBackfaceCulling)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                if(g.IsDepthTesting == false)
                {
                    GL.Disable(EnableCap.DepthTest);
                }
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);

                if (material.RenderBackFace && g.DisableBackfaceCulling)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                if (g.IsDepthTesting == false)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
            }
        }

        private void UploadTextures(ref GeoMaterial material, GameObject g)
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

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Draw(Framebuffer fbSource)
        {
            throw new NotImplementedException();
        }

        public void RenderScene()
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<RenderObject> transparentObjects)
        {
            throw new NotImplementedException();
        }

        public void RenderSceneForLight(LightObject l)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameObject g)
        {
            Draw(g, false);
        }

        public void Draw(TerrainObject t)
        {
            throw new NotImplementedException();
        }

        public void Draw(RenderObject r)
        {
            throw new NotImplementedException();
        }

        public void Draw(ViewSpaceGameObject vsgo)
        {
            throw new NotImplementedException();
        }
    }
}
