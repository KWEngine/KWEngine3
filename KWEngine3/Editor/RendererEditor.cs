using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Editor
{
    internal static class RendererEditor
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UType { get; private set; } = -1;
        public static int UModelViewProjectionMatrix { get; private set; } = -1;
        public static int UCenterOfMass { get; private set; } = -1;
        public static int UDimensions { get; private set; } = -1;
        public static int ULookAtVector { get; private set; } = -1;
        public static int ULookAtVectorRight { get; private set; } = -1;
        public static int ULookAtVectorTop { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.editor.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.editor.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.editor.frag";

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
                UCenterOfMass = GL.GetUniformLocation(ProgramID, "uCenterOfMass");
                UDimensions = GL.GetUniformLocation(ProgramID, "uDimensions");
                ULookAtVector = GL.GetUniformLocation(ProgramID, "uLookAtVector");
                ULookAtVectorRight = GL.GetUniformLocation(ProgramID, "uLookAtVectorRight");
                ULookAtVectorTop = GL.GetUniformLocation(ProgramID, "uLookAtVectorTop");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            Matrix4 vp = KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewProjectionMatrix;
            GL.UniformMatrix4(UModelViewProjectionMatrix, false, ref vp);
        }

        public static void Draw(GameObject g)
        {
            GL.BindVertexArray(PrimitivePoint.VAO);
            DrawLookAtVector(g);
            GL.BindVertexArray(0);
        }

        public static void Draw(TerrainObject g)
        {
            GL.BindVertexArray(PrimitivePoint.VAO);
            DrawBoundingBox(g);
            GL.BindVertexArray(0);
        }

        private static void DrawLookAtVector(GameObject g)
        {
            GL.Uniform3(UColor, Vector3.One);
            GL.Uniform1(UType, 0);
            GL.Uniform3(UCenterOfMass, g._stateRender._center);
            GL.Uniform3(UDimensions, 0f, 0f, 0f);
            GL.Uniform3(ULookAtVector, g._stateRender._lookAtVector);
            GL.Uniform3(ULookAtVectorRight, g.LookAtVectorLocalRight);
            GL.Uniform3(ULookAtVectorTop, g.LookAtVectorLocalUp);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
        }
        private static void DrawBoundingBox(TerrainObject t)
        {
            GL.Uniform3(UColor, Vector3.One);
            GL.Uniform1(UType, 1);
            GL.Uniform3(UCenterOfMass, t._stateRender._center);
            GL.Uniform3(UDimensions, t._stateRender._dimensions);
            GL.Uniform3(ULookAtVector, Vector3.Zero);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
        }
    }
}
