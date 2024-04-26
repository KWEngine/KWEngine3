using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal static class HelperOctree
    {
        public static OctreeNode _rootNode;

        public static void Init(Vector3 worldCenter, float maxDimension)
        {
            OctreeNode.ResetCounter();
            _rootNode = new OctreeNode(new Vector3(maxDimension), worldCenter);
            
        }

        public static void Add(GameObjectHitbox g)
        {
            _rootNode.AddGameObjectHitbox(g);
        }
    }
}
