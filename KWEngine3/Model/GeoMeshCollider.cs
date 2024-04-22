using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    internal class GeoMeshCollider
    {
        public string Name { get; set; } = "";
        public GeoNode Root { get; set; } = null;
        public List<GeoMeshHitbox> MeshHitboxes = new();


    }
}
