using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererBloomUpsample
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureSmaller { get; private set; } = -1;
        public static int UTextureBigger { get; private set; } = -1;
        public static int UBloomRadius { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.bloom.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.bloom.upsample.frag";

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

                GL.LinkProgram(ProgramID);
                UTextureSmaller = GL.GetUniformLocation(ProgramID, "uTextureSmaller");
                UTextureBigger = GL.GetUniformLocation(ProgramID, "uTextureBigger");
                UBloomRadius = GL.GetUniformLocation(ProgramID, "uBloomRadius");
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

        public static void Draw(Framebuffer fbSource1, Framebuffer fbSource2) // currently from lighting pass
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource1.Attachments[0].ID);
            GL.Uniform1(UTextureSmaller, 0);

            GL.Uniform1(UBloomRadius, KWEngine.GlowRadius);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource2.Attachments[0].ID);
            GL.Uniform1(UTextureBigger, 1);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
