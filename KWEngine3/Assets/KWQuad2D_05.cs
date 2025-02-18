using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Assets
{
    internal static class KWQuad2D_05
    {
        public static int VAO;
        public static int VAOSize;


        public static void Init()
        {
            float[] vertices = new float[]
            {
                -0.5f, -0.5f,
                +0.5f, -0.5f,
                +0.5f, +0.5f,

                +0.5f, +0.5f,
                -0.5f, +0.5f,
                -0.5f, -0.5f

            };

            float[] uvs = new float[]
            {
                0, 0,
                1, 0,
                1, 1,

                1, 1,
                0, 1,
                0, 0
            };

            VAOSize = vertices.Length / 2;
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int vboUV = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboUV);
            GL.BufferData(BufferTarget.ArrayBuffer, uvs.Length * sizeof(float), uvs, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }
    }
}
