using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    public class FlowField
    {
        public FlowFieldCell[,] Grid { get; private set; }
        public Vector2i GridSize { get; private set; }
        public float CellRadius { get; private set; }

        private float _cellDiametre;

        public FlowField(Vector2i gridSize, float cellRadius)
        {
            GridSize = gridSize;
            CellRadius = cellRadius;
            _cellDiametre = cellRadius * 2;

        }

        private void CreateGrid()
        {
            Grid = new FlowFieldCell[GridSize.X, GridSize.Y];
            for(int x = 0; x < GridSize.X; x++)
            {
                for(int y = 0; y < GridSize.Y; y++)
                {
                    Vector3 worldPos = new Vector3(_cellDiametre * x + CellRadius, 0, _cellDiametre * y + CellRadius);
                    Grid[x, y] = new FlowFieldCell(worldPos, new Vector2i(x, y));
                }
            }
        }
    }
}
