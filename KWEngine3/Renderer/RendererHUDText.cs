using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererHUDText
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorGlow { get; private set; } = -1;
        public static int UUVOffsets { get; private set; } = -1;
        public static int UAdvanceList { get; private set; } = -1;
        public static int UWidths { get; private set; } = -1;
        public static int UTextAlign { get; private set; } = -1;
        public static int UCursorInfo { get; private set; } = -1;
        public static int UMode { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.hudtext.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hudtext.frag";

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
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorGlow = GL.GetUniformLocation(ProgramID, "uColorGlow");
                UUVOffsets = GL.GetUniformLocation(ProgramID, "uUVOffsetsAndWidths");
                UWidths = GL.GetUniformLocation(ProgramID, "uWidths");
                UAdvanceList = GL.GetUniformLocation(ProgramID, "uAdvanceList");
                UTextAlign = GL.GetUniformLocation(ProgramID, "uTextAlign");
                UMode = GL.GetUniformLocation(ProgramID, "uMode"); // 0 = text, 1 = image, 2 = textInput, etc.
                UOptions = GL.GetUniformLocation(ProgramID, "uOptions");
                UCursorInfo = GL.GetUniformLocation(ProgramID, "uCursorInfo");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void SetGlobals()
        {
            GL.Viewport(0, 0, KWEngine.Window.ClientSize.X, KWEngine.Window.ClientSize.Y);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUD);

        }

        public static void RenderHUDObjectTexts()
        {
            
            for (int i = 0; i < KWEngine.CurrentWorld._hudObjects.Count; i++)
            {
                if(KWEngine.CurrentWorld._hudObjects[i] is HUDObjectText)
                    Draw(KWEngine.CurrentWorld._hudObjects[i] as HUDObjectText);
            }
        }

        public static void Draw(HUDObjectText ho)
        {
            if (ho == null || !ho.IsVisible || !ho.IsInsideScreenSpace())
                return;

            GL.Uniform4(UColorTint, ho._tint);
            GL.Uniform4(UColorGlow, ho._glow);
            GL.Uniform1(UOptions, 0);
            GL.Uniform3(UCursorInfo, 0f, 0f, 0f);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            GL.BindVertexArray(KWQuad2D_05.VAO);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._font.Texture);
            GL.Uniform1(UTexture, 0);

            GL.Uniform1(UMode, 0);
            GL.Uniform1(UOptions, 0);
            GL.Uniform2(UUVOffsets, ho._text.Length, ho._uvOffsets);
            GL.Uniform1(UAdvanceList, ho._text.Length, ho._advances);
            GL.Uniform1(UWidths, ho._text.Length, ho._glyphWidths);
            GL.Uniform1(UTextAlign, (int)ho.TextAlignment);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, ho._text.Length);

            /*
            if(ho is HUDObjectTextInput)
            {
                HUDObjectTextInput i = (HUDObjectTextInput)ho;
                if(i.HasFocus && i._offsetsCursor != null)
                {
                    GL.Uniform1(UMode, i.CursorType == KeyboardCursorType.Pipe ? 3 : i.CursorType == KeyboardCursorType.Underscore ? 4 : 5);
                    GL.Uniform3(UCursorInfo, (float)i.CursorBehaviour, KWEngine.CurrentWorld.WorldTime, i.CursorBlinkSpeed);
                    GL.Uniform1(UOffsetsAndWidths, i._offsetsCursor.Length, i._offsetsCursor);
                    GL.Uniform1(UOffsetCount, i._offsetsCursor.Length);
                    GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, i._offsetsCursor.Length);
                }
            }
            */

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        

        internal static void DrawText(HUDObjectText ho)
        {
            
        }
    }
}

