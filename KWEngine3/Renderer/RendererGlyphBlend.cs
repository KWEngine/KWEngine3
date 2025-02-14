using OpenTK.Graphics.OpenGL4;
using System.Reflection;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using KWEngine3.Model;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using KWEngine3.Assets;


namespace KWEngine3.Renderer
{
    internal static class RendererGlyphBlend
    {
        public static int UTexture { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UColor { get; private set; } = -1;
        public static int UBloom { get; private set; } = -1;

        public static int ProgramID { get; private set; } = -1;
        public static void Init()
        {
            ProgramID = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hud_glyph_blend.frag";
            string resourceNameVertexShader = "KWEngine3.Shaders.shader.hud_glyph_blend.vert";
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
            HelperGeneral.CheckGLErrors();
            UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
            UModelMatrix = GL.GetUniformLocation(ProgramID, "uModel");
            UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjection");
            UColor = GL.GetUniformLocation(ProgramID, "uColor");
            UBloom = GL.GetUniformLocation(ProgramID, "uBloom");
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void Draw()
        {
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            //GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUDOffCenter);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RenderManager.FramebufferGlyphs.Attachments[0].ID);
            GL.Uniform1(UTexture, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());

            /*
            foreach (HUDObjectText t in KWEngine.CurrentWorld._hudObjectsText)
            {
                Matrix4 model = HelperMatrix.CreateModelMatrixForHUDText(new Vector3(t.Width, t.Height, 1f), new Vector3(t.Position.X, t.Position.Y, 0f));
                GL.UniformMatrix4(UModelMatrix, false, ref model);

                GL.Uniform4(UColor, new Vector4(t.Color, t.Opacity));
                GL.Uniform4(UBloom, new Vector4(t.ColorEmissive, t.ColorEmissiveIntensity));
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, RenderManager.FramebufferGlyphs.Attachments[0].ID);
                GL.Uniform1(UTexture, 0);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
            */

            GL.BindVertexArray(0);
        }
    }
}