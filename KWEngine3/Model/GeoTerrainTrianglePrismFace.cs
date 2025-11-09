using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal class GeoTerrainTrianglePrismFace
    {
        public int VertexCount;
        public Vector3[] Vertices;
        public Vector3 Normal;
        
        public GeoTerrainTrianglePrismFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, bool v4IsNormal)
        {
            if(v4IsNormal)
            {
                Vertices = new Vector3[3];
                Vertices[0] = v1;
                Vertices[1] = v2;
                Vertices[2] = v3;
                Normal = v4;
                VertexCount = 3;
            }
            else
            {
                Vertices = new Vector3[4];
                Vertices[0] = v1;
                Vertices[1] = v2;
                Vertices[2] = v3;
                Vertices[3] = v4;
                Normal = GeoTerrainTriangle.CalculateSurfaceNormal(v3, v2, v1);
                VertexCount = 4;
            }
        }

        public override string ToString()
        {
            string name = "";
            foreach(Vector3 v in Vertices)
            {
                name += v + " | ";
            }
            name += "- | " + Normal;
            return name;
        }
    }
}
