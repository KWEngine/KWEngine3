using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererOctreeNodes
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

                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.octree.frag";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.octree.geom";
                string resourceNameVertexShader = "KWEngine3.Shaders.shader.octree.vert";

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
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void Draw(OctreeNode n, ref Matrix4 vp)
        {
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            GL.Uniform3(UCenter, n.Center);
            GL.Uniform3(URadius, n.Scale);
            GL.Uniform3(UColor, n.GameObjectCount == 0 ? new Vector3(1,1,1) : n.GameObjectCount == 1 ? new Vector3(1,1,0) : n.GameObjectCount == 2 ? new Vector3(1,0,0) : new Vector3(0,1,1));
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);
            HelperGeneral.CheckGLErrors();

            foreach (OctreeNode child in n.ChildOctreeNodes)
            {
                Draw(child, ref vp);
            }
        }
    }
}
