using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal class RendererForwardSimple
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureTransform { get; private set; } = -1;
        public static int UUseAnimations { get; private set; } = -1;
        public static int UBoneTransforms { get; private set; } = -1;
        public static int UTextureClip { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.lighting.forward_simple.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.lighting.forward_simple.frag";

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
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UTextureClip = GL.GetUniformLocation(ProgramID, "uTextureClip");

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

        public static void RenderScene(List<GameObject> gameobjects)
        {
            foreach (GameObject g in gameobjects)
            { 
                Draw(g, false);
            }
        }

        public static void Draw(GameObject g, bool vsg)
        {
            GL.Uniform4(UColorTint, g._colorHighlight);

            GeoMesh[] meshes = g._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = g._model.Material[i];
                if (material.ColorAlbedo.W <= 0)
                    continue;

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
                GL.Uniform1(UTextureAlbedo, 0);

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

                Matrix4 modelmatrix = g._stateRender._modelMatrices[i];

                GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, g._stateRender._uvClip);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);

                if (g._colorHighlightMode == HighlightMode.WhenOccludedOutline)
                {
                    // render first simple pass with same size:
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Lequal);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Keep);
                    GL.StencilMask(0xFF);
                    GL.ColorMask(false, false, false, false);

                    modelmatrix = HelperMatrix.Scale110 * modelmatrix;
                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);


                    // render second pass with larger size:
                    GL.ColorMask(true, true, true, true);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Disable(EnableCap.DepthTest);

                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
                else if(g._colorHighlightMode == HighlightMode.PermanentOutline)
                {
                    modelmatrix = HelperMatrix.Scale110 * modelmatrix;
                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);

                    // render second pass with larger size:
                    GL.ColorMask(true, true, true, true);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Disable(EnableCap.DepthTest);

                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);

                    GL.Disable(EnableCap.DepthTest);
                    GL.ColorMask(true, true, true, true);
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public static void Draw(GameObject g)
        {
            Draw(g, false);
        }
    }
}
