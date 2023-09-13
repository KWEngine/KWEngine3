using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using System.Globalization;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererHUD
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorGlow { get; private set; } = -1;
        public static int UOffsets { get; private set; } = -1;
        public static int UOffsetCount { get; private set; } = -1;
        public static int UTextAlign { get; private set; } = -1;
        public static int UCharacterDistance { get; private set; } = -1;
        public static int UMode { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        public static int UCharacterWidth { get; private set; } = -1;
        

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.hud.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hud.frag";

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
                UCharacterDistance = GL.GetUniformLocation(ProgramID, "uCharacterDistance");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorGlow = GL.GetUniformLocation(ProgramID, "uColorGlow");
                UOffsets = GL.GetUniformLocation(ProgramID, "uOffsets");
                UOffsetCount = GL.GetUniformLocation(ProgramID, "uOffsetCount");
                UCharacterWidth = GL.GetUniformLocation(ProgramID, "uCharacterWidth");
                UTextAlign = GL.GetUniformLocation(ProgramID, "uTextAlign");
                UMode = GL.GetUniformLocation(ProgramID, "uMode"); // 0 = text, 1 = image, 2 = sliderhorizontal, etc.
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

        public static void RenderHUDObjects()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GeoMesh mesh = KWEngine.GetModel("KWQuad").Meshes.Values.ElementAt(0);
            foreach (HUDObject h in KWEngine.CurrentWorld._hudObjects)
            {
                Draw(h, mesh);
            }
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void Draw(HUDObject ho, GeoMesh mesh)
        {
            if (ho == null || !ho.IsVisible)
                return;

            GL.Uniform4(UColorTint, ho._tint);
            GL.Uniform4(UColorGlow, ho._glow);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUDNew);
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);

            if (ho is HUDObjectText)
            {
                DrawText(ho as HUDObjectText, mesh);
            }
            else if(ho is HUDObjectImage)
            {
                DrawImage(ho as HUDObjectImage, mesh);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        internal static void DrawText(HUDObjectText ho, GeoMesh mesh)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.FontTextureArray[(int)ho.Font]);
            GL.Uniform1(UTexture, 0);
            GL.Uniform1(UMode, 0);
            GL.Uniform1(UOffsets, ho._offsets.Length, ho._offsets);
            GL.Uniform1(UOffsetCount, ho._offsets.Length);
            GL.Uniform1(UTextAlign, (int)ho.TextAlignment);
            GL.Uniform1(UCharacterWidth, ho._scale.X);
            GL.Uniform1(UCharacterDistance, ho._spread);

            GL.DrawElementsInstanced(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, ho._offsets.Length);
        }

        internal static void DrawImage(HUDObjectImage ho, GeoMesh mesh)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);
            GL.Uniform1(UMode, 1);
            GL.Uniform1(UOffsets, 0, _arrayEmptyInt32);
            GL.Uniform1(UOffsetCount, 0);
            GL.Uniform1(UTextAlign, 0);
            GL.Uniform1(UCharacterWidth, ho._scale.X);
            GL.Uniform1(UCharacterDistance, 1f);

            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        internal static int[] _arrayEmptyInt32 = new int[0];
    }
}
