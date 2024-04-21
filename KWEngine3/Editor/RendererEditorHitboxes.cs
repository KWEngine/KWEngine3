using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Editor
{
    internal static class RendererEditorHitboxes
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UModelViewProjectionMatrix { get; private set; } = -1;
        public static int UVertexPositions { get; private set; } = -1;
        public static int UVertexCount { get; private set; } = -1;


        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.editorTriangles.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.editorTriangles.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.editorTriangles.frag";

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
                UModelViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uModelViewProjection");
                UVertexPositions = GL.GetUniformLocation(ProgramID, "uVertexPositions");
                UVertexCount = GL.GetUniformLocation(ProgramID, "uVertexCount");
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
            GL.Uniform3(UColor, 0.5f, 1f, 0.25f);
        }

        public static void Draw(GameObject g)
        {
            GL.BindVertexArray(PrimitivePoint.VAO);
            
            foreach(GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                if(hb.IsActive == false)
                {
                    continue;
                }

                for (int j = 0; j < hb._mesh.Faces.Length; j++)
                {
                    if (hb.IsExtended)
                    {
                        hb.GetVerticesForTriangleFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 currentFaceNormal);
                        float[] pos = new float[] { v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z };
                        GL.Uniform3(UVertexPositions, pos.Length / 3, pos);
                        GL.Uniform1(UVertexCount, 3);
                        GL.DrawArrays(PrimitiveType.Points, 0, 1);
                    }
                    else
                    {
                        hb.GetVerticesForCubeFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 currentFaceNormal);
                        
                        float[] pos = new float[] { v1.X, v1.Y, v1.Z, v5.X, v5.Y, v5.Z, v6.X, v6.Y, v6.Z, v3.X, v3.Y, v3.Z};
                        GL.Uniform3(UVertexPositions, pos.Length / 3, pos);
                        GL.Uniform1(UVertexCount, 4);
                        GL.DrawArrays(PrimitiveType.Points, 0, 1);
                    }
                }
            }

            
            GL.BindVertexArray(0);
        }
    }
}
