using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Linq;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererDebug
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UTextureCube { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;
        public static int UCameraLookAtVector { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.debug.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.debug.frag";

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
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UOptions = GL.GetUniformLocation(ProgramID, "uOptions");
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

        public static void Draw(List<FramebufferShadowMap> maps)
        {
            // Debug Modes:
            // 1 = Depth
            // 2 = Color
            // 3 = Normals
            // 4 = SSAO
            // 5 = Bloom
            // 6 = MetallicRoughness

            int val = 0;
            int attachmentID = -1;
            int mode = (int)KWEngine.DebugMode;

            if (mode == 1)
            {
                attachmentID = RenderManager.FramebufferDeferred.Attachments[4].ID;
            }
            else if(mode == 2)
            {
                attachmentID = RenderManager.FramebufferDeferred.Attachments[0].ID;
            }
            else if(mode == 3)
            {
                attachmentID = RenderManager.FramebufferDeferred.Attachments[1].ID;
            }
            else if(mode == 4)
            {
                attachmentID = KWEngine.SSAO_Enabled ? RenderManager.FramebufferSSAOBlur.Attachments[0].ID : KWEngine.TextureWhite;
            }
            else if(mode == 5)
            {
                attachmentID = RenderManager.FramebuffersBloomTemp[0].Attachments[0].ID;
            }
            else if(mode == 6)
            {
                attachmentID = RenderManager.FramebufferDeferred.Attachments[2].ID;
            }
            else if ((int)KWEngine.DebugMode >= 7 && (int)KWEngine.DebugMode <= 9)
            {
                if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap1)
                {
                    if (maps.Count >= 1)
                    {
                        attachmentID = maps[0].Attachments[0].ID;
                    }
                }
                else if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap2)
                {
                    if (maps.Count >= 2)
                    {
                        attachmentID = maps[1].Attachments[0].ID;
                    }
                }
                else if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap2)
                {
                    if (maps.Count >= 3)
                    {
                        attachmentID = maps[2].Attachments[0].ID;
                    }
                }
            }
            else
            {
                return;
            }

            if (attachmentID <= 0)
            {
                return;
            }
            GL.Uniform4(UOptions, 
                (int)KWEngine.DebugMode, 
                val, 
                (int)(KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._zNear * 100 : KWEngine.CurrentWorld._cameraGame._zNear * 100),
                (int)(KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._zFar * 100 : KWEngine.CurrentWorld._cameraGame._zFar * 100));


            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, attachmentID);
            GL.Uniform1(UTexture, 0);

            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            HelperGeneral.CheckGLErrors();
        }

    }
}
