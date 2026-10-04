using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererBloomDownsample
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UBloomRadius { get; private set; } = -1;
        public static int UBoxParams { get; private set; } = -1;
        public static int SamplerLinear { get; private set; } = -1; // the bloom attachment of the lighting pass uses nearest filtering

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.bloom.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.bloom.downsample.frag";

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
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UBloomRadius = GL.GetUniformLocation(ProgramID, "uBloomRadius");
                UBoxParams = GL.GetUniformLocation(ProgramID, "uBoxParams");

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


        public static void SetGlobals()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Uniform1(UTexture, 0);
            GL.Uniform1(UBloomRadius, KWEngine._glowRadius);
            GL.Uniform3(UBoxParams, 0f, 0f, 0f);
        }

        // First pass of the half resolution chain (Default/Low): box filter over about (ratio x ratio) texels of the
        // lighting pass' bloom attachment with 4 bilinear taps; expects SetGlobals() to be called and the quad VAO to be bound
        public static void DrawBox(Framebuffer fbSource, Framebuffer target, float energy)
        {
            float offsetX = fbSource.Width / (float)target.Width * 0.25f;
            float offsetY = fbSource.Height / (float)target.Height * 0.25f;
            GL.Uniform3(UBoxParams, offsetX, offsetY, energy);
            GL.BindSampler(0, SamplerLinear);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[fbSource is FramebufferLighting ? 1 : 0].ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindSampler(0, 0);
            GL.Uniform3(UBoxParams, 0f, 0f, 0f);
        }

        // expects SetGlobals() to be called and the fullscreen quad VAO to be bound (see RenderManager.DoBloomPass)
        public static void Draw(Framebuffer fbSource) // currently from lighting pass
        {
            int attachmentIndex = fbSource is FramebufferLighting ? 1 : 0;
            GL.BindTexture(TextureTarget.Texture2D,  fbSource.Attachments[attachmentIndex].ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
        }

    }
}
