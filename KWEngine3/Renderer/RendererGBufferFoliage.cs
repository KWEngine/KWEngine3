using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererGBufferFoliage
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNormalMatrix { get; private set; } = -1;
        public static int UColorTintAndEmissive { get; private set; } = -1;
        public static int UColorMaterial { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureNoise { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UPatchSizeTime { get; private set; } = -1;
        public static int UInstanceCount { get; private set; } = -1;
        public static int UNXNZ { get; private set; } = -1;
        public static int UDXDZSwayRound { get; private set; } = -1;
        public static int UNoise { get; private set; } = -1;
        public static int UTerrainPosition { get; private set; } = -1;
        public static int UTerrainScale { get; private set; } = -1;
        public static int UTerrainHeightMap { get; private set; } = -1;
        public static int ULightConfig { get; private set; } = -1;
        public static int URoughnessMetallic { get; private set; } = -1;

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

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                
                UPatchSizeTime = GL.GetUniformLocation(ProgramID, "uPatchSizeTime");
                UInstanceCount = GL.GetUniformLocation(ProgramID, "uInstanceCount");
                UNXNZ = GL.GetUniformLocation(ProgramID, "uNXNZ");
                UDXDZSwayRound = GL.GetUniformLocation(ProgramID, "uDXDZSwayRound");

                UNoise = GL.GetUniformLocation(ProgramID, "uNoise");
                ULightConfig = GL.GetUniformLocation(ProgramID, "uLightConfig");
                UTerrainPosition = GL.GetUniformLocation(ProgramID, "uTerrainPosition");
                UTerrainScale = GL.GetUniformLocation(ProgramID, "uTerrainScale");
                UTerrainHeightMap = GL.GetUniformLocation(ProgramID, "uTerrainHeightMap");

                UTextureNoise = GL.GetUniformLocation(ProgramID, "uNoiseMap");

                URoughnessMetallic = GL.GetUniformLocation(ProgramID, "uRoughnessMetallic");
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
                    if (KWEngine.Mode != EngineMode.Edit && !f.IsInsideScreenSpaceForRenderPass)
                    {
                        continue;
                    }
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
            GL.Uniform3(UPatchSizeTime, new Vector3(f._patchSize.X, f._patchSize.Y, KWEngine.CurrentWorld.WorldTime));
            GL.Uniform1(UInstanceCount, (float)f._instanceCount);
            GL.Uniform2(UNXNZ, f._nXZ);
            GL.Uniform4(UDXDZSwayRound, f._dXZ.X, f._dXZ.Y, f._swayFactor, f.IsSizeReducedAtCorners ? 0.05f : 1.0f);
            GL.Uniform2(UNoise, f._noise.Length, f._noise);

            GL.Uniform2(URoughnessMetallic, f._roughness, 0.0f);

            int val = f.IsShadowReceiver ? 1 : -1;
            val *= f.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(ULightConfig, val);

            // Bind textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, f._textureId != -1 ? f._textureId : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, f.Type == FoliageType.GrassMinecraft || f.Type == FoliageType.Fern ? KWEngine.TextureNormalEmpty : KWEngine.TextureFoliageGrassNormal);
            GL.Uniform1(UTextureNormal, 1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureNoise);
            GL.Uniform1(UTextureNoise, 2);

            if (f._terrainObject != null && f._terrainObject.ID != 0)
            {
                GeoTerrain m = f._terrainObject._gModel.ModelOriginal.Meshes.ElementAt(0).Value.Terrain;
                GL.Uniform3(UTerrainScale, (float)m.GetWidth(), (float)m.GetHeight(), (float)m.GetDepth());
                GL.Uniform3(UTerrainPosition, f._terrainObject._stateRender._position);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, m._texHeight);
                GL.Uniform1(UTerrainHeightMap, 3);
            }
            else
            {
                GL.Uniform3(UTerrainScale, 0f, 0f, 0f);
                GL.Uniform3(UTerrainPosition, 0f, 0f, 0f);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
                GL.Uniform1(UTerrainHeightMap, 3);
            }

            // Draw calls...
            if(f.Type == FoliageType.GrassMinecraft)
            {
                GeoMesh m = KWEngine.KWFoliageMinecraft.Meshes.ElementAt(0).Value;
                GL.BindVertexArray(m.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, m.VBOIndex);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, m.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, f._instanceCount);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
            else if(f.Type == FoliageType.Fern)
            {
                GeoMesh m = KWEngine.KWFoliageFern.Meshes.ElementAt(0).Value;
                GL.BindVertexArray(m.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, m.VBOIndex);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, m.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, f._instanceCount);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
            else
            {
                GL.BindVertexArray(KWFoliageGrass.VAO);
                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, KWFoliageGrass._vertices.Length / 3, f._instanceCount);
                GL.BindVertexArray(0);
            }
            

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
