using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererTerrainGBufferNew
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNormalMatrix { get; private set; } = -1;
        public static int UMetallicRoughness { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UUseTexturesAlbedoNormalEmissive { get; private set; } = -1;
        public static int UUseTexturesMetallicRoughness { get; private set; } = -1;
        public static int UColorMaterial { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureMetallic { get; private set; } = -1;
        public static int UTextureRoughness { get; private set; } = -1;
        public static int UTextureEmissive { get; private set; } = -1;
        public static int UTextureTransform { get; private set; } = -1;
        public static int UIdShadowCaster { get; private set; } = -1;

        public static int UCamPosition { get; private set; } = -1;
        public static int UCamDirection { get; private set; } = -1;
        public static int UTextureHeightMap { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;
        private static int HeightmapTextureID = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.terrainNew.gbuffer.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.terrainNew.gbuffer.frag";
                string resourceNameTCShader = "KWEngine3.Shaders.shader.terrainNew.gbuffer.tesc";
                string resourceNameTEShader = "KWEngine3.Shaders.shader.terrainNew.gbuffer.tese";

                int vertexShader;
                int fragmentShader;
                int tcshader;
                int teshader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameTCShader))
                {
                    tcshader = HelperShader.LoadCompileAttachShader(s, ShaderType.TessControlShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameTEShader))
                {
                    teshader = HelperShader.LoadCompileAttachShader(s, ShaderType.TessEvaluationShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, -1, tcshader, teshader);


                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");
                UColorMaterial = GL.GetUniformLocation(ProgramID, "uColorMaterial");
                UMetallicRoughness = GL.GetUniformLocation(ProgramID, "uMetallicRoughness");
                UUseTexturesAlbedoNormalEmissive = GL.GetUniformLocation(ProgramID, "uUseTexturesAlbedoNormalEmissive");
                UUseTexturesMetallicRoughness = GL.GetUniformLocation(ProgramID, "uUseTexturesMetallicRoughness");
                UIdShadowCaster = GL.GetUniformLocation(ProgramID, "uIdShadowCaster");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UCamDirection = GL.GetUniformLocation(ProgramID, "uCamDirection");
                UCamPosition = GL.GetUniformLocation(ProgramID, "uCamPosition");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UTextureMetallic = GL.GetUniformLocation(ProgramID, "uTextureMetallic");
                UTextureRoughness = GL.GetUniformLocation(ProgramID, "uTextureRoughness");
                UTextureEmissive = GL.GetUniformLocation(ProgramID, "uTextureEmissive");
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UTextureHeightMap = GL.GetUniformLocation(ProgramID, "uTextureHeightMap");
            }

            if(true)
            {
                HeightmapTextureID = HelperTexture.LoadTextureForModelInternalExecutingAssembly("Assets.Textures.heightmap_test.png", out int mipMaps);


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
                foreach (TerrainObject t in KWEngine.CurrentWorld.GetTerrainObjects())
                {
                    if (t.IsInsideScreenSpace && t.IsVisible)
                        Draw(t);
                }
            }
        }

        public static void DrawTestTerrain()
        {
            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);
            GL.Uniform3(UColorTint, 1f, 1f, 1f);
            GL.Uniform4(UColorEmissive, 0f, 0f, 0f, 0f);
            GL.Uniform2(UIdShadowCaster, new Vector2i(1, 1));
            GL.Uniform3(UMetallicRoughness, new Vector3(0f, 1f, 0f));

            Matrix4 modelMatrix = Matrix4.Identity;
            Matrix4 normalMatrix = modelMatrix;
            GL.UniformMatrix4(UModelMatrix, false, ref modelMatrix);
            GL.UniformMatrix4(UNormalMatrix, false, ref normalMatrix);

            Vector3i useTexturesAlbedoNormalEmissive = new Vector3i(0, 0, 0);
            Vector3i useTexturesMetallicRoughness = new Vector3i(0, 0, 0);

            GL.Uniform3(UUseTexturesAlbedoNormalEmissive, useTexturesAlbedoNormalEmissive);
            GL.Uniform3(UUseTexturesMetallicRoughness, useTexturesMetallicRoughness);
            GL.Uniform3(UColorMaterial, 1f, 1f, 1f);

            GL.Uniform3(UCamPosition, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position);
            GL.Uniform3(UCamDirection, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector);

            UploadTexturesTest();

            GL.BindVertexArray(KWTerrainQuad.VAO);
            GL.DrawArrays(PrimitiveType.Patches, 0, 4);
            GL.BindVertexArray(0);

            HelperGeneral.CheckGLErrors();
        }

        public static void Draw(TerrainObject t)
        {
            GL.Uniform3(UColorTint, t._stateRender._colorTint);
            GL.Uniform4(UColorEmissive, t._stateRender._colorEmissive);
            GL.Uniform2(UIdShadowCaster, new Vector2i(t.ID, t.IsShadowCaster ? 1 : 0));
            GL.Uniform3(UMetallicRoughness, new Vector3(t._gModel._metallicTerrain, t._gModel._roughnessTerrain, Convert.ToSingle((int)t._gModel._metallicType)));

            GeoMesh[] meshes = t._gModel.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = t._gModel.Material[0]; // only for terrain objects: always index 0

                GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
                GL.UniformMatrix4(UNormalMatrix, false, ref t._stateRender._normalMatrix);

                Vector3i useTexturesAlbedoNormalEmissive = new Vector3i(
                    material.TextureAlbedo.IsTextureSet ? 1 : 0,
                    material.TextureNormal.IsTextureSet ? 1 : 0,
                    material.TextureEmissive.IsTextureSet ? 1 : 0
                    );
                Vector3i useTexturesMetallicRoughness = new Vector3i(
                    material.TextureMetallic.IsTextureSet ? 1 : 0,
                    material.TextureRoughness.IsTextureSet ? 1 : 0,
                    0 //always opaque
                    );
                GL.Uniform3(UUseTexturesAlbedoNormalEmissive, useTexturesAlbedoNormalEmissive);
                GL.Uniform3(UUseTexturesMetallicRoughness, useTexturesMetallicRoughness);
                GL.Uniform3(UColorMaterial, material.ColorAlbedo.Xyz);

                UploadTextures(ref material, t);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
        }

        private static void UploadTextures(ref GeoMaterial material, TerrainObject t)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
            GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * t._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * t._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + t._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + t._stateRender._uvTransform.W));

            // Normal
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 1);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureNormal.IsTextureSet ? material.TextureNormal.OpenGLID : KWEngine.TextureNormalEmpty);
            GL.Uniform1(UTextureNormal, TEXTUREOFFSET + 1);

            // Emissive
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 2);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureEmissive.IsTextureSet ? material.TextureEmissive.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureEmissive, TEXTUREOFFSET + 2);

            // Metallic/Roughness
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 3);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureMetallic.IsTextureSet ? material.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 3);

            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 4);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureRoughness.IsTextureSet ? material.TextureRoughness.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureRoughness, TEXTUREOFFSET + 4);
        }
        private static void UploadTexturesTest()
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
            GL.Uniform4(UTextureTransform, new Vector4(1f, 1f, 0f, 0f));

            // Normal
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 1);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureNormalEmpty);
            GL.Uniform1(UTextureNormal, TEXTUREOFFSET + 1);

            // Emissive
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 2);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
            GL.Uniform1(UTextureEmissive, TEXTUREOFFSET + 2);

            // Metallic/Roughness
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 3);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
            GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 3);

            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 4);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureWhite);
            GL.Uniform1(UTextureRoughness, TEXTUREOFFSET + 4);

            // testing height map
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 5);
            GL.BindTexture(TextureTarget.Texture2D, HeightmapTextureID);
            GL.Uniform1(UTextureHeightMap, TEXTUREOFFSET + 5);
        }
    }
}
