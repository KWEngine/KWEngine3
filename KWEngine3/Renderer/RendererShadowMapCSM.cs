using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererShadowMapCSM
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UUseAnimations { get; private set; } = -1;
        public static int UBoneTransforms { get; private set; } = -1;
        public static int UNearFarSun { get; private set; } = -1;
        public static int UTextureTransformOpacity { get; private set; } = -1;
        public static int UTextureOffset { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureClip { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16_csm.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.msm16_csm.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16_csm.frag";

                int vertexShader;
                int geometryShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameGeometryShader))
                {
                    geometryShader = HelperShader.LoadCompileAttachShader(s, ShaderType.GeometryShader, ProgramID);
                }
                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader, geometryShader);


                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UUseAnimations = GL.GetUniformLocation(ProgramID, "uUseAnimations");
                UBoneTransforms = GL.GetUniformLocation(ProgramID, "uBoneTransforms");
                UNearFarSun = GL.GetUniformLocation(ProgramID, "uNearFarSun");
                UTextureTransformOpacity = GL.GetUniformLocation(ProgramID, "uTextureTransformOpacity");
                UTextureOffset = GL.GetUniformLocation(ProgramID, "uTextureOffset");
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderSceneForLight(LightObject l)
        {
            if (KWEngine.CurrentWorld != null && l.ShadowQualityLevel != ShadowQuality.NoShadow && l.Color.W > 0)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);

                for (int i = 0; i < 2; i++)
                {
                    GL.UniformMatrix4(UViewProjectionMatrix + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[i]);
                }

                GL.Uniform3(UNearFarSun, new Vector3(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y, l._stateRender._nearFarFOVType.W));
                foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                {
                    if(g.IsShadowCaster && g._stateRender._opacity > 0 && g.IsAffectedByLight)
                    {
                        if(l._frustumShadowMap.IsBoxInFrustum(l.Position, l._stateRender._lookAtVector, l._stateRender._nearFarFOVType.Y, g.Center, g.Diameter))
                        {
                            Draw(g);
                        }
                    }
                }

                if (KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached && KWEngine.CurrentWorld._viewSpaceGameObject.DepthTestingEnabled)
                {
                    ViewSpaceGameObject vsgo = KWEngine.CurrentWorld.GetViewSpaceGameObject();
                    if (vsgo.IsShadowCaster && vsgo._gameObject._stateRender._opacity > 0 && vsgo._gameObject.IsAffectedByLight)
                    {
                        Draw(vsgo);
                    }
                }
            }
        }

        public static void Draw(GameObject g)
        {
            GeoMesh[] meshes = g._model.ModelOriginal.Meshes.Values.ToArray();
            GL.Uniform2(UTextureClip, g._stateRender._uvClip);
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = g._model.Material[i];

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
                GL.Uniform3(UTextureTransformOpacity, new Vector3(material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X, material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y, material.ColorAlbedo.W * g._stateRender._opacity));
                GL.Uniform2(UTextureOffset, new Vector2(material.TextureAlbedo.UVTransform.Z * g._stateRender._uvTransform.Z, material.TextureAlbedo.UVTransform.W * g._stateRender._uvTransform.W));
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

        public static void Draw(ViewSpaceGameObject vsgo)
        {
            GeoMesh[] meshes = vsgo._gameObject._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = vsgo._gameObject._model.Material[i];

                if (material.ColorAlbedo.W == 0)
                    continue;

                if (vsgo._gameObject.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    for (int j = 0; j < vsgo._gameObject._stateRender._boneTranslationMatrices[mesh.Name].Length; j++)
                    {
                        Matrix4 tmp = vsgo._gameObject._stateRender._boneTranslationMatrices[mesh.Name][j];
                        GL.UniformMatrix4(UBoneTransforms + j * KWEngine._uniformOffsetMultiplier, false, ref tmp);
                    }
                }
                else
                {
                    GL.Uniform1(UUseAnimations, 0);
                }

                GL.UniformMatrix4(UModelMatrix, false, ref vsgo._gameObject._stateRender._modelMatrices[i]);
                GL.Uniform3(UTextureTransformOpacity, new Vector3(material.TextureAlbedo.UVTransform.X * vsgo._gameObject._stateRender._uvTransform.X, material.TextureAlbedo.UVTransform.Y * vsgo._gameObject._stateRender._uvTransform.Y, material.ColorAlbedo.W * vsgo._gameObject._stateRender._opacity));
                GL.Uniform2(UTextureOffset, new Vector2(material.TextureAlbedo.UVTransform.Z * vsgo._gameObject._stateRender._uvTransform.Z, material.TextureAlbedo.UVTransform.W * vsgo._gameObject._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, vsgo._gameObject._stateRender._uvClip);
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
