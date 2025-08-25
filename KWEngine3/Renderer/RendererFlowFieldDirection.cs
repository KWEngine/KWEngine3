using KWEngine3.Assets;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererFlowFieldDirection
    { 

        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int URotationMatrix { get; private set; } = -1;
        public static int UCenter { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;

        //private static int _indexCount = -1;

        private static int LoadShader(Stream pFileStream, ShaderType pType, int pProgram)
        {
            int address = GL.CreateShader(pType);
            using (StreamReader sr = new StreamReader(pFileStream))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(pProgram, address);
            return address;
        }

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.flowfielddirection.frag";
                string resourceNameVertexShader = "KWEngine3.Shaders.shader.flowfielddirection.vert";

                int vertexShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = LoadShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = LoadShader(s, ShaderType.FragmentShader, ProgramID);
                }
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);
                GL.LinkProgram(ProgramID);
              
                UColor = GL.GetUniformLocation(ProgramID, "uColor");
                UCenter = GL.GetUniformLocation(ProgramID, "uCenter");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                URotationMatrix = GL.GetUniformLocation(ProgramID, "uRotationMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            if(KWEngine.Mode == EngineMode.Play)
            {
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix);
            }
            else
            {
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix);
            }

            GL.BindVertexArray(KWQuad2D_05.VAO); // KWEngine.Models["KWQuad"].Meshes.ElementAt(0).Value.VAO);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, KWEngine.Models["KWQuad"].Meshes.ElementAt(0).Value.VBOIndex);
            //_indexCount = KWEngine.Models["KWQuad"].Meshes.ElementAt(0).Value.IndexCount;

           
        }

        public static void UnsetGlobals()
        {
            GL.BindVertexArray(0);
            
        }

        public static void Draw(FlowField f)
        {
            for (int x = 0; x < f.Grid.GetLength(0); x++)
            {
                for (int z = 0; z < f.Grid.GetLength(1); z++)
                {
                    GL.Uniform3(UCenter, f.Grid[x, z].Position);

                    Matrix4 rm = Matrix4.CreateRotationX(-(MathF.PI / 2f));
                    GL.ActiveTexture(TextureUnit.Texture0);
                    if (f.Grid[x, z]._gridIndex == f.Destination._gridIndex)
                    {
                        GL.Uniform3(UColor, new Vector3(0, 1, 0));
                        GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureFlowFieldCross);
                    }
                    else
                    {
                        if(f.Grid[x, z].Cost == byte.MaxValue)
                        {
                            GL.Uniform3(UColor, new Vector3(1, 0, 0));
                        }
                        else
                        {
                            GL.Uniform3(UColor, new Vector3(0, 0, 1));
                        }
                        rm *= GetRotationMatrixForDirection(f.Grid[x,z].Position, f.Grid[x, z].BestDirection);
                        GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureFlowFieldArrow);
                    }
                    GL.Uniform1(UTexture, 0);

                    GL.UniformMatrix4(URotationMatrix, false, ref rm);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                    //GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
            GL.BindVertexArray(0);
        }

        internal static Matrix4 GetRotationMatrixForDirection(Vector3 source, Vector3 direction)
        {
            Quaternion q = HelperRotation.GetRotationForPoint(source, source + direction);
            return Matrix4.CreateFromQuaternion(q);
        }
    }
}
