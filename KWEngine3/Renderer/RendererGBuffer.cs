using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererGBuffer
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UNormalMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorEmissive { get; private set; } = -1;
        public static int UColorMaterial { get; private set; } = -1;
        public static int UMetallicRoughness { get; private set; } = -1;
        public static int UUseTexturesAlbedoNormalEmissive { get; private set; } = -1;
        public static int UUseTexturesMetallicRoughness { get; private set; } = -1;
        public static int UTextureMetallicRoughnessCombined { get; private set; } = -1;
        public static int UTextureNormal { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureMetallic { get; private set; } = -1;
        public static int UTextureRoughness { get; private set; } = -1;
        public static int UTextureEmissive { get; private set; } = -1;
        public static int UTextureTransform { get; private set; } = -1;
        public static int UTextureClip { get; private set; } = -1;
        public static int UUseAnimations { get; private set; } = -1;
        public static int UBoneTransforms { get; private set; } = -1;
        public static int UIdShadowCaster { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.gbuffer.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.gbuffer.frag";

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


                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorMaterial = GL.GetUniformLocation(ProgramID, "uColorMaterial");
                UColorEmissive = GL.GetUniformLocation(ProgramID, "uColorEmissive");
                UMetallicRoughness = GL.GetUniformLocation(ProgramID, "uMetallicRoughness");
                UUseTexturesAlbedoNormalEmissive = GL.GetUniformLocation(ProgramID, "uUseTexturesAlbedoNormalEmissive");
                UUseTexturesMetallicRoughness = GL.GetUniformLocation(ProgramID, "uUseTexturesMetallicRoughness");
                UIdShadowCaster = GL.GetUniformLocation(ProgramID, "uIdShadowCaster");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UTextureMetallic = GL.GetUniformLocation(ProgramID, "uTextureMetallic");
                UTextureRoughness = GL.GetUniformLocation(ProgramID, "uTextureRoughness");
                UTextureEmissive = GL.GetUniformLocation(ProgramID, "uTextureEmissive");
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");
                UTextureMetallicRoughnessCombined = GL.GetUniformLocation(ProgramID, "uTextureIsMetallicRoughnessCombined");
                UUseAnimations = GL.GetUniformLocation(ProgramID, "uUseAnimations");
                UBoneTransforms = GL.GetUniformLocation(ProgramID, "uBoneTransforms");
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

        public static List<GameObject> RenderScene()
        {
            List<GameObject> forwardObjects = new();
            if (KWEngine.CurrentWorld != null)
            {
                SetGlobals();
                foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                {
                    if (KWEngine.Mode != EngineMode.Edit && (g.SkipRender || !g.IsInsideScreenSpace))
                        continue;
                    if (g.IsTransparent || g.IsDepthTesting == false)
                    {
                        forwardObjects.Add(g);
                        continue;
                    }
                    Draw(g);
                }
            }
            return forwardObjects;
        }

        public static void Draw(GameObject g)
        {
            if (g._stateCurrent._opacity <= 0f)
                return;

            GL.Uniform3(UColorTint, g._stateRender._colorTint);

            int val = g.IsShadowCaster ? 1 : -1;
            val *= g.IsAffectedByLight ? 1 : 10;
            GL.Uniform2(UIdShadowCaster, new Vector2i(g.ID, val));

            GeoMesh[] meshes = g._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = g._model.Material[i];

                if (material.ColorAlbedo.W == 0)
                    continue;

                GL.Uniform3(UMetallicRoughness, new Vector3(material.Metallic, material.Roughness, Convert.ToSingle((int)g._model._metallicType)));

                if (material.TextureEmissive.IsTextureSet)
                    GL.Uniform4(UColorEmissive, Vector4.Zero);
                else
                    GL.Uniform4(UColorEmissive, g._stateRender._colorEmissive);

                if (g.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    int boneMatrixCount = g._stateRender._boneTranslationMatrices[mesh.Name].Length;
                    
                    for (int j = 0; j < boneMatrixCount; j++)
                    {
                        Matrix4 tmp = g._stateRender._boneTranslationMatrices[mesh.Name][j];
                        GL.UniformMatrix4(UBoneTransforms + j * KWEngine._uniformOffsetMultiplier, false, ref tmp);
                    }
                }
                else
                {
                    GL.Uniform1(UUseAnimations, 0);
                }

                GL.UniformMatrix4(UModelMatrix, false, ref g._stateRender._modelMatrices[i]);
                GL.UniformMatrix4(UNormalMatrix, false, ref g._stateRender._normalMatrices[i]);
                Vector4 uvTrans = new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W);
                GL.Uniform4(UTextureTransform, uvTrans);
                GL.Uniform2(UTextureClip, g._stateRender._uvClip);

                GL.Uniform3(UColorMaterial, material.ColorAlbedo.Xyz);

                Vector3i useTexturesAlbedoNormalEmissive = new(
                    material.TextureAlbedo.IsTextureSet ? 1 : 0,
                    material.TextureNormal.IsTextureSet ? 1 : 0,
                    material.TextureEmissive.IsTextureSet ? 1 : 0
                    );
                Vector3i useTexturesMetallicRoughness = new(
                    material.TextureMetallic.IsTextureSet ? 1 : 0,
                    material.TextureRoughness.IsTextureSet ? 1 : 0,
                    g.IsTransparent ? 1 : 0 // TODO: opacity < 1? but this is not used in shader yet!
                    );
                GL.Uniform3(UUseTexturesAlbedoNormalEmissive, useTexturesAlbedoNormalEmissive);
                GL.Uniform3(UUseTexturesMetallicRoughness, useTexturesMetallicRoughness);
                
                UploadTextures(ref material, g);

                if(material.RenderBackFace && g.DisableBackfaceCulling)
                {
                    GL.Disable(EnableCap.CullFace);
                }

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);

                if (material.RenderBackFace && g.DisableBackfaceCulling)
                {
                    GL.Enable(EnableCap.CullFace);
                }
            }
        }

        private static void UploadTextures(ref GeoMaterial material, GameObject g)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
            GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W));

            // Normal
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 1);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureNormal.IsTextureSet ? material.TextureNormal.OpenGLID : KWEngine.TextureNormalEmpty);
            GL.Uniform1(UTextureNormal, TEXTUREOFFSET + 1);

            // Emissive
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 2);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureEmissive.IsTextureSet ? material.TextureEmissive.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureEmissive, TEXTUREOFFSET + 2);

            // Metallic/Roughness
            GL.Uniform1(UTextureMetallicRoughnessCombined, material.TextureRoughnessInMetallic ? 1 : 0);
            if(material.TextureRoughnessInMetallic)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 3);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureMetallic.IsTextureSet ? material.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
                GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 3);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 3);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureMetallic.IsTextureSet ? material.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
                GL.Uniform1(UTextureMetallic, TEXTUREOFFSET + 3);

                GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 4);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureRoughness.IsTextureSet ? material.TextureRoughness.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureRoughness, TEXTUREOFFSET + 4);
            }
        }
    }
}
