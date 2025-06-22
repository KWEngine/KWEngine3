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

            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, RenderManager.FramebufferLightingPass.Attachments[2].ID);
            //GL.Uniform1(UTextureAlbedo, 0);
        }

        public static void RenderScene(List<GameObject> gameobjects)
        {
            foreach (GameObject g in gameobjects)
            {
                Matrix4 tmpTranslate1 = Matrix4.CreateTranslation(-g._model.Center.X, -g._model.Center.Y, -g._model.Center.Z);
                Matrix4 scaleMatrix = HelperMatrix.Scale110;
                Matrix4 tmpTranslate2 = Matrix4.CreateTranslation(g._model.Center.X, g._model.Center.Y, g._model.Center.Z);
                Matrix4 tmpTranslate3 = tmpTranslate1 * scaleMatrix * tmpTranslate2;

                if (g._colorHighlightMode == HighlightMode.WhenOccludedOutline)
                {
                    GL.ColorMask(false, false, false, false);
                    GL.StencilMask(0xFF);

                    // step 1: populate only stencil with ones
                    GL.Enable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Front);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
                    Draw(g, false, ref HelperMatrix.Scale100);

                    
                    // step 2: populate stencil buffer with scaled mesh
                    GL.DepthFunc(DepthFunction.Less);
                    GL.CullFace(TriangleFace.Front);
                    GL.StencilFunc(StencilFunction.Equal, 0, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Invert);
                    Draw(g, false, ref tmpTranslate3);


                    // step 3: actually draw color shape
                    GL.ColorMask(true, true, true, true);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Disable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Back);
                    GL.StencilFunc(StencilFunction.Greater, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    Draw(g, false, ref tmpTranslate3);

                }
                else if(g._colorHighlightMode == HighlightMode.WhenOccluded)
                {
                    GL.ColorMask(false, false, false, false);
                    GL.StencilMask(0xFF);

                    // step 1: populate only stencil with ones where the object is passing depth test
                    GL.Enable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Back);
                    GL.DepthFunc(DepthFunction.Lequal);
                    GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                    Draw(g, false, ref HelperMatrix.Scale100);

                    // step 2: actually draw color shape where no 1s have been written
                    GL.ColorMask(true, true, true, true);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Disable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Back);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    Draw(g, false, ref HelperMatrix.Scale100);
                }
                else if(g._colorHighlightMode == HighlightMode.Permanent)
                {
                    GL.ColorMask(true, true, true, true);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Disable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Back);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    Draw(g, false, ref HelperMatrix.Scale100);
                }
                else if(g._colorHighlightMode == HighlightMode.PermanentOutline)
                {
                    // step 1: populate only stencil with ones
                    GL.Enable(EnableCap.DepthTest);
                    GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                    GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                    Draw(g, false, ref HelperMatrix.Scale100);

                    // step 2: actually draw color shape
                    GL.ColorMask(true, true, true, true);
                    GL.Disable(EnableCap.DepthTest);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    GL.StencilMask(0x00);
                    Draw(g, false, ref tmpTranslate3);
                }
                
            }
        }

        public static void Draw(GameObject g, bool vsg, ref Matrix4 scaleMatrix)
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

                Matrix4 modelmatrix = scaleMatrix * g._stateRender._modelMatrices[i];
                GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);

                GL.Uniform4(UTextureTransform, new Vector4(
                material.TextureAlbedo.UVTransform.X * g._stateRender._uvTransform.X,
                material.TextureAlbedo.UVTransform.Y * g._stateRender._uvTransform.Y,
                material.TextureAlbedo.UVTransform.Z + g._stateRender._uvTransform.Z,
                material.TextureAlbedo.UVTransform.W + g._stateRender._uvTransform.W));
                GL.Uniform2(UTextureClip, g._stateRender._uvClip);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public static void DrawSteps(ref Matrix4 modelmatrix, GeoMesh mesh, HighlightMode mode)
        {
            // pre step: disable writing to color attachments, because we only need stencil/depth write ops
            GL.ColorMask(false, false, false, false);
            
            GL.StencilMask(0xFF);
            if (mode == HighlightMode.WhenOccludedOutline)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);

                // first step: just populate stencil buffer with regular shape
                {
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Keep);

                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                GL.DepthFunc(DepthFunction.Less);

                // second step: increate stencil value for larger 
                {
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Incr, StencilOp.Keep);

                    modelmatrix = HelperMatrix.Scale110 * modelmatrix;
                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);

                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                // re-enable writing to color attachments for 3rd step:
                GL.ColorMask(true, true, true, true);
                GL.Disable(EnableCap.DepthTest);
                GL.StencilMask(0x00);

                // third step: really write to color attachments
                {
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
            }
            else if(mode == HighlightMode.WhenOccluded)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);

                // first step: just populate stencil buffer with regular shape
                {
                    GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Keep);

                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                GL.DepthFunc(DepthFunction.Less);


                // re-enable writing to color attachments for 3rd step:
                GL.ColorMask(true, true, true, true);
                GL.Disable(EnableCap.DepthTest);
                GL.StencilMask(0x00);

                // third step: really write to color attachments
                {
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
            }
            else if (mode == HighlightMode.Permanent)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);
                GL.ColorMask(true, true, true, true);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            else if(mode == HighlightMode.PermanentOutline)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                // first step: just populate stencil buffer with regular shape
                {
                    GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                    GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);

                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                // second step: increate stencil value for larger 
                {
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Incr, StencilOp.Keep, StencilOp.Decr);

                    modelmatrix = HelperMatrix.Scale110 * modelmatrix;
                    GL.UniformMatrix4(UModelMatrix, false, ref modelmatrix);

                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }

                // re-enable writing to color attachments for 3rd step:
                GL.ColorMask(true, true, true, true);
                GL.StencilMask(0x00);

                // third step: really write to color attachments
                {
                    GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                    GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}
