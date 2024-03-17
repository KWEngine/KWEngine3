using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace KWEngine3.Model
{
    internal static class PrimitiveTriangle
    {
        private static int _vao;
        private static int _vboPosition;

        public static int VAO { get { return _vao; } }

        public static void Init()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vboPosition = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPosition);
            GL.BufferData(BufferTarget.ArrayBuffer, 3 * 4, new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0}, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }
    }
}
