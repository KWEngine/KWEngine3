using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Assets
{
    internal static class KWTerrainQuad
    {
        public static float[] _vertices;
        public static float[] _normals;
        public static float[] _tangents;
        public static float[] _bitangents;
        public static float[] _uvs;

        public static int VAO;

        private static float multiplier = 10f;

        public static void Init()
        {
            _vertices = new float[]
            {
                +0.5f * multiplier, 0, -0.5f * multiplier,
                -0.5f * multiplier, 0, -0.5f * multiplier,
                -0.5f * multiplier, 0, +0.5f * multiplier,
                +0.5f * multiplier, 0, +0.5f * multiplier,

            };

            _uvs = new float[]
            {
                1, 1,
                0, 1,
                0, 0,
                1, 0,
            };

            _normals = new float[]
            {
                0,0,1,
                0,0,1,
                0,0,1,
                0,0,1,

            };

            _tangents = new float[]
            {
                1, 0, 0,
                1, 0, 0,
                1, 0, 0,
                1, 0, 0,
               };

            _bitangents = new float[]
            {
                0, 1, 0,
                0, 1, 0,
                0, 1, 0,
                0, 1, 0,
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

            // normals
            int vbo_normal = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_normal);
            GL.BufferData(BufferTarget.ArrayBuffer, _normals.Length * 4, _normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // tangents
            int vbo_tangent = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_tangent);
            GL.BufferData(BufferTarget.ArrayBuffer, _tangents.Length * 4, _tangents, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // bitangents
            int vbo_bitangent = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bitangent);
            GL.BufferData(BufferTarget.ArrayBuffer, _bitangents.Length * 4, _bitangents, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(4);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }
    }
}
