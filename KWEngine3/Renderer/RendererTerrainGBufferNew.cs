﻿using KWEngine3.Assets;
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
        public static int UTextureHeight { get; private set; } = -1;
        public static int UTextureTransform { get; private set; } = -1;
        public static int UPOMScale { get; private set; } = -1;
        

        // SLOPE
        public static int UUseTexturesAlbedoNormalEmissiveSlope { get; private set; } = -1;
        public static int UUseTexturesMetallicRoughnessSlope { get; private set; } = -1;
        public static int UTextureNormalSlope { get; private set; } = -1;
        public static int UTextureAlbedoSlope { get; private set; } = -1;
        public static int UTextureMetallicSlope { get; private set; } = -1;
        public static int UTextureRoughnessSlope { get; private set; } = -1;
        public static int UTextureEmissiveSlope { get; private set; } = -1;
        public static int UTextureTransformSlope { get; private set; } = -1;

        public static int UIdShadowCaster { get; private set; } = -1;

        public static int UTerrainData { get; private set; } = -1;
        public static int UTerrainThreshold { get; private set; } = -1;
        public static int UCamPosition { get; private set; } = -1;
        public static int UCamDirection { get; private set; } = -1;
        public static int UTextureHeightMap { get; private set; } = -1;

        private const int TEXTUREOFFSET = 0;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.terrain.gbuffer.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.terrain.gbuffer.frag";
                string resourceNameTCShader = "KWEngine3.Shaders.shader.terrain.gbuffer.tesc";
                string resourceNameTEShader = "KWEngine3.Shaders.shader.terrain.gbuffer.tese";

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
                
                UIdShadowCaster = GL.GetUniformLocation(ProgramID, "uIdShadowCaster");

                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UNormalMatrix = GL.GetUniformLocation(ProgramID, "uNormalMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");

                UCamDirection = GL.GetUniformLocation(ProgramID, "uCamDirection");
                UCamPosition = GL.GetUniformLocation(ProgramID, "uCamPosition");

                UUseTexturesAlbedoNormalEmissive = GL.GetUniformLocation(ProgramID, "uUseTexturesAlbedoNormalEmissive");
                UUseTexturesMetallicRoughness = GL.GetUniformLocation(ProgramID, "uUseTexturesMetallicRoughness");
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureNormal = GL.GetUniformLocation(ProgramID, "uTextureNormal");
                UTextureMetallic = GL.GetUniformLocation(ProgramID, "uTextureMetallic");
                UTextureRoughness = GL.GetUniformLocation(ProgramID, "uTextureRoughness");
                UTextureEmissive = GL.GetUniformLocation(ProgramID, "uTextureEmissive");
                UTextureHeight = GL.GetUniformLocation(ProgramID, "uTextureHeight");
                UTextureTransform = GL.GetUniformLocation(ProgramID, "uTextureTransform");
                UPOMScale = GL.GetUniformLocation(ProgramID, "uPOMScale");

                UUseTexturesAlbedoNormalEmissiveSlope = GL.GetUniformLocation(ProgramID, "uUseTexturesAlbedoNormalEmissiveSlope");
                UUseTexturesMetallicRoughnessSlope = GL.GetUniformLocation(ProgramID, "uUseTexturesMetallicRoughnessSlope");
                UTextureAlbedoSlope = GL.GetUniformLocation(ProgramID, "uTextureAlbedoSlope");
                UTextureNormalSlope = GL.GetUniformLocation(ProgramID, "uTextureNormalSlope");
                UTextureMetallicSlope = GL.GetUniformLocation(ProgramID, "uTextureMetallicSlope");
                UTextureRoughnessSlope = GL.GetUniformLocation(ProgramID, "uTextureRoughnessSlope");
                UTextureEmissiveSlope = GL.GetUniformLocation(ProgramID, "uTextureEmissiveSlope");
                UTextureTransformSlope = GL.GetUniformLocation(ProgramID, "uTextureTransformSlope");

                UTextureHeightMap = GL.GetUniformLocation(ProgramID, "uTextureHeightMap");
                UTerrainData = GL.GetUniformLocation(ProgramID, "uTerrainData");
                UTerrainThreshold = GL.GetUniformLocation(ProgramID, "uTerrainThreshold");
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
                    if (t.IsInsideScreenSpaceForRenderPass && t.IsVisible)
                    {
                        Draw(t);
                    }
                }
            }
        }

        public static void Draw(TerrainObject t)
        {
            GL.Uniform1(UPOMScale, KWEngine.Window._renderQuality >= RenderQualityLevel.Default ? t._pomScale : 0f);
            GL.Uniform3(UColorTint, t._stateRender._colorTint);
            GL.Uniform4(UColorEmissive, t._stateRender._colorEmissive);
            GL.Uniform2(UIdShadowCaster, new Vector2i(t.ID, t.IsShadowCaster ? 1 : 0));
            GL.Uniform3(UMetallicRoughness, new Vector3(t._gModel._metallicTerrain, t._gModel._roughnessTerrain, Convert.ToSingle((int)t._gModel._metallicType)));
            GeoMesh mesh = t._gModel.ModelOriginal.Meshes.Values.ToArray()[0];

            GeoMaterial material = t._gModel.Material[0]; 
            GeoMaterial materialSlope = t._gModel.Material[1];

            GL.UniformMatrix4(UModelMatrix, false, ref t._stateRender._modelMatrix);
            GL.UniformMatrix4(UNormalMatrix, false, ref t._stateRender._normalMatrix);
            
            GL.Uniform3(UColorMaterial, material.ColorAlbedo.Xyz);


            // REGULAR TEXTURES:
            Vector3i useTexturesAlbedoNormalEmissive = new Vector3i(
                material.TextureAlbedo.IsTextureSet ? 1 : 0,
                material.TextureNormal.IsTextureSet ? 1 : 0,
                material.TextureEmissive.IsTextureSet ? 1 : 0
                );
            Vector3i useTexturesMetallicRoughness = new Vector3i(
                material.TextureMetallic.IsTextureSet ? 1 : 0,
                material.TextureRoughness.IsTextureSet ? 1 : 0,
                (int)(t._textureSlopeSmoothingFactor * 1000)
                );
            GL.Uniform3(UUseTexturesAlbedoNormalEmissive, useTexturesAlbedoNormalEmissive);
            GL.Uniform3(UUseTexturesMetallicRoughness, useTexturesMetallicRoughness);

            // SLOPE TEXTURES:
            Vector3i useTexturesAlbedoNormalEmissiveSlope = new Vector3i(
                materialSlope.TextureAlbedo.IsTextureSet ? 1 : 0,
                materialSlope.TextureNormal.IsTextureSet ? 1 : 0,
                materialSlope.TextureEmissive.IsTextureSet ? 1 : 0
                );
            Vector3i useTexturesMetallicRoughnessSlope = new Vector3i(
                materialSlope.TextureMetallic.IsTextureSet ? 1 : 0,
                materialSlope.TextureRoughness.IsTextureSet ? 1 : 0,
                (int)(t._textureSlopeBlendFactor * 1000)
                );
            GL.Uniform3(UUseTexturesAlbedoNormalEmissiveSlope, useTexturesAlbedoNormalEmissiveSlope);
            GL.Uniform3(UUseTexturesMetallicRoughnessSlope, useTexturesMetallicRoughnessSlope);


            GL.Uniform3(UCamPosition, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position);
            GL.Uniform3(UCamDirection, KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector);
            GL.Uniform1(UTerrainThreshold, (int)t.TessellationThreshold);
            GL.Uniform4(UTerrainData, t.Width, t.Depth, KWEngine.TERRAIN_PATCH_SIZE, t.Height);

            // UPLOAD TEXTURES:
            UploadTextures(ref material, ref materialSlope, t);

            GL.BindVertexArray(KWTerrainQuad.VAO);
            GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 4, (t.Width * t.Depth) / (KWEngine.TERRAIN_PATCH_SIZE * KWEngine.TERRAIN_PATCH_SIZE));
            GL.BindVertexArray(0);
        }
        private static void UploadTextures(ref GeoMaterial material, ref GeoMaterial materialSlope, TerrainObject t)
        {
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureAlbedo.IsTextureSet ? material.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedo, TEXTUREOFFSET);
            if (material.TextureAlbedo.IsTextureSet)
            {
                GL.Uniform4(UTextureTransform, new Vector4(
                    material.TextureAlbedo.UVTransform.X * t._stateRender._uvTransform.X,
                    material.TextureAlbedo.UVTransform.Y * t._stateRender._uvTransform.Y,
                    material.TextureAlbedo.UVTransform.Z + t._stateRender._uvTransform.Z,
                    material.TextureAlbedo.UVTransform.W + t._stateRender._uvTransform.W));
            }
            else
                GL.Uniform4(UTextureTransform, new Vector4(1, 1, 0, 0));

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

            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 5);
            GL.BindTexture(TextureTarget.Texture2D, t._heightmap > 0 ? t._heightmap : KWEngine.TextureBlack);
            GL.Uniform1(UTextureHeightMap, TEXTUREOFFSET + 5);

            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 6);
            GL.BindTexture(TextureTarget.Texture2D, material.TextureHeight.IsTextureSet ? material.TextureHeight.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureHeight, TEXTUREOFFSET + 6);


            // SLOPE TEXTURES
            // Albedo
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 7);
            GL.BindTexture(TextureTarget.Texture2D, materialSlope.TextureAlbedo.IsTextureSet ? materialSlope.TextureAlbedo.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureAlbedoSlope, TEXTUREOFFSET + 7);
            if (materialSlope.TextureAlbedo.IsTextureSet)
            {
                GL.Uniform4(UTextureTransformSlope, new Vector4(
                    materialSlope.TextureAlbedo.UVTransform.X * t._stateRender._uvTransformSlope.X,
                    materialSlope.TextureAlbedo.UVTransform.Y * t._stateRender._uvTransformSlope.Y,
                    materialSlope.TextureAlbedo.UVTransform.Z + t._stateRender._uvTransformSlope.Z,
                    materialSlope.TextureAlbedo.UVTransform.W + t._stateRender._uvTransformSlope.W));
            }
            else
                GL.Uniform4(UTextureTransformSlope, new Vector4(1, 1, 0, 0));

            // Normal
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 8);
            GL.BindTexture(TextureTarget.Texture2D, materialSlope.TextureNormal.IsTextureSet ? materialSlope.TextureNormal.OpenGLID : KWEngine.TextureNormalEmpty);
            GL.Uniform1(UTextureNormalSlope, TEXTUREOFFSET + 8);

            // Emissive
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 9);
            GL.BindTexture(TextureTarget.Texture2D, materialSlope.TextureEmissive.IsTextureSet ? materialSlope.TextureEmissive.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureEmissiveSlope, TEXTUREOFFSET + 9);

            // Metallic
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 10);
            GL.BindTexture(TextureTarget.Texture2D, materialSlope.TextureMetallic.IsTextureSet ? materialSlope.TextureMetallic.OpenGLID : KWEngine.TextureBlack);
            GL.Uniform1(UTextureMetallicSlope, TEXTUREOFFSET + 10);

            // Roughness
            GL.ActiveTexture(TextureUnit.Texture0 + TEXTUREOFFSET + 11);
            GL.BindTexture(TextureTarget.Texture2D, materialSlope.TextureRoughness.IsTextureSet ? materialSlope.TextureRoughness.OpenGLID : KWEngine.TextureWhite);
            GL.Uniform1(UTextureRoughnessSlope, TEXTUREOFFSET + 11);
        }
    }
}
