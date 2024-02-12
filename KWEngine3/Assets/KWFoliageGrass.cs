using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Assets
{
    internal static class KWFoliageGrass
    {
        public static float[] _vertices;
        public static float[] _normals;
        public static float[] _tangents;
        public static float[] _bitangents;
        public static float[] _uvs;
        public static float[] _colors;

        public static int VAO;

        internal const float XL = 0.025f;
        internal const float XM = 0.01875f;
        internal const float XH = 0.008333f;

        internal const float YL = 0.0f;
        internal const float YM = 0.2f;
        internal const float YH = 0.6f;

        internal const float ZL = 0.01f;
        internal const float ZM = 0.0075f;
        internal const float ZH = 0.003333f;

        internal const float NXFront = -0.25f;
        internal const float NYFront = +0.10f;
        internal const float NZFront = +0.75f;


        public static void Init()
        {
            /*
            _colors = new float[]
            {
                // Front
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                // Back left:
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                // Back right:
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,

                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
                0.05f, 1.00f, 0.05f,
            };
            */
            _vertices = new float[]
            {
                // Front
                XL * +1f, YL * 1f, ZL * +1f, // 0 right  low front
                XM * -1f, YM * 1f, ZM * +1f, // 2 left   mid front
                XL * -1f, YL * 1f, ZL * +1f, // 1 left   low front 

                XM * -1f, YM * 1f, ZM * +1f, // 2 left   mid
                XL * +1f, YL * 1f, ZL * +1f, // 0 right  low
                XM * +1f, YM * 1f, ZM * +1f, // 3 right  mid

                XM * -1f, YM * 1f, ZM * +1f, // 2 left   mid
                XM * +1f, YM * 1f, ZM * +1f, // 3 right  mid
                XH * -1f, YH * 1f, ZH * +1f, // 4 left   high

                XH * -1f, YH * 1f, ZH * +1f, // 4 left   high
                XM * +1f, YM * 1f, ZM * +1f, // 3 right  mid
                XH * +1f, YH * 1f, ZH * +1f, // 5 right  high

                XH * -1f, YH * 1f, ZH * +1f, // 4
                XH * +1f, YH * 1f, ZH * +1f, // 5
                0f,            1f,       0f, // 6

                // Back left:
                XL * +1f, YL * 1f, ZL * +1f, // 0
                XL * +0f, YL * 1f, ZL * -1f, // 7
                XM * +1f, YM * 1f, ZM * +1f, // 3

                XM * +1f, YM * 1f, ZM * +1f, // 3
                XL * +0f, YL * 1f, ZL * -1f, // 7
                XM * +0f, YM * 1f, ZM * -1f, // 8

                XM * +1f, YM * 1f, ZM * +1f, // 3
                XM * +0f, YM * 1f, ZM * -1f, // 8
                XH * +1f, YH * 1f, ZH * +1f, // 5

                XH * +1f, YH * 1f, ZH * +1f, // 5
                XM * +0f, YM * 1f, ZM * -1f, // 8
                XH * +0f, YH * 1f, ZH * -1f, // 9

                XH * +1f, YH * 1f, ZH * +1f, // 5
                XH * +0f, YH * 1f, ZH * -1f, // 9
                0f,            1f,       0f, // 6


                // Back right:
                XL * +0f, YL * 1f, ZL * -1f, // 7
                XL * -1f, YL * 1f, ZL * +1f, // 1
                XM * +0f, YM * 1f, ZM * -1f, // 8

                XM * +0f, YM * 1f, ZM * -1f, // 8
                XL * -1f, YL * 1f, ZL * +1f, // 1
                XM * -1f, YM * 1f, ZM * +1f, // 2

                XM * +0f, YM * 1f, ZM * -1f, // 8
                XM * -1f, YM * 1f, ZM * +1f, // 2
                XH * +0f, YH * 1f, ZH * -1f, // 9

                XH * +0f, YH * 1f, ZH * -1f, // 9
                XM * -1f, YM * 1f, ZM * +1f, // 2
                XH * -1f, YH * 1f, ZH * +1f, // 4

                XH * +0f, YH * 1f, ZH * -1f, // 9
                XH * -1f, YH * 1f, ZH * +1f, // 4
                0f,            1f,       0f, // 6
            };

            _uvs = new float[]
            {
                // Front
                1, YL * 1,  // 0
                0, YM * 1,  // 2
                0, YL * 1,  // 1
                
                0, YM * 1,  // 2
                1, YL * 1,  // 0
                1, YM * 1,  // 3

                0, YM * 1,  // 2
                1, YM * 1,  // 3
                0, YH * 1,  // 4

                0, YH * 1,  // 4
                1, YM * 1,  // 3
                1, YH * 1,  // 5

                0.0f, YH * 1,  // 4
                1.0f, YH * 1,  // 5
                0.5f, 1  * 1,  // 6

                // Back left:
                0.0f, YL * 1,  // 0
                0.5f, YL * 1,  // 7
                0.0f, YM * 1,  // 3
                
                0.0f, YM * 1,  // 3
                0.5f, YL * 1,  // 7
                0.5f, YM * 1,  // 8
                
                0.0f, YM * 1,  // 3
                0.5f, YM * 1,  // 8
                0.0f, YH * 1,  // 5
                
                0.0f, YH * 1,  // 5
                0.5f, YM * 1,  // 8
                0.5f, YH * 1,  // 9
                
                0.0f, YH * 1,  // 5
                0.5f, YH * 1,  // 9
                0.5f, 1  * 1,  // 6

                // Back right:
                0.5f, YL * 1,  // 7
                1.0f, YL * 1,  // 1
                0.5f, YM * 1,  // 8
                
                0.5f, YM * 1,  // 8
                1.0f, YL * 1,  // 1
                1.0f, YM * 1,  // 2
                
                0.5f, YM * 1,  // 8
                1.0f, YM * 1,  // 2
                0.5f, YL * 1,  // 9
                
                0.5f, YH * 1,  // 9
                0.0f, YM * 1,  // 2
                0.0f, YH * 1,  // 4
                
                0.5f, YH * 1,  // 9
                1.0f, YH * 1,  // 4
                0.5f, 1  * 1   // 6
            };

            _normals = new float[]
            {
                NXFront * +1f, NYFront, NZFront * +1f,  // 0
                NXFront * -1f, NYFront, NZFront * +1f,  // 2
                NXFront * -1f, NYFront, NZFront * +1f,  // 1

                NXFront * -1f, NYFront, NZFront * +1f,  // 2
                NXFront * +1f, NYFront, NZFront * +1f,  // 0
                NXFront * +1f, NYFront, NZFront * +1f,  // 3
                
                NXFront * -1f, NYFront, NZFront * +1f,  // 2
                NXFront * -1f, NYFront, NZFront * +1f,  // 3
                NXFront * -1f, NYFront, NZFront * +1f,  // 4
                
                NXFront * -1f, NYFront, NZFront * +1f,  // 4
                NXFront * -1f, NYFront, NZFront * +1f,  // 3
                NXFront * -1f, NYFront, NZFront * +1f,  // 5
                
                NXFront * -1f, NYFront, NZFront * +1f,  // 4
                NXFront * -1f, NYFront, NZFront * +1f,  // 5
                0f, 1f, 0f,                             // 6


                // BACK LEFT:
                NXFront * -1f, NYFront, NZFront * -1f, // 0
                NXFront * -1f, NYFront, NZFront * -1f, // 7
                NXFront * -1f, NYFront, NZFront * -1f, // 3

                NXFront * -1f, NYFront, NZFront * -1f, // 3
                NXFront * -1f, NYFront, NZFront * -1f, // 7
                NXFront * -1f, NYFront, NZFront * -1f, // 8

                NXFront * -1f, NYFront, NZFront * -1f,// 3
                NXFront * -1f, NYFront, NZFront * -1f,// 8
                NXFront * -1f, NYFront, NZFront * -1f,// 5

                NXFront * -1f, NYFront, NZFront * -1f,// 5
                NXFront * -1f, NYFront, NZFront * -1f,// 8
                NXFront * -1f, NYFront, NZFront * -1f,// 9

                NXFront * -1f, NYFront, NZFront * -1f,// 5
                NXFront * -1f, NYFront, NZFront * -1f,// 9
                NXFront * -1f, NYFront, NZFront * -1f,// 6

                // BACK RIGHT:
                NXFront * +1f, NYFront, NZFront * -1f,// 7
                NXFront * +1f, NYFront, NZFront * -1f,// 1
                NXFront * +1f, NYFront, NZFront * -1f,// 8

                NXFront * +1f, NYFront, NZFront * -1f,// 8
                NXFront * +1f, NYFront, NZFront * -1f,// 1
                NXFront * +1f, NYFront, NZFront * -1f,// 2

                NXFront * +1f, NYFront, NZFront * -1f,// 8
                NXFront * +1f, NYFront, NZFront * -1f,// 2
                NXFront * +1f, NYFront, NZFront * -1f,// 9

                NXFront * +1f, NYFront, NZFront * -1f,// 9
                NXFront * +1f, NYFront, NZFront * -1f,// 2
                NXFront * +1f, NYFront, NZFront * -1f,// 4

                NXFront * +1f, NYFront, NZFront * -1f,// 9
                NXFront * +1f, NYFront, NZFront * -1f,// 4
                NXFront * +1f, NYFront, NZFront * -1f// 6
            };

            _tangents = GenerateTangentsFromNormals(out _bitangents);

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            /*
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec2 aTexture;
                layout(location = 2) in vec3 aNormal;
                layout(location = 3) in	vec3 aTangent;
                layout(location = 4) in	vec3 aBiTangent;
                layout(location = 5) in vec3 aColor;
             */
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

            /*
            // colors
            int vbo_color = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData(BufferTarget.ArrayBuffer, _colors.Length * 4, _colors, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(5);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            */

            GL.BindVertexArray(0);
        }

        internal static float[] GenerateTangentsFromNormals(out float[] bitangents)
        {
            float[] tangents = new float[_normals.Length];
            bitangents = new float[_normals.Length];
            for (int i = 0, uv = 0; i < _vertices.Length; i += 9, uv += 6)
            {
                Vector3 v0 = new(_vertices[i + 0], _vertices[i + 1], _vertices[i + 2]);
                Vector3 v1 = new(_vertices[i + 3], _vertices[i + 4], _vertices[i + 5]);
                Vector3 v2 = new(_vertices[i + 6], _vertices[i + 7], _vertices[i + 8]);

                Vector3 n0 = new(_normals[i + 0], _normals[i + 1], _normals[i + 2]);
                Vector3 n1 = new(_normals[i + 3], _normals[i + 4], _normals[i + 5]);
                Vector3 n2 = new(_normals[i + 6], _normals[i + 7], _normals[i + 8]);

                n0.Normalize(); n1.Normalize(); n2.Normalize();

                _normals[i + 0] = n0.X; _normals[i + 1] = n0.Y; _normals[i + 2] = n0.Z;
                _normals[i + 3] = n1.X; _normals[i + 4] = n1.Y; _normals[i + 5] = n1.Z;
                _normals[i + 6] = n2.X; _normals[i + 7] = n2.Y; _normals[i + 8] = n2.Z;

                Vector2 uv0 = new(_uvs[uv + 0], _uvs[uv + 1]);
                Vector2 uv1 = new(_uvs[uv + 2], _uvs[uv + 3]);
                Vector2 uv2 = new(_uvs[uv + 4], _uvs[uv + 5]);

                // Edges of the triangle : position delta
                Vector3 deltaPos1 = v1 - v0;
                Vector3 deltaPos2 = v2 - v0;

                // UV delta
                Vector2 deltaUV1 = uv1 - uv0;
                Vector2 deltaUV2 = uv2 - uv0;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                tangent.Normalize();

                tangents[i + 0] = tangent.X;
                tangents[i + 1] = tangent.Y;
                tangents[i + 2] = tangent.Z;

                tangents[i + 3] = tangent.X;
                tangents[i + 4] = tangent.Y;
                tangents[i + 5] = tangent.Z;
                         
                tangents[i + 6] = tangent.X;
                tangents[i + 7] = tangent.Y;
                tangents[i + 8] = tangent.Z;

                Vector3 bt = Vector3.Cross(new Vector3(_normals[i + 0], _normals[i + 0], _normals[i + 0]), tangent);
                bitangents[i + 0] = bt.X;
                bitangents[i + 1] = bt.Y;
                bitangents[i + 2] = bt.Z;
                bitangents[i + 3] = bt.X;
                bitangents[i + 4] = bt.Y;
                bitangents[i + 5] = bt.Z;
                bitangents[i + 6] = bt.X;
                bitangents[i + 7] = bt.Y;
                bitangents[i + 8] = bt.Z;
            }

            return tangents;
        }
    }
}
