using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal static class HelperOctree
    {
        public static OctreeNode _rootNode;

        public static void Init(Vector3 worldCenter, Vector3 worldDimensions)
        {
            OctreeNode.ResetCounter();
            _rootNode = new OctreeNode(worldDimensions, worldCenter);
            
        }

        public static void Add(GameObject g)
        {
            if(g.IsCollisionObject)
                _rootNode.AddGameObject(g);
        }
    }
}
