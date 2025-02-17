using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.FontGenerator
{
    internal static class RendererGlyph2
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UOffset { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.FontGenerator.shaderArc.vert";
                string resourceNameFragmentShader = "KWEngine3.FontGenerator.shaderArc.frag";

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

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                GL.LinkProgram(ProgramID);
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjection");
                UOffset = GL.GetUniformLocation(ProgramID, "uOffsetScale");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            Matrix4 vp = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0) * Matrix4.CreateOrthographicOffCenter(0, width, 0, height, 0.1f, 10f);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
        }

        public static void Draw(KWFontGlyph g, Vector2 offset, float scale)
        {
            // ARC PASS:
            GL.Uniform3(UOffset, new Vector3(offset.X, offset.Y, scale));

            GL.BindVertexArray(g.VAO_Step2);
            GL.DrawArrays(PrimitiveType.Triangles, 0, g.VertexCount_Step2);
            GL.BindVertexArray(0);

            HelperGeneral.CheckGLErrors();
        }
    }
}
