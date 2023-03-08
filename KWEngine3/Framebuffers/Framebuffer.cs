using KWEngine3.Exceptions;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Framebuffers
{
    internal abstract class Framebuffer
    {
        internal static int _fbShadowMapCounter = 0;
        public int ID { get; set; } = -1;
        public LightType _lightType = LightType.Point;
        public bool _isLight = false;

        public static int ShadowMapCount { get { return _fbShadowMapCounter; } }
        public List<FramebufferTexture> Attachments { get; set; } = new List<FramebufferTexture>();
        public List<int> Renderbuffers { get; set; } = new List<int>();

        public Framebuffer(int width, int height, bool isLight, LightType lightType)
        {
            ID = GL.GenFramebuffer();
            _isLight = isLight;
            _lightType = lightType;
            Init(width, height);
        }
        
        public Dictionary<int, float[]> ClearColorValues = new Dictionary<int, float[]>();
        public Dictionary<int, float[]> ClearDepthValues = new Dictionary<int, float[]>();

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind(bool clear = true)
        {
            if (ID > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
                if(clear) Clear();
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

            GL.DeleteRenderbuffers(Renderbuffers.Count, Renderbuffers.ToArray());
            Renderbuffers.Clear();

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

        public abstract void Clear();
    }
}
