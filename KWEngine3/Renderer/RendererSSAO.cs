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
        //public static int UTextureAlbedo { get; private set; } = -1;
        public static int UViewMatrix3 { get; private set; } = -1;
        public static int UProjectionParams { get; private set; } = -1;
        public static int UKernel { get; private set; } = -1;
        public static int UKernelSize { get; private set; } = -1;
        public static int UTextureNoise { get; private set; } = -1;
        public static int URadiusBias { get; private set; } = -1;

        public static float[] Kernel { get; private set; }
        public static int NoiseTexture { get; private set; } = -1;
        private static bool _kernelDirty = true;

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
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                //UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UViewMatrix3 = GL.GetUniformLocation(ProgramID, "uViewMatrix3");
                UProjectionParams = GL.GetUniformLocation(ProgramID, "uProjectionParams");
                UKernel = GL.GetUniformLocation(ProgramID, "uKernel");
                UTextureNoise = GL.GetUniformLocation(ProgramID, "uTextureNoise");
                URadiusBias = GL.GetUniformLocation(ProgramID, "uRadiusBias");
                UKernelSize = GL.GetUniformLocation(ProgramID, "uKernelSize");

                GenerateKernel();
                GenerateNoise();
            }
        }

        public static void GenerateKernel()
        {
            Kernel = new float[KWEngine._ssaoKernelSize * 3];

            // generate sample kernel:
            for (uint i = 0; i < Kernel.Length; i += 3)
            {
                Vector3 kernelTmp = Vector3.Normalize(new Vector3(Random.Shared.NextSingle() * 2.0f - 1.0f, Random.Shared.NextSingle() * 2.0f - 1.0f, Random.Shared.NextSingle()));
                kernelTmp *= Random.Shared.NextSingle();

                float scale = (i / 3) / (float)KWEngine._ssaoKernelSize; // i steps by 3 (xyz)
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                kernelTmp *= scale;

                Kernel[i + 0] = kernelTmp.X;
                Kernel[i + 1] = kernelTmp.Y;
                Kernel[i + 2] = kernelTmp.Z;
            }
            _kernelDirty = true;
        }

        public static void GenerateNoise()
        {
            // generate noise tex:
            float[] noise = new float[16 * 3];
            for (uint i = 0; i < noise.Length; i += 3)
            {
                // unit length rotation vectors (xy plane)
                float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
                noise[i + 0] = MathF.Cos(angle);
                noise[i + 1] = MathF.Sin(angle);
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
            // G-buffer normals are world space -> view rotation; projection reduced to 4 constants (symmetric perspective)
            Matrix3 view3 = new Matrix3(KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewMatrix);
            GL.UniformMatrix3(UViewMatrix3, false, ref view3);

            Matrix4 proj = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ProjectionMatrix;
            GL.Uniform4(UProjectionParams, 0.5f * proj.M11, 0.5f * proj.M22, -0.5f * proj.M43, 0.5f * (proj.M33 - 1f));

            // depth tex:
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[5].ID);
            GL.Uniform1(UTextureDepth, 0);

            // albedo:
            //GL.ActiveTexture(TextureUnit.Texture1);
            //GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            //GL.Uniform1(UTextureAlbedo, 1);

            // normal:
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[1].ID);
            GL.Uniform1(UTextureNormal, 1);

            // noise:
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, NoiseTexture);
            GL.Uniform1(UTextureNoise, 2);

            // kernel samples (count in vec3 units, only uploaded after a change):
            if (_kernelDirty)
            {
                GL.Uniform3(UKernel, Kernel.Length / 3, Kernel);
                GL.Uniform1(UKernelSize, (uint)(Kernel.Length / 3));
                _kernelDirty = false;
            }
            GL.Uniform2(URadiusBias, KWEngine._ssaoRadius, KWEngine._ssaoBias);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
