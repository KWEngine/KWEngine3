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
            Matrix4 vp;
            if(KWEngine.Mode == EngineMode.Play)
            {
                vp = KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewProjectionMatrix;
                GL.Uniform3(UColor, 0.9f, 0.1f, 0.9f);
            }
            else
            {
                vp = KWEngine.CurrentWorld._cameraEditor._stateCurrent.ViewProjectionMatrix;
                GL.Uniform3(UColor, 0.5f, 1f, 0.25f);
            }
            
            GL.UniformMatrix4(UModelViewProjectionMatrix, false, ref vp);
            
        }

        private const int MAX_SHADER_VERTICES = 128; // size of uVertexPositions in shader.editorTriangles.geom
        private static readonly float[] _vertexPositions = new float[MAX_SHADER_VERTICES * 3];

        public static void Draw(GameObject g)
        {
            Span<Vector3> vertexBuffer = stackalloc Vector3[HelperIntersection.MAX_STACK_FACE_VERTICES]; // one buffer for all faces (stackalloc inside a loop grows the stack per iteration)
            GL.BindVertexArray(PrimitivePoint.VAO);
            
            foreach(GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                if(KWEngine.EnableDebugHitboxes == HitboxDebugMode.DepthNone)
                {
                    GL.Disable(EnableCap.DepthTest);
                }
                else if(KWEngine.EnableDebugHitboxes == HitboxDebugMode.DepthAll)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
                else if (hb._colliderType == ColliderType.PlaneCollider)
                {
                    if (KWEngine.EnableDebugHitboxes < HitboxDebugMode.DepthPlanesOnly)
                        GL.Disable(EnableCap.DepthTest);
                    else
                        GL.Enable(EnableCap.DepthTest);
                }
                else if (hb._colliderType == ColliderType.ConvexHull)
                {
                    if (KWEngine.EnableDebugHitboxes == HitboxDebugMode.DepthConvexOnly)
                        GL.Enable(EnableCap.DepthTest);
                    else
                        GL.Disable(EnableCap.DepthTest);
                }
                else
                {
                    GL.Disable(EnableCap.DepthTest);
                }

                for (int j = 0; j < hb._mesh.Faces.Length; j++)
                {
                    int faceVertexCount = hb._mesh.Faces[j].VertexCount;
                    Span<Vector3> vertices = faceVertexCount <= vertexBuffer.Length ? vertexBuffer.Slice(0, faceVertexCount) : new Vector3[faceVertexCount];
                    hb.GetVerticesFromFace(j, ref vertices, out Vector3 currentFaceNormal);
                    int count = Math.Min(vertices.Length, MAX_SHADER_VERTICES);
                    for(int i = 0, floatIndex = 0; i < count; i++, floatIndex += 3)
                    {
                        _vertexPositions[floatIndex + 0] = vertices[i].X;
                        _vertexPositions[floatIndex + 1] = vertices[i].Y;
                        _vertexPositions[floatIndex + 2] = vertices[i].Z;
                    }
                    GL.Uniform3(UVertexPositions, count, _vertexPositions);
                    GL.Uniform1(UVertexCount, count);
                    GL.DrawArrays(PrimitiveType.Points, 0, 1);
                }
            }

            GL.BindVertexArray(0);
        }
    }
}
