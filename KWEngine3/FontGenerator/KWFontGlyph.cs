using OpenTK.Mathematics;

namespace KWEngine3.FontGenerator
{
    internal class KWFontGlyph
    {
        public bool IsValid { get; internal set; }
        public int VAO_Step1 { get; internal set; }
        public int VAO_Step2 { get; internal set; }
        public int VBO_Step1 { get; internal set; }
        public int VBO_Step2 { get; internal set; }
        public float Width { get; internal set; }
        public float Height { get; internal set; }
        public Vector2 UCoordinate { get; internal set; }
        public int VertexCount_Step1 { get; internal set; }
        public int VertexCount_Step2 { get; internal set; }
        public Vector2 Advance { get; internal set; }
        public char CodePoint { get; internal set; }

        public KWFontGlyph()
        {

        }

        public KWFontGlyph(char codepoint, int vao1, int vao2, int vbo1, int vbo2, int count1, int count2, float width, float height, float advanceWidth, float advanceHeight, Vector2 uv)
        {
            CodePoint = codepoint;
            VertexCount_Step1 = count1;
            VertexCount_Step2 = count2;
            VAO_Step1 = vao1;
            VAO_Step2 = vao2;
            VBO_Step1 = vbo1;
            VBO_Step2 = vbo2;
            Width = width;
            Height = height;
            UCoordinate = uv;
            Advance = new Vector2(advanceWidth, advanceHeight);
            IsValid = true;
        }
        public override string ToString()
        {
            return CodePoint.ToString();
        }

        public void UpdateUUNext(float u, float uNext)
        {
            //Console.WriteLine("u: " + u + " | uNext: " + uNext);
            UCoordinate = new Vector2(u, uNext);
        }
    }
}
