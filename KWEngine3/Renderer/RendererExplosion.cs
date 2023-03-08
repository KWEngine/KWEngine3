using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererExplosion
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTime { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UNumber { get; private set; } = -1;
        public static int USpread { get; private set; } = -1;
        public static int UPosition { get; private set; } = -1;
        public static int USize { get; private set; } = -1;
        public static int UAxes { get; private set; } = -1;
        public static int UAlgorithm { get; private set; } = -1;
        public static int UTowardsIndex { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.explosion.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.explosion.frag";

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


                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                UTime = GL.GetUniformLocation(ProgramID, "uTime");
                UNumber = GL.GetUniformLocation(ProgramID, "uNumber");
                USpread = GL.GetUniformLocation(ProgramID, "uSpread");
                USize = GL.GetUniformLocation(ProgramID, "uSize");
                UPosition = GL.GetUniformLocation(ProgramID, "uPosition");
                UAxes = GL.GetUniformLocation(ProgramID, "uAxes");
                UAlgorithm = GL.GetUniformLocation(ProgramID, "uAlgorithm");
                UTowardsIndex = GL.GetUniformLocation(ProgramID, "uTowardsIndex");
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
            GL.Uniform3(UColorAmbient, ref KWEngine.CurrentWorld._colorAmbient);
        }

        public static void RenderExplosions(List<TimeBasedObject> explosions)
        {
            foreach(TimeBasedObject tbo in explosions)
            {
                if(tbo is ExplosionObject)
                {
                    ExplosionObject explosionObject = (ExplosionObject)tbo;
                    Draw(explosionObject);
                }
            }
        }

        public static void Draw(ExplosionObject e)
        {
            int type = (int)e._type;

            GL.Uniform4(UColorEmissive, e.ColorEmissive);
            GL.Uniform1(UNumber, (float)e._amount);
            GL.Uniform1(USpread, e._spread);
            GL.Uniform3(UPosition, e.Position);
            GL.Uniform1(UTime, e._secondsAlive / e._duration);
            GL.Uniform1(USize, e._particleSize);
            GL.Uniform1(UAlgorithm, e._algorithm);

            if (type < 100)
                GL.Uniform1(UTowardsIndex, 0);
            else if (type >= 100 && type < 1000)
                GL.Uniform1(UTowardsIndex, 1);
            else
                GL.Uniform1(UTowardsIndex, 2);
            GL.Uniform2(UAxes, e._amount, e._directions);

            

            GeoMesh mesh = e._model.Meshes.ElementAt(0).Value;
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElementsInstanced(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, e._amount);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

    }
}
