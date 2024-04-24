using KWEngine3.Helper;
using KWEngine3.Model;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// GameObject-Klasse
    /// </summary>
    public abstract class GameObject : EngineObject, IComparable<GameObject>
    {
        /// <summary>
        /// Interne ID des Objekts
        /// </summary>
        public int ID { get; internal set; } = -1;

        /// <summary>
        /// Gibt die Kosten dieser Instanz auf einem (optionalen) Flowfield an (1 = kein Hindernis, 255 = unüberwindbares Hindernis; Standardwert: 1)
        /// </summary>
        public byte FlowFieldCost {
            get
            {
                return _flowfieldcost;
            }
            set
            {
                if (value == 0)
                    value = 1;
                else if (value > 255)
                    value = 255;
                _flowfieldcost = value;
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt ein Kollisionen erzeugen und überprüfen kann
        /// </summary>
        public bool IsCollisionObject 
        { 
            get 
            {
                return _isCollisionObject;
            }
            set
            {
                if (this.ID > 0)
                {
                    if (_isCollisionObject != value)
                    {
                        KWEngine.CurrentWorld._gameObjectsColliderChange.Add(this);
                        if (value == true)
                        {
                            _addRemoveHitboxes = AddRemoveHitboxMode.Add;
                        }
                        else
                        {
                            _addRemoveHitboxes = AddRemoveHitboxMode.Remove;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("!!");
                }
                _isCollisionObject = value;
            }

        }

        /// <summary>
        /// Löscht ein ggf. verwendetes benutzerdefiniertes Collider-Modell für die aktuelle Instanz
        /// </summary>
        public void UnsetColliderModel()
        {
            if (this.ID > 0)
            {
                KWEngine.CurrentWorld._gameObjectsColliderChange.Add(this);
                _addRemoveHitboxes = AddRemoveHitboxMode.AddDefaultRemoveCustom;
                _colliderModel._hitboxesNew.Clear();
                foreach(GeoMeshHitbox gmh in this._model.ModelOriginal.MeshCollider.MeshHitboxes)
                {
                    _colliderModel._hitboxesNew.Add(new GameObjectHitbox(this, gmh));
                }
            }
            else
            {
                foreach (GameObjectHitbox ghb in _colliderModel._hitboxes)
                {
                    if (ghb.IsActive)
                        KWEngine.CurrentWorld._gameObjectHitboxes.Remove(ghb);
                }
                _colliderModel._hitboxes.Clear();

                foreach (GeoMeshHitbox gmh in _model.ModelOriginal.MeshCollider.MeshHitboxes)
                {
                    GameObjectHitbox ghbNew = new GameObjectHitbox(this, gmh);
                    _colliderModel._hitboxes.Add(ghbNew);
                }
                UpdateModelMatrixAndHitboxes();
                HelperSweepAndPrune.SweepAndPrune();
            }
        }

        /// <summary>
        /// Setzt ein benutzerdefiniertes Collider-Modell für die GameObject-Instanz (muss zuvor via KWEngine.LoadModelCollider() importiert worden sein)
        /// </summary>
        /// <param name="colliderModelName">Name des Collider-Modells</param>
        public void SetColliderModel(string colliderModelName)
        {
            if (KWEngine.CustomColliders.ContainsKey(colliderModelName) == false)
            {
                KWEngine.LogWriteLine("[GameObject] Cannot set custom collider model - name not found in database");
                return;
            }

            if (this.ID > 0)
            {
                if (_isCollisionObject == true)
                {
                    KWEngine.CurrentWorld._gameObjectsColliderChange.Add(this);
                    _addRemoveHitboxes = AddRemoveHitboxMode.AddCustomRemoveDefault;
                }
            }
            else
            {
                foreach (GameObjectHitbox ghb in _colliderModel._hitboxes)
                {
                    if (ghb.IsActive)
                        KWEngine.CurrentWorld._gameObjectHitboxes.Remove(ghb);
                }
                _colliderModel._hitboxes.Clear();
                

                GeoMeshCollider meshCollider = KWEngine.CustomColliders[colliderModelName];
                foreach(GeoMeshHitbox gmh in meshCollider.MeshHitboxes)
                {
                    GameObjectHitbox ghbNew = new GameObjectHitbox(this, gmh);
                    _colliderModel._hitboxes.Add(ghbNew);
                    KWEngine.CurrentWorld._gameObjectHitboxes.Add(ghbNew);
                }
                UpdateModelMatrixAndHitboxes();
                HelperSweepAndPrune.SweepAndPrune();
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt in der Liste aller Objekte zuletzt aktualisiert werden soll (z.B. für Spielerfiguren)
        /// </summary>
        public bool UpdateLast { get; set; } = false;

        /// <summary>
        /// Standardkonstruktor (erzeugt mit einem Würfel als 3D-Modell)
        /// </summary>
        public GameObject()
            : this("KWCube")
        {

        }
        /// <summary>
        /// Konstruktormethode, der der 3D-Modellname mitgegeben werden kann
        /// </summary>
        /// <param name="modelname">Name des zu verwendenden 3D-Modells</param>
        public GameObject(string modelname)
        {
            bool modelSetSuccessfully = SetModel(modelname);
            if(!modelSetSuccessfully)
            {
                KWEngine.LogWriteLine("[GameObject] Cannot set " + (modelname == null ? "" : modelname.Trim()));
            }

            InitStates();
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">Position auf x-Achse</param>
        /// <param name="y">Position auf y-Achse</param>
        /// <param name="z">Position auf z-Achse</param>
        /// <param name="mode">Bestimmt, auf welchen Fixpunkt des Objekts sich die Positionsangaben beziehen sollen</param>
        public void SetPosition(float x, float y, float z, PositionMode mode = PositionMode.Position)
        {
            if(mode == PositionMode.Position)
            {
                base.SetPosition(x, y, z);
            }
            else if(mode == PositionMode.CenterOfHitbox)
            {
                Vector3 delta = this.Position - this.Center;
                base.SetPosition(x + delta.X, y + delta.Y, z + delta.Z);
            }
            else
            {
                Vector3 delta = this.Position - new Vector3(Position.X, AABBLow, Position.Z);
                base.SetPosition(x + delta.X, y + delta.Y, z + delta.Z);
            }
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="position">Position auf XYZ-Achse</param>
        /// <param name="mode">Bestimmt, auf welchen Fixpunkt des Objekts sich die Positionsangaben beziehen sollen</param>
        public void SetPosition(Vector3 position, PositionMode mode = PositionMode.Position)
        {
            this.SetPosition(position.X, position.Y, position.Z, mode);
        }

        /// <summary>
        /// Setzt die y-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="y">Position auf y-Achse</param>
        /// <param name="mode">Bestimmt, auf welchen Fixpunkt des Objekts sich die Positionsangaben beziehen sollen</param>
        public void SetPositionY(float y, PositionMode mode = PositionMode.Position)
        {
            if (mode == PositionMode.Position)
            {
                base.SetPositionY(y);
            }
            else if (mode == PositionMode.CenterOfHitbox)
            {
                float delta = this.Position.Y - this.Center.Y;
                base.SetPositionY(y + delta);
            }
            else
            {
                float delta = this.Position.Y - (this.Center.Y - _obbRadii.Y);
                base.SetPositionY(y + delta);
            }
        }

        /// <summary>
        /// Prüft auf eine Kollision mit einem Terrain-Objekt auf der vertikalen Weltachse und erzeugt einen Vector, der das
        /// aufrufende Objekt stets auf die Höhe des Terrains verschiebt
        /// </summary>
        /// <returns>Kollisionsobjekt mit weiteren Details</returns>
        public IntersectionTerrain GetIntersectionWithTerrain()
        {
            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                GameObjectHitbox lowestHitbox = null;
                float y = float.MaxValue;
                foreach (GameObjectHitbox hbcaller in _colliderModel._hitboxes)
                {
                    if (hbcaller.IsActive && hbcaller._colliderType == ColliderType.ConvexHull && hbcaller._low < y)
                    {
                        y = hbcaller._low;
                        lowestHitbox = hbcaller;
                    }
                }
                if(lowestHitbox != null)
                {
                    IntersectionTerrain it = HelperIntersection.TestIntersectionTerrain(lowestHitbox, t._hitboxes[0]);
                    if (it != null)
                    {
                        return it;
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// Prüft auf Kollisionen mit PlaneCollider-Instanzen (EXPERIMENTELL)
        /// </summary>
        /// <returns>Liste der gefundenen Kollisionen</returns>
        public List<Intersection> GetIntersectionsWithPlaneColliders()
        {
            return GetIntersectionsWithPlaneColliders(Vector3.Zero);
        }

        /// <summary>
        /// Prüft auf Kollisionen mit PlaneCollider-Instanzen (EXPERIMENTELL)
        /// </summary>
        /// <param name="offset">Verschiebung der aufrufenden Instanz vor der Kollisionsprüfung</param>
        /// <returns>Liste der gefundenen Kollisionen</returns>
        public List<Intersection> GetIntersectionsWithPlaneColliders(Vector3 offset)
        {
            List<Intersection> intersections = new();
            
            foreach (GameObjectHitbox ghb in _collisionCandidates)
            {
                if (ghb._colliderType == ColliderType.PlaneCollider)
                {
                    foreach (GameObjectHitbox hbcaller in _colliderModel._hitboxes)
                    {
                        if (hbcaller.IsActive == false)
                            continue;

                        List<Vector3> normals = new();
                        foreach (GeoMeshFace face in ghb._mesh.Faces)
                        {
                            Vector3 n = ghb._normals[face.Normal] * (face.Flip ? -1f : 1f);
                            List<Vector3> faceVertices = new();
                            Vector3 centerTemp = Vector3.Zero;
                            foreach(int fvi in face.Vertices)
                            {
                                faceVertices.Add(ghb._vertices[fvi]);
                                centerTemp += faceVertices[faceVertices.Count - 1];
                            }
                            centerTemp /= face.VertexCount;

                            Intersection i = HelperIntersection.TestIntersectionForPlaneFace(hbcaller, faceVertices, n, centerTemp, offset, ghb);
                            if (i != null && normals.Contains(i.MTV) == false)
                            {
                                normals.Add(i.MTV);
                                intersections.Add(i);
                            }
                        }
                    }
                }
            }
            
            return intersections;
        }
        

        /// <summary>
        /// Prüft, ob das Objekt gerade mit anderen Objekten kollidiert und gibt die erstbeste Kollision zurück
        /// </summary>
        /// <returns>zuerst gefundene Kollision</returns>
        public Intersection GetIntersection()
        {
            if (!_isCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if (hbother.Owner.ID > 0 && hbother._colliderType == ColliderType.ConvexHull)
                {
                    foreach (GameObjectHitbox hbcaller in this._colliderModel._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother, HelperVector.VectorZero);
                        if (i != null && i.Object.ID > 0)
                            return i;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Prüft, ob das Objekt gerade mit anderen Objekten eines bestimmten Typs kollidiert und gibt die erstbeste Kollision zurück
        /// </summary>
        /// <typeparam name="T">Objekttyp</typeparam>
        /// <returns>zuerst gefundene Kollision</returns>
        public Intersection GetIntersection<T>() where T : GameObject
        {
            if (!_isCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " is not a collision object.");
                return null;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if ((hbother.Owner is T) == false || hbother.Owner.ID <= 0)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._colliderModel._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother, HelperVector.VectorZero);
                    if (i != null)
                        return i;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Prüft ob Kollisionen mit umgebenden GameObject-Instanzen
        /// </summary>
        /// <returns>Liste mit allen gefundenen Kollisionen</returns>
        public List<Intersection> GetIntersections()
        {
            List<Intersection> intersections = new();
            if (!_isCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " is not a collision object.");
                return intersections;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if (hbother.Owner.ID > 0 && hbother._colliderType == ColliderType.ConvexHull)
                {
                    foreach (GameObjectHitbox hbcaller in this._colliderModel._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother, HelperVector.VectorZero);
                        if (i != null && i.Object.ID > 0)
                            intersections.Add(i);
                    }
                }
            }
            
            return intersections;
        }

        /// <summary>
        /// Prüft ob Kollisionen mit umgebenden GameObject-Instanzen eines bestimmten Typs
        /// </summary>
        /// <typeparam name="T">Klasse der zu prüfenden Instanzen</typeparam>
        /// <returns>Liste mit gefundenen Kollisionen</returns>
        public List<Intersection> GetIntersections<T>() where T : GameObject
        {
            List<Intersection> intersections = new();
            if (_isCollisionObject == false)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " is not a collision object.");
                return intersections;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if ((hbother.Owner is T) == false || hbother.Owner.ID <= 0)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._colliderModel._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother, HelperVector.VectorZero);
                    if (i != null && i.Object.ID > 0)
                        intersections.Add(i);
                }
            }
            
            return intersections;
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger auf der Hitbox des Objekts liegt
        /// </summary>
        /// <returns>true, wenn sich der Mauszeiger innerhalb der Hitbox des Objekts befindet</returns>
        public bool IsMouseCursorInsideMyHitbox()
        {
            return HelperIntersection.IsMouseCursorInsideHitbox(this, true);
        }

        /// <summary>
        /// Misst die Distanz zu einem Punkt
        /// </summary>
        /// <param name="position">Zielpunkt</param>
        /// <param name="absolute">wenn true, wird die Position statt des Hitbox-Mittelpunkts zur Berechnung verwendet</param>
        /// <returns>Distanz</returns>
        public float GetDistanceTo(Vector3 position, bool absolute = false)
        {
            if (absolute)
                return (Position - position).LengthFast;
            else
                return (Center - position).LengthFast;
        }

        /// <summary>
        /// Misst die Distanz zu einem GameObject
        /// </summary>
        /// <param name="g">GameObject-Instanz</param>
        /// <param name="absolute">wenn true, wird die Position statt des Hitbox-Mittelpunkts zur Berechnung verwendet</param>
        /// <returns>Distanz</returns>
        public float GetDistanceTo(GameObject g, bool absolute = false)
        {
            if (absolute)
                return (Position - g.Position).LengthFast;
            else
                return (Center - g.Center).LengthFast;
        }

        /// <summary>
        /// Verweis auf die Keyboard-Aktivitäten
        /// </summary>
        public KeyboardExt Keyboard { get { return KWEngine.Window._keyboard; } }
        /// <summary>
        /// Verweis auf die Mausaktivitäten
        /// </summary>
        public MouseExt Mouse { get { return KWEngine.Window._mouse; } }
        /// <summary>
        /// Gibt die Strecke an, die der Mauszeiger seit der letzten Überprüfung zurückgelegt hat
        /// </summary>
        public Vector2 MouseMovement
        {
            get
            {
                return KWEngine.Window._mouseDeltaToUse;
            }
        }

        /// <summary>
        /// Setzt die Größenskalierung der Objekt-Hitbox (muss > 0 sein)
        /// </summary>
        /// <param name="s">Skalierung</param>
        public void SetHitboxScale(float s)
        {
            SetHitboxScale(s, s, s);
        }

        /// <summary>
        /// Setzt die Größenskalierung der Objekt-Hitbox (muss > 0 sein)
        /// </summary>
        /// <param name="x">Skalierung in lokale x-Richtung</param>
        /// <param name="y">Skalierung in lokale x-Richtung</param>
        /// <param name="z">Skalierung in lokale x-Richtung</param>
        public void SetHitboxScale(float x, float y, float z)
        {
            if (x > float.Epsilon && y > float.Epsilon && z > float.Epsilon)
            {
                _stateCurrent._scaleHitbox = new Vector3(x, y, z);
            }
            else
            {
                _stateCurrent._scaleHitbox = Vector3.One;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Bewegt das Objekt entlang der Blickrichtung der Kamera
        /// </summary>
        /// <param name="move">1 = Vorwärts, -1 = Rückwärts</param>
        /// <param name="strafe">1 = Rechts, -1 = Links</param>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveAndStrafeAlongCamera(int move, int strafe, float units)
        {
            move = move > 0 ? 1 : move < 0 ? -1 : 0;
            strafe = strafe > 0 ? 1 : strafe < 0 ? -1 : 0;
            Vector3 lavForward = CurrentWorld._cameraGame._stateCurrent.LookAtVector * move;
            Vector3 tmp = CurrentWorld._cameraGame._stateCurrent.LookAtVector;
            Vector3 lavStrafe = Vector3.NormalizeFast(Vector3.Cross(tmp, KWEngine.WorldUp)) * strafe;
            Vector3 movement = Vector3.NormalizeFast(lavForward + lavStrafe);
            MoveOffset(movement * units);
        }

        /// <summary>
        /// Bewegt das Objekt entlang der Blickrichtung der Kamera (ohne Höhenunterschied)
        /// </summary>
        /// <param name="move">1 = Vorwärts, -1 = Rückwärts</param>
        /// <param name="strafe">1 = Rechts, -1 = Links</param>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveAndStrafeAlongCameraXZ(int move, int strafe, float units)
        {
            if (move == 0 && strafe == 0)
                return;
            move = move > 0 ? 1 : move < 0 ? -1 : 0;
            strafe = strafe > 0 ? 1 : strafe < 0 ? -1 : 0;
            units = Math.Abs(units);

            Vector3 lav = CurrentWorld._cameraGame._stateCurrent.LookAtVector;
            lav.Y = 0;
            Vector3 lavStrafe = Vector3.NormalizeFast(Vector3.Cross(lav, KWEngine.WorldUp)) * strafe;
            Vector3 lavForward = Vector3.NormalizeFast(lav) * move;
            Vector3 movement = Vector3.NormalizeFast(lavForward + lavStrafe);
            MoveOffset(movement * units);
        }

        /// <summary>
        /// Gibt an, ob dieses Objekt an einem anderen Objekt angeheftet wurde
        /// </summary>
        public bool IsAttachedToGameObject { get { return _attachedTo != null; } }

        /// <summary>
        /// Prüft, ob das Objekt in Richtung des gegebenen Punkts blickt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <param name="diameter">Durchmesser um den Punkt</param>
        /// <param name="offsetY">(optionale) y-Verschiebung der Blickrichtung</param>
        /// <returns>true, wenn die aktuelle Blickrichtung den Punkt inkl. Durchmesser schneidet</returns>
        public bool IsLookingAt(float x, float y, float z, float diameter, float offsetY = 0)
        {
            return IsLookingAt(new Vector3(x, y, z), diameter, offsetY);
        }

        /// <summary>
        /// Prüft, ob das Objekt in Richtung des gegebenen Punkts blickt
        /// </summary>
        /// <param name="target">Zielposition</param>
        /// <param name="diameter">Durchmesser um das Ziel</param>
        /// <param name="offsetY">(optionale) y-Verschiebung der Blickrichtung</param>
        /// <returns>true, wenn die aktuelle Blickrichtung den Punkt inkl. Durchmesser schneidet</returns>
        public bool IsLookingAt(Vector3 target, float diameter, float offsetY = 0f)
        {
            Vector3 position = Center + new Vector3(0, offsetY, 0);
            Vector3 deltaGO = target - position;
            Vector3 rayDirection = LookAtVector;
            Vector3[] aabb = new Vector3[] { new(-0.5f * diameter, -0.5f * diameter, -0.5f * diameter), new(0.5f * diameter, 0.5f * diameter, 0.5f * diameter) };

            Matrix4 matrix = Matrix4.CreateTranslation(target);
            Vector3 x = new(matrix.Row0);
            x.NormalizeFast();
            Vector3 y = new(matrix.Row1);
            y.NormalizeFast();
            Vector3 z = new(matrix.Row2);
            z.NormalizeFast();
            Vector3[] axes = new Vector3[] { x, y, z };

            float tMin = 0.0f;
            float tMax = 100000.0f;
            for (int i = 0; i < axes.Length; i++)
            {
                Vector3 axis = axes[i];

                Vector3.Dot(axis, deltaGO, out float e);
                Vector3.Dot(rayDirection, axis, out float f);
                float t1 = (e + aabb[0][i]) / f; 
                float t2 = (e + aabb[1][i]) / f; 
                if (t1 > t2)
                {
                    float w = t1;
                    t1 = t2;
                    t2 = w;
                }
                if (t2 < tMax) tMax = t2;
                if (t1 > tMin) tMin = t1;


                if (tMax < tMin)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Bindet eine andere GameObject-Instanz an den jeweiligen Knochen des aktuell verwendeten Modells
        /// </summary>
        /// <param name="g">Anzuhängende Instanz</param>
        /// <param name="boneName">Name des Knochens, an den die Instanz angehängt werden soll</param>
        public void AttachGameObjectToBone(GameObject g, string boneName)
        {
            GeoModel modelToBeAttached = g._model.ModelOriginal;
            if (g.IsAttachedToGameObject == false && modelToBeAttached != null && _model.ModelOriginal.BoneNames.IndexOf(boneName) >= 0)
            {
                GeoNode node = GeoNode.FindChild(_model.ModelOriginal.Armature, boneName);
                if (node != null)
                {
                    if (_gameObjectsAttached.ContainsKey(node) == false)
                    {
                        g.SetRotation(0, 0, 0);
                        g.SetScale(1, 1, 1);
                        g._attachedTo = this;
                        _gameObjectsAttached.Add(node, g);
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[GameObject] Bone already in use");
                    }
                }
            }
            else
            {
                if(modelToBeAttached == null)
                    KWEngine.LogWriteLine("[GameObject] Unknown model - cannot use attachment feature");
                else if(_model.ModelOriginal.BoneNames.IndexOf(boneName) < 0)
                    KWEngine.LogWriteLine("[GameObject] Unknown bone name - cannot use attachment feature");
            }
        }

        /// <summary>
        /// Entfernt die Bindung (Attachment) einer GameObject-Instanz 
        /// </summary>
        /// <param name="boneName">Name des Knochens</param>
        public void DetachGameObjectFromBone(string boneName)
        {
            if (_model.ModelOriginal.BoneNames.IndexOf(boneName) >= 0)
            {
                GeoNode node = GeoNode.FindChild(_model.ModelOriginal.Armature, boneName);
                if (node != null)
                {
                    if (_gameObjectsAttached.ContainsKey(node))
                    {
                        GameObject attachment = _gameObjectsAttached[node];

                        attachment._attachedTo = null;
                        _gameObjectsAttached.Remove(node);

                        Vector3 position = attachment._stateCurrent._modelMatrix.ExtractTranslation();
                        Quaternion rotation = attachment._stateCurrent._modelMatrix.ExtractRotation();
                        Vector3 scale = attachment._stateCurrent._modelMatrix.ExtractScale();
                        attachment.SetPosition(position);
                        attachment.SetRotation(rotation);
                        attachment.SetScale(scale.X, scale.Y, scale.Z);
                    }
                    else
                    {
                        KWEngine.LogWriteLine("[GameObject] No object to detach from bone");
                    }
                }
            }
            else
            {
                KWEngine.LogWriteLine("[GameObject] Invalid bone name");
            }
        }

        /// <summary>
        /// Liefert true, wenn mind. eine GameObject-Instanz an einen Knochen des aufrufenden Objekts gebunden ist
        /// </summary>
        public bool HasAttachedGameObjects { get { return _gameObjectsAttached.Count > 0; } }

        /// <summary>
        /// Liefert eine Liste der Knochennamen, an die aktuell eine andere GameObject-Instanz gebunden ist
        /// </summary>
        /// <returns>Liste der verwendeten Knochennamen (für Attachments)</returns>
        public List<string> GetBoneNamesForAttachedGameObject()
        {
            List<string> boneNames = new();
            foreach (GeoNode n in _gameObjectsAttached.Keys)
            {
                boneNames.Add(n.Name);
            }
            return boneNames;
        }

        /// <summary>
        /// Liefert die Referenz auf das Objekt, an das die aktuelle Instanz gebunden ist
        /// </summary>
        /// <returns>GameObject, an das die Instanz (via Knochen) gebunden ist</returns>
        public GameObject GetGameObjectThatIAmAttachedTo()
        {
            return _attachedTo;
        }

        /// <summary>
        /// Liefert die an einen Knochen gebundene GameObject-Instanz
        /// </summary>
        /// <param name="boneName">Knochenname</param>
        /// <returns>Gebundene GameObject-Instanz</returns>
        public GameObject GetAttachedGameObjectForBone(string boneName)
        {
            GeoNode node = GeoNode.FindChild(_model.ModelOriginal.Armature, boneName);
            if (node != null)
            {
                if (_gameObjectsAttached.ContainsKey(node))
                {
                    return _gameObjectsAttached[node];
                }
            }
            return null;
        }

        /// <summary>
        /// Vergleicht das Objekt bzgl. seiner Entfernung zur Kamera mit einem anderen Objekt
        /// </summary>
        /// <param name="other">anderes GameObjekt</param>
        /// <returns>1, wenn das aufrufende Objekt näher an der Kamera ist, sonst -1</returns>
        public int CompareTo(GameObject other)
        {
            Vector3 camPos = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateCurrent._position: KWEngine.CurrentWorld.CameraPosition;

            float distanceToCameraThis = (this.Center - camPos).LengthSquared;
            float distanceToCameraOther = (other.Center - camPos).LengthSquared;
            return distanceToCameraOther > distanceToCameraThis ? 1 : -1;
        }

        /// <summary>
        /// Ersetzt die eigentliche Hitbox-Form mit der für Spielfiguren gängigen Kapselform
        /// </summary>
        /// <param name="meshIndex">Index des 3D-Mesh, für das die Hitbox getauscht werden soll</param>
        /// <param name="mode">Modus zur Bestimmung der richtigen Hitbox-Orientierung</param>
        /// <param name="type">Art/Form der Kapsel</param>
        public void SetHitboxToCapsuleForMesh(int meshIndex = 0, CapsuleHitboxMode mode = CapsuleHitboxMode.Default, CapsuleHitboxType type = CapsuleHitboxType.Default)
        {
            if(_colliderModel._hitboxes.Count > meshIndex)
            {
                Matrix4 meshTransform = HelperIntersection.CalculateMeshTransformForGameObject(this, meshIndex, mode);

                Vector3 currentHitboxCenter = Vector4.TransformRow(new Vector4(_colliderModel._hitboxes[meshIndex]._mesh.Center, 1.0f), meshTransform).Xyz;
                Vector3 frontbottomleft = Vector4.TransformRow(new Vector4(_colliderModel._hitboxes[meshIndex]._mesh.minX, _colliderModel._hitboxes[meshIndex]._mesh.minY, _colliderModel._hitboxes[meshIndex]._mesh.maxZ, 1f), meshTransform).Xyz;
                Vector3 backtopright = Vector4.TransformRow(new Vector4(_colliderModel._hitboxes[meshIndex]._mesh.maxX, _colliderModel._hitboxes[meshIndex]._mesh.maxY, _colliderModel._hitboxes[meshIndex]._mesh.minZ, 1f), meshTransform).Xyz;
                lock (CurrentWorld._gameObjectHitboxes)
                {
                    if (CurrentWorld._gameObjectHitboxes.Contains(this._colliderModel._hitboxes[meshIndex]))
                    {
                        CurrentWorld._gameObjectHitboxes.Remove(this._colliderModel._hitboxes[meshIndex]);
                    }
                    if(type == CapsuleHitboxType.Default)
                        this._colliderModel._hitboxes[meshIndex] = new GameObjectHitbox(this, KWEngine.KWCapsule.MeshCollider.MeshHitboxes[0], currentHitboxCenter, frontbottomleft, backtopright);
                    else
                        this._colliderModel._hitboxes[meshIndex] = new GameObjectHitbox(this, KWEngine.KWCapsule2.MeshCollider.MeshHitboxes[0], currentHitboxCenter, frontbottomleft, backtopright);
                    CurrentWorld._gameObjectHitboxes.Add(this._colliderModel._hitboxes[meshIndex]);
                    UpdateModelMatrixAndHitboxes();
                }
            }
        }

        /// <summary>
        /// Gibt Auskunft über den Prozentwert der aktuell gewählten Animation (Werte zwischen 0f und 1f)
        /// </summary>
        public float AnimationPercentage 
        { 
            get
            {
                return _stateCurrent._animationPercentage;
            }
        }

        /// <summary>
        /// Gibt an, welche Animations-ID für diese Instanz aktuell gewählt ist. Beträgt der Wert -1, ist keine Animation gewählt.
        /// </summary>
        public int AnimationID
        {
            get
            {
                return _stateCurrent._animationID;
            }
        }

        /// <summary>
        /// Gibt an, ob Änderungen an TextureOffset-Werten für die Render-Phase interpoliert werden (Standard: true) 
        /// (für 2D-Objekte mit Spritesheet-Animationen sollte dieser Wert auf 'false' gesetzt werden)
        /// </summary>
        public bool BlendTextureStates { get; set; } = true;

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekte des angegebenen Typs trifft (Präzise, aber langsam!)
        /// </summary>
        /// <remarks>Es werden nur Objekte in unmittelbarer Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersectionExt&gt; results = RaytraceObjectsNearby(new Vector3(0, 2, 0), -Vector3.UnitY, typeof(Floor), typeof(Wall));
        /// </code>
        /// </example>
        /// <param name="rayOrigin">Startpunkt des Strahls</param>
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <param name="typelist">Liste der Typen (Klassen), die für den Strahl getestet werden sollen</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersectionExt> RaytraceObjectsNearby(Vector3 rayOrigin, Vector3 rayDirectionNormalized, params Type[] typelist)
        {
            if(typelist.Length == 0)
            {
                KWEngine.LogWriteLine("[GameObject] WARNING: No list of types given for ray testing, assuming type 'GameObject'");
                typelist = new Type[] { typeof(GameObject) };
            }
            List<RayIntersectionExt> list = new();

            foreach (GameObjectHitbox hb in _collisionCandidates)
            {
                if (hb.IsActive && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, hb.Owner))
                {
                    if(rayDirectionNormalized == -Vector3.UnitY && !HelperIntersection.IsPointInsideRectangle(rayOrigin, hb._center, hb._dimensionsAABB.X, hb._dimensionsAABB.Z))
                    {
                        continue;
                    }
                    bool result = HelperIntersection.RaytraceHitbox(hb, rayOrigin, rayDirectionNormalized, out Vector3 intersectionPoint, out Vector3 faceNormal);
                    if (result == true)
                    {
                        RayIntersectionExt gd = new()
                        {
                            Distance = (intersectionPoint - rayOrigin).LengthFast,
                            Object = hb.Owner,
                            HitboxName = hb._mesh.Name,
                            IntersectionPoint = intersectionPoint,
                            SurfaceNormal = faceNormal
                        };
                        list.Add(gd);
                    }
                }
            }
            list.Sort();
            return list;
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekte des angegebenen Typs trifft (Präzise, aber langsam!)
        /// </summary>
        /// <remarks>Es werden nur Objekte in unmittelbarer Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersectionExt&gt; results = RaytraceObjectsNearby(0, 2, 0, -Vector3.UnitY, typeof(Floor), typeof(Wall));
        /// </code>
        /// </example>
        /// <param name="rayPositionX">X-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionY">Y-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionZ">Z-Komponente des Strahlstartpunkts</param>    
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <param name="typelist">Liste der Typen (Klassen), die für den Strahl getestet werden sollen</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersectionExt> RaytraceObjectsNearby(float rayPositionX, float rayPositionY, float rayPositionZ, Vector3 rayDirectionNormalized, params Type[] typelist)
        {
            return RaytraceObjectsNearby(new Vector3(rayPositionX, rayPositionY, rayPositionZ), rayDirectionNormalized, typelist);
        }

        /// <summary>
        /// Schießt mehrere Strahlen von der Mitte der Instanz nach unten und prüft, ob diese Strahlen in der Nähe liegende Objekte des angegebenen Typs treffen (Präzise, aber langsamer!)
        /// </summary>
        /// <param name="rayMode">Richtung und Anzahl der Teststrahlen</param>
        /// <param name="sizeFactor">Wenn mehrere Strahlen gewählt sind, werden diese innerhalb der Hitbox des Aufrufers gemäß dieses Faktors verteilt. Ist der Faktor 1, befinden sich die Strahlen an den Hitboxrändern. Ist er größer, befinden sich die Strahlen außerhalb der Hitbox. Ist der Faktor kleiner als 1, befinden sich die Strahlen weiter innerhalb der Hitbox</param>
        /// <param name="minDistance">Trifft ein Strahl ein Objekt, wird es als Ergebnis verworfen, wenn die Strahlendistanz kleiner als die angegebene Distanz ist (abhängig von der Größe des Aufrufers)</param>
        /// <param name="maxDistance">Trifft ein Strahl ein Objekt, wird es als Ergebnis verworfen, wenn die Strahlendistanz größer als die angegebene Distanz ist (abhängig von der Größe des Aufrufers)</param>
        /// <param name="typelist">Liste der Typen (Klassen), die für den Strahl getestet werden sollen</param>
        /// <returns>Ergebnis des Strahlentests</returns>
        public RayIntersectionExtSet RaytraceObjectsBelowPosition(RayMode rayMode, float sizeFactor, float minDistance, float maxDistance, params Type[] typelist)
        {
            Vector3 position = Center;
            Vector3 rayDirection;
            Vector3 offset;
            if(rayMode == RayMode.FourRaysZ || rayMode == RayMode.SingleZ)
            {
                rayDirection = -LookAtVector;
                offset = rayDirection * (_obbRadii.Z * 1f);
            }
            else
            {
                rayDirection = -LookAtVectorLocalUp;
                offset = rayDirection * (_obbRadii.Y * 1f);
            }
            position -= offset;

            if (typelist.Length == 0)
            {
                KWEngine.LogWriteLine("[GameObject] WARNING: No list of types given for ray testing, assuming type 'GameObject'");
                typelist = new Type[] { typeof(GameObject) };
            }
            
            List<HitboxFace> selectedFaces = new List<HitboxFace>();
            bool facesFound = false;
            foreach (GameObjectHitbox hb in _collisionCandidates)
            {
                if (hb.IsActive && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, hb.Owner))
                {
                    for (int j = 0; j < hb._mesh.Faces.Length; j++)
                    {
                        //if (hb.IsExtended)
                        //{
                            GeoMeshFace f = hb._mesh.Faces[j];
                            Span<Vector3> faceVertices = stackalloc Vector3[f.Vertices.Length];
                            hb.GetVerticesFromFace(j, ref faceVertices, out Vector3 currentFaceNormal);

                            if (hb.GetVerticesFromFaceAndCheckAngle(j, rayDirection, ref faceVertices, out HitboxFace face))
                            {
                                face.Owner = hb;
                                selectedFaces.Add(face);
                                facesFound = true;
                            }
                        //}
                        /*
                        else
                        {
                            if(hb.GetVerticesForCubeFaceAndCheckAngle(j, rayDirection, out HitboxFace face1, out HitboxFace face2))
                            {
                                facesFound = true;
                                face1.Owner = hb;
                                face2.Owner = hb;
                                selectedFaces.Add(face1);
                                selectedFaces.Add(face2);
                            }
                        }
                        */
                    }
                }
            }

            RayIntersectionExtSet grp = new RayIntersectionExtSet();
            if (facesFound)
            {
                // Get the results...
                Vector3 positionAvg = new Vector3();
                Vector3 positionNearest = new Vector3();
                Vector3 normalAvg = new Vector3();
                Vector3 normalNearest = new Vector3();
                float distanceMin = float.MaxValue;
                float distanceSum = 0f;
                GameObject gNearest = null;
                string hitboxname = "";
                
                Vector3[] rayOrigins;
                if (rayMode == RayMode.SingleY || rayMode == RayMode.SingleZ)
                {
                    rayOrigins = _rayOrigins1;
                    rayOrigins[0] = new Vector3(position);
                }
                else if(rayMode == RayMode.TwoRays2DPlatformerY)
                {
                    rayOrigins = _rayOrigins2;
                    rayOrigins[0] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor);
                    rayOrigins[1] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor);
                }
                else if(rayMode == RayMode.FourRaysZ)
                {
                    rayOrigins = _rayOrigins4;
                    rayOrigins[0] = new Vector3(position + LookAtVectorLocalUp * _obbRadii.Y * sizeFactor); // top
                    rayOrigins[1] = new Vector3(position - LookAtVectorLocalUp * _obbRadii.Y * sizeFactor); // bottom
                    rayOrigins[2] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // left
                    rayOrigins[3] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // right
                }
                else if(rayMode == RayMode.EightRaysY)
                {
                    rayOrigins = _rayOrigins8;
                    rayOrigins[0] = new Vector3(position + LookAtVector * _obbRadii.Z * sizeFactor); // front
                    rayOrigins[1] = new Vector3(position - LookAtVector * _obbRadii.Z * sizeFactor); // back
                    rayOrigins[2] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // left
                    rayOrigins[3] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // right
                    rayOrigins[4] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor + LookAtVector * _obbRadii.Z * sizeFactor);
                    rayOrigins[5] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor + LookAtVector * _obbRadii.Z * sizeFactor);
                    rayOrigins[6] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor - LookAtVector * _obbRadii.Z * sizeFactor);
                    rayOrigins[7] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor - LookAtVector * _obbRadii.Z * sizeFactor);
                }
                else
                {
                    rayOrigins = _rayOrigins4;
                    rayOrigins[0] = new Vector3(position + LookAtVector * _obbRadii.Z * sizeFactor); // front
                    rayOrigins[1] = new Vector3(position - LookAtVector * _obbRadii.Z * sizeFactor); // back
                    rayOrigins[2] = new Vector3(position - LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // left
                    rayOrigins[3] = new Vector3(position + LookAtVectorLocalRight * _obbRadii.X * sizeFactor); // right
                }

                int hits = 0;
                foreach (HitboxFace face in selectedFaces)
                {
                    for(int i = 0; i < rayOrigins.Length; i++)
                    {
                        bool hit = HelperIntersection.RayNGonIntersection(rayOrigins[i], rayDirection, face.Normal, face.Vertices, out Vector3 currentContact);
                        if (hit)
                        {
                            Vector3 delta = rayOrigins[i] + (offset * 2f) - currentContact;
                            float dotDelta = Vector3.Dot(-delta, rayDirection);
                            float currentDistance = (delta).LengthFast * (dotDelta >= 0 ? 1f : -1f);
                            if (currentDistance <= maxDistance && currentDistance >= minDistance)
                            {
                                hits++;
                                grp.AddObject(face.Owner.Owner);
                                grp.AddSurfaceNormal(face.Normal);
                                distanceSum += currentDistance;
                                normalAvg += face.Normal;
                                positionAvg += currentContact;
                                if (currentDistance < distanceMin)
                                {
                                    hitboxname = face.Owner._mesh.Name;
                                    distanceMin = currentDistance;
                                    normalNearest = face.Normal;
                                    positionNearest = currentContact;
                                    gNearest = face.Owner.Owner;
                                }
                            }
                        }
                    }
                }
                // Calculate metrics:
                grp.SurfaceNormalAvg = normalAvg / hits;
                grp.SurfaceNormalNearest = normalNearest;
                grp.IntersectionPointAvg = positionAvg / hits;
                grp.IntersectionPointNearest = positionNearest;
                grp.DistanceAvg = distanceSum / hits;
                grp.DistanceMin = distanceMin;
                grp.ObjectNearest = gNearest;
                grp.ObjectNearestHitboxName = hitboxname;
            }
            return grp;
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekte des angegebenen Typs trifft
        /// </summary>
        /// <remarks>Es werden nur Objekte in der Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersection&gt; results = RaytraceObjectsNearbyFast(0, 2, 0, -Vector3.UnitY, typeof(Floor), typeof(Wall));
        /// </code>
        /// </example>
        /// <param name="rayPositionX">X-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionY">Y-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionZ">Z-Komponente des Strahlstartpunkts</param>        
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <param name="typelist">Liste der Typen (Klassen), die für den Strahl getestet werden sollen</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersection> RaytraceObjectsNearbyFast(float rayPositionX, float rayPositionY, float rayPositionZ, Vector3 rayDirectionNormalized, params Type[] typelist)
        {
            return RaytraceObjectsNearbyFast(new Vector3(rayPositionX, rayPositionY, rayPositionZ), rayDirectionNormalized, typelist);
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekte des angegebenen Typs trifft
        /// </summary>
        /// <remarks>Es werden nur Objekte in der Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersection&gt; results = RaytraceObjectsNearbyFast(new Vector3(0, 2, 0), -Vector3.UnitY, typeof(Floor), typeof(Wall));
        /// </code>
        /// </example>
        /// <param name="rayOrigin">Startpunkt des Strahls</param>
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <param name="typelist">Liste der Typen (Klassen), die für den Strahl getestet werden sollen</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersection> RaytraceObjectsNearbyFast(Vector3 rayOrigin, Vector3 rayDirectionNormalized, params Type[] typelist)
        {
            if (typelist.Length == 0)
            {
                KWEngine.LogWriteLine("[GameObject] WARNING: No list of types given for ray testing, assuming type 'GameObject'");
                typelist = new Type[] { typeof(GameObject) };
            }

            List<RayIntersection> list = new();

            foreach (GameObjectHitbox hb in _collisionCandidates)
            {
                if (hb.IsActive && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, hb.Owner))
                {
                    if (rayDirectionNormalized == -Vector3.UnitY && !HelperIntersection.IsPointInsideRectangle(rayOrigin, hb._center, hb._dimensionsAABB.X, hb._dimensionsAABB.Z))
                    {
                        continue;
                    }
                    HelperIntersection.ConvertRayToMeshSpaceForAABBTest(ref rayOrigin, ref rayDirectionNormalized, ref hb.Owner._stateCurrent._modelMatrixInverse, out Vector3 originTransformed, out Vector3 directionTransformed);
                    Vector3 directionTransformedInv = new(1f / directionTransformed.X, 1f / directionTransformed.Y, 1f / directionTransformed.Z);

                    bool result = HelperIntersection.RayAABBIntersection(originTransformed, directionTransformedInv, hb._mesh.Center, new Vector3(hb._mesh.width, hb._mesh.height, hb._mesh.depth), out float currentDistance);
                    if (result == true)
                    {
                        HelperIntersection.ConvertRayToWorldSpaceAfterAABBTest(ref originTransformed, ref directionTransformed, currentDistance, ref hb.Owner._stateCurrent._modelMatrix, ref rayOrigin, out Vector3 intersectionPoint, out float distanceWorldspace);
                        if (distanceWorldspace >= 0)
                        {
                            RayIntersection gd = new()
                            {
                                Distance = (intersectionPoint - rayOrigin).LengthFast,
                                Object = hb.Owner,
                                IntersectionPoint = intersectionPoint
                            };
                            list.Add(gd);
                        }
                    }
                }
            }
            list.Sort();
            return list;
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekt trifft
        /// </summary>
        /// <remarks>Es werden nur Objekte in der Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersection&gt; results = RaytraceObjectsNearbyFast(new Vector3(0, 2, 0), -Vector3.UnitY);
        /// </code>
        /// </example>
        /// <param name="rayOrigin">Startpunkt des Strahls</param>
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersection> RaytraceObjectsNearbyFast(Vector3 rayOrigin, Vector3 rayDirectionNormalized)
        {
            return RaytraceObjectsNearbyFast(rayOrigin, rayDirectionNormalized, typeof(GameObject));
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebenen Position in die angegebene Richtung und prüft, ob dieser Strahl in der Nähe liegende Objekt trifft
        /// </summary>
        /// <remarks>Es werden nur Objekte in unmittelbarer Nähe betrachtet. Die Strahlenlänge hängt von dem globalen Wert KWEngine.SweepAndPruneTolerance ab</remarks>
        /// <example>
        /// <code>
        /// List&lt;RayIntersection&gt; results = RaytraceObjectsNearbyFast(0, 2, 0, -Vector3.UnitY);
        /// </code>
        /// </example>
        /// <param name="rayPositionX">X-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionY">Y-Komponente des Strahlstartpunkts</param>
        /// <param name="rayPositionZ">Z-Komponente des Strahlstartpunkts</param>
        /// <param name="rayDirectionNormalized">Normalisierter Richtungsvektor des Strahls (z.B. -Vector3.UnitY für einen Strahl nach unten)</param>
        /// <returns>Nach Entfernung aufsteigend sortierte Liste der Messergebnisse</returns>
        public List<RayIntersection> RaytraceObjectsNearbyFast(float rayPositionX, float rayPositionY, float rayPositionZ, Vector3 rayDirectionNormalized)
        {
            return RaytraceObjectsNearbyFast(new Vector3(rayPositionX, rayPositionY, rayPositionZ), rayDirectionNormalized, typeof(GameObject));
        }

        /// <summary>
        /// Prüft auf eine Strahlenkollision mit einem Terrain-Objekt direkt unterhalb der angegebenen Position
        /// </summary>
        /// <param name="position">Startposition des nach unten gerichteten Teststrahls</param>
        /// <returns>Ergebnis der Strahlenkollisionsmessung</returns>
        public static RayTerrainIntersection RaytraceTerrainBelowPosition(Vector3 position)
        {
            return HelperIntersection.RaytraceTerrainBelowPosition(position);
        }

        /// <summary>
        /// Setzt das 3D-Modell des Objekts
        /// </summary>
        /// <param name="modelname">Name des 3D-Modells</param>
        /// <returns>true, wenn das Modell gesetzt werden konnte</returns>
        public override bool SetModel(string modelname)
        {
            if (modelname == null || modelname.Trim().Length == 0)
                return false;

            modelname = modelname.Trim();
            bool modelFound = KWEngine.Models.TryGetValue(modelname, out GeoModel model);
            if (modelFound)
            {
                if(model.HasTransparencyTexture && HasTransparencyTexture == false)
                {
                    HasTransparencyTexture = model.HasTransparencyTexture;
                }
                if (!model.IsTerrain)
                {
                    _modelNameInDB = modelname;
                    _model = new EngineObjectModel(model);
                    for (int i = 0; i < _model.Material.Length; i++)
                    {
                        _model.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                    }
                    InitHitboxes();
                    InitRenderStateMatrices();
                    ResetBoneAttachments();
                    InitPreRotationQuaternions();
                }
                else
                {
                    KWEngine.LogWriteLine("[GameObject] Cannot set a terrain model (" + modelname + ") as GameObject model.");
                }
            }
            else
            {
                KWEngine.LogWriteLine("[GameObject] Cannot find model '" + modelname + "'.");
            }
            return modelFound;
        }

        #region Internals
        internal List<GeoNode> _attachBoneNodes = new();
        internal GameObject _attachedTo = null;
        internal Matrix4 _attachmentMatrix = Matrix4.Identity;
        internal Dictionary<GeoNode, GameObject> _gameObjectsAttached = new();
        internal bool _isCollisionObject = false;
        //internal ColliderType _colliderType = ColliderType.None;
        internal Vector3 _positionOffsetForAttachment = Vector3.Zero;
        internal Vector3 _scaleOffsetForAttachment = Vector3.One;
        internal Quaternion _rotationOffsetForAttachment = Quaternion.Identity;
        internal bool IsAttachedToViewSpaceGameObject { get { return _attachedTo != null && KWEngine.CurrentWorld._viewSpaceGameObject != null && _attachedTo == KWEngine.CurrentWorld._viewSpaceGameObject._gameObject; } }
        internal AddRemoveHitboxMode _addRemoveHitboxes = AddRemoveHitboxMode.None;

        

        internal string GetBoneNameForAttachedGameObject(GameObject g)
        {
            foreach(GeoNode node in _gameObjectsAttached.Keys)
            {
                GameObject attachedObject = _gameObjectsAttached[node];
                if(attachedObject == g)
                {
                    return node.Name;
                }
            }
            return null;
        }

        
        internal override void InitHitboxes()
        {
            if (this.ID > 0)
            {
                RemoveHitboxesFromWorldHitboxList();
            }
            if(_colliderModel == null)
            {
                _colliderModel = new ColliderModel();
            }
            _colliderModel._hitboxes.Clear();
            foreach(GeoMeshHitbox gmh in _model.ModelOriginal.MeshCollider.MeshHitboxes)
            {
                _colliderModel._hitboxes.Add(new GameObjectHitbox(this, gmh));
            }
            if (this.ID > 0)
            {
                UpdateWorldHitboxList();
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void RemoveHitboxesFromWorldHitboxList()
        {
            lock (CurrentWorld._gameObjectHitboxes)
            {
                if (_colliderModel != null)
                {
                    foreach (GameObjectHitbox hb in _colliderModel._hitboxes)
                    {
                        CurrentWorld._gameObjectHitboxes.Remove(hb);
                    }
                }
            }
        }

        internal void UpdateWorldHitboxList()
        {
            lock (KWEngine.CurrentWorld._gameObjectHitboxes)
            {
                foreach (GameObjectHitbox hb in _colliderModel._hitboxes)
                {
                    if (hb.IsActive)
                        KWEngine.CurrentWorld._gameObjectHitboxes.Add(hb);
                }
            }
        }

        internal void DetachGameObjectFromBone(GameObject attachment)
        {
            foreach (GeoNode node in _gameObjectsAttached.Keys)
            {
                if (_gameObjectsAttached[node] == attachment)
                {
                    _gameObjectsAttached.Remove(node);
                    return;
                }
            }
        }

        internal void ResetBoneAttachments()
        {
            _attachBoneNodes.Clear();
            //_attachBoneNodesOffsets.Clear();

            foreach(GameObject attachment in _gameObjectsAttached.Values)
            {
                DetachGameObjectFromBone(attachment);
            }
            _gameObjectsAttached.Clear();
        }

        internal override void UpdateModelMatrixAndHitboxes()
        {
            if (_stateCurrent._rotation == new Quaternion(0, 0, 0, 0))
            {
                return;
            }
            _stateCurrent._modelMatrix = HelperMatrix.CreateModelMatrix(_stateCurrent);
            _stateCurrent._modelMatrixInverse = Matrix4.Invert(_stateCurrent._modelMatrix);
            _stateCurrent._lookAtVector = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitZ, _stateCurrent._modelMatrixInverse));
            _stateCurrent._lookAtVectorRight = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitX, _stateCurrent._modelMatrixInverse));
            _stateCurrent._lookAtVectorUp = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitY, _stateCurrent._modelMatrixInverse));

            _stateCurrent._center = Vector3.Zero;
            Vector3 dimMax = new(float.MinValue);
            Vector3 dimMin = new(float.MaxValue);

            Vector3 obbRadii = new(0);

            foreach (GameObjectHitbox hb in _colliderModel._hitboxes)
            {
                if (hb.Update(ref _stateCurrent._center))
                {
                    if (hb._left < dimMin.X) dimMin.X = hb._left;
                    if (hb._low < dimMin.Y) dimMin.Y = hb._low;
                    if (hb._back < dimMin.Z) dimMin.Z = hb._back;
                    if (hb._right > dimMax.X) dimMax.X = hb._right;
                    if (hb._high > dimMax.Y) dimMax.Y = hb._high;
                    if (hb._front > dimMax.Z) dimMax.Z = hb._front;

                    obbRadii = new Vector3(
                        hb._dimensionsOBB.X * 0.5f > obbRadii.X ? hb._dimensionsOBB.X * 0.5f : obbRadii.X,
                        hb._dimensionsOBB.Y * 0.5f > obbRadii.Y ? hb._dimensionsOBB.Y * 0.5f : obbRadii.Y,
                        hb._dimensionsOBB.Z * 0.5f > obbRadii.Z ? hb._dimensionsOBB.Z * 0.5f : obbRadii.Z);
                }
            }

            _stateCurrent._dimensions.X = dimMax.X - dimMin.X;
            _stateCurrent._dimensions.Y = dimMax.Y - dimMin.Y;
            _stateCurrent._dimensions.Z = dimMax.Z - dimMin.Z;
            _stateCurrent._center = (dimMax + dimMin) / 2f;
            _fullDiameter = (new Vector3(dimMax.X, dimMax.Y, dimMax.Z) - new Vector3(dimMin.X, dimMin.Y, dimMin.Z)).LengthFast;
            _obbRadii = obbRadii;
        }

        internal List<GameObjectHitbox> _collisionCandidates = new();

        internal void SetScaleRotationAndTranslation(Vector3 s, Quaternion r, Vector3 t)
        {
            _stateCurrent._rotation = r;
            _stateCurrent._scale = s;
            _stateCurrent._position = t;
            UpdateModelMatrixAndHitboxes();
        }

        internal byte _flowfieldcost = 1;
        internal float _fullDiameter = 1f;
        internal Vector3 _obbRadii = new Vector3(0.5f);

        internal static Vector3[] _rayOrigins1 = new Vector3[1];
        internal static Vector3[] _rayOrigins2 = new Vector3[2];
        internal static Vector3[] _rayOrigins4 = new Vector3[4];
        internal static Vector3[] _rayOrigins8 = new Vector3[8];
        #endregion
    }
}
