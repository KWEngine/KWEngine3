using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererTerrainCollision
    {
        public static int VAO;
        public static float[] DUMMY = new float[] { 0f, 0f, 0f };
        public static int ProgramID { get; private set; } = -1;
        public static int UPosition0 { get; private set; } = -1;
        public static int UPosition1 { get; private set; } = -1;
        public static int UPosition2 { get; private set; } = -1;
        public static int UPosition3 { get; private set; } = -1;
        public static int UModelViewProjection { get; private set; } = -1;
        public static int UIsSector { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                VAO = GL.GenVertexArray();
                GL.BindVertexArray(VAO);
                int VBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, DUMMY.Length * 4, DUMMY, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.terrainDebug.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.terrainDebug.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.terrainDebug.frag";

                int vertexShader;
                int geometryShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameGeometryShader))
                {
                    geometryShader = HelperShader.LoadCompileAttachShader(s, ShaderType.GeometryShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, geometryShader);

                UPosition0 = GL.GetUniformLocation(ProgramID, "uPosition0");
                UPosition1 = GL.GetUniformLocation(ProgramID, "uPosition1");
                UPosition2 = GL.GetUniformLocation(ProgramID, "uPosition2");
                UPosition3 = GL.GetUniformLocation(ProgramID, "uPosition3");
                UModelViewProjection = GL.GetUniformLocation(ProgramID, "uModelViewProjection");
                UIsSector = GL.GetUniformLocation(ProgramID, "uIsSector");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            Matrix4 vp = KWEngine.Mode != EngineMode.Edit ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            GL.UniformMatrix4(UModelViewProjection, false, ref vp);
            HelperGeneral.CheckGLErrors();
        }

        public static void Draw(TerrainObject t)
        {
            GL.BindVertexArray(VAO);
            Sector[,] map = t._gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.mSectorMap;
            foreach (Sector sector in map)
            {
                if (sector != null)
                {
                    foreach (GeoTerrainTriangle tri in sector.Triangles)
                    {
                        GL.Uniform3(UPosition0, tri.Vertices[0]);
                        GL.Uniform3(UPosition1, tri.Vertices[1]);
                        GL.Uniform3(UPosition2, tri.Vertices[2]);
                        GL.Uniform3(UPosition3, Vector3.Zero);
                        GL.Uniform1(UIsSector, 0);
                        GL.DrawArrays(PrimitiveType.Points, 0, 1);
                    }

                    Vector3 sectorLeftBack = new Vector3(sector.Left, 0, sector.Back);
                    Vector3 sectorLeftFront = new Vector3(sector.Left, 0, sector.Front);
                    Vector3 sectorRightFront = new Vector3(sector.Right, 0, sector.Front);
                    Vector3 sectorRightBack = new Vector3(sector.Right, 0, sector.Back);

                    GL.Uniform3(UPosition0, sectorLeftBack);
                    GL.Uniform3(UPosition1, sectorLeftFront);
                    GL.Uniform3(UPosition2, sectorRightFront);
                    GL.Uniform3(UPosition3, sectorRightBack);
                    GL.Uniform1(UIsSector, 1);
                    GL.DrawArrays(PrimitiveType.Points, 0, 1);
                }
            }
            GL.BindVertexArray(0);
        }

    }
}
