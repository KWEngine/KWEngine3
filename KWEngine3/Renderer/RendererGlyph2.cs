using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using KWEngine3.GameObjects;
using KWEngine3.Model;
using KWEngine3;

namespace KWEngine3.Renderer
{
    internal static class RendererGlyph2 
    {
        public static int UModelInternal { get; private set; } = -1;
        public static int UModelExternal { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        //public static int UColorTint { get; private set; } = -1;
        public static int ProgramID { get; private set; } = -1;
        public static void Init()
        {
            ProgramID = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hud_glyph_2.frag";
            string resourceNameVertexShader = "KWEngine3.Shaders.shader.hud_glyph_2.vert";
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


            UModelInternal = GL.GetUniformLocation(ProgramID, "uModelInternal");
            UModelExternal = GL.GetUniformLocation(ProgramID, "uModelExternal");
            UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjection");
            //UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
            
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }
    }
}