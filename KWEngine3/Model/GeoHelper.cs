using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    internal static class GeoHelper
    {
        internal const float ERROR_MARGIN = 0.01f;
        internal static void FindIndexOfVertexInList(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, List<Vector3> verticesForWholeMesh, List<Vector3> normalsForWholeMesh, out int iv1, out int iv2, out int iv3, out int in1)
        {
            iv1 = -1;
            iv2 = -1;
            iv3 = -1;
            in1 = -1;

            for(int i = 0; i < verticesForWholeMesh.Count; i++)
            {
                if (iv1 < 0 && v1 == verticesForWholeMesh[i])
                    iv1 = i;
                if (iv2 < 0 && v2 == verticesForWholeMesh[i])
                    iv2 = i;
                if (iv3 < 0 && v3 == verticesForWholeMesh[i])
                    iv3 = i;

                if (iv1 >= 0 && iv2 >= 0 && iv3 >= 0)
                    break;
            }

            Vector3 u = v2 - v1;
            Vector3 v = v3 - v1;
            Vector3 n = Vector3.Normalize(Vector3.Cross(u, v));
            for (int i = 0; i < normalsForWholeMesh.Count; i++)
            {
                float dot = Vector3.Dot(n, normalsForWholeMesh[i]);
                if (dot >= (1 - ERROR_MARGIN) && dot <= 1 + ERROR_MARGIN)
                {
                    in1 = i;
                    break;
                }
            }

            if(in1 < 0 || iv1 < 0 || iv2 < 0 || iv3 < 0)
            {
                throw new Exception("Could not find appropriate face normal index in hitbox' normal array.");
            }
        }
    }
}
