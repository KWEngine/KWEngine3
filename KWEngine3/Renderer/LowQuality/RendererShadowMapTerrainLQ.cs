using KWEngine3.Assets;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer.LowQuality
{
    internal class RendererShadowMapTerrainLQ : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UNearFarSun { get; private set; } = -1;
        public int UTerrainData { get; private set; } = -1;
        public int UTextureHeightMap { get; private set; } = -1;
        public int UCamPosition { get; private set; } = -1;
        public int UCamDirection { get; private set; } = -1;
        public int UTerrainThreshold { get; private set; } = -1;

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrainLQ.vert";
                string resourceNameTesselationControlShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrainLQ.tesc";
                string resourceNameTesselationEvaluationShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrainLQ.tese";
                string resourceNameFragmentShader = "KWEngine3.Shaders.LowQuality.shader.msm16terrainLQ.frag";

                int vertexShader;
                int fragmentShader;
                int tessControl;
                int tessEval;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
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
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, -1, tessControl, tessEval);


                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UNearFarSun = GL.GetUniformLocation(ProgramID, "uNearFarSun");
                UTerrainData = GL.GetUniformLocation(ProgramID, "uTerrainData");
                UCamPosition = GL.GetUniformLocation(ProgramID, "uCamPosition");
                UCamDirection = GL.GetUniformLocation(ProgramID, "uCamDirection");
                UTerrainThreshold = GL.GetUniformLocation(ProgramID, "uTerrainThreshold");
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
                Matrix4 vp = l._stateRender._viewProjectionMatrix[0];
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
                GL.Uniform3(UNearFarSun, new Vector3(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y, l._stateRender._nearFarFOVType.W));
                foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    if (t.IsShadowCaster)
                    {
                        if (l._frustumShadowMap.IsBoxInFrustum(
                            l.Position, 
                            l._stateRender._lookAtVector, 
                            l._stateRender._nearFarFOVType.Y, 
                            t._hitboxes[0]._center + new Vector3(0, t.Height * 0.5f, 0),
                            new Vector3(t._hitboxes[0]._center.X - t.Width / 2f, t._hitboxes[0]._center.Y, t._hitboxes[0]._center.Z - t.Depth / 2f),
                            new Vector3(t._hitboxes[0]._center.X + t.Width / 2f, t._hitboxes[0]._center.Y + t.Height, t._hitboxes[0]._center.Z + t.Depth / 2f),
                            t._hitboxes[0]._fullDiameter))
                        {
                            Draw(t);
                        }
                    }
                }
            }
        }
        public void Draw(TerrainObject t)
        {
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            GL.Uniform3(UCamPosition, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position);
            GL.Uniform3(UCamDirection, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector);
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
