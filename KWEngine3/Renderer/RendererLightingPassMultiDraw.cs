using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererLightingPassMultiDraw
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureID { get; private set; } = -1;
        public static int UTextureSSAO { get; private set; } = -1;
        public static int UTextureMetallicRoughnessMetallicType { get; private set; } = -1;
        public static int ULights { get; private set; } = -1;
        
        public static int ULightIndices { get; private set; } = -1;
        public static int ULightIndicesCount { get; private set; } = -1;
        public static int UTextureOffset { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        public static int UShadowMap { get; private set; } = -1;
        public static int UShadowMapCube { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap2 { get; private set; } = -1;
        public static int UTextureSkybox { get; private set; } = -1;
        public static int UTextureBackground { get; private set; } = -1;
        public static int UUseTextureReflection { get; private set; } = -1;
        public static int UTextureSkyboxRotation { get; private set; } = -1;
        public static int UTextureDepth { get; private set; } = -1;
        public static int UViewProjectionMatrixInverted { get; private set; } = -1;

        
        public static int ULightIndicesCounts { get; private set; } = -1;
        public static int UBlockIndex1 { get; private set; } = -1;
        public static int UBlockIndex2 { get; private set; } = -1;
        public static int UBlockIndex3 { get; private set; } = -1;

        public static int UBO { get; private set; } = -1;
        public static int UBO2 { get; private set; } = -1;
        public static int UBO3 { get; private set; } = -1;

        
        public static float[] _uboData;
        public static float[] _uboData2;
        public static int[] _uboData3;
        public static int[] _indexCounts;
        
        
        internal static void InitUBOs()
        {
            if (_uboData == null)
            {
                _uboData = new float[RenderManager._screenGrid._tiles.Length * 4];
                _uboData2 = new float[RenderManager._screenGrid._tiles.Length * 4];
                _uboData3 = new int[50 * RenderManager._screenGrid._tiles.Length];
            }

            float resDivider = Math.Max(RenderManager._screenGrid._width, RenderManager._screenGrid._height);
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            for (int i = 0, j = 0; i < RenderManager._screenGrid._tiles.Length; i++, j += 4)
            {
                ScreenGridTile tile = RenderManager._screenGrid._tiles[i];
                _uboData[j + 0] = tile._ndcRight - tile._ndcLeft;
                _uboData[j + 1] = tile._ndcTop - tile._ndcBottom;
                _uboData[j + 2] = tile._ndcCenter.X;
                _uboData[j + 3] = tile._ndcCenter.Y;
            }
            GL.BufferData(BufferTarget.UniformBuffer, _uboData.Length * sizeof(float), _uboData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            UBO2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO2);
            for (int i = 0, j = 0; i < RenderManager._screenGrid._tiles.Length; i++, j += 4)
            {
                ScreenGridTile tile = RenderManager._screenGrid._tiles[i];
                float uvL = tile._ndcLeft * 0.5f + 0.5f;
                float uvR = tile._ndcRight * 0.5f + 0.5f;
                float uvT = tile._ndcTop * 0.5f + 0.5f;
                float uvB = tile._ndcBottom * 0.5f + 0.5f;
                _uboData2[j + 0] = uvL;
                _uboData2[j + 1] = uvR;
                _uboData2[j + 2] = uvT;
                _uboData2[j + 3] = uvB;
            }
            GL.BufferData(BufferTarget.UniformBuffer, _uboData2.Length * sizeof(float), _uboData2, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            UBO3 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO3);
            GL.BufferData(BufferTarget.UniformBuffer, _uboData3.Length * sizeof(int), _uboData3, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            _indexCounts = new int[RenderManager._screenGrid._tiles.Length];
        }
        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.lighting.gbuffer_multi.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.lighting.gbuffer_multi.frag";

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

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);
                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);
                
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UTextureMetallicRoughnessMetallicType = GL.GetUniformLocation(ProgramID, "uTexturePBR");
                UTextureID = GL.GetUniformLocation(ProgramID, "uTextureId");
                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UTextureSSAO = GL.GetUniformLocation(ProgramID, "uTextureSSAO");

                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");

                UTextureSkybox = GL.GetUniformLocation(ProgramID, "uTextureSkybox");
                UTextureBackground = GL.GetUniformLocation(ProgramID, "uTextureBackground");
                UUseTextureReflection = GL.GetUniformLocation(ProgramID, "uUseTextureReflectionQuality");
                UTextureSkyboxRotation = GL.GetUniformLocation(ProgramID, "uTextureSkyboxRotation");

                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                UViewProjectionMatrixShadowMap2 = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap2");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");

                ULightIndicesCount = GL.GetUniformLocation(ProgramID, "uLightIndicesCount");
                ULightIndices = GL.GetUniformLocation(ProgramID, "uLightIndices");
                UTextureOffset = GL.GetUniformLocation(ProgramID, "uTextureOffset");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            TextureUnit currentTextureUnit = TextureUnit.Texture6;
            int currentTextureNumber = 6;
            // upload shadow maps (tex2d):
            int i = 0;
            for (i = 0; i < KWEngine.CurrentWorld._preparedTex2DIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedTex2DIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2DArray, l._fbShadowMap.Attachments[0].ID);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[0]);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap2 + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[1]);
            }
            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2DArray, KWEngine.TextureWhite3D);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i * KWEngine._uniformOffsetMultiplier, false, ref KWEngine.Identity);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap2 + i * KWEngine._uniformOffsetMultiplier, false, ref KWEngine.Identity);
            }


            // upload cube maps, so reset i to 0:
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

            // camera pos:
            if (KWEngine.Mode == EngineMode.Play)
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraGame._stateRender._position);
            else
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraEditor._stateRender._position);

            // standard 2d background upload:
            GL.ActiveTexture(currentTextureUnit++);
            if(KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular)
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
            if(KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular)
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
            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            vp.Invert();
            GL.UniformMatrix4(UViewProjectionMatrixInverted, false, ref vp);
        }

        public static void Draw(Framebuffer fbSource)
        {
            // depth tex:
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[4].ID);
            GL.Uniform1(UTextureDepth, 0);
            // albedo:
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            GL.Uniform1(UTextureAlbedo, 1);

            // normal:
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[1].ID);
            GL.Uniform1(UTextureNormal, 2);

            // pbr:
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[2].ID);
            GL.Uniform1(UTextureMetallicRoughnessMetallicType, 3);

            // id:
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[3].ID);
            GL.Uniform1(UTextureID, 4);

            // ssao:
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.SSAO_Enabled ? RenderManager.FramebufferSSAOBlur.Attachments[0].ID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureSSAO, 5);

            // lights array:
            GL.Uniform1(ULights, KWEngine.CurrentWorld._preparedLightsCount * 17, KWEngine.CurrentWorld._preparedLightsArray);
            GL.Uniform3(UColorAmbient, KWEngine.CurrentWorld._colorAmbient);
            
            // render that damn quad already:
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            
            foreach (ScreenGridTile tile in RenderManager._screenGrid._tiles)
            {
                GL.Viewport(tile._offsetX, RenderManager._screenGrid._height - tile._height - tile._offsetY, tile._width, tile._height);
                GL.Uniform1(ULightIndices, tile._preparedLightsIndicesCount, tile._preparedLightsIndices);
                GL.Uniform1(ULightIndicesCount, tile._preparedLightsIndicesCount);

                float uvL = tile._ndcLeft * 0.5f + 0.5f;
                float uvR = tile._ndcRight * 0.5f + 0.5f;
                float uvT = tile._ndcTop * 0.5f + 0.5f;
                float uvB = tile._ndcBottom * 0.5f + 0.5f;

                GL.Uniform4(UTextureOffset, uvL, uvR, uvT, uvB);
                GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            }
            
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
