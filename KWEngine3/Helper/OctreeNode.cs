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
                return HitboxesInThisNode.Count;
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

        public List<GameObjectHitbox> HitboxesInThisNode = new List<GameObjectHitbox>();

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

        public bool DoesNodeEncloseHitbox(GameObjectHitbox g)
        {
            bool leftRightOK = g._center.X + g._fullDiameterAABB <= Center.X + Scale.X && g._center.X - g._fullDiameterAABB >= Center.X - Scale.X;
            bool bottomTopOK = g._center.Y + g._fullDiameterAABB <= Center.Y + Scale.Y && g._center.Y - g._fullDiameterAABB >= Center.Y - Scale.Y;
            bool backFrontOK = g._center.Z + g._fullDiameterAABB <= Center.Z + Scale.Z && g._center.Z - g._fullDiameterAABB >= Center.Z - Scale.Z;

            return leftRightOK && bottomTopOK && backFrontOK;
        }

        public void AddHitboxToNode(GameObjectHitbox g)
        {
            HitboxesInThisNode.Add(g);
            g._currentOctreeNode = this;
        }

        public bool AddGameObjectHitbox(GameObjectHitbox g)
        {
            // If it has child OctreeNodes already, place the new hitbox
            // center point in one of those child OctreeNodes:
            if (ChildOctreeNodes.Count != 0)
            {
                // would a child octree node enclose this object?
                foreach (OctreeNode n in ChildOctreeNodes)
                {
                    bool result = n.AddGameObjectHitbox(g);
                    if (result)
                        return true;
                }

                // if we did not return true by now, 
                // no child was able to take it.
                // so, this instance right here must take it:
                AddHitboxToNode(g);
                return true;
            }
            else
            {
                // If it has no child OctreeNodes yet,
                // check if the hitbox center point is inside
                // the region and if it is, decide whether
                // to subdivide or to just store it:
                if (HitboxesInThisNode.Count < MAXGAMEOBJECTSPERNODE )
                {
                    if (DoesNodeEncloseHitbox(g))
                    {
                        AddHitboxToNode(g);
                        return true;
                    }
                }
                else
                {
                    Subdivide();
                    // would a child node enclose it?
                    foreach (OctreeNode n in ChildOctreeNodes)
                    {
                        bool result = n.AddGameObjectHitbox(g);
                        if (result)
                            return true;
                    }

                    // ok, we have to take this object anyway :-)
                    AddHitboxToNode(g);
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
