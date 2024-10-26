using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System.Linq;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererDebugCube
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UTextureDepth { get; private set; } = -1;
        public static int UViewProjectionMatrixInverted { get; private set; } = -1;
        public static int UCameraPosition { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.debugcube.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.debugcube.frag";

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

                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UTextureDepth = GL.GetUniformLocation(ProgramID, "uTextureDepth");
                UCameraPosition = GL.GetUniformLocation(ProgramID, "uCameraPosition");
                UViewProjectionMatrixInverted = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrixInverted");
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
            // 7 = SM1
            // 8 = SM2
            // 9 = SM3
            int val = 0;
            int attachmentID = -1;
            int mode = (int)KWEngine.DebugMode;

            if((int)KWEngine.DebugMode >= 7 && (int)KWEngine.DebugMode <= 9)
            {
                if (KWEngine.DebugMode == DebugMode.DepthBufferShadowMap1)
                {
                    if(maps.Count >= 1)
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

            if(attachmentID < 0)
            {
                return;
            }
           
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, attachmentID);
            GL.Uniform1(UTexture, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RenderManager.FramebufferDeferred.Attachments[4].ID);
            GL.Uniform1(UTextureDepth, 1);

            Matrix4 vp = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix;
            vp.Invert();
            GL.UniformMatrix4(UViewProjectionMatrixInverted, false, ref vp);

            GL.Uniform3(UCameraPosition, KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender._position : KWEngine.CurrentWorld._cameraGame._stateRender._position);
            
            GL.BindVertexArray(FramebufferQuad.GetVAOId());
            GL.DrawArrays(PrimitiveType.Triangles, 0, FramebufferQuad.GetVertexCount());
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

    }
}
