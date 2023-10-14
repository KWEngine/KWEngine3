using KWEngine3.EngineCamera;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KWEngine3.Editor
{
    internal static class RendererLightOverlay
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UId { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.editor.lightbulb.gbuffer.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.editor.lightbulb.gbuffer.frag";

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


                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UId = GL.GetUniformLocation(ProgramID, "uId");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
        }

        public static void Draw(List<LightObject> lights)
        {
            GeoMesh m = KWEngine.KWLightBulb.Meshes.Values.ElementAt(0);
            GL.BindVertexArray(m.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m.VBOIndex);
            GL.Uniform3(UColorTint, Vector3.One);
            foreach (LightObject l in lights)
            {
                GL.Uniform1(UId, l.ID);
                float s = (l._stateRender._position - KWEngine.CurrentWorld._cameraEditor._stateRender._position).LengthFast / 30f;
                Matrix4 mm = Matrix4.CreateScale(s) * Matrix4.CreateTranslation(l._stateRender._position);
                GL.UniformMatrix4(UModelMatrix, false, ref mm);
                GL.DrawElements(PrimitiveType.Triangles, m.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
