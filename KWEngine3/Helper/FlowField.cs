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
        /// Erfragt oder setzt den Namen der Instanz
        /// </summary>
        public string Name { get; set; } = "undefined FlowField name";

        /// <summary>
        /// Anzahl der Zellen im Feld (in allen drei Dimensionen, wobei nur X- und Z-Dimension zählen)
        /// </summary>
        public Vector3i GridCellCount { get; internal set; }
        /// <summary>
        /// Radius einer Zelle
        /// </summary>
        public float CellRadius { get; internal set; }
        /// <summary>
        /// Mittelpunkt der FlowField-Instanz
        /// </summary>
        public Vector3 Center { get; internal set; }
        /// <summary>
        /// Gibt an, ob die Instanz zu Debugging-Zwecken sichtbar gemacht werden soll
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gibt die letztbekannte Zielposition innerhalb des Feldes an
        /// </summary>
        public Vector3 TargetPosition { get { return _target.Xyz; } }

        /// <summary>
        /// Messmodus für die Erstellung der Streckenkosten (Simple oder Box)
        /// </summary>
        public FlowFieldMode Mode { get; internal set; }

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
        /// <param name="mode">Genauigkeit bei der Messung der Hindernisse (Simple oder Box)</param>
        /// <param name="types">Liste der Klassen, die das FlowField scannen soll</param>
        public FlowField(float positionX, float positionY, float positionZ, int cellCountX, int cellCountZ, float cellRadius, int fieldHeight, FlowFieldMode mode, params Type[] types)
            : this(new Vector3(positionX, positionY, positionZ), cellCountX, cellCountZ, cellRadius, fieldHeight, mode, types)
        {

        }

        /// <summary>
        /// Erzeugt ein FlowField für die angegebenen GameObject-Typen
        /// </summary>
        /// <param name="center">Mittelpunkt des Felds</param>
        /// <param name="cellCountX">Anzahl Zellen in X-Richtung</param>
        /// <param name="cellCountZ">Anzahl Zellen in Z-Richtung</param>
        /// <param name="cellRadius">Radius je Zelle (minimum: 0.1f)</param>
        /// <param name="fieldHeight">Höhe des Felds</param>
        /// <param name="mode">Genauigkeit bei der Messung der Hindernisse (Simple oder Box)</param>
        /// <param name="types">Liste der Klassen, die das FlowField scannen soll</param>
        /// <exception cref="Exception">Eine genannte Klasse erbt nicht von GameObject</exception>
        public FlowField(Vector3 center, int cellCountX, int cellCountZ, float cellRadius, int fieldHeight, FlowFieldMode mode, params Type[] types)
        {
            Mode = mode;
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

            CellRadius = Math.Max(Math.Abs(cellRadius), 0.1f);
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

            _hitbox = new FlowFieldHitbox(CellRadius, GridCellCount.Y * 0.5f);

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
            bool overlapY = (g.AABBHigh >= _ffbottom && g.AABBHigh <= _fftop) || (g.AABBLow <= _fftop && g.AABBLow >= _ffbottom) || (g.AABBLow <= _ffbottom && g.AABBHigh >= _fftop);
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
            _updateCostField = Mode == FlowFieldMode.Simple ? 1 : 2;
        }

        /// <summary>
        /// Berechnet die Richungsanweisungen im gesamten FlowField für die neue Zielposition
        /// </summary>
        /// <param name="position">Neue Positionsangabe</param>
        /// <param name="clampToGrid">Gibt an, ob die Zielposition auf die nächstgelegene FlowField-Zelle angepasst werden soll, wenn es außerhalb der FlowField-Instanz liegt (Standard: false)</param>
        public void SetTarget(Vector3 position, bool clampToGrid = false)
        {
            bool contains = this.Contains(position);
            if (!clampToGrid && !contains)
            {
                UnsetTarget();
                return;
            }

            if (Destination != null)
            {
                lock (Destination)
                {
                    _target = new Vector4(position, 1f);
                    _targetIsUpdated = true;
                }
            }
            else
            {
                _target = new Vector4(position, 1f);
                _targetIsUpdated = true;
            }

        }

        /// <summary>
        /// Gibt die (normalisierte) Luftlinienrichtung von der angegebenen Position zum aktuellen Ziel zurück.
        /// </summary>
        /// <param name="position">Zielposition</param>
        /// <returns>Luftlinienrichtung</returns>
        public Vector3 GetLinearDirectionForPosition(Vector3 position)
        {
            if(HasTarget)
            {
                return Vector3.NormalizeFast(new Vector3(_target.X - position.X, 0, _target.Z - position.Z));
            }
            return Vector3.Zero;
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
        /// Erfragt Details zu der FlowField-Zelle, die der übergebenen Weltposition entspricht
        /// </summary>
        /// <param name="position">Position in Weltkoordinaten</param>
        /// <param name="clampToGrid">Gibt an, ob die Position auf die nächstgelegene Zelle angepasst werden soll, wenn sie eigentlich außerhalb des Grids liegt (Standard: false)</param>
        /// <returns>Zell-Instanz falls das FlowField diese Position abdeckt - null, falls die Position außerhalb des FlowFields liegt</returns>
        public FlowFieldCell GetCellForWorldPosition(Vector3 position, bool clampToGrid = false)
        {
            return GetCellFromWorldPosition(position, clampToGrid);
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
                FlowFieldCell result = GetCellFromWorldPosition(position, false);
                return result == Destination;
            }
            return false;
        }

        /// <summary>
        /// Aktualisiert den Mittelpunkt der FlowField-Instanz (erzeugt eine Verschiebung des FlowFields)
        /// </summary>
        /// <param name="x">Verschiebung des FlowField in x-Richtung</param>
        /// <param name="z">Verschiebung des FlowField in z-Richtung</param>
        public void SetPosition(float x, float z)
        {
            Center = new Vector3(x, Center.Y, z);
            _ffleft = Center.X - GridCellCount.X * CellRadius;
            _ffright = Center.X + GridCellCount.X * CellRadius;
            _ffback = Center.Z - GridCellCount.Z * CellRadius;
            _fffront = Center.Z + GridCellCount.Z * CellRadius;
            _fftop = Center.Y + GridCellCount.Y * 0.5f;
            _ffbottom = Center.Y - GridCellCount.Y * 0.5f;
            UpdateGridAndTarget();
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
            _targetIsUpdated = true;
            _target.W = 0f;
        }

        #region Internals
        internal void UpdateCostField()
        {
            if (_updateCostField > 0)
            {
                List<GameObject> checkObjects = new List<GameObject>();
                lock (KWEngine.CurrentWorld._gameObjects)
                {
                    foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
                    {
                        if (HelperGeneral.IsObjectClassOrSubclassOfTypes(_types, g))
                        {
                            if (g.FlowFieldCost > 1 && Contains(g))
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
                        if (_updateCostField == 1)
                        {
                            Vector3 rayStart = new Vector3(cell.Position.X, cell.Position.Y + GridCellCount.Y * 0.5f + KWEngine.RAYTRACE_SAFETY_SQ, cell.Position.Z);
                            if (HelperIntersection.RaytraceObjectFast(g, rayStart, -Vector3.UnitY))
                            {
                                if (!hasIncreasedCost)
                                {
                                    cell.SetCostTo(g.FlowFieldCost);
                                    hasIncreasedCost = true;
                                }
                            }
                        }
                        else
                        {
                            _hitbox.Update(cell.Position.X, cell.Position.Y, cell.Position.Z);
                            foreach(GameObjectHitbox ghb in g._colliderModel._hitboxes)
                            {
                                if(HelperIntersection.TestIntersection(_hitbox, ghb))
                                {
                                    cell.SetCostTo(g.FlowFieldCost);
                                    hasIncreasedCost = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                _updateCostField = 0;
            }
        }

        internal void UpdateGridAndTarget()
        {
            for (int x = 0; x < GridCellCount.X; x++)
            {
                for (int z = 0; z < GridCellCount.Z; z++)
                {
                    Vector3 worldPos = new Vector3(Center.X - (GridCellCount.X / 2f) * _cellDiametre, Center.Y, Center.Z - (GridCellCount.Z / 2f) * _cellDiametre) + new Vector3(_cellDiametre * x + CellRadius, 0, _cellDiametre * z + CellRadius);
                    Grid[x, z].SetPosition(worldPos);
                }
            }
            SetTargetAfterRepositioning();
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

        internal FlowFieldCell GetCellFromWorldPosition(Vector3 worldPosition, bool clampToGrid = true)
        {
            float percentX = (worldPosition.X - Center.X) / (GridCellCount.X * _cellDiametre);
            float percentZ = (worldPosition.Z - Center.Z) / (GridCellCount.Z * _cellDiametre);

            if(clampToGrid)
            {
                percentX = MathHelper.Clamp(percentX + 0.5f, 0f, 1f);
                percentZ = MathHelper.Clamp(percentZ + 0.5f, 0f, 1f);
            }
            else
            {
                percentX = percentX + 0.5f;
                percentZ = percentZ + 0.5f;
            }

            int x, z;
            if (clampToGrid)
            {
                x = MathHelper.Clamp((int)(GridCellCount.X * percentX), 0, GridCellCount.X - 1);
                z = MathHelper.Clamp((int)(GridCellCount.Z * percentZ), 0, GridCellCount.Z - 1);
                return Grid[x, z];
            }
            else
            {
                if (percentX > 1f || percentX < 0f || percentZ > 1f || percentZ < 0f)
                    return null;
                x = (int)(GridCellCount.X * percentX);
                z = (int)(GridCellCount.Z * percentZ);
                if (x < 0 || x >= GridCellCount.X || z < 0 || z >= GridCellCount.Z)
                {
                    return null;
                }
                else
                {
                    return Grid[x, z];
                }
            }
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
                if (_target.W > 0f)
                {
                    FlowFieldCell destination = GetCellFromWorldPosition(_target.Xyz, false);
                    if (destination == null)
                    {
                        Destination = null;
                    }
                    else
                    {
                        Destination = destination;
                    }
                }
                else
                {
                    Destination = null;
                }
                _targetIsUpdated = false;
            }
            else
            {
                return;
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

        internal void SetTargetAfterRepositioning()
        {
            Update();
            bool contains = this.Contains(_target.Xyz);
            if (!contains)
            {
                UnsetTarget();
                return;
            }

            if (Destination != null)
            {
                _targetIsUpdated = true;
                _target.W = 1f;
            }
        }

        internal FlowFieldHitbox _hitbox;
        internal FlowFieldCell[,] Grid { get; private set; }
        internal FlowFieldCell Destination { get; private set; }
        internal Vector4 _target = Vector4.Zero;
        internal bool _targetIsUpdated = false;
        internal int _updateCostField = 0;
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
