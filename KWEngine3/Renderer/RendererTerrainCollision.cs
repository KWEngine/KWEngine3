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
        public static int UPositions { get; private set; } = -1;
        public static int UPositionsCount { get; private set; } = -1;

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
                UPositions = GL.GetUniformLocation(ProgramID, "uPositions");
                UPositionsCount = GL.GetUniformLocation(ProgramID, "uPositionsCount");
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
        }

        public static void Draw(TerrainObject t)
        {
            GL.BindVertexArray(VAO);
            Sector[,] map = t._gModel.ModelOriginal.Meshes.Values.ToArray()[0].Terrain.mSectorMap;
            foreach (Sector sector in map)
            {
                List<float> tripositions = new();
                List<float> tripositions2 = new();
                int c = 0;
                foreach (GeoTerrainTriangle tri in sector.Triangles)
                {
                    if (c > sector.Triangles.Length / 2)
                    {
                        tripositions2.Add(tri.Vertices[0].X);
                        tripositions2.Add(tri.Vertices[0].Y);
                        tripositions2.Add(tri.Vertices[0].Z);
                        tripositions2.Add(tri.Vertices[1].X);
                        tripositions2.Add(tri.Vertices[1].Y);
                        tripositions2.Add(tri.Vertices[1].Z);
                        tripositions2.Add(tri.Vertices[2].X);
                        tripositions2.Add(tri.Vertices[2].Y);
                        tripositions2.Add(tri.Vertices[2].Z);
                    }
                    else
                    {
                        tripositions.Add(tri.Vertices[0].X);
                        tripositions.Add(tri.Vertices[0].Y);
                        tripositions.Add(tri.Vertices[0].Z);
                        tripositions.Add(tri.Vertices[1].X);
                        tripositions.Add(tri.Vertices[1].Y);
                        tripositions.Add(tri.Vertices[1].Z);
                        tripositions.Add(tri.Vertices[2].X);
                        tripositions.Add(tri.Vertices[2].Y);
                        tripositions.Add(tri.Vertices[2].Z);
                    }

                    c++;
                }

                GL.Uniform1(UPositionsCount, tripositions.Count / 3);
                GL.Uniform3(UPositions, tripositions.Count / 3, tripositions.ToArray());
                GL.Uniform1(UIsSector, 0);
                GL.DrawArrays(PrimitiveType.Points, 0, 1);
                GL.Uniform1(UPositionsCount, tripositions2.Count / 3);
                GL.Uniform3(UPositions, tripositions2.Count / 3, tripositions2.ToArray());
                GL.Uniform1(UIsSector, 0);
                GL.DrawArrays(PrimitiveType.Points, 0, 1);

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
            GL.BindVertexArray(0);
        }

    }
}
