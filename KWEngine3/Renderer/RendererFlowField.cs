using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererFlowField
    {
        private static float[] _dummy = new float[] { 0f, 0f, 0f };
        private static int VAO = -1;
        private static int VBO = -1;

        public static int ProgramID { get; private set; } = -1;

        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UCenter { get; private set; } = -1;
        public static int URadius { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;

        private static int LoadShader(Stream pFileStream, ShaderType pType, int pProgram)
        {
            int address = GL.CreateShader(pType);
            using (StreamReader sr = new StreamReader(pFileStream))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(pProgram, address);
            return address;
        }

        public static void Init()
        {
            if (ProgramID < 0)
            {
                VAO = GL.GenVertexArray();
                GL.BindVertexArray(VAO);
                VBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, _dummy.Length * 4, _dummy, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                ProgramID = GL.CreateProgram();

                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.flowfield.frag";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.flowfield.geom";
                string resourceNameVertexShader = "KWEngine3.Shaders.shader.flowfield.vert";

                int vertexShader;
                int geometryShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = LoadShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameGeometryShader))
                {
                    geometryShader = LoadShader(s, ShaderType.GeometryShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = LoadShader(s, ShaderType.FragmentShader, ProgramID);
                }
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, geometryShader);
                GL.LinkProgram(ProgramID);
              
                UCenter = GL.GetUniformLocation(ProgramID, "uCenter");
                URadius = GL.GetUniformLocation(ProgramID, "uRadius");
                UColor = GL.GetUniformLocation(ProgramID, "uColor");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                HelperGeneral.CheckGLErrors();
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void Draw(FlowField f, ref Matrix4 vp)
        {
            GL.BindVertexArray(VAO);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            GL.Uniform2(URadius, f.CellRadius, f.GridCellCount.Y * 0.5f);
            for (int x = 0; x < f.Grid.GetLength(0); x++)
            {
                for (int z = 0; z < f.Grid.GetLength(1); z++)
                {
                    if (f.Grid[x, z] == f.Destination)
                    {
                        GL.Uniform3(UColor, new Vector3(1, 1, 0));
                    }
                    else
                    {
                        GL.Uniform3(UColor, f.Grid[x, z].Cost > 1 ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0));
                    }
                    
                    GL.Uniform3(UCenter, f.Grid[x, z].WorldPos); 
                    GL.DrawArrays(PrimitiveType.Points, 0, 1); 
                }
            }
            GL.BindVertexArray(0);
        }
    }
}
