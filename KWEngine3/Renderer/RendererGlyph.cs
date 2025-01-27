using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using KWEngine3.GameObjects;
using KWEngine3.Model;
using KWEngine3;

namespace KWEngine3.Renderer
{
    internal static class RendererGlyph 
    {
        public static int UModelInternal { get; private set; } = -1;
        public static int UModelExternal { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int ProgramID { get; private set; } = -1;
        public static void Init()
        {
            ProgramID = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hud_glyph.frag";
            string resourceNameVertexShader = "KWEngine3.Shaders.shader.hud_glyph.vert";
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


            UModelInternal = GL.GetUniformLocation(ProgramID, "uModelInternal");
            UModelExternal = GL.GetUniformLocation(ProgramID, "uModelExternal");
            UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
            UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
            
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderHUDObjectTextInstances(List<HUDObjectText> objects)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            foreach (HUDObjectText txt in objects)
            {
                Draw(txt);
            }
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Blend);
        }

        public static void Draw(HUDObjectText text)
        {
            
            GL.Uniform4(UColorTint, ref text._tint);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUD);

            foreach(char c in text._text)
            {
                //GL.BindVertexArray(...);
                //GL.DrawArrays(...);

               
            }
            GL.BindVertexArray(0);
        }
    }
}