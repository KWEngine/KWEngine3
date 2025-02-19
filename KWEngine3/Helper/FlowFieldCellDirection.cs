using Assimp;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal class FlowFieldCellDirection
    {
        public readonly Vector2i Vector;

        public FlowFieldCellDirection(int x, int y)
        {
            Vector = new Vector2i(x, y);
        }

        public static implicit operator Vector2i(FlowFieldCellDirection direction) 
        {  
            return direction.Vector; 
        }

        public static Vector3 GetDirectionFromVector2i(Vector2i vector, FlowFieldDirections directions)
        {
            foreach(FlowFieldCellDirection dir in (directions == FlowFieldDirections.CardinalAndIntercardinalDirections ? CardinalIntercardinalDirections : CardinalDirections))
            {
                if(dir.Vector == vector)
                {
                    return Vector3.NormalizeFast(new Vector3(vector.X, 0, vector.Y));
                }
            }
            return Vector3.Zero;
        }

        public static readonly FlowFieldCellDirection None = new FlowFieldCellDirection(0, 0);
        public static readonly FlowFieldCellDirection North = new FlowFieldCellDirection(0, 1);
        public static readonly FlowFieldCellDirection South = new FlowFieldCellDirection(0, -1);
        public static readonly FlowFieldCellDirection East = new FlowFieldCellDirection(1, 0);
        public static readonly FlowFieldCellDirection West = new FlowFieldCellDirection(-1, 0);

        public static readonly FlowFieldCellDirection NorthEast = new FlowFieldCellDirection(1, 1);
        public static readonly FlowFieldCellDirection SouthEast = new FlowFieldCellDirection(1, -1);
        public static readonly FlowFieldCellDirection NorthWest = new FlowFieldCellDirection(-1, 1);
        public static readonly FlowFieldCellDirection SouthWest = new FlowFieldCellDirection(-1, -1);

        public static readonly List<FlowFieldCellDirection> CardinalDirections = new List<FlowFieldCellDirection>()
        {
            North, East, South, West
        };
        public static readonly List<FlowFieldCellDirection> CardinalIntercardinalDirections = new List<FlowFieldCellDirection>()
        {
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
        };
        public static readonly List<FlowFieldCellDirection> AllDirections = new List<FlowFieldCellDirection>()
        {
            None, North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
        };
    }
}
