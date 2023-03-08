using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    internal class GeoNode
    {
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; internal set; } = null;
        public Matrix4 Transform = Matrix4.Identity;

        public List<GeoNode> Children { get; internal set; } = new List<GeoNode>();
        public GeoNode Parent { get; internal set; } = null;

        public static GeoNode FindChild(GeoNode nodeStart, string name)
        {
            if(nodeStart.Name == name)
            {
                return nodeStart;
            }
            else
            {
                foreach (GeoNode child in nodeStart.Children)
                {
                    if(child.Name == name)
                    {
                        return child;
                    }
                    GeoNode v = FindChild(child, name);
                    if (v != null && v.Name == name)
                        return v;
                }
            }
            return null;
        }
    }
}
