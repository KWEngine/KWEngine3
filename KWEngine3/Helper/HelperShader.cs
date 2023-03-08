using KWEngine3.Model;
using KWEngine3.Renderer;
using KWEngine3.ShadowMapping;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal static class HelperShader
    {
        public static int LoadCompileAttachShader(Stream stream, ShaderType type, int program)
        {
            int address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(stream))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            return address;
        }
    }
}
