using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererShadowMap
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UUseAnimations { get; private set; } = -1;
        public static int UBoneTransforms { get; private set; } = -1;
        public static int UNearFarSun { get; private set; } = -1;
        public static int UTextureTransformOpacity { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16.frag";

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


                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UUseAnimations = GL.GetUniformLocation(ProgramID, "uUseAnimations");
                UBoneTransforms = GL.GetUniformLocation(ProgramID, "uBoneTransforms");
                UNearFarSun = GL.GetUniformLocation(ProgramID, "uNearFarSun");
                UTextureTransformOpacity = GL.GetUniformLocation(ProgramID, "uTextureTransformOpacity");
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderSceneForLight(LightObject l)
        {
            if (KWEngine.CurrentWorld != null && l.ShadowCasterType != ShadowQuality.NoShadow)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
                Matrix4 vp = l._stateRender._viewProjectionMatrix[0];
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);

                //_nearFarFOVType = new Vector4(NEARDEFAULT, FARDEFAULT, FOVDEFAULT, type == LightType.Point ? 0 : type == LightType.Sun ? -1 : 1);
                GL.Uniform3(UNearFarSun, new Vector3(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y, l._stateRender._nearFarFOVType.W));
                foreach (GameObject g in KWEngine.CurrentWorld.GetGameObjects())
                {
                    if(g.IsShadowCaster)
                        Draw(g);
                }

                foreach(TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    if (t.IsShadowCaster)
                        Draw(t);
                }
            }
        }

        public static void Draw(GameObject g)
        {
            float o = g._stateRender._opacity;
            if(o == 0f)
                return;

            GeoMesh[] meshes = g._gModel.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = g._gModel.Material[i];

                if(material.ColorAlbedo.W == 0)
                    continue;

                if (g.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    for (int j = 0; j < g._stateRender._boneTranslationMatrices[mesh.Name].Length; j++)
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
                GL.Uniform3(UTextureTransformOpacity, new Vector3(material.TextureAlbedo.UVTransform.X, material.TextureAlbedo.UVTransform.Y, material.ColorAlbedo.W));
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureAlbedo, 0);
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
        }

        public static void Draw(TerrainObject t)
        {
            GeoMesh[] meshes = t._gModel.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = t._gModel.Material[i];

                if (material.ColorAlbedo.W == 0)
                    continue;

                GL.Uniform1(UUseAnimations, 0);

                GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
                GL.Uniform3(UTextureTransformOpacity, new Vector3(material.TextureAlbedo.UVTransform.X, material.TextureAlbedo.UVTransform.Y, material.ColorAlbedo.W));
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureAlbedo, 0);
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
        }
    }
}
