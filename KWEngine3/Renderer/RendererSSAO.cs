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
    internal static class RendererSSAO
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureDepth { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UViewProjectionMatrixInverted { get; private set; } = -1;
        public static int UKernel { get; private set; } = -1;
        public static Vector3[] Kernel { get; private set; } = new Vector3[64];
        public static float[] Noise { get; private set; } = new float[16 * 3];

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.ssao.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.ssao.frag";

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

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");
                UKernel = GL.GetUniformLocation(ProgramID, "uKernel");
            }
        }

        public static void GenerateKernel()
        {
            Random rng = new Random();
            for (uint i = 0; i < 64; i++)
            {
                Kernel[i] = new Vector3(rng.NextSingle() * 2.0f - 1.0f, rng.NextSingle() * 2.0f - 1.0f, rng.NextSingle());
                float scale = i / 64.0f;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                Kernel[i] *= scale;
            }

            for(uint i = 0; i < Noise.Length; i+=3)
            {
                Noise[i + 0] = rng.NextSingle() * 2.0f - 1.0f;
                Noise[i + 1] = rng.NextSingle() * 2.0f - 1.0f;
                Noise[i + 2] = 0f;
            }

            /*
            std::uniform_real_distribution<float> randomFloats(0.0, 1.0); // random floats between [0.0, 1.0]
            std::default_random_engine generator;
            std::vector<glm::vec3> ssaoKernel;
            for (unsigned int i = 0; i < 64; ++i)
            {
                glm::vec3 sample(
                    randomFloats(generator)* 2.0 - 1.0, 
        randomFloats(generator) * 2.0 - 1.0, 
        randomFloats(generator)
    );
            sample = glm::normalize(sample);
            sample *= randomFloats(generator);
            ssaoKernel.push_back(sample);
            */
        }
    }

        public static void Draw(Framebuffer fbSource)
        {
            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            vp.Invert();
            GL.UniformMatrix4(UViewProjectionMatrixInverted, false, ref vp);

            // depth tex:
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[4].ID);
            GL.Uniform1(UTextureDepth, 0);

            // albedo:
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            GL.Uniform1(UTextureAlbedo, 1);

            // normal:
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[1].ID);
            GL.Uniform1(UTextureNormal, 2);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);
        }
    }
}
