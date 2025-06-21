using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererShadowMapInstancedCSM : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UUseAnimations { get; private set; } = -1;
        public int UBoneTransforms { get; private set; } = -1;
        public int UNearFarSun { get; private set; } = -1;
        public int UTextureTransformOpacity { get; private set; } = -1;
        public int UTextureOffset { get; private set; } = -1;
        public int UTextureAlbedo { get; private set; } = -1;
        public int UBlockIndex { get; private set; } = -1;
        public int UTextureClip { get; private set; } = -1;

        public void SetGlobals()
        {

        }

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16instanced_csm.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.msm16instanced_csm.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16instanced_csm.frag";

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
                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                UBlockIndex = GL.GetUniformBlockIndex(ProgramID, "uInstanceBlock");
                GL.UniformBlockBinding(ProgramID, UBlockIndex, 0);

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

        public void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public void RenderSceneForLight(LightObject l)
        {
            if (KWEngine.CurrentWorld != null && l.ShadowQualityLevel != ShadowQuality.NoShadow && l.Color.W > 0)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
                for (int i = 0; i < 2; i++)
                {
                    GL.UniformMatrix4(UViewProjectionMatrix + i * KWEngine._uniformOffsetMultiplier, false, ref l._stateRender._viewProjectionMatrix[i]);
                }

                GL.Uniform3(UNearFarSun, new Vector3(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y, l._stateRender._nearFarFOVType.W));

                foreach (RenderObject r in KWEngine.CurrentWorld._renderObjects)
                {
                    if (r.IsShadowCaster && r._stateRender._opacity > 0 && r.IsAffectedByLight)
                    {
                        if (l._frustumShadowMap.IsBoxInFrustum(
                            l.Position,
                            l._stateRender._lookAtVector,
                            l._stateRender._nearFarFOVType.Y,
                            r.Center,
                            new Vector3(r.AABBLeft, r.AABBLow, r.AABBBack),
                            new Vector3(r.AABBRight, r.AABBHigh, r.AABBFront),
                            r._stateRender._dimensions.LengthFast))
                        {
                            Draw(r);
                        }
                    }
                }
            }
        }

        public void Draw(RenderObject r)
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBlockIndex, r._ubo);

            GeoMesh[] meshes = r._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = r._model.Material[i];

                if (material.ColorAlbedo.W == 0)
                    continue;

                if (r.IsAnimated)
                {
                    GL.Uniform1(UUseAnimations, 1);
                    for (int j = 0; j < r._stateRender._boneTranslationMatrices[mesh.Name].Length; j++)
                    {
                        Matrix4 tmp = r._stateRender._boneTranslationMatrices[mesh.Name][j];
                        GL.UniformMatrix4(UBoneTransforms + j * KWEngine._uniformOffsetMultiplier, false, ref tmp);
                    }
                }
                else
                {
                    GL.Uniform1(UUseAnimations, 0);
                }

                GL.UniformMatrix4(UModelMatrix, false, ref r._stateRender._modelMatrices[i]);
                GL.Uniform3(UTextureTransformOpacity, new Vector3(material.TextureAlbedo.UVTransform.X * r._stateRender._uvTransform.X, material.TextureAlbedo.UVTransform.Y * r._stateRender._uvTransform.Y, material.ColorAlbedo.W * r._stateRender._opacity));
                GL.Uniform2(UTextureOffset, new Vector2(material.TextureAlbedo.UVTransform.Z * r._stateRender._uvTransform.Z, material.TextureAlbedo.UVTransform.W * r._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, r._stateRender._uvClip);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureAlbedo, 0);
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, r.InstanceCount);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UBlockIndex, 0);
        }

        public void Draw(TerrainObject t)
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

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Draw(Framebuffer fbSource)
        {
            throw new NotImplementedException();
        }

        public void RenderScene()
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<GameObject> transparentObjects)
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<RenderObject> transparentObjects)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameObject g)
        {
            throw new NotImplementedException();
        }

        public void Draw(ViewSpaceGameObject vsgo)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameObject g, bool isVSG = false)
        {
            throw new NotImplementedException();
        }

        public void RenderScene(List<GameObject> transparentObjects, List<GameObject> stencilObjects)
        {
            throw new NotImplementedException();
        }
    }
}
