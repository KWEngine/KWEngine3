using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using KWEngine3.Assets;

namespace KWEngine3.Renderer
{
    internal static class RendererParticle 
    {
        public static int UAnimationState { get; private set; } = -1;
        public static int UAnimationStates { get; private set; } = -1;
        public static int UModelViewProjectionMatrix { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int ProgramID { get; private set; } = -1;
        public static void Init()
        {
            ProgramID = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine3.Shaders.shader.forward.particle.frag";
            string resourceNameVertexShader = "KWEngine3.Shaders.shader.forward.particle.vert";
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

            
            UModelViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uModelViewProjectionMatrix");

            // Textures:
            UTexture = GL.GetUniformLocation(ProgramID, "uTexture");

            UAnimationState = GL.GetUniformLocation(ProgramID, "uAnimationState");
            UAnimationStates = GL.GetUniformLocation(ProgramID, "uAnimationStates");
            UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        public static void RenderParticles(List<TimeBasedObject> objects)
        {
            GL.Enable(EnableCap.Blend);
            GL.BindVertexArray(KWGlyphQuad.VAO);
            foreach (TimeBasedObject tbo in objects)
            {
                if (tbo is ParticleObject)
                {
                    ParticleObject po = (ParticleObject)tbo;
                    Draw(po);
                }
            }
            GL.BindVertexArray(0);
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

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}