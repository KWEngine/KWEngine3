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
        public static int UColorOutline { get; private set; } = -1;
        public static int UScreenOffset { get; private set; } = -1;
        public static int UTextAlign { get; private set; } = -1;
        public static int UCursorInfo { get; private set; } = -1;
        public static int UMode { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;
        public static int UGlyphInfo { get; private set; } = -1;
        public static int UAdvances { get; private set; } = -1;
        public static int UCursorBounds { get; private set; } = -1;

        public static float[] _dummyCursorBounds = new float[4];
        public static float[] _dummyUVOffsets = new float[4];

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
                    vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorGlow = GL.GetUniformLocation(ProgramID, "uColorGlow");
                UUVOffsets = GL.GetUniformLocation(ProgramID, "uUVOffsetsAndWidths");
                UScreenOffset = GL.GetUniformLocation(ProgramID, "uOffset");
                UTextAlign = GL.GetUniformLocation(ProgramID, "uTextAlign");
                UOptions = GL.GetUniformLocation(ProgramID, "uOptions");
                UCursorInfo = GL.GetUniformLocation(ProgramID, "uCursorInfo");
                UColorOutline = GL.GetUniformLocation(ProgramID, "uColorOutline");
                UGlyphInfo = GL.GetUniformLocation(ProgramID, "uGlyphInfo");
                UCursorBounds = GL.GetUniformLocation(ProgramID, "uCursorBounds");
                UAdvances = GL.GetUniformLocation(ProgramID, "uAdvances");

                RenderManager.CheckShaderStatus(ProgramID, vertexShader, fragmentShader);
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

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureBuffer, ho._textureBufferTex);
            GL.Uniform1(UGlyphInfo, 1);

            GL.Uniform1(UMode, 0);
            GL.Uniform1(UOptions, 0);
            GL.Uniform1(UUVOffsets, ho._text.Length * 4, ho._uvOffsets);
            GL.Uniform1(UTextAlign, (int)ho.TextAlignment);
            GL.Uniform4(UColorOutline, ho._colorOutline);
            GL.Uniform1(UAdvances, ho._text.Length, ho._advances);
            GL.Uniform1(UScreenOffset, ho.TextAlignment == TextAlignMode.Left ? 0f : ho.TextAlignment == TextAlignMode.Center ? -ho._width * 0.5f : -ho._width);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, ho._text.Length);

            
            if(ho is HUDObjectTextInput)
            {
                HUDObjectTextInput i = (HUDObjectTextInput)ho;
                if(i.HasFocus)
                {
                    Vector4 details = GetUOffsetForGlyph(i, out Vector4 bounds);
                    _dummyCursorBounds[0] = bounds.X;
                    _dummyCursorBounds[1] = bounds.Y - i._font.Descent * 0.5f;
                    _dummyCursorBounds[2] = bounds.Z - i._font.Descent * 0.5f;
                    _dummyCursorBounds[3] = bounds.W;
                    _dummyUVOffsets[0] = details.X;
                    _dummyUVOffsets[1] = details.Y;
                    _dummyUVOffsets[2] = details.Z;
                    _dummyUVOffsets[3] = details.W;
                    GL.Uniform1(UUVOffsets, 4, _dummyUVOffsets);
                    GL.Uniform1(UCursorBounds, 4, _dummyCursorBounds);

                    GL.Uniform4(UCursorInfo, (float)i.CursorBehaviour, KWEngine.CurrentWorld.WorldTime, i.CursorBlinkSpeed, i._advanceForCursor);
                    GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 1);
                }
            }
            
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        internal static Vector4 GetUOffsetForGlyph(HUDObjectTextInput ti, out Vector4 bounds)
        {
            KWFontGlyph glyph = ti._font.GlyphDict[ti._cursorType == KeyboardCursorType.Pipe ? '|' : ti._cursorType == KeyboardCursorType.Underscore ? '_' : '·'];
            bounds = new Vector4(glyph.Left, glyph.Top, glyph.Bottom, glyph.Right);
            return new Vector4(glyph.UCoordinate.X, glyph.UCoordinate.Y, glyph.UCoordinate.Z, glyph.UCoordinate.W);
        }
    }
}

