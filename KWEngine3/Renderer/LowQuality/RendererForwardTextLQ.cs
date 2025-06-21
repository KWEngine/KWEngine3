using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer.LowQuality
{
    internal class RendererForwardTextLQ : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UColorTint { get; private set; } = -1;
        public int UColorEmissive { get; private set; } = -1;
        public int UTextureAlbedo { get; private set; } = -1;
        public int UShadowMap { get; private set; } = -1;
        public int UShadowMapCube { get; private set; } = -1;
        public int UViewProjectionMatrixShadowMap { get; private set; } = -1;
        public int UViewProjectionMatrixShadowMapOuter { get; private set; } = -1;
        public int UCameraPos { get; private set; } = -1;
        public int ULights { get; private set; } = -1;
        public int ULightCount { get; private set; } = -1;
        public int UColorAmbient { get; private set; } = -1;
        public int UUVOffsets { get; private set; } = -1;
        public int UAdvanceList { get; private set; } = -1;
        public int UWidths { get; private set; } = -1;
        public int UShadowCaster { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;        

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.LowQuality.shader.lighting.forwardtextLQ.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.LowQuality.shader.lighting.forwardtextLQ.frag";

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
                UUVOffsets = GL.GetUniformLocation(ProgramID, "uUVOffsetsAndWidths");
                UUVOffsets = GL.GetUniformLocation(ProgramID, "uUVOffsetsAndWidths");
                UWidths = GL.GetUniformLocation(ProgramID, "uWidths");
                UAdvanceList = GL.GetUniformLocation(ProgramID, "uAdvanceList");
                
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                UViewProjectionMatrixShadowMapOuter = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMapOuter");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UShadowCaster = GL.GetUniformLocation(ProgramID, "uShadowCaster");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");
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
        }

        private void SortByZ()
        {
            KWEngine.CurrentWorld._textObjects.Sort();
        }

        public void RenderScene()
        {
            if (KWEngine.CurrentWorld != null)
            {
                SortByZ();
                GL.Enable(EnableCap.Blend);
                GeoMesh mesh = KWEngine.Models["KWQuad"].Meshes.Values.ElementAt(0);
                foreach (TextObject t in KWEngine.CurrentWorld._textObjects)
                {
                    if (KWEngine.EditModeActive || t.IsInsideScreenSpace)
                    {
                        Draw(t, mesh);
                    }
                }
                GL.Disable(EnableCap.Blend);
            }
        }

        public void Draw(TextObject t, GeoMesh mesh)
        {
            GL.Uniform4(UColorTint, t._stateRender._color);
            GL.Uniform4(UColorEmissive, t._stateRender._colorEmissive);
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);

            GL.Uniform2(UUVOffsets, t._text.Length, t._uvOffsets);
            GL.Uniform1(UAdvanceList, t._text.Length, t._advances);
            GL.Uniform1(UWidths, t._text.Length, t._glyphWidths);
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            
            int val = t.IsShadowReceiver ? 1 : -1;
            val *= t.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(UShadowCaster, val);

            UploadTextures(t);

            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, t._text.Length);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void UploadTextures(TextObject t)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, t._font.Texture);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Draw(Framebuffer fbSource)
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<GameObject> transparentObjects)
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
            throw new NotImplementedException();
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

        public void Draw(GameObject g, bool isVSG = false)
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<GameObject> transparentObjects, List<GameObject> stencilObjects)
        {
            throw new NotImplementedException();
        }
    }
}
