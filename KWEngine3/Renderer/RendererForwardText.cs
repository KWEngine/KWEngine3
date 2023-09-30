using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererForwardText
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UShadowMap { get; private set; } = -1;
        public static int UShadowMapCube { get; private set; } = -1;
        public static int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;
        public static int ULights { get; private set; } = -1;
        public static int ULightCount { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UCharacterOffsets { get; private set; } = -1;
        public static int UTextOffset { get; private set; } = -1;
        public static int USpread { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.lighting.forward.text.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.lighting.forward.text.frag";

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
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");

                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                ULightCount = GL.GetUniformLocation(ProgramID, "uLightCount");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UCharacterOffsets = GL.GetUniformLocation(ProgramID, "uCharacterOffsets");
                USpread = GL.GetUniformLocation(ProgramID, "uSpread");
                UTextOffset = GL.GetUniformLocation(ProgramID, "uTextOffset");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");

                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
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

            TextureUnit currentTextureUnit = TextureUnit.Texture1;
            int currentTextureNumber = 1;
            // upload shadow maps (tex2d):
            int i;
            for (i = 0; i < KWEngine.CurrentWorld._preparedTex2DIndices.Count; i++, currentTextureUnit++, currentTextureNumber++)
            {
                LightObject l = KWEngine.CurrentWorld._lightObjects[KWEngine.CurrentWorld._preparedTex2DIndices[i]];
                GL.ActiveTexture(currentTextureUnit);
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.Window._ppQuality == PostProcessingQuality.High ? l._fbShadowMap._blurBuffer2.Attachments[0].ID : l._fbShadowMap.Attachments[0].ID);
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
        }

        private static void SortByZ()
        {
            KWEngine.CurrentWorld._textObjects.Sort();
        }

        public static void RenderScene()
        {
            if (KWEngine.CurrentWorld != null)
            {
                SortByZ();
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.CullFace);
                GeoMesh mesh = KWEngine.Models["KWQuad"].Meshes.Values.ElementAt(0);
                foreach (TextObject t in KWEngine.CurrentWorld._textObjects)
                {
                    if (KWEngine.EditModeActive)
                    {
                        Draw(t, mesh);
                    }
                    else
                    {
                        if (t.IsInsideScreenSpace)
                            Draw(t, mesh);
                    }
                }
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
            }
        }

        public static void Draw(TextObject t, GeoMesh mesh)
        {
            GL.Uniform4(UColorTint, t._stateRender._color);
            GL.Uniform4(UColorEmissive, t._stateRender._colorEmissive);
            GL.Uniform1(UCharacterOffsets, t._offsets.Count, t._offsets.ToArray());
            GL.Uniform1(UTextOffset, (t._offsets.Count - 1) * t._stateRender._spreadFactor / 2.0f);
            GL.Uniform1(USpread, t._stateRender._spreadFactor);
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            
            UploadTextures(t);
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, t._offsets.Count);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private static void UploadTextures(TextObject t)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);

            int texId = KWEngine.FontTextureArray[(int)t._fontFace];
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
        }
    }
}
