using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererFrustum
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UViewProjectionMatrixEditor { get; private set; } = -1;
        public static int UViewProjectionMatrixGame { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.editor.frustum.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.editor.frustum.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.editor.frustum.frag";

                int vertexShader;
                int fragmentShader;
                int geometryShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameGeometryShader))
                {
                    geometryShader = RenderManager.LoadCompileAttachShader(s, ShaderType.GeometryShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UViewProjectionMatrixEditor = GL.GetUniformLocation(ProgramID, "uVPEditor");
                UViewProjectionMatrixGame = GL.GetUniformLocation(ProgramID, "uVPGame");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            // no globals necessary here
        }

        public static void Draw()
        {
            Matrix4 vpEditor = KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            Matrix4 vpGame = KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix;
            Vector3 color = Vector3.UnitX + Vector3.UnitY;

            GL.BindVertexArray(PrimitivePoint.VAO);
            GL.Uniform3(UColorTint, ref color);
            GL.UniformMatrix4(UViewProjectionMatrixEditor, false, ref vpEditor);
            GL.UniformMatrix4(UViewProjectionMatrixGame, false, ref vpGame);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);
        }

    }
}
