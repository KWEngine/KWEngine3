using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererShadowMapBlur
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTextureInput { get; private set; } = -1;
        public static int UAxis { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.msm16.blur.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.msm16.blur.frag";

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
                UTextureInput = GL.GetUniformLocation(ProgramID, "uTextureInput");
                UAxis = GL.GetUniformLocation(ProgramID, "uAxis");

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

        public static void Draw(LightObject light)
        {
            if (light.Type == LightType.Point)
                return;

            // horizontal pass:
            light._fbShadowMap._blurBuffer1.Bind(true);
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.Viewport(0, 0, light._shadowMapSize, light._shadowMapSize);
            GL.Disable(EnableCap.DepthTest);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, light._fbShadowMap.Attachments[0].ID); // original shadow map scene texture
            GL.Uniform1(UTextureInput, 0);
            GL.Uniform1(UAxis, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            
            //vertical pass:
            light._fbShadowMap._blurBuffer2.Bind(true);
            GL.Viewport(0, 0, light._shadowMapSize, light._shadowMapSize);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, light._fbShadowMap._blurBuffer1.Attachments[0].ID); // horizontally blurred shadow map as input
            GL.Uniform1(UTextureInput, 0);
            GL.Uniform1(UAxis, 1);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            light._fbShadowMap._blurBuffer2.Unbind();
            
            // Generate MipMap:
            GL.BindTexture(TextureTarget.Texture2D, light._fbShadowMap._blurBuffer2.Attachments[0].ID);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
            // clear bindings:
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }

    }
}
