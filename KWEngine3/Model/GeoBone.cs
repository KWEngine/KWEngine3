using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal class GeoBone
    {
        public override string ToString()
        {
            return Name;
        }
        public int Index { get; internal set; }
        public string Name { get; internal set; }
        public Matrix4 Offset { get; internal set; }
    }
}
