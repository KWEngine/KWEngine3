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
       
        public static int UTexturePositionDepth { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureNormalId { get; private set; } = -1;
        public static int UTextureCSDepthMetallicRoughnessOcclusion { get; private set; } = -1;
        public static int UTextureEmissive { get; private set; } = -1;
        public static int ULights { get; private set; } = -1;
        public static int ULightCount { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        public static int UShadowMap { get; private set; } = -1;
        public static int UShadowMapCube { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        //public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UTextureSkybox { get; private set; } = -1;
        public static int UTextureBackground { get; private set; } = -1;
        public static int UUseTextureReflection { get; private set; } = -1;
        public static int UTextureSkyboxRotation { get; private set; } = -1;

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
                UTexturePositionDepth = GL.GetUniformLocation(ProgramID, "uTexturePositionDepth");
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormalId = GL.GetUniformLocation(ProgramID, "uTextureNormalId");
                UTextureCSDepthMetallicRoughnessOcclusion = GL.GetUniformLocation(ProgramID, "uTexturePBR");
                UTextureEmissive = GL.GetUniformLocation(ProgramID, "uTextureEmissive");
                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                ULightCount = GL.GetUniformLocation(ProgramID, "uLightCount");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                //UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureSkybox = GL.GetUniformLocation(ProgramID, "uTextureSkybox");
                UTextureBackground = GL.GetUniformLocation(ProgramID, "uTextureBackground");
                UUseTextureReflection = GL.GetUniformLocation(ProgramID, "uUseTextureReflection");
                UTextureSkyboxRotation = GL.GetUniformLocation(ProgramID, "uTextureSkyboxRotation");
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
                GL.BindTexture(TextureTarget.Texture2D, l._fbShadowMap._blurBuffer2.Attachments[0].ID);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i, false, ref l._stateRender._viewProjectionMatrix[0]);
            }
            for (; i < KWEngine.MAX_SHADOWMAPS; i++, currentTextureUnit++, currentTextureNumber++)
            {
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureWhite);
                GL.Uniform1(UShadowMap + i, currentTextureNumber);
                GL.UniformMatrix4(UViewProjectionMatrixShadowMap + i, false, ref KWEngine.Identity);
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

            // camera pos:
            if (KWEngine.Mode == EngineMode.Play)
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraGame._stateRender._position);
            else
                GL.Uniform3(UCameraPos, KWEngine.CurrentWorld._cameraEditor._stateRender._position);


            GL.ActiveTexture(currentTextureUnit++);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard ? KWEngine.CurrentWorld._background._standardId : KWEngine.TextureBlack);
            GL.Uniform1(UTextureBackground, currentTextureNumber++);

            GL.ActiveTexture(currentTextureUnit++);
            GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox ? KWEngine.CurrentWorld._background._skyboxId : KWEngine.TextureCubemapEmpty);
            GL.Uniform1(UTextureSkybox, currentTextureNumber++);

            GL.Uniform3(UUseTextureReflection, new Vector3i(KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Skybox ? 1 : KWEngine.CurrentWorld.BackgroundTextureType == BackgroundType.Standard ? -1 : 0, KWEngine.CurrentWorld._background._mipMapLevels, (int)(KWEngine.CurrentWorld._background._brightnessMultiplier * 1000)));
            GL.UniformMatrix3(UTextureSkyboxRotation, false, ref KWEngine.CurrentWorld._background._rotation);
        }

        public static void Draw(Framebuffer fbSource)
        {
            //Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            //GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);

            // position & depth texture:
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            GL.Uniform1(UTexturePositionDepth, 0);

            // albedo:
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[1].ID);
            GL.Uniform1(UTextureAlbedo, 1);

            // normalId:
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[2].ID);
            GL.Uniform1(UTextureNormalId, 2);

            // pbr:
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[3].ID);
            GL.Uniform1(UTextureCSDepthMetallicRoughnessOcclusion, 3);

            // emissive:
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[4].ID);
            GL.Uniform1(UTextureEmissive, 4);

            // lights array:
            GL.Uniform1(ULights, KWEngine.CurrentWorld._preparedLightsCount * 17, KWEngine.CurrentWorld._preparedLightsArray);
            GL.Uniform1(ULightCount, KWEngine.CurrentWorld._preparedLightsCount);
            GL.Uniform3(UColorAmbient, KWEngine.CurrentWorld._colorAmbient);

            // render that damn quad already:
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
