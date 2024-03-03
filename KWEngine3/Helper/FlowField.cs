using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Klasse zum Erstellen einer FlowField-Instanz für KI-Navigation
    /// </summary>
    public class FlowField
    {
        /// <summary>
        /// Anzahl der Zellen im Feld (in allen drei Dimensionen, wobei nur X- und Z-Dimension zählen)
        /// </summary>
        public Vector3i GridCellCount { get; private set; }
        /// <summary>
        /// Radius einer Zelle
        /// </summary>
        public float CellRadius { get; private set; }
        /// <summary>
        /// Mittelpunkt der FlowField-Instanz
        /// </summary>
        public Vector3 Center { get; private set; }
        /// <summary>
        /// Gibt an, ob die Instanz zu Debugging-Zwecken sichtbar gemacht werden soll
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gibt die letztbekannte Zielposition innerhalb des Feldes an
        /// </summary>
        public Vector3 TargetPosition { get { return _target; } }

        /// <summary>
        /// Erzeugt ein FlowField für die angegebenen GameObject-Typen
        /// </summary>
        /// <param name="positionX">X-Koordinate des Mittelpunkts</param>
        /// <param name="positionY">Y-Koordinate des Mittelpunkts</param>
        /// <param name="positionZ">Z-Koordinate des Mittelpunkts</param>
        /// <param name="cellCountX">Anzahl Zellen in X-Richtung</param>
        /// <param name="cellCountZ">Anzahl Zellen in Z-Richtung</param>
        /// <param name="cellRadius">Radius je Zelle</param>
        /// <param name="fieldHeight">Höhe des Felds</param>
        /// <param name="types">Liste der Klassen, die das FlowField scannen soll</param>
        public FlowField(float positionX, float positionY, float positionZ, int cellCountX, int cellCountZ, float cellRadius, int fieldHeight, params Type[] types)
            : this(new Vector3(positionX, positionY, positionZ), cellCountX, cellCountZ, cellRadius, fieldHeight, types)
        {

        }

        /// <summary>
        /// Erzeugt ein FlowField für die angegebenen GameObject-Typen
        /// </summary>
        /// <param name="center">Mittelpunkt des Felds</param>
        /// <param name="cellCountX">Anzahl Zellen in X-Richtung</param>
        /// <param name="cellCountZ">Anzahl Zellen in Z-Richtung</param>
        /// <param name="cellRadius">Radius je Zelle</param>
        /// <param name="fieldHeight">Höhe des Felds</param>
        /// <param name="types">Liste der Klassen, die das FlowField scannen soll</param>
        /// <exception cref="Exception">Eine genannte Klasse erbt nicht von GameObject</exception>
        public FlowField(Vector3 center, int cellCountX, int cellCountZ, float cellRadius, int fieldHeight, params Type[] types)
        {
            IsVisible = false;
            Center = center;
            GridCellCount = new Vector3i(MathHelper.Max(Math.Abs(cellCountX), 4), MathHelper.Max(Math.Abs(fieldHeight), 1), MathHelper.Max(Math.Abs(cellCountZ), 4));
            if (GridCellCount.X % 2 != 0)
            {
                GridCellCount = new Vector3i(GridCellCount.X + 1, GridCellCount.Y, GridCellCount.Z);
            }
            if (GridCellCount.Z % 2 != 0)
            {
                GridCellCount = new Vector3i(GridCellCount.X, GridCellCount.Y, GridCellCount.Z + 1);
            }

            CellRadius = Math.Max(Math.Abs(cellRadius), 0.5f);
            _cellDiametre = CellRadius * 2;

            _types = new Type[types.Length];
            for(int i = 0; i < types.Length; i++)
            {
                if (!HelperGeneral.IsTypeClassOrSubclassOfGameObject(types[i]))
                {
                    throw new Exception("Type is not a subclass of GameObject");
                }
                _types[i] = types[i];
            }

            _ffleft  = Center.X - GridCellCount.X * CellRadius;
            _ffright = Center.X + GridCellCount.X * CellRadius;
            _ffback  = Center.Z - GridCellCount.Z * CellRadius;
            _fffront = Center.Z + GridCellCount.Z * CellRadius;
            _fftop   = Center.Y + GridCellCount.Y * 0.5f;
            _ffbottom= Center.Y - GridCellCount.Y * 0.5f;

            CreateGrid();
            Update();
        }

        /// <summary>
        /// Prüft anhand der XZ-Achsen, ob ein Objekt ansatzweise innerhalb des Feldes liegt
        /// </summary>
        /// <param name="g">zu prüfendes Objekt</param>
        /// <returns>true, wenn das Objekt im Feld liegt</returns>
        public bool ContainsXZ(GameObject g)
        {
            bool overlapX = (g.AABBRight >= _ffleft && g.AABBRight <= _ffright) || (g.AABBLeft >= _ffleft && g.AABBLeft <= _ffright);
            if(overlapX)
            {
                bool overlapZ = (g.AABBFront >= _ffback && g.AABBFront <= _fffront) || (g.AABBBack >= _ffback && g.AABBBack <= _fffront);
                return overlapZ;
            }
            return false;
        }

        /// <summary>
        /// Prüft anhand der XZ-Achsen, ob eine Position innerhalb des Feldes liegt
        /// </summary>
        /// <param name="position">zu prüfende Position</param>
        /// <returns>true, wenn die Position im Feld liegt</returns>
        public bool ContainsXZ(Vector3 position)
        {
            bool overlapX = (position.X >= _ffleft && position.X <= _ffright);
            if (overlapX)
            {
                bool overlapZ = (position.Z >= _ffback && position.Z <= _fffront);
                return overlapZ;
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob ein Objekt ansatzweise innerhalb des Feldes liegt
        /// </summary>
        /// <param name="g">zu prüfendes Objekt</param>
        /// <returns>true, wenn das Objekt im Feld liegt</returns>
        public bool Contains(GameObject g)
        {
            bool overlapY = (g.AABBHigh >= _ffbottom && g.AABBHigh <= _fftop) || (g.AABBLow <= _fftop && g.AABBLow >= _ffbottom);
            if (overlapY)
            {
                return ContainsXZ(g);
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob eine Position innerhalb des Feldes liegt
        /// </summary>
        /// <param name="position">zu prüfende Position</param>
        /// <returns>true, wenn die Position im Feld liegt</returns>
        public bool Contains(Vector3 position)
        {
            bool overlapY = (position.Y >= _ffbottom && position.Y <= _fftop);
            if (overlapY)
            {
                return ContainsXZ(position);
            }
            return false;
        }

        /// <summary>
        /// Führt einen Scan über alle im FlowField liegenden Objekte durch und berechnet die Wegkosten pro Knoten neu
        /// </summary>
        /// <remarks>
        /// Die Ausführung dieser Methode ist immer dann notwendig, wenn sich die Position, Rotation oder Skalierung eines 
        /// für das FlowField relevanten Objekts ändert
        /// </remarks>
        public void Update()
        {
            _updateCostField = true;
        }

       

        /// <summary>
        /// Berechnet die Richungsanweisungen im gesamten FlowField für die neue Zielposition
        /// </summary>
        /// <param name="position">Neue Positionsangabe</param>
        public void SetTarget(Vector3 position)
        {
            if (Destination != null)
            {
                lock (Destination)
                {
                    _target = position;
                    _targetIsUpdated = true;
                }
            }
            else
            {
                _target = position;
                _targetIsUpdated = true;
            }
        }

        /// <summary>
        /// Gibt die bestmögliche Bewegungsrichtung gemäß der aktuellen Position zurück
        /// </summary>
        /// <param name="position">aktuelle Position</param>
        /// <returns>bestmögliche Bewegungsrichtung</returns>
        public Vector3 GetBestDirectionForPosition(Vector3 position)
        {
            if (Destination != null)
            {
                FlowFieldCell result = GetCellFromWorldPosition(position);
                if(result != Destination)
                {
                    return result.BestDirection;
                }
                else
                {
                    return Vector3.Zero;
                }                  
            }
            return Vector3.Zero;
        }

        /// <summary>
        /// Gibt an, ob die übergebene Position in der Zielzelle liegt (falls festgelegt)
        /// </summary>
        /// <param name="position">Zu prüfende Position</param>
        /// <returns>true, wenn die Position in der Zielzelle liegt</returns>
        public bool IsPositionInsideDestinationCell(Vector3 position)
        {
            if (Destination != null)
            {
                FlowFieldCell result = GetCellFromWorldPosition(position);
                return result == Destination;
            }
            return false;
        }

        /// <summary>
        /// Gibt an, ob derzeit ein Zielpunkt im FlowField gesetzt ist
        /// </summary>
        public bool HasTarget
        {
            get
            {
                return Destination != null;
            }
        }

        /// <summary>
        /// Löscht ein aktuell festgesetztes Ziel im FlowField
        /// </summary>
        public void UnsetTarget()
        {
            if (Destination != null)
            {
                lock (Destination)
                {
                    Destination = null;
                    _targetIsUpdated = false;
                }
            }
            else
            {
                Destination = null;
                _targetIsUpdated = false;
            }
        }

        #region Internals
        internal void UpdateCostField()
        {
            if (_updateCostField)
            {
                List<GameObject> checkObjects = new List<GameObject>();
                lock (KWEngine.CurrentWorld._gameObjects)
                {
                    foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                    {
                        if (HelperGeneral.IsObjectClassOrSubclassOfTypes(_types, g))
                        {
                            if (Contains(g))
                            {
                                checkObjects.Add(g);
                            }
                        }
                    }
                }
                foreach (FlowFieldCell cell in Grid)
                {
                    bool hasIncreasedCost = false;
                    cell.Cost = 1;
                    cell.BestCost = uint.MaxValue;

                    foreach (GameObject g in checkObjects)
                    {
                        Vector3 rayStart = new Vector3(cell.WorldPos.X, cell.WorldPos.Y + GridCellCount.Y * 0.5f + KWEngine.RAYTRACE_SAFETY_SQ, cell.WorldPos.Z);
                        if (HelperIntersection.RaytraceObjectFast(g, rayStart, -Vector3.UnitY))
                        {
                            if (!hasIncreasedCost)
                            {
                                cell.SetCostTo(g.FlowFieldCost);
                                hasIncreasedCost = true;
                            }
                        }
                    }
                }
                _updateCostField = false;
            }
        }

        internal void CreateGrid()
        {
            Grid = new FlowFieldCell[GridCellCount.X, GridCellCount.Z];
            for (int x = 0; x < GridCellCount.X; x++)
            {
                for (int z = 0; z < GridCellCount.Z; z++)
                {
                    Vector3 worldPos = new Vector3(Center.X - (GridCellCount.X / 2f) * _cellDiametre, Center.Y, Center.Z - (GridCellCount.Z / 2f) * _cellDiametre) + new Vector3(_cellDiametre * x + CellRadius, 0, _cellDiametre * z + CellRadius);
                    Grid[x, z] = new FlowFieldCell(worldPos, new Vector2i(x, z), this);
                }
            }
        }
        internal FlowFieldCell GetCellAtRelativePos(Vector2i originalPos, Vector2i relativePos)
        {
            Vector2i pos = originalPos + relativePos;
            if (pos.X < 0 || pos.X >= GridCellCount.X || pos.Y < 0 || pos.Y >= GridCellCount.Z)
            {
                return null;
            }
            else
            {
                return Grid[pos.X, pos.Y];
            }
        }

        internal FlowFieldCell GetCellFromWorldPosition(Vector3 worldPosition)
        {
            float percentX = (worldPosition.X - Center.X) / ((GridCellCount.X) * _cellDiametre);
            float percentZ = (worldPosition.Z - Center.Z) / ((GridCellCount.Z) * _cellDiametre);

            percentX = MathHelper.Clamp(percentX + 0.5f, 0f, 1f);
            percentZ = MathHelper.Clamp(percentZ + 0.5f, 0f, 1f);

            int x = MathHelper.Clamp((int)(GridCellCount.X * percentX), 0, GridCellCount.X - 1);
            int z = MathHelper.Clamp((int)(GridCellCount.Z * percentZ), 0, GridCellCount.Z - 1);

            return Grid[x, z];
        }

        internal void CreateFlowField()
        {
            foreach (FlowFieldCell currentCell in Grid)
            {
                List<FlowFieldCell> currentNeighbours = GetNeighbourCells(currentCell._gridIndex, FlowFieldCellDirection.AllDirections);
                uint bestCost = currentCell.BestCost;
                foreach (FlowFieldCell currentNeighbour in currentNeighbours)
                {
                    if (currentNeighbour.BestCost < bestCost)
                    {
                        bestCost = currentNeighbour.BestCost;
                        currentCell.BestDirection = FlowFieldCellDirection.GetDirectionFromVector2i(currentNeighbour._gridIndex - currentCell._gridIndex);
                    }
                }
            }
        }

        internal List<FlowFieldCell> GetNeighbourCells(Vector2i gridIndex, List<FlowFieldCellDirection> directions)
        {
            List<FlowFieldCell> neighbourCells = new List<FlowFieldCell>();
            foreach (Vector2i currentDirection in directions)
            {
                FlowFieldCell newNeighbour = GetCellAtRelativePos(gridIndex, currentDirection);
                if (newNeighbour != null)
                {
                    neighbourCells.Add(newNeighbour);
                }
            }
            return neighbourCells;
        }

        internal void UpdateFlowField()
        {
            if (_targetIsUpdated)
            {
                FlowFieldCell destination = GetCellFromWorldPosition(_target);
                if (destination == null)
                {
                    Destination = null;
                }
                else
                {
                    Destination = destination;
                }
                _targetIsUpdated = false;
            }


            if (Destination != null)
            {
                lock (Destination)
                {
                    foreach (FlowFieldCell cell in Grid)
                    {
                        cell.BestCost = uint.MaxValue;
                    }

                    Queue<FlowFieldCell> cellsToCheck = new Queue<FlowFieldCell>();
                    cellsToCheck.Enqueue(Destination);

                    while (cellsToCheck.Count > 0)
                    {
                        FlowFieldCell currentCell = cellsToCheck.Dequeue();
                        List<FlowFieldCell> currentNeighbours = GetNeighbourCells(currentCell._gridIndex, FlowFieldCellDirection.CardinalDirections);
                        foreach (FlowFieldCell currentNeighbour in currentNeighbours)
                        {
                            if (currentNeighbour.Cost == byte.MaxValue) { continue; }
                            if (currentNeighbour.Cost + currentCell.BestCost < currentNeighbour.BestCost)
                            {
                                currentNeighbour.BestCost = currentNeighbour.Cost + currentCell.BestCost;
                                cellsToCheck.Enqueue(currentNeighbour);
                            }
                        }
                    }
                    CreateFlowField();
                }
            }
        }

        internal FlowFieldCell[,] Grid { get; private set; }
        internal FlowFieldCell Destination { get; private set; }
        internal Vector3 _target = Vector3.Zero;
        internal bool _targetIsUpdated = false;
        internal bool _updateCostField = false;
        internal Type[] _types;
        internal float _cellDiametre;
        internal float _ffleft;  
        internal float _ffright; 
        internal float _ffback;  
        internal float _fffront; 
        internal float _fftop;   
        internal float _ffbottom;
        #endregion
    }
}
