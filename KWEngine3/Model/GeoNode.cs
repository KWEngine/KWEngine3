using OpenTK.Mathematics;
using System.Collections.Generic;

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
        public bool IsAssimpFBXNode = false;
        public string NameWithoutFBXSuffix = null;

        public List<GeoNode> Children { get; internal set; } = new List<GeoNode>();
        public GeoNode Parent  = null;

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

        /// <summary>
        /// Sammelt den Namen dieses Knotens sowie aller Nachfahren rekursiv in das übergebene HashSet.
        /// Wird für Bone-Presets mit includeChildren=true genutzt.
        /// </summary>
        internal static void CollectDescendantNames(GeoNode node, HashSet<string> target)
        {
            target.Add(node.Name);
            if (node.NameWithoutFBXSuffix != null && node.NameWithoutFBXSuffix != node.Name)
                target.Add(node.NameWithoutFBXSuffix);
            foreach (GeoNode child in node.Children)
                CollectDescendantNames(child, target);
        }
    }
}
