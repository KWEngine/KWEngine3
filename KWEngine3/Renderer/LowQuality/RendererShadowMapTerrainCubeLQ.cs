using KWEngine3.Assets;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer.LowQuality
{
    internal class RendererShadowMapTerrainCubeLQ : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UNearFar { get; private set; } = -1;
        public int UTerrainData { get; private set; } = -1;
        public int UTextureHeightMap { get; private set; } = -1;
        public int UCamPosition { get; private set; } = -1;
        public int UCamDirection { get; private set; } = -1;
        public int ULightPosition { get; private set; } = -1;
        public int UTerrainThreshold { get; private set; } = -1;

        public void UnbindUBO(int ubo)
        {
            
        }

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrain.cubeLQ.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrain.cubeLQ.geom";
                string resourceNameTesselationControlShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrain.cubeLQ.tesc";
                string resourceNameTesselationEvaluationShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrain.cubeLQ.tese";
                string resourceNameFragmentShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrain.cubeLQ.frag";

                int vertexShader;
                int geometryShader;
                int fragmentShader;
                int tessControl;
                int tessEval;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameGeometryShader))
                {
                    geometryShader = HelperShader.LoadCompileAttachShader(s, ShaderType.GeometryShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameTesselationControlShader))
                {
                    tessControl = HelperShader.LoadCompileAttachShader(s, ShaderType.TessControlShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameTesselationEvaluationShader))
                {
                    tessEval = HelperShader.LoadCompileAttachShader(s, ShaderType.TessEvaluationShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, geometryShader, tessControl, tessEval);

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UNearFar = GL.GetUniformLocation(ProgramID, "uNearFar");
                UTerrainData = GL.GetUniformLocation(ProgramID, "uTerrainData");
                UCamPosition = GL.GetUniformLocation(ProgramID, "uCamPosition");
                UCamDirection = GL.GetUniformLocation(ProgramID, "uCamDirection");
                UTerrainThreshold = GL.GetUniformLocation(ProgramID, "uTerrainThreshold");
                ULightPosition = GL.GetUniformLocation(ProgramID, "uLightPosition");
            }
        }

        public void SetGlobals()
        {

        }

        public void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public void RenderSceneForLight(LightObject l)
        {
            if (l.ShadowQualityLevel != ShadowQuality.NoShadow && l.Color.W > 0)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
                for (int i = 0; i < 6; i++)
                {
                    GL.UniformMatrix4(UViewProjectionMatrix + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[i]);
                }

                GL.Uniform2(UNearFar, new Vector2(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y));
                GL.Uniform3(ULightPosition, l._stateRender._position);
                GL.Uniform3(UCamPosition, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position);
                GL.Uniform3(UCamDirection, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector);

                foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    if (t.IsShadowCaster)
                    {
                        Draw(t);
                    }
                }
            }
        }
        public void Draw(TerrainObject t)
        {
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            
            GL.Uniform4(UTerrainData, t.Width, t.Depth, KWEngine.TERRAIN_PATCH_SIZE, t.Height);
            GL.Uniform1(UTerrainThreshold, (int)t.TessellationThreshold);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, t._heightmap > 0 ? t._heightmap : KWEngine.TextureBlack);
            GL.Uniform1(UTextureHeightMap, 0);

            GL.BindVertexArray(KWTerrainQuad.VAO);
            GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 4, (t.Width * t.Depth) / (KWEngine.TERRAIN_PATCH_SIZE * KWEngine.TERRAIN_PATCH_SIZE));
            GL.BindVertexArray(0);
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

        public void RenderScene(List<GameObject> transparentObjects)
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<RenderObject> transparentObjects)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameObject g)
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

