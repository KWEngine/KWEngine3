using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererBloomUpsample
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureSmaller { get; private set; } = -1;
        public static int UTextureBigger { get; private set; } = -1;
        public static int UBloomParams { get; private set; } = -1;

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
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                UTextureSmaller = GL.GetUniformLocation(ProgramID, "uTextureSmaller");
                UTextureBigger = GL.GetUniformLocation(ProgramID, "uTextureBigger");
                UBloomParams = GL.GetUniformLocation(ProgramID, "uBloomParams");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            GL.Uniform1(UTextureSmaller, 0);
            GL.Uniform1(UTextureBigger, 1);
        }

        // expects SetGlobals() to be called and the fullscreen quad VAO to be bound (see RenderManager.DoBloomPass)
        // tapDistance <= 1 lets the shader use its exact 8-tap path (GlowRadius is limited to 0..1)
        public static void Draw(Framebuffer fbSource1, Framebuffer fbSource2, float tapDistance, float weightSmaller, float weightBigger)
        {
            GL.Uniform3(UBloomParams, tapDistance, weightSmaller, weightBigger);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource1.Attachments[0].ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource2.Attachments[0].ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
        }

    }
}
