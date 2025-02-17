
using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
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
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UOffset = GL.GetUniformLocation(ProgramID, "uOffset");
            }
        }
        public static void Draw(KWFontGlyph glyph)
        {

        }
    }
}
