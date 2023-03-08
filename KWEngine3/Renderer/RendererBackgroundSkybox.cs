using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererBackgroundSkybox
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.background.skybox.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.background.skybox.frag";

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
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
                //UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            // ?
        }

        public static void Draw()
        {
            Matrix4 viewMatrix = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewMatrix.ClearTranslation() : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewMatrix.ClearTranslation();
            Matrix4 projectionMatrix = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ProjectionMatrix;
            if (projectionMatrix == Matrix4.Identity)
                return;

            viewMatrix = new Matrix4(KWEngine.CurrentWorld._background._rotation) * viewMatrix;
            Matrix4 viewProjectionMatrix = viewMatrix * projectionMatrix;

            GL.FrontFace(FrontFaceDirection.Cw);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref viewProjectionMatrix);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, KWEngine.CurrentWorld._background._skyboxId);
            GL.Uniform1(UTexture, 0);

            Vector3 skyColor = new Vector3(KWEngine.CurrentWorld._colorAmbient * KWEngine.CurrentWorld._background._brightnessMultiplier);
            GL.Uniform3(UColorAmbient, skyColor);

            GeoMesh mesh = KWEngine.Models["KWCube"].Meshes.Values.ElementAt(0);
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);

            GL.DepthFunc(DepthFunction.Less);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.UseProgram(0);
        }

    }
}
