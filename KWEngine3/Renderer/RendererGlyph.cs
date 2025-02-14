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
        //public static int UColorTint { get; private set; } = -1;
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
            UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjection");
            //UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
            
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderHUDObjectTextInstances(List<HUDObjectText> objects)
        {
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            foreach (HUDObjectText txt in objects)
            {
                Draw(txt);
            }

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.CullFace);
        }

        public static void Draw(HUDObjectText text)
        {
            Bind();

            Vector2 offset = text._hitboxOffsetUnscaled;

            foreach (char c in text._text)
            {
                KWFontGlyph g = text._currentFont.GetGlyphForCodepoint(c);

                // Draw step #1:
                Bind();
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUDOffCenter);
                GL.UniformMatrix4(UModelExternal, false, ref text._modelMatrix);
                GL.Uniform2(UModelInternal, offset);
                GL.BindVertexArray(g.VAO_Step1);
                GL.DrawArrays(PrimitiveType.Triangles, 0, g.VertexCount_Step1);
                
                // Draw step #2:
                RendererGlyph2.Bind();
                GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUDOffCenter);
                GL.UniformMatrix4(UModelExternal, false, ref text._modelMatrix);
                GL.Uniform2(UModelInternal, offset);
                GL.BindVertexArray(g.VAO_Step2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, g.VertexCount_Step2);
                

                offset += new Vector2(g.Advance.X * text.CharacterDistanceFactor, g.Advance.Y);
            }
            GL.BindVertexArray(0);
        }
    }
}