﻿using KWEngine3.FontGenerator;
using KWEngine3.Framebuffers;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererCopy
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureAlbedo { get; private set; } = -1;
        public static int UTextureBloom { get; private set; } = -1;
        public static int UFadeColor { get; private set; } = -1;

        public static int UId { get; private set; } = -1;
        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.copy.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.copy.frag";

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
                UTextureAlbedo = GL.GetUniformLocation(ProgramID, "uTextureAlbedo");
                UTextureBloom = GL.GetUniformLocation(ProgramID, "uTextureBloom");
                UFadeColor = GL.GetUniformLocation(ProgramID, "uFadeColor");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            // ?
        }

        public static void Draw(Framebuffer fbSource, Framebuffer fbSourceBloom, Vector4 fadeColor) // currently from lighting pass
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fbSource.Attachments[0].ID);
            //GL.BindTexture(TextureTarget.Texture2D, KWEngine.TestFont.Texture);
            GL.Uniform1(UTextureAlbedo, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            if (fbSourceBloom != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, fbSourceBloom.Attachments[0].ID);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
            }
            GL.Uniform1(UTextureAlbedo, 1);

            GL.Uniform4(UFadeColor, ref fadeColor);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
