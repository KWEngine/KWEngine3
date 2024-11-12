using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererMapKiller
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;
        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.mapkiller.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.mapkiller.frag";

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

                GL.LinkProgram(ProgramID);

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                UOptions = GL.GetUniformLocation(ProgramID, "uOptions");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            
        }

        public static void KillBloomForMap()
        {
            GeoMesh mesh = KWEngine.GetModel("KWQuad").Meshes.Values.ElementAt(0);

            GL.Viewport(
                KWEngine.CurrentWorld.Map._targetCenter.X - KWEngine.CurrentWorld.Map._targetDimensions.X / 2,
                (KWEngine.Window.ClientSize.Y - KWEngine.CurrentWorld.Map._targetCenter.Y) - KWEngine.CurrentWorld.Map._targetDimensions.Y / 2,
                KWEngine.CurrentWorld.Map._targetDimensions.X,
                KWEngine.CurrentWorld.Map._targetDimensions.Y
                );

            GL.Uniform1(UOptions, KWEngine.CurrentWorld.Map._drawAsCircle ? 1 : 0);
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
