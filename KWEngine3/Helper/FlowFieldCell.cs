using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class FlowFieldCell
    {
        internal Vector2i _gridIndex;

        public Vector3 WorldPos { get; private set; }
        public Vector3 BestDirection { get; internal set; }
        public byte Cost
        {
            get
            {
                if (Parent.Destination != null && _gridIndex == Parent.Destination._gridIndex)
                {
                    return 0;
                }
                else
                {
                    return _cost;
                }
            }
            set
            {
                _cost = value;
            }
        }
        public uint BestCost
        {
            get
            {
                if (Parent.Destination != null && _gridIndex == Parent.Destination._gridIndex)
                {
                    return 0;
                }
                else
                {
                    return _bestCost;
                }
            }
            set
            {
                _bestCost = value;
            }
        }
        public FlowField Parent { get; internal set; }

        internal byte _cost;
        internal uint _bestCost;

        internal FlowFieldCell(Vector3 worldpos, Vector2i gridIndex, FlowField parent)
        {
            Parent = parent;
            WorldPos = worldpos;
            _gridIndex = gridIndex;
            Cost = 1;
            //CostForFlowField = 1;
            BestCost = uint.MaxValue;
            BestDirection = Vector3.Zero;
        }

        internal void SetCostTo(byte cost)
        {
            Cost = cost;            
        }

    }
}
