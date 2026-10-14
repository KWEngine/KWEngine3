using KWEngine3.Framebuffers;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererFXAA
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureScene { get; private set; } = -1;
        public static int UInverseScreenSize { get; private set; } = -1;
        public static int SamplerLinear { get; private set; } = -1; // the color attachment of the lighting pass uses nearest filtering, FXAA needs bilinear taps

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.copy.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.fxaa.frag";

                int vertexShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                UTextureScene = GL.GetUniformLocation(ProgramID, "uTextureScene");
                UInverseScreenSize = GL.GetUniformLocation(ProgramID, "uInverseScreenSize");

                SamplerLinear = GL.GenSampler();
                GL.SamplerParameter(SamplerLinear, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.SamplerParameter(SamplerLinear, SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.SamplerParameter(SamplerLinear, SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.SamplerParameter(SamplerLinear, SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        // Reads the color attachment of fbSource and writes the anti-aliased image to the currently bound framebuffer
        public static void Draw(Framebuffer fbSource)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            GL.BindSampler(0, SamplerLinear);
            GL.Uniform1(UTextureScene, 0);
            GL.Uniform2(UInverseScreenSize, 1f / fbSource.Width, 1f / fbSource.Height);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindSampler(0, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
