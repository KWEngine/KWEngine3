using KWEngine3.Exceptions;
using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace KWEngine3.Framebuffers
{
    internal abstract class Framebuffer
    {
        internal static int _fbShadowMapCounter = 0;
        public int ID { get; set; } = -1;
        public LightType _lightType = LightType.Point;
        public SunShadowType _shadowType = SunShadowType.Default;
        public bool _isLight = false;
        public Vector3 _clearColor = Vector3.Zero;
        public int SizeInBytes = 0;

        public static int ShadowMapCount { get { return _fbShadowMapCounter; } }
        public List<FramebufferTexture> Attachments { get; set; } = new List<FramebufferTexture>();
        //public List<int> Renderbuffers { get; set; } = new List<int>();

        public Framebuffer(int width, int height, bool isLight, LightType lightType, SunShadowType shadowType = SunShadowType.Default)
        {
            ID = GL.GenFramebuffer();
            _isLight = isLight;
            _lightType = lightType;
            _shadowType = shadowType;
            Init(width, height);
        }
        
        public Dictionary<int, float[]> ClearColorValues = new Dictionary<int, float[]>();
        //public Dictionary<int, float[]> ClearDepthValues = new Dictionary<int, float[]>();

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind(bool clear = true, bool keepDepth = false)
        {
            if (ID > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
                if(clear) Clear(keepDepth);
            }
            else
                throw new EngineException("Framebuffer::Bind() -> Framebuffer ID invalid.");
        }

        public abstract void Init(int width, int height);

        public void Dispose()
        {
            foreach (FramebufferTexture attachment in Attachments)
            {
                attachment.Dispose();
            }

            //GL.DeleteRenderbuffers(Renderbuffers.Count, Renderbuffers.ToArray());
            //Renderbuffers.Clear();

            Attachments.Clear();
            GL.DeleteFramebuffers(1, new int[] { ID });
            ID = -1;
        }

        public static void UpdateGlobalShadowMapCounter(bool increase)
        {
            if (increase)
                _fbShadowMapCounter++;
            else
                _fbShadowMapCounter--;
        }

        public bool CheckComplete()
        {
            FramebufferErrorCode error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            return error == FramebufferErrorCode.FramebufferComplete;
        }

        public void CopyDepthFrom(Framebuffer src) // usually: src = deferred
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, src.ID);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, ID);

            GL.BlitFramebuffer(
                0,
                0,
                KWEngine.Window.ClientRectangle.Size.X,
                KWEngine.Window.ClientRectangle.Size.Y,
                0,
                0,
                KWEngine.Window.ClientRectangle.Size.X,
                KWEngine.Window.ClientRectangle.Size.Y,
                ClearBufferMask.DepthBufferBit,
                BlitFramebufferFilter.Nearest
                );
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public abstract void Clear(bool keepDepth = false);
    }
}
