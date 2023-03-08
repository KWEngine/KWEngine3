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
    internal static class RendererLightFrustum
    { 
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrixLight { get; private set; } = -1;
        public static int UPosition { get; private set; } = -1;
        public static int ULookAtVector { get; private set; } = -1;
        public static int UNearFarFOVType { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.editor.light.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.editor.light.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.editor.light.frag";

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

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, geometryShader);
                GL.LinkProgram(ProgramID);

                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UViewProjectionMatrixLight = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixLight");
                UPosition = GL.GetUniformLocation(ProgramID, "uPosition");
                UNearFarFOVType = GL.GetUniformLocation(ProgramID, "uNearFarFOVType");
                ULookAtVector = GL.GetUniformLocation(ProgramID, "uLookAtVector");
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

        public static void Draw(LightObject l)
        {
            GL.BindVertexArray(PrimitivePoint.VAO);
            GL.Uniform3(UColorTint, Vector3.One);
            GL.Uniform3(ULookAtVector, l._stateRender._lookAtVector);
            GL.UniformMatrix4(UViewProjectionMatrixLight, false, ref l._stateRender._viewProjectionMatrix[0]);
            GL.Uniform3(UPosition, l._stateRender._position);
            GL.Uniform4(UNearFarFOVType, l._stateRender._nearFarFOVType);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);
        }
    }
}
