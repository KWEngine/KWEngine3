using OpenTK;
using System.Collections.Generic;

namespace KWEngine3.Model
{
    internal struct GeoMeshFace
    {
        public int Normal { get; set; }
        public int[] Vertices { get; set; }
        public bool Flip { get; set; }

        public GeoMeshFace(int normal, bool flip, params int[] indices)
        {
            Normal = normal;
            Vertices = new int[indices.Length];
            for(int i = 0; i < indices.Length; i++)
            {
                Vertices[i] = indices[i];
            }
            Flip = flip;
        }

        public void SetNormal(int newIndex)
        {
            Normal = newIndex;
        }
    }
}
