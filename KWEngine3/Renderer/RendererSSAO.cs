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
        public static int UProjectionMatrix { get; private set; } = -1;
        public static int UKernel { get; private set; } = -1;
        public static int UTextureNoise { get; private set; } = -1;
        public static int UNoiseScale { get; private set; } = -1;
        public static int URadiusBias { get; private set; } = -1;

        public static float[] Kernel { get; private set; } = new float[64 * 3];
        public static int NoiseTexture { get; private set; } = -1;

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

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");
                UProjectionMatrix = GL.GetUniformLocation(ProgramID, "uProjectionMatrix");
                UKernel = GL.GetUniformLocation(ProgramID, "uKernel");
                UTextureNoise = GL.GetUniformLocation(ProgramID, "uTextureNoise");
                UNoiseScale = GL.GetUniformLocation(ProgramID, "uNoiseScale");
                URadiusBias = GL.GetUniformLocation(ProgramID, "uRadiusBias");

                GenerateKernel();
            }
        }

        public static void GenerateKernel()
        {
            Random rng = new Random();

            // generate sample kernel:
            for (uint i = 0; i < Kernel.Length; i+=3)
            {
                Vector3 kernelTmp = Vector3.Normalize(new Vector3(rng.NextSingle() * 2.0f - 1.0f, rng.NextSingle() * 2.0f - 1.0f, rng.NextSingle()));
                kernelTmp *= rng.NextSingle();

                float scale = i / 64.0f;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                kernelTmp *= scale;

                Kernel[i + 0] = kernelTmp.X;
                Kernel[i + 1] = kernelTmp.Y;
                Kernel[i + 2] = kernelTmp.Z;
            }

            // generate noise tex:
            float[] noise = new float[16 * 3];
            for(uint i = 0; i < noise.Length; i+=3)
            {
                noise[i + 0] = rng.NextSingle() * 2.0f - 1.0f;
                noise[i + 1] = rng.NextSingle() * 2.0f - 1.0f;
                noise[i + 2] = 0f;
            }
            NoiseTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, NoiseTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, 4, 4, 0, PixelFormat.Rgb, PixelType.Float, noise);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { (int)TextureWrapMode.Repeat });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { (int)TextureWrapMode.Repeat });
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    

        public static void Draw(Framebuffer fbSource)
        {
            Matrix4 vpInv = Matrix4.Invert(KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ProjectionMatrix);
            GL.UniformMatrix4(UViewProjectionMatrixInverted, false, ref vpInv);

            Matrix4 proj = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ProjectionMatrix;
            GL.UniformMatrix4(UProjectionMatrix, false, ref proj);

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

            // noise:
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, NoiseTexture);
            GL.Uniform1(UTextureNoise, 3);

            // kernel samples:
            GL.Uniform3(UKernel, Kernel.Length, Kernel);
            GL.Uniform2(URadiusBias, KWEngine._ssaoRadius, KWEngine._ssaoBias);

            // scale:
            GL.Uniform2(UNoiseScale, KWEngine.Window.ClientRectangle.Size.X / 4f, KWEngine.Window.ClientRectangle.Size.Y / 4f);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
