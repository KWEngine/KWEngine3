using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal struct GeoMeshFaceHelper
    {
        public Vector3[] Vertices { get; set; }

        public GeoMeshFaceHelper(params GeoVertex[] vertices)
        {
            Vertices = new Vector3[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                Vertices[i] = new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z);
            }
        }

        public GeoMeshFaceHelper(params Vector3[] vertices)
        {
            Vertices = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vertices[i] = new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z);
            }
        }
    }
}
