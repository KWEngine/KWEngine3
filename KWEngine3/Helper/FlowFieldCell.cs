using KWEngine3.GameObjects;
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
        public CardinalDirection BestDirectionCardinal { get; internal set; } = CardinalDirection.None;

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

        /// <summary>
        /// Prüft anhand der XZ-Achsen, ob ein Objekt ansatzweise innerhalb der Zelle liegt
        /// </summary>
        /// <param name="g">zu prüfendes Objekt</param>
        /// <param name="partial">wenn true, zählen auch teilweise enthaltene Objekte (Standard: true)</param>
        /// <returns>true, wenn das Objekt im Feld liegt</returns>
        public bool ContainsXZ(GameObject g, bool partial = true)
        {
            float cellLeft = Position.X - Parent.CellRadius;
            float cellRight = Position.X + Parent.CellRadius;
            float cellBack = Position.Z - Parent.CellRadius;
            float cellFront = Position.Z + Parent.CellRadius;

            if (partial)
            {
                bool overlapX = ((cellLeft <= g.AABBLeft && cellRight >= g.AABBLeft) || (cellLeft <= g.AABBRight && cellRight >= g.AABBRight) || (cellLeft <= g.AABBLeft && cellRight >= g.AABBRight) || (cellLeft >= g.AABBLeft && cellRight <= g.AABBRight));
                if (overlapX)
                {
                    bool overlapZ = ((cellBack <= g.AABBBack && cellFront >= g.AABBBack) || (cellFront >= g.AABBFront && cellBack <= g.AABBFront) || (cellFront >= g.AABBFront && cellBack <= g.AABBBack) || (cellFront <= g.AABBFront && cellBack >= g.AABBBack));
                    return overlapZ;
                }
            }
            else
            {
                bool overlapX = (g.AABBRight >= cellLeft && g.AABBRight <= cellRight) || (g.AABBLeft >= cellLeft && g.AABBLeft <= cellRight);
                if (overlapX)
                {
                    bool overlapZ = (g.AABBFront >= cellBack && g.AABBFront <= cellFront) || (g.AABBBack >= cellBack && g.AABBBack <= cellFront);
                    return overlapZ;
                }
            }

            return false;
        }

        /// <summary>
        /// Prüft anhand der XZ-Achsen, ob eine Position innerhalb der Zelle liegt
        /// </summary>
        /// <param name="position">zu prüfende Position</param>
        /// <returns>true, wenn die Position im Feld liegt</returns>
        public bool ContainsXZ(Vector3 position)
        {
            float cellLeft = Position.X - Parent.CellRadius;
            float cellRight = Position.X + Parent.CellRadius;
            float cellBack = Position.Z - Parent.CellRadius;
            float cellFront = Position.Z + Parent.CellRadius;
            bool overlapX = (position.X >= cellLeft && position.X <= cellRight);
            if (overlapX)
            {
                bool overlapZ = (position.Z >= cellBack && position.Z <= cellFront);
                return overlapZ;
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob ein Objekt ansatzweise innerhalb der Zelle liegt
        /// </summary>
        /// <param name="g">zu prüfendes Objekt</param>
        /// <param name="partial">wenn true, zählen auch teilweise enthaltene Objekte (Standard: true)</param>
        /// <returns>true, wenn das Objekt im Feld liegt</returns>
        public bool Contains(GameObject g, bool partial = true)
        {
            if (partial)
            {
                bool overlapY = ((Parent._fftop >= g.AABBHigh && Parent._ffbottom <= g.AABBHigh) || (Parent._fftop >= g.AABBLow && Parent._ffbottom <= g.AABBLow) || (Parent._fftop >= g.AABBHigh && Parent._ffbottom <= g.AABBLow) || (Parent._fftop <= g.AABBHigh && Parent._ffbottom >= g.AABBLow));
                if (overlapY)
                {
                    return ContainsXZ(g, partial);
                }
            }
            else
            {
                bool overlapY = (g.AABBHigh >= Parent._ffbottom && g.AABBHigh <= Parent._fftop) || (g.AABBLow <= Parent._fftop && g.AABBLow >= Parent._ffbottom) || (g.AABBLow <= Parent._ffbottom && g.AABBHigh >= Parent._fftop);
                if (overlapY)
                {
                    return ContainsXZ(g, partial);
                }
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob eine Position innerhalb der Zelle liegt
        /// </summary>
        /// <param name="position">zu prüfende Position</param>
        /// <returns>true, wenn die Position im Feld liegt</returns>
        public bool Contains(Vector3 position)
        {

            bool overlapY = (position.Y >= Parent._ffbottom && position.Y <= Parent._fftop);
            if (overlapY)
            {
                return ContainsXZ(position);
            }
            return false;
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
