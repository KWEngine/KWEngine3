using OpenTK;
using System.Collections.Generic;
using System.Numerics;

namespace KWEngine3.Model
{
    internal struct GeoMeshFace
    {
        internal int index = 0;
        public int Normal { get; set; }
        public int[] Vertices { get; set; }
        public bool Flip { get; set; }

        public int VertexCount { get; set; }

        public GeoMeshFace(int vertexCount)
        {
            Normal = -1;
            Vertices = new int[vertexCount];
            VertexCount = vertexCount;
            Flip = false;
        }

        public GeoMeshFace(int normal, bool flip, params int[] indices)
        {
            Normal = normal;
            Vertices = new int[indices.Length];
            for(int i = 0; i < indices.Length; i++)
            {
                Vertices[i] = indices[i];
            }
            VertexCount = Vertices.Length;
            Flip = flip;
        }

        public void SetNormal(int newIndex)
        {
            Normal = newIndex;
        }

        public void AddVertex(int i)
        {
            Vertices[index++] = i;
        }
    }
}
