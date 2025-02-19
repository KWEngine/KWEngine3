using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Bildet eine einzelne Zelle einer FlowField-Instanz ab
    /// </summary>
    public class FlowFieldCell
    {
        /// <summary>
        /// Gibt die Position der Zelle in Weltkoordinaten an
        /// </summary>
        public Vector3 Position { get; internal set; }

        /// <summary>
        /// Gibt den horizontalen Index der Zelle im FlowField an (nullbasiert)
        /// </summary>
        public int IndexX { get { return _gridIndex.X; } }
        /// <summary>
        /// Gibt den vertikalen Index der Zelle im FlowField an (nullbasiert)
        /// </summary>
        public int IndexZ { get { return _gridIndex.Y; } }

        /// <summary>
        /// Gibt die aktuelle Zielrichtung für diese Zelle an (kann der Nullvektor sein)
        /// </summary>
        public Vector3 BestDirection { get; internal set; }

        /// <summary>
        /// Gibt die aktuelle Zielrichtung in Form einer Himmelsrichtung an
        /// </summary>
        public CardinalDirection BestDirectionCardinal { get; internal set; } = CardinalDirection.None

        /// <summary>
        /// Erfragt die Zelle, die von der aktuellen Zelle ausgehend am angegebenen Offset liegt
        /// </summary>
        /// <param name="offsetX">Offset in X-Richtung</param>
        /// <param name="offsetZ">Offset in Z-Richtung</param>
        /// <returns>Benachbarte Zelle gemäß Offset. Gibt null zurück, falls es keine benachbarte Zelle für den angegebenen Offset gibt.</returns>
        public FlowFieldCell GetNeighbourCellAtOffset(int offsetX, int offsetZ)
        {
            Vector2i giWithOffset = _gridIndex + new Vector2i(offsetX, offsetZ);
            if (giWithOffset.X >= Parent.GridCellCount.X || giWithOffset.X < 0 || giWithOffset.Y >= Parent.GridCellCount.Z || giWithOffset.Y < 0)
            {
                return null;
            }
            else
            {
                return Parent.Grid[giWithOffset.X, giWithOffset.Y];
            }
        }

        /// <summary>
        /// Gibt die aktuell gemessenen Kosten der Zelle an
        /// </summary>
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
            internal set
            {
                _cost = value;
            }
        }

        internal Vector2i _gridIndex;

        internal uint BestCost
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

        internal FlowField Parent { get; set; }

        internal byte _cost;
        internal uint _bestCost;

        internal FlowFieldCell(Vector3 worldpos, Vector2i gridIndex, FlowField parent)
        {
            Parent = parent;
            Position = worldpos;
            _gridIndex = gridIndex;
            Cost = 1;
            BestCost = uint.MaxValue;
            BestDirection = Vector3.Zero;
        }

        internal void SetPosition(Vector3 worldPos)
        {
            Position = worldPos;
        }


        internal void SetCostTo(byte cost)
        {
            Cost = cost;            
        }

    }
}
