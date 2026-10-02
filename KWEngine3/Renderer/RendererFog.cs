using KWEngine3.Framebuffers;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererFog
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureDepth { get; private set; } = -1;
        public static int UViewProjectionMatrixInverted { get; private set; } = -1;
        public static int UCameraPos { get; private set; } = -1;

        // Binding for ubo uBlockFog: (0-2: Lighting/Instancing, 10: HUD Text)
        public const int UBO_BINDINGPOINT = 11;
        private const string UBO_BLOCKNAME = "uBlockFog";
        public static int UBO { get; private set; } = -1;
        private static readonly float[] _uboData = new float[8]; // std140: 2x vec4

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.copy.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.fog.frag";

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

                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");
                UCameraPos = GL.GetUniformLocation(ProgramID, "uCameraPos");

                UBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
                GL.BufferData(BufferTarget.UniformBuffer, _uboData.Length * sizeof(float), _uboData, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBO_BINDINGPOINT, UBO);

                BindFogBlockToProgram(ProgramID);
            }
        }

        /// <summary>
        /// Verknüpft den Uniform-Block uBlockFog eines Shader-Programms mit dem Nebel-UBO.
        /// Muss einmalig nach dem Linken aufgerufen werden (Programme ohne Block werden ignoriert).
        /// </summary>
        /// <param name="programId">ID des gelinkten Shader-Programms</param>
        internal static void BindFogBlockToProgram(int programId)
        {
            int blockIndex = GL.GetUniformBlockIndex(programId, UBO_BLOCKNAME);
            if (blockIndex >= 0)
            {
                GL.UniformBlockBinding(programId, blockIndex, UBO_BINDINGPOINT);
            }
        }

        /// <summary>
        /// Schreibt die Nebel-Parameter der aktuellen Welt in den UBO (1x pro Frame, vor Fog- und Forward-Pass)
        /// </summary>
        internal static void UpdateFogBlock()
        {
            World w = KWEngine.CurrentWorld;
            _uboData[0] = w._fogColor.X;
            _uboData[1] = w._fogColor.Y;
            _uboData[2] = w._fogColor.Z;
            _uboData[3] = w._fogDensity;
            _uboData[4] = w._fogHeight;
            _uboData[5] = w._fogHeightFalloff;
            _uboData[6] = 0f;
            _uboData[7] = 0f;

            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, _uboData.Length * sizeof(float), _uboData);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBO_BINDINGPOINT, UBO);
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            Matrix4 vpInv;
            Vector3 cameraPosition;
            if (KWEngine.Mode == EngineMode.Play)
            {
                vpInv = KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrixInverse;
                cameraPosition = KWEngine.CurrentWorld._cameraGame._stateRender._position;
            }
            else
            {
                vpInv = KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrixInverse;
                cameraPosition = KWEngine.CurrentWorld._cameraEditor._stateRender._position;
            }
            GL.UniformMatrix4(UViewProjectionMatrixInverted, false, ref vpInv);
            GL.Uniform3(UCameraPos, cameraPosition);
        }

        public static void Draw(Framebuffer fbSource) // fbSource = G-Buffer (FramebufferDeferred)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[5].ID); // [5] = Depth
            GL.Uniform1(UTextureDepth, 0);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
