using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererLightingPass
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureID { get; private set; } = -1;
        public static int UTextureMetallicRoughnessMetallicType { get; private set; } = -1;
        public static int ULights { get; private set; } = -1;
        public static int ULightCount { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        public static int UShadowMap { get; private set; } = -1;
        public static int UShadowMapCube { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public static int UTextureSkybox { get; private set; } = -1;
        public static int UTextureBackground { get; private set; } = -1;
        public static int UUseTextureReflection { get; private set; } = -1;
        public static int UTextureSkyboxRotation { get; private set; } = -1;
        public static int UTextureDepth { get; private set; } = -1;
        public static int UViewProjectionMatrixInverted { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.lighting.gbuffer.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.lighting.gbuffer.frag";

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

                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                ULightCount = GL.GetUniformLocation(ProgramID, "uLightCount");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");

                
                UTextureSkybox = GL.GetUniformLocation(ProgramID, "uTextureSkybox");
                UTextureBackground = GL.GetUniformLocation(ProgramID, "uTextureBackground");
                UUseTextureReflection = GL.GetUniformLocation(ProgramID, "uUseTextureReflection");
                UTextureSkyboxRotation = GL.GetUniformLocation(ProgramID, "uTextureSkyboxRotation");

                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            TextureUnit currentTextureUnit = TextureUnit.Texture5;
            int currentTextureNumber = 5;
            // upload shadow maps (tex2d):
            int i = 0;
            for (i = 0; i < KWEngine.CurrentWorld._preparedTex2DIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedTex2DIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                if (KWEngine.Window._ppQuality == PostProcessingQuality.High)
                {
                    GL.BindTexture(TextureTarget.Texture2D, l._fbShadowMap._blurBuffer2.Attachments[0].ID);
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, l._fbShadowMap.Attachments[0].ID);
                }
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


            // upload cube maps, so reset i to 0:
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
            else
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.CurrentWorld._background._skyboxId);
            }
            GL.Uniform1(UTextureSkybox, currentTextureNumber++);

            Vector3i reflectionStats = new Vector3i(
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.Equirectangular ? 2 :
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox && KWEngine.CurrentWorld._background.SkyBoxType == SkyboxType.CubeMap ? 1 :
                KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard ? -1 : 0,
                KWEngine.CurrentWorld._background._mipMapLevels,
                (int)(KWEngine.CurrentWorld._background._brightnessMultiplier * 1000));
            GL.Uniform3(UUseTextureReflection, reflectionStats);
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

            // lights array:
            GL.Uniform1(ULights, KWEngine.CurrentWorld._preparedLightsCount * 17, KWEngine.CurrentWorld._preparedLightsArray);
            GL.Uniform1(ULightCount, KWEngine.CurrentWorld._preparedLightsCount);
            GL.Uniform3(UColorAmbient, KWEngine.CurrentWorld._colorAmbient);

            // render that damn quad already:
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.Enable(EnableCap.DepthTest);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
