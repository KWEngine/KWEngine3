using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererHUD
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UModelViewProjectionMatrix { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorGlow { get; private set; } = -1;
        public static int UOffset { get; private set; } = -1;
        public static int UIsText { get; private set; } = -1;

        public static int UId { get; private set; } = -1;
        

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
                UModelViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uModelViewProjectionMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorGlow = GL.GetUniformLocation(ProgramID, "uColorGlow");
                UOffset = GL.GetUniformLocation(ProgramID, "uOffset");
                UIsText = GL.GetUniformLocation(ProgramID, "uIsText");
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
            GeoMesh mesh = KWEngine.GetModel("KWQuad").Meshes.Values.ElementAt(0);
            foreach (HUDObject h in KWEngine.CurrentWorld._hudObjects)
            {
                Draw(h, ref mesh);
            }
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        public static void Draw(HUDObject ho, ref GeoMesh mesh)
        {
            if (!ho.IsVisible)
                return;

            GL.Uniform4(UColorTint, ho._tint);
            GL.Uniform4(UColorGlow, ho._glow);

            GL.ActiveTexture(TextureUnit.Texture0);
            if (ho._type == HUDObjectType.Text)
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.FontTextureArray[(int)ho.Font]);
            else
                GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);


            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            for (int i = 0; i < ho._positions.Count; i++)
            {
                Matrix4 mvp = ho._modelMatrices[i] * KWEngine.Window._viewProjectionMatrixHUD;
                GL.UniformMatrix4(UModelViewProjectionMatrix, false, ref mvp);
                GL.Uniform1(UOffset, ho._offsets[i]);
                GL.Uniform1(UIsText, ho._type == HUDObjectType.Text ? 1 : 0);
                GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

    }
}
