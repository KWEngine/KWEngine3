﻿using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererShadowMapInstanced
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
        public static int UBlockIndex { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16instanced.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16instanced.frag";

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
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderSceneForLight(LightObject l)
        {
            if (KWEngine.CurrentWorld != null && l.ShadowCasterType != ShadowQuality.NoShadow && l.Color.W > 0)
            {
                GL.Viewport(0, 0, l._shadowMapSize, l._shadowMapSize);
                Matrix4 vp = l._stateRender._viewProjectionMatrix[0];
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref vp);

                GL.Uniform3(UNearFarSun, new Vector3(l._stateRender._nearFarFOVType.X, l._stateRender._nearFarFOVType.Y, l._stateRender._nearFarFOVType.W));

                foreach (RenderObject r in KWEngine.CurrentWorld._renderObjects)
                {
                    if (r.IsShadowCaster && r._stateRender._opacity > 0 && r.IsAffectedByLight)
                        Draw(r);
                }
            }
        }

        public static void Draw(RenderObject r)
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