using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererBackgroundStandard
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorAmbient { get; private set; } = -1;
        public static int UTextureScaleAndOffset { get; private set; } = -1;
        public static int UTextureClip { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.background.2d.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.background.2d.frag";

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
                UTextureScaleAndOffset = GL.GetUniformLocation(ProgramID, "uTextureScaleAndOffset");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");
                UColorAmbient = GL.GetUniformLocation(ProgramID, "uColorAmbient");
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
            GL.DepthFunc(DepthFunction.Lequal);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.CurrentWorld._background._standardId);
            GL.Uniform1(UTexture, 0);

            Vector3 skyColor = new Vector3(KWEngine.CurrentWorld._colorAmbient * KWEngine.CurrentWorld._background._brightnessMultiplier);
            GL.Uniform3(UColorAmbient, skyColor);

            GL.Uniform4(UTextureScaleAndOffset, new Vector4(
                KWEngine.CurrentWorld._background._stateRender.Scale.X,
                KWEngine.CurrentWorld._background._stateRender.Scale.Y,
                KWEngine.CurrentWorld._background._stateRender.Offset.X,
                KWEngine.CurrentWorld._background._stateRender.Offset.Y));
            GL.Uniform2(UTextureClip, KWEngine.CurrentWorld._background._stateRender.Clip);

            
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.DepthFunc(DepthFunction.Less);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }

    }
}
