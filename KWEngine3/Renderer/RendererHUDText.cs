using KWEngine3.Assets;
using KWEngine3.FontGenerator;
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
        public static int UScreenOffset { get; private set; } = -1;
        public static int UTextAlign { get; private set; } = -1;
        public static int UCursorInfo { get; private set; } = -1;
        public static int UMode { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;

        public static float[] _dummyWidthAndAdvance = new float[2];
        public static float[] _dummyOffset = new float[3];

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
                UScreenOffset = GL.GetUniformLocation(ProgramID, "uOffset");
                UAdvanceList = GL.GetUniformLocation(ProgramID, "uAdvanceList");
                UTextAlign = GL.GetUniformLocation(ProgramID, "uTextAlign");
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
            KWEngine.Window.SetGLViewportToClientSize();
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUD);
        }

        public static void RenderHUDObjectTexts()
        {
            GL.BindVertexArray(KWQuad2D_05.VAO);
            for (int i = 0; i < KWEngine.CurrentWorld._hudObjects.Count; i++)
            {
                if(KWEngine.CurrentWorld._hudObjects[i] is HUDObjectText)
                    Draw(KWEngine.CurrentWorld._hudObjects[i] as HUDObjectText);
            }
            GL.BindVertexArray(0);
        }

        public static void Draw(HUDObjectText ho)
        {
            if (ho == null || !ho.IsVisible || !ho.IsInsideScreenSpace())
                return;

            GL.Uniform4(UColorTint, ho._tint);
            GL.Uniform4(UColorGlow, ho._glow);
            GL.Uniform4(UCursorInfo, 0f, 0f, 0f, 0f);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._font.Texture);
            GL.Uniform1(UTexture, 0);

            GL.Uniform1(UMode, 0);
            GL.Uniform1(UOptions, 0);
            GL.Uniform3(UUVOffsets, ho._text.Length, ho._uvOffsets);
            GL.Uniform1(UAdvanceList, ho._text.Length, ho._advances);
            GL.Uniform1(UWidths, ho._text.Length, ho._glyphWidths);
            GL.Uniform1(UTextAlign, (int)ho.TextAlignment);
            GL.Uniform1(UScreenOffset, ho.TextAlignment == TextAlignMode.Left ? 0f : ho.TextAlignment == TextAlignMode.Center ? -ho._width * 0.5f : -ho._width);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, ho._text.Length);

            
            if(ho is HUDObjectTextInput)
            {
                HUDObjectTextInput i = (HUDObjectTextInput)ho;
                if(i.HasFocus)
                {
                    Vector4 details = GetUOffsetForGlyph(i, out float vOffset);
                    _dummyWidthAndAdvance[0] = details.Z;
                    _dummyWidthAndAdvance[1] = i._advanceForCursor;
                    _dummyOffset[0] = details.X; 
                    _dummyOffset[1] = details.Y;
                    _dummyOffset[2] = vOffset;
                    GL.Uniform3(UUVOffsets, 1, _dummyOffset);
                    GL.Uniform1(UWidths, 2, _dummyWidthAndAdvance);
                    GL.Uniform4(UCursorInfo, (float)i.CursorBehaviour, KWEngine.CurrentWorld.WorldTime, i.CursorBlinkSpeed, (float)i.CursorPosition);
                    GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 1);
                }
            }
            
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        internal static Vector4 GetUOffsetForGlyph(HUDObjectTextInput ti, out float vOffset)
        {
            KWFontGlyph glyph = ti._font.GlyphDict[ti._cursorType == KeyboardCursorType.Pipe ? '|' : ti._cursorType == KeyboardCursorType.Underscore ? '_' : '·'];
            vOffset = glyph.UCoordinate.Z;
            return new Vector4(glyph.UCoordinate.X, glyph.UCoordinate.Y, glyph.Width, ti._advances[ti.CursorPosition]);
        }
    }
}

