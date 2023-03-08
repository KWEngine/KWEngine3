using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class OctreeNode
    {
        public static void ResetColors()
        {
            Counter = 0;
        }

        public int GameObjectCount
        {
            get
            {
                return GameObjectsInThisNode.Count;
            }
        }

        private static readonly Vector3[] COLORS = new Vector3[] {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 1),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.25f, 0.75f, 0.5f)
        };
        private const int MAXGAMEOBJECTSPERNODE = 4;
        private static uint Counter = 0;
        public Vector3 Scale { get; private set; } = new Vector3(1, 1, 1);
        public Vector3 Center { get; private set; } = Vector3.Zero;
        public Vector3 Color { get; private set; } = COLORS[Counter % COLORS.Length];
        public OctreeNode Parent { get; private set; } = null;

        public List<GameObject> GameObjectsInThisNode = new List<GameObject>();

        public List<OctreeNode> ChildOctreeNodes { get; private set; } = new List<OctreeNode>();

        public OctreeNode(Vector3 scale, Vector3 center)
        {
            Counter++;
            Scale = scale;
            Center = center;
        }

        public static void ResetCounter()
        {
            Counter = 0;
        }

        public bool WouldAChildNodeEncloseHitbox(GameObject g, out OctreeNode child)
        {
            child = null;
            return false;
        }

        public bool DoesNodeEncloseGameObject(GameObject g)
        {
            bool leftOk = Center.X - Scale.X + KWEngine._octreeSafetyZone <= g._stateCurrent._center.X - g._stateCurrent._dimensions.X / 2;
            bool rightOk = Center.X + Scale.X - KWEngine._octreeSafetyZone >= g._stateCurrent._center.X + g._stateCurrent._dimensions.X / 2;
            bool topOk = Center.Y + Scale.Y - KWEngine._octreeSafetyZone >= g._stateCurrent._center.Y + g._stateCurrent._dimensions.Y / 2;
            bool bottomOk = Center.Y - Scale.Y + KWEngine._octreeSafetyZone <= g._stateCurrent._center.Y - g._stateCurrent._dimensions.Y / 2;
            bool backOk = Center.Z - Scale.Z + KWEngine._octreeSafetyZone <= g._stateCurrent._center.Z - g._stateCurrent._dimensions.Z / 2;
            bool frontOk = Center.Z + Scale.Z - KWEngine._octreeSafetyZone  >= g._stateCurrent._center.Z + g._stateCurrent._dimensions.Z / 2;

            bool result = leftOk && rightOk && topOk && bottomOk && backOk && frontOk;
            return result;
        }

        public void AddGameObjectToNode(GameObject g)
        {
            GameObjectsInThisNode.Add(g);
            g._currentOctreeNode = this;
        }

        public bool AddGameObject(GameObject g)
        {
            // If it has child OctreeNodes already, place the new hitbox
            // center point in one of those child OctreeNodes:
            if (ChildOctreeNodes.Count != 0)
            {
                // would a child octree node enclose this object?
                foreach (OctreeNode n in ChildOctreeNodes)
                {
                    bool result = n.AddGameObject(g);
                    if (result)
                        return true;
                }

                // if we did not return true by now, 
                // no child was able to take it.
                // so, this instance right here must take it:
                AddGameObjectToNode(g);
                return true;
            }
            else
            {
                // If it has no child OctreeNodes yet,
                // check if the hitbox center point is inside
                // the region and if it is, decide whether
                // to subdivide or to just store it:
                if (GameObjectsInThisNode.Count < MAXGAMEOBJECTSPERNODE )
                {
                    if (DoesNodeEncloseGameObject(g))
                    {
                        AddGameObjectToNode(g);
                        return true;
                    }
                }
                else
                {
                    Subdivide();
                    // would a child node enclose it?
                    foreach (OctreeNode n in ChildOctreeNodes)
                    {
                        bool result = n.AddGameObject(g);
                        if (result)
                            return true;
                    }

                    // ok, we have to take this object anyway :-)
                    AddGameObjectToNode(g);
                    return true;
                }
            }
            return false;
        }

        public void Subdivide()
        {
            if (ChildOctreeNodes.Count != 0 || Scale.LengthFast < 1)
                return;

            OctreeNode child01 = new OctreeNode(Scale / 2, Center + new Vector3(Scale.X / 2, Scale.Y / 2, Scale.Z / 2));
            child01.Parent = this;
            ChildOctreeNodes.Add(child01);

            OctreeNode child02 = new OctreeNode(Scale / 2, Center + new Vector3(-Scale.X / 2, Scale.Y / 2, Scale.Z / 2));
            child02.Parent = this;
            ChildOctreeNodes.Add(child02);

            OctreeNode child03 = new OctreeNode(Scale / 2, Center + new Vector3(Scale.X / 2, -Scale.Y / 2, Scale.Z / 2));
            child03.Parent = this;
            ChildOctreeNodes.Add(child03);

            OctreeNode child04 = new OctreeNode(Scale / 2, Center + new Vector3(-Scale.X / 2, -Scale.Y / 2, Scale.Z / 2));
            child04.Parent = this;
            ChildOctreeNodes.Add(child04);

            OctreeNode child05 = new OctreeNode(Scale / 2, Center + new Vector3(Scale.X / 2, Scale.Y / 2, -Scale.Z / 2));
            child05.Parent = this;
            ChildOctreeNodes.Add(child05);

            OctreeNode child06 = new OctreeNode(Scale / 2, Center + new Vector3(-Scale.X / 2, Scale.Y / 2, -Scale.Z / 2));
            child06.Parent = this;
            ChildOctreeNodes.Add(child06);

            OctreeNode child07 = new OctreeNode(Scale / 2, Center + new Vector3(Scale.X / 2, -Scale.Y / 2, -Scale.Z / 2));
            child07.Parent = this;
            ChildOctreeNodes.Add(child07);

            OctreeNode child08 = new OctreeNode(Scale / 2, Center + new Vector3(-Scale.X / 2, -Scale.Y / 2, -Scale.Z / 2));
            child08.Parent = this;
            ChildOctreeNodes.Add(child08);

        }
    }
}
