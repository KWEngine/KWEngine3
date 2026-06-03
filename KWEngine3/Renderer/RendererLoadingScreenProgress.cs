using KWEngine3.Assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererLoadingScreenProgress
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UProgress { get; private set; } = -1;
        public static int UType { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UBorderThickness { get; private set; } = -1;
        public static int USize { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader   = "KWEngine3.Shaders.shader.loadingprogress.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.loadingprogress.frag";

                Assembly assembly = Assembly.GetExecutingAssembly();
                int vShader = -1;
                int fShader = -1;
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vShader, fShader);

                UModelMatrix          = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UProgress             = GL.GetUniformLocation(ProgramID, "uProgress");
                UType                 = GL.GetUniformLocation(ProgramID, "uType");
                UColor                = GL.GetUniformLocation(ProgramID, "uColor");
                UColorEmissive        = GL.GetUniformLocation(ProgramID, "uColorEmissive");
                UBorderThickness      = GL.GetUniformLocation(ProgramID, "uBorderThickness");
                USize                 = GL.GetUniformLocation(ProgramID, "uSize");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void Draw(LoadingScreen ls)
        {
            // Nichts zeichnen wenn: unsichtbar, oder (kein Fortschritt UND kein Rand)
            if (!ls._progressVisible)
                return;
            if (ls._progressValue <= 0f && ls._progressBorderThickness <= 0f)
                return;

            GL.UniformMatrix4(UModelMatrix, false, ref ls._progressModelMatrix);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUD);
            GL.Uniform1(UProgress, ls._progressValue);
            GL.Uniform1(UType, (int)ls._progressType);
            GL.Uniform4(UColor, ls._progressColor);
            GL.Uniform1(UBorderThickness, ls._progressBorderThickness);
            GL.Uniform2(USize, (float)ls._progressSize.X, (float)ls._progressSize.Y);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}
