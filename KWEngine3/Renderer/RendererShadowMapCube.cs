using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererShadowMapCube : IRenderer
    {
        public int ProgramID { get; private set; } = -1;
        public int UViewProjectionMatrix { get; private set; } = -1;
        public int UModelMatrix { get; private set; } = -1;
        public int UUseAnimations { get; private set; } = -1;
        public int UBoneTransforms { get; private set; } = -1;
        public int UNearFar { get; private set; } = -1;
        public int ULightPosition { get; private set; } = -1;
        public int UTextureTransformOpacity { get; private set; } = -1;
        public int UTextureOffset { get; private set; } = -1;
        public int UTextureAlbedo { get; private set; } = -1;
        public int UTextureClip { get; private set; } = -1;

        public void UnbindUBO(int ubo)
        {
            
        }

        public void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16.cube.vert";
                string resourceNameGeometryShader = "KWEngine3.Shaders.shader.msm16.cube.geom";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16.cube.frag";

                int vertexShader;
                int fragmentShader;
                int geometryShader;
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
                UNearFar = GL.GetUniformLocation(ProgramID, "uNearFar");
                ULightPosition = GL.GetUniformLocation(ProgramID, "uLightPosition");
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

            GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
            for(int i = 0; i < 6; i++)
            {
                Matrix4 vp = l._stateRender._viewProjectionMatrix[i];
                GL.UniformMatrix4(UViewProjectionMatrix + i * KWEngine._uniformOffsetMultiplier, false, ref vp);
            }
            GL.Uniform3(ULightPosition, l._stateRender._position);
            GL.Uniform2(UNearFar, new Vector2(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y));
            foreach (GameObject g in KWEngine.CurrentWorld.GetGameObjects())
            {
                if(g.IsShadowCaster && g._stateRender._opacity > 0 && g.IsAffectedByLight)
                    Draw(g);
            }

            if(KWEngine.CurrentWorld.IsViewSpaceGameObjectAttached && KWEngine.CurrentWorld._viewSpaceGameObject.DepthTestingEnabled)
            {
                ViewSpaceGameObject vsgo = KWEngine.CurrentWorld.GetViewSpaceGameObject();
                if(vsgo.IsShadowCaster && vsgo._gameObject._stateRender._opacity > 0 && vsgo._gameObject.IsAffectedByLight)
                {
                    Draw(vsgo);
                }
            }
        }

        public void Draw(GameObject g)
        {
            GeoMesh[] meshes = g._model.ModelOriginal.Meshes.Values.ToArray();
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
                GL.Uniform2(UTextureClip, g._stateRender._uvClip);
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

        public void Draw(ViewSpaceGameObject vsgo)
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

        public void SetGlobals()
        {
            
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

        public void Draw(RenderObject r)
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
