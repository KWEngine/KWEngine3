﻿using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererShadowMapTerrainCube
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNearFar { get; private set; } = -1;
        public static int UTerrainData { get; private set; } = -1;
        public static int UTextureHeightMap { get; private set; } = -1;
        public static int UCamPosition { get; private set; } = -1;
        public static int UCamDirection { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16terrain.cube.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.msm16terrain.cube.geom";
                string resourceNameTesselationControlShader = "KWEngine3.Shaders.shader.msm16terrain.cube.tesc";
                string resourceNameTesselationEvaluationShader = "KWEngine3.Shaders.shader.msm16terrain.cube.tese";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16terrain.cube.frag";

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
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderSceneForLight(LightObject l)
        {
            if (l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
                for (int i = 0; i < 6; i++)
                {
                    Matrix4 vp = l._stateRender._viewProjectionMatrix[i];
                    GL.UniformMatrix4(UViewProjectionMatrix + i * KWEngine._uniformOffsetMultiplier, false, ref vp);
                }

                GL.Uniform2(UNearFar, new Vector2(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y));
                GL.Uniform3(UCamPosition, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position);
                GL.Uniform3(UCamDirection, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector);

                foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    if (t.IsShadowCaster)
                        Draw(t);
                }
            }
        }
        public static void Draw(TerrainObject t)
        {
            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            
            GL.Uniform4(UTerrainData, t.Width, t.Depth, KWEngine.TERRAIN_PATCH_SIZE, t.Height);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, t._heightmap > 0 ? t._heightmap : KWEngine.TextureBlack);
            GL.Uniform1(UTextureHeightMap, 0);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.BindVertexArray(KWTerrainQuad.VAO);
            GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 4, (t.Width * t.Depth) / (KWEngine.TERRAIN_PATCH_SIZE * KWEngine.TERRAIN_PATCH_SIZE));
            GL.BindVertexArray(0);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
    }
}
