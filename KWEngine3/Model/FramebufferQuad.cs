using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.ShadowMapping
{
    internal class FramebufferQuad
    {
        private static int _vao = -1;

        private static readonly float[] _vertices = new float[]
        {
            -1f, -1f, 0,
            +1f, -1f, 0,
            -1f, +1f, 0,

            +1f, -1f, 0,
            +1f, +1f, 0,
            -1f, +1f, 0

        };

        private static readonly float[] _uvs = new float[]
        {
           0, 0,
           1, 0,
           0, 1,

           1, 0,
           1, 1,
           0, 1
        };

        public static void Init()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            int vboVertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 4, _vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int vboUVs = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboUVs);
            GL.BufferData(BufferTarget.ArrayBuffer, _uvs.Length * 4, _uvs, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }

        public static int GetVAOId()
        {
            return _vao;
        }

        public static int GetVertexCount()
        {
            return 6;
        }
    }
}
