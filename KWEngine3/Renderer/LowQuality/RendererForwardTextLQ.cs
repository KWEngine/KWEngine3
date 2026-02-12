using KWEngine3.Assets;
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
        public int UColorOutline { get; private set; } = -1;
        public int UShadowCaster { get; private set; } = -1;
        public int UGlyphInfo { get; private set; } = -1;
        public int UPositionAndOffset { get; private set; } = -1;
        public int URotation { get; private set; } = -1;
        public int UScale { get; private set; } = -1;
        public int UAdvances { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;

        public void UnbindUBO(int ubo)
        {
            
        }

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
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);


                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");

                ULights = GL.GetUniformLocation(ProgramID, "uLights");
                ULightCount = GL.GetUniformLocation(ProgramID, "uLightCount");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UUVOffsets = GL.GetUniformLocation(ProgramID, "uUVOffsetsAndWidths");
                UColorOutline = GL.GetUniformLocation(ProgramID, "uColorOutline");
                UGlyphInfo = GL.GetUniformLocation(ProgramID, "uGlyphInfo");

                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UViewProjectionMatrixShadowMap = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMap");
                UViewProjectionMatrixShadowMapOuter = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixShadowMapOuter");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UShadowCaster = GL.GetUniformLocation(ProgramID, "uShadowCaster");
                UShadowMap = GL.GetUniformLocation(ProgramID, "uShadowMap");
                UShadowMapCube = GL.GetUniformLocation(ProgramID, "uShadowMapCube");
                
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");

                UPositionAndOffset = GL.GetUniformLocation(ProgramID, "uPositionAndOffset");
                URotation = GL.GetUniformLocation(ProgramID, "uRotation");
                UScale = GL.GetUniformLocation(ProgramID, "uScale");
                UAdvances = GL.GetUniformLocation(ProgramID, "uAdvances");
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

            TextureUnit currentTextureUnit = TextureUnit.Texture2;
            int currentTextureNumber = 2;
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
                GL.Disable(EnableCap.CullFace);
                foreach (TextObject t in KWEngine.CurrentWorld._textObjects)
                {
                    if (KWEngine.EditModeActive || t.IsInsideScreenSpace)
                    {
                        Draw(t, null);
                    }
                }
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
            }
        }

        public void Draw(TextObject t, GeoMesh mesh)
        {
            GL.Uniform4(UColorTint, t._stateRender._color);
            GL.Uniform4(UColorEmissive, t._stateRender._colorEmissive);
            GL.Uniform4(UColorOutline, t._colorOutline);
            GL.Uniform1(UUVOffsets, t._text.Length * 4, t._uvOffsets);
            GL.Uniform4(UScale, new Vector4(t._stateRender._scale, 0f));
            GL.Uniform1(UAdvances, t._text.Length, t._advances);
            GL.Uniform4(URotation, new Vector4(t._stateRender._rotation.X, t._stateRender._rotation.Y, t._stateRender._rotation.Z, t._stateRender._rotation.W));
            GL.Uniform4(UPositionAndOffset, new Vector4(t._stateRender._position,
                t._textAlignMode == TextAlignMode.Left ? 0f :
                t._textAlignMode == TextAlignMode.Right ? -t._widthNormalised : -t._widthNormalised * 0.5f));

            int val = t.IsShadowReceiver ? 1 : -1;
            val *= t.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(UShadowCaster, val);

            UploadTextures(t);

            GL.BindVertexArray(KWQuad2D_05.VAO);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, t._text.Length);
            GL.BindVertexArray(0);
        }

        private void UploadTextures(TextObject t)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, t._font.Texture);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);

            //MSDF-Info
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureBuffer, t._textureBufferTex);
            GL.Uniform1(UGlyphInfo, 1);
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
