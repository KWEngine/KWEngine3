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

        public static void RenderParticles(List<TimeBasedObject> objects)
        {
            GL.Enable(EnableCap.Blend);
            foreach (TimeBasedObject tbo in objects)
            {
                if (tbo is ParticleObject)
                {
                    ParticleObject po = (ParticleObject)tbo;
                    Draw(po);
                }
            }
            GL.Disable(EnableCap.Blend);
        }

        public static void Draw(ParticleObject po)
        {
            
                GL.Uniform4(UColorTint, ref po._tint);
                Matrix4 mvp = po._modelMatrix * (KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix);
                GL.UniformMatrix4(UModelViewProjectionMatrix, false, ref mvp);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, po._info.Texture);
                GL.Uniform1(UTexture, 0);

                GL.Uniform1(UAnimationState, po._frame);
                GL.Uniform1(UAnimationStates, po._info.Images);

                GeoMesh mesh = po._model.Meshes.Values.ElementAt(0);
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindVertexArray(0);
        }
    }
}