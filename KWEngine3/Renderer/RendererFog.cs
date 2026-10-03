using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
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
        public const int FOG_MAX_VOLUMES = 16;            // must match FOG_MAX_VOLUMES in fog.glsl
        private const int UBO_HEADERFLOATS = 24;          // 6x vec4 before the volume array
        private static readonly float[] _uboData = new float[UBO_HEADERFLOATS + FOG_MAX_VOLUMES * FogVolume.UBO_FLOATS]; // std140, see uBlockFog in fog.glsl

        // tileable 3D noise for fog patches and the wavy fog top (generated once at startup)
        public const int NOISE_TEXTUREUNIT = 15;          // only unit that is still free in the forward shaders (GL 4.0 guarantees 16)
        private const string NOISE_SAMPLERNAME = "uTextureFogNoise";
        private const int NOISE_SIZE = 64;                // 64^3 texels, R8 = 256 KB (+ mipmaps)
        private const int NOISE_BASECELLS = 4;            // noise cells per tile in the coarsest octave
        private const int NOISE_OCTAVES = 3;
        private const int NOISE_SEED = 1337;
        private const double NOISE_EVOLUTIONRATE = 0.01;  // slow change of the pattern (noise tiles per second), independent of wind
        private const float NOISE_FEATURETEXELS = 16f;    // approx. size of one noise feature in texels (64 texels / 4 base cells)
        public static int TextureNoise3D { get; private set; } = -1;
        public static int ActiveVolumeCount { get; private set; } = 0; // fog volumes inside the view frustum (this frame)

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

                CreateNoiseTexture();
                BindFogResourcesToProgram(ProgramID);
            }
        }

        private static void CreateNoiseTexture()
        {
            byte[] data = HelperPerlinNoise.GenerateTileableNoise3D(NOISE_SIZE, NOISE_BASECELLS, NOISE_OCTAVES, NOISE_SEED);

            // rows are 64 bytes long -> default unpack alignment (4) fits, no PixelStore change needed
            TextureNoise3D = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, TextureNoise3D);
            GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.R8, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }

        /// <summary>
        /// Connects the uniform block uBlockFog and the sampler of the fog noise texture (unit 15) of a shader program.
        /// Call once after linking (programs without block/sampler are ignored).
        /// </summary>
        /// <param name="programId">id of the linked shader program</param>
        internal static void BindFogResourcesToProgram(int programId)
        {
            int blockIndex = GL.GetUniformBlockIndex(programId, UBO_BLOCKNAME);
            if (blockIndex >= 0)
            {
                GL.UniformBlockBinding(programId, blockIndex, UBO_BINDINGPOINT);
            }

            // sampler uniforms are program state: set once after linking (GL 4.0 has no layout(binding = ...))
            int samplerLocation = GL.GetUniformLocation(programId, NOISE_SAMPLERNAME);
            if (samplerLocation >= 0)
            {
                int previousProgram = GL.GetInteger(GetPName.CurrentProgram);
                GL.UseProgram(programId);
                GL.Uniform1(samplerLocation, NOISE_TEXTUREUNIT);
                GL.UseProgram(previousProgram);
            }
        }

        /// <summary>
        /// Writes the fog parameters of the current world to the UBO (once per frame, before fog and forward pass)
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

            // noise parameters
            float frequency = 1f / w._fogNoiseSize;
            _uboData[8] = w._fogNoiseStrength;
            _uboData[9] = frequency;
            _uboData[10] = w._fogHeightNoise;
            _uboData[11] = KWEngine.Window._renderQuality == RenderQualityLevel.Low ? 0f : 1f; // Low: wavy top only, no patches

            // wind: shift the noise against the wind direction so that the pattern travels with the wind
            // (computed in double precision and wrapped to one noise tile -> no precision loss after hours)
            double worldTime = KWEngine.WorldTime;
            double travel = worldTime * w._fogWindSpeed * frequency;
            _uboData[12] = WrapToTile(-w._fogWindDirection.X * travel);
            _uboData[13] = WrapToTile(-w._fogWindDirection.Y * travel);
            _uboData[14] = WrapToTile(-w._fogWindDirection.Z * travel);
            _uboData[15] = WrapToTile(worldTime * NOISE_EVOLUTIONRATE);

            // mip selection: noise texels covered by one pixel per world unit of distance
            // (uses the already computed projection matrix: M22 = 1 / tan(fovY / 2))
            Matrix4 projection = KWEngine.Mode == EngineMode.Play ? w._cameraGame._stateRender.ProjectionMatrix : w._cameraEditor._stateRender.ProjectionMatrix;
            float viewportHeight = Math.Max(1, KWEngine.Window.ClientRectangle.Size.Y);
            _uboData[16] = 2f / (Math.Max(projection.M22, 0.0001f) * viewportHeight) * frequency * NOISE_SIZE;
            // averaging along long rays: noise features per world unit of ray path
            _uboData[17] = frequency * NOISE_SIZE / NOISE_FEATURETEXELS;
            _uboData[18] = 0f;
            _uboData[19] = 0f;

            // only volumes inside the view frustum (view rays never leave it)
            Frustum frustum = KWEngine.Mode == EngineMode.Play ? w._cameraGame._frustum : w._cameraEditor._frustum;
            int volumeCount = 0;
            foreach (FogVolume v in w._fogVolumes)
            {
                if (volumeCount == FOG_MAX_VOLUMES)
                    break;
                if (v._density <= 0f || !frustum.VolumeVsFrustum(v._aabbCenter, v._aabbHalfExtent.X + 1f, v._aabbHalfExtent.Y + 1f, v._aabbHalfExtent.Z + 1f))
                    continue;
                Array.Copy(v._uboData, 0, _uboData, UBO_HEADERFLOATS + volumeCount * FogVolume.UBO_FLOATS, FogVolume.UBO_FLOATS);
                volumeCount++;
            }
            ActiveVolumeCount = volumeCount;
            _uboData[20] = volumeCount;
            _uboData[21] = 0f;
            _uboData[22] = 0f;
            _uboData[23] = 0f;

            int uploadFloats = UBO_HEADERFLOATS + volumeCount * FogVolume.UBO_FLOATS;
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, uploadFloats * sizeof(float), _uboData);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBO_BINDINGPOINT, UBO);

            // noise texture stays on its own unit for the fog pass and all forward shaders of this frame
            GL.ActiveTexture(TextureUnit.Texture0 + NOISE_TEXTUREUNIT);
            GL.BindTexture(TextureTarget.Texture3D, TextureNoise3D);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private static float WrapToTile(double value)
        {
            return (float)(value - Math.Floor(value));
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
