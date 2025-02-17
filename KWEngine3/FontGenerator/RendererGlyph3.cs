using KWEngine3.Assets;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.FontGenerator
{
    internal static class RendererGlyph3
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureGlyphs { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.FontGenerator.shaderBlend.vert";
                string resourceNameFragmentShader = "KWEngine3.FontGenerator.shaderBlend.frag";

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

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);

                GL.LinkProgram(ProgramID);

                UTextureGlyphs = GL.GetUniformLocation(ProgramID, "uTextureGlyphs");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }

        public static void Draw()
        {
            // BLEND PASS
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FramebuffersGlyphs.FBGlyphsTexture);
            GL.Uniform1(UTextureGlyphs, 0);

            GL.BindVertexArray(KWQuad2D.VAOBlend);
            GL.DrawArrays(PrimitiveType.Triangles, 0, KWQuad2D.VAOBlendSize);
            GL.BindVertexArray(0);
        }
    }
}
