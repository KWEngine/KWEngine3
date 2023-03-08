using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Editor
{
    internal static class RendererGrid
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UType { get; private set; } = -1;
        public static int UModelViewProjectionMatrix { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.grid.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.grid.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.grid.frag";

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

                UColor = GL.GetUniformLocation(ProgramID, "uColor");
                UType = GL.GetUniformLocation(ProgramID, "uType");
                UModelViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uModelViewProjection");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            Matrix4 mvp = KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewProjectionMatrix;
            GL.UniformMatrix4(UModelViewProjectionMatrix, false, ref mvp);
        }

        public static void Draw()
        {
            GL.BindVertexArray(PrimitivePoint.VAO);
            GL.Uniform3(UColor, new Vector3(1,0,0));
            GL.Uniform1(UType, 0);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.Uniform3(UColor, new Vector3(0, 1, 0));
            GL.Uniform1(UType, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.Uniform3(UColor, new Vector3(0, 0, 1));
            GL.Uniform1(UType, 2);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);
        }   
    }
}
