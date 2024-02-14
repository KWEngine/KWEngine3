using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererGBufferFoliage
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UPlayerPosShadowCaster { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNormalMatrix { get; private set; } = -1;
        public static int UColorTintAndEmissive { get; private set; } = -1;
        public static int UColorMaterial { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UPatchSizeTime { get; private set; } = -1;
        public static int UInstanceCount { get; private set; } = -1;
        public static int UNXNZ { get; private set; } = -1;
        public static int UDXDZ { get; private set; } = -1;
        public static int UNoise { get; private set; } = -1;

        public static int ULightConfig { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.gbufferfoliage.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.gbufferfoliage.frag";

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

                UColorTintAndEmissive = GL.GetUniformLocation(ProgramID, "uColorTintEmissive");
                UPlayerPosShadowCaster = GL.GetUniformLocation(ProgramID, "uPlayerPosShadowCaster");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                
                UPatchSizeTime = GL.GetUniformLocation(ProgramID, "uPatchSizeTime");
                UInstanceCount = GL.GetUniformLocation(ProgramID, "uInstanceCount");
                UNXNZ = GL.GetUniformLocation(ProgramID, "uNXNZ");
                UDXDZ = GL.GetUniformLocation(ProgramID, "uDXDZSway");

                UNoise = GL.GetUniformLocation(ProgramID, "uNoise");
                ULightConfig = GL.GetUniformLocation(ProgramID, "uLightConfig");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            
        }

        public static void RenderScene()
        {
            if (KWEngine.CurrentWorld != null)
            {
                SetGlobals();
                foreach (FoliageObject f in KWEngine.CurrentWorld._foliageObjects)
                {
                    if (KWEngine.Mode != EngineMode.Edit && !f.IsInsideScreenSpace)
                        continue;
                    Draw(f);
                }
            }
        }

        public static void Draw(FoliageObject f)
        {
            // Bind uniforms
            GL.UniformMatrix4(UModelMatrix, false, ref f._modelMatrix);
            GL.UniformMatrix4(UNormalMatrix, false, ref f._normalMatrix);
            GL.Uniform4(UColorTintAndEmissive, f._color);
            GL.Uniform4(UPlayerPosShadowCaster, 0f, 0f, 0f, 0f);
            GL.Uniform3(UPatchSizeTime, new Vector3(f._patchSize.X, f._patchSize.Y, KWEngine.CurrentWorld.WorldTime));
            GL.Uniform1(UInstanceCount, (float)f._instanceCount);
            GL.Uniform2(UNXNZ, f._nXZ);
            GL.Uniform3(UDXDZ, f._dXZ.X, f._dXZ.Y, f._swayFactor);
            GL.Uniform2(UNoise, f._noise.Length, f._noise);

            int val = f.IsShadowReceiver ? 1 : -1;
            val *= f.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(ULightConfig, val);

            // Bind textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, f._textureId != -1 ? f._textureId : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureFoliageGrassNormal);
            GL.Uniform1(UTextureNormal, 1);

            // Draw calls...
            GL.BindVertexArray(KWFoliageGrass.VAO);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, KWFoliageGrass._vertices.Length / 3, (int)f._instanceCount);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
