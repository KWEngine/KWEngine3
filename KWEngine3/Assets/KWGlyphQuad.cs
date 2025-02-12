using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Assets
{
    internal class KWGlyphQuad
    {
        public static float[] _vertices;
        public static float[] _uvs;

        public static int VAO;
        public static void Init()
        {
            _vertices = new float[]
            {
                -0.5f, -0.5f, 0f,
                +0.5f, -0.5f, 0f,
                -0.5f, +0.5f, 0f,
                              
                -0.5f, +0.5f, 0f,
                +0.5f, -0.5f, 0f,
                +0.5f, +0.5f, 0f
            };

            _uvs = new float[]
            {
                0, 0,
                1, 0,
                0, 1,

                0, 1,
                1, 0,
                1, 1
            };

          

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // position
            int vbo_vertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vertices);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 4, _vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // uvs
            int vbo_texture = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_texture);
            GL.BufferData(BufferTarget.ArrayBuffer, _uvs.Length * 4, _uvs, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }
    }
}
