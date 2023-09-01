using KWEngine3.Exceptions;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// GameObject-Klasse
    /// </summary>
    public abstract class GameObject : IComparable<GameObject>
    {
        /// <summary>
        /// Abstrakte Methode die von jeder erbenden Klasse implementiert werden muss
        /// </summary>
        public abstract void Act();
        /// <summary>
        /// Gibt an, ob das Objekt ein Kollisionen erzeugen und überprüfen kann
        /// </summary>
        public bool IsCollisionObject { 
            get 
            { 
                return _isCollisionObject;
            } 
            set 
            {
                bool valueBefore = _isCollisionObject;
                _isCollisionObject = value;
                if (valueBefore == true && value == false)
                {
                    foreach (GameObjectHitbox hb in _hitboxes)
                    {
                        if (hb.IsActive)
                            KWEngine.CurrentWorld._gameObjectHitboxes.Remove(hb);
                    }
                }
                else if(valueBefore == false && value == true)
                {
                    foreach (GameObjectHitbox hb in _hitboxes)
                    {
                        if (hb.IsActive && !KWEngine.CurrentWorld._gameObjectHitboxes.Contains(hb))
                            KWEngine.CurrentWorld._gameObjectHitboxes.Add(hb);
                    }
                }
            } 
        }
        /// <summary>
        /// Gibt an, ob das Objekt Schatten werfen und empfangen kann
        /// </summary>
        public bool IsShadowCaster { get { return _isShadowCaster; } set { _isShadowCaster = value; } }
        /// <summary>
        /// Gibt an, ob sich das Objekt gerade auf dem Bildschirm befindet
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;
        /// <summary>
        /// Interne ID des Objekts
        /// </summary>
        public int ID { get; internal set; } = -1;
        /// <summary>
        /// Gibt an, ob das Objekt in der Liste aller Objekte zuletzt aktualisiert werden soll (z.B. für Spielerfiguren)
        /// </summary>
        public bool UpdateLast { get; set; } = false;
        /// <summary>
        /// Names des Objekts
        /// </summary>
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        /// <summary>
        /// Gibt an, ob für das Objekt auch die der Kamera abgewandten Seiten gerendet werden sollen. Dies kann helfen, einseitige Meshes korrekt zu rendern.
        /// </summary>
        public bool DisableBackfaceCulling { get; set; } = false;

        /// <summary>
        /// Erfragt den Namen des aktuell gesetzten 3D-Modells
        /// </summary>
        /// <returns>Modellname</returns>
        public string GetModelName()
        {
            return _modelNameInDB;
        }

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
        /// Setzt das 3D-Modell des Objekts
        /// </summary>
        /// <param name="modelname">Name des 3D-Modells</param>
        /// <returns>true, wenn das Modell gesetzt werden konnte</returns>
        public bool SetModel(string modelname)
        {
            if (modelname == null || modelname.Trim().Length == 0)
                return false;

            modelname = modelname.Trim();
            bool modelFound = KWEngine.Models.TryGetValue(modelname, out GeoModel model);
            if (modelFound)
            {
                if (!model.IsTerrain)
                {
                    _modelNameInDB = modelname;
                    _gModel = new GameObjectModel(model);
                    for (int i = 0; i < _gModel.Material.Length; i++)
                    {
                        _gModel.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                    }
                    InitHitboxes();
                    InitRenderStateMatrices();
                    ResetBoneAttachments();
                }
                else
                {
                    KWEngine.LogWriteLine("[GameObject] Cannot set a terrain model (" + modelname + ") as GameObject model.");
                }
            }
            return modelFound;
        }

        /// <summary>
        /// Gibt an, ob das Objekt gerade eine ausgewählt hat
        /// </summary>
        public bool IsAnimated
        {
            get
            {
                return _gModel.ModelOriginal.HasBones && _statePrevious._animationID >= 0;
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt über Animationen verfügt
        /// </summary>
        public bool HasAnimations
        {
            get
            {
                return _gModel.ModelOriginal.HasBones && _gModel.ModelOriginal.Animations != null && _gModel.ModelOriginal.Animations.Count > 0;
            }
        }

        /// <summary>
        /// Prüft auf eine Kollision mit einem Terrain-Objekt
        /// </summary>
        /// <returns>Kollisionsobjekt mit weiteren Details</returns>
        public IntersectionTerrain GetIntersectionWithTerrain()
        {
            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                foreach (GameObjectHitbox hbcaller in _hitboxes)
                {
                    //TODO: find largest hitbox and test only that one!
                    IntersectionTerrain it = HelperIntersection.TestIntersectionTerrain(hbcaller, t._hitboxes[0]);
                    if (it != null)
                    {
                        return it;
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// Prüft, ob das Objekt gerade mit anderen Objekten kollidiert und gibt die erstbeste Kollision zurück
        /// </summary>
        /// <returns>zuerst gefundene Kollision</returns>
        public Intersection GetIntersection()
        {
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }
            //else if (_currentOctreeNode == null)
            //{
            //    return null;
            //}
            List<GameObjectHitbox> potentialColliders = this._collisionCandidates;//new List<GameObject>();
            //CollectPotentialIntersections<GameObject>(potentialColliders, _currentOctreeNode);

            foreach (GameObjectHitbox hbother in potentialColliders)
            {
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                    if (i != null)
                        return i;
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
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }
            //else if (_currentOctreeNode == null)
            //{
            //    return null;
            //}
            List<GameObjectHitbox> potentialColliders = this._collisionCandidates;//; new List<GameObject>();
            //CollectPotentialIntersections<T>(potentialColliders, _currentOctreeNode);


            foreach (GameObjectHitbox hbother in potentialColliders)
            {
                if((hbother.Owner is T) == false)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
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
            List<Intersection> intersections = new List<Intersection>();
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }
            //else if (_currentOctreeNode == null)
            //{
            //    return intersections;
            //}
            List<GameObjectHitbox> potentialColliders = this._collisionCandidates;// new List<GameObject>();
            //CollectPotentialIntersections<GameObject>(potentialColliders, _currentOctreeNode);


            foreach (GameObjectHitbox hbother in potentialColliders)
            {
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                    if (i != null)
                        intersections.Add(i);
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
            List<Intersection> intersections = new List<Intersection>();
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }
            //else if (_currentOctreeNode == null)
            //{
            //    return intersections;
            //}
            List<GameObjectHitbox> potentialColliders = this._collisionCandidates;
            //CollectPotentialIntersections<T>(potentialColliders, _currentOctreeNode);


            foreach (GameObjectHitbox hbother in potentialColliders)
            {
                if((hbother.Owner is T) == false)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
                {
                    if (!hbcaller.IsActive)
                        continue;
                    Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                    if (i != null)
                        intersections.Add(i);
                }
            }
            return intersections;
        }

        /// <summary>
        /// Erfragt die Rotation, die zu einem bestimmten Ziel notwendig wäre
        /// </summary>
        /// <param name="target">Ziel</param>
        /// <returns>Rotation (als Quaternion)</returns>
        public Quaternion GetRotationToTarget(Vector3 target)
        {
            Matrix3 lookat = new Matrix3(Matrix4.LookAt(target, Center, KWEngine.WorldUp));
            lookat = Matrix3.Transpose(lookat);
            Quaternion q = Quaternion.FromMatrix(lookat);
            q.Invert();
            return q;
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="target">Zielkoordinate</param>
        public void TurnTowardsXYZ(Vector3 target)
        {
            SetRotation(GetRotationToTarget(target));
        }

        /// <summary>
        /// Gleicht die Rotation der Instanz an die der Kamera an
        /// </summary>
        public void AdjustRotationToCameraRotation()
        {
            SetRotation(HelperRotation.GetRotationTowardsCamera());
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="targetX">X-Koordinate</param>
        /// <param name="targetY">Y-Koordinate</param>
        public void TurnTowardsXY(float targetX, float targetY)
        {
            Vector3 target = new Vector3(targetX, targetY, 0);
            TurnTowardsXY(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XY-Koordinaten blickt. Z-Unterschiede Unterschiede werden ignoriert.
        /// [Geeignet, wenn die Kamera entlang der z-Achse blickt (Standard)]
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXY(Vector3 target)
        {
            target.Z = Position.Z + 0.000001f;
            Matrix4 lookat = Matrix4.LookAt(target, Position, Vector3.UnitZ);
            lookat.Transpose();
            if (lookat.Determinant != 0)
            {
                lookat.Invert();
                SetRotation(Quaternion.FromMatrix(new Matrix3(lookat)));
            }
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="targetX">Zielkoordinate der x-Achse</param>
        /// <param name="targetZ">Zielkoordinate der z-Achse</param>
        public void TurnTowardsXZ(float targetX, float targetZ)
        {
            Vector3 target = new Vector3(targetX, 0, targetZ);
            TurnTowardsXZ(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXZ(Vector3 target)
        {
            target.Y = Position.Y + 0.000001f;
            Matrix4 lookat = Matrix4.LookAt(target, Position, Vector3.UnitY);
            lookat.Transpose();
            if (lookat.Determinant != 0)
            {
                lookat.Invert();
                SetRotation(Quaternion.FromMatrix(new Matrix3(lookat)));
            }
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger auf der Hitbox des Objekts liegt
        /// </summary>
        /// <returns>true, wenn sich der Mauszeiger innerhalb der Hitbox des Objekts befindet</returns>
        public bool IsMouseCursorInsideMyHitbox()
        {
            return HelperIntersection.IsMouseCursorInsideGameObjectHitbox(this, true);
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
        /// Anzahl der Sekunden, die die Anwendung bereits läuft
        /// </summary>
        public float ApplicationTime { get { return KWEngine.ApplicationTime; } }
        /// <summary>
        /// Anzahl der Sekunden, die die aktuelle Welt bereits läuft
        /// </summary>
        public float WorldTime { get { return KWEngine.WorldTime; } }
        /// <summary>
        /// Verweis auf die Keyboard-Aktivitäten
        /// </summary>
        public KeyboardState Keyboard { get { return KWEngine.Window.KeyboardState; } }
        /// <summary>
        /// Verweis auf die Mausaktivitäten
        /// </summary>
        public MouseState Mouse { get { return KWEngine.Window.MouseState; } }
        /// <summary>
        /// Gibt die Strecke an, die der Mauszeiger seit der letzten Überprüfung zurückgelegt hat
        /// </summary>
        public Vector2 MouseMovement
        {
            get
            {
                return KWEngine.Window._mouseDeltaToUse;
                //return KWEngine.Window.MouseState.Delta;
            }
        }
        /// <summary>
        /// Verweis auf die aktuelle Welt
        /// </summary>
        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }
        /// <summary>
        /// Verweis auf das Anwendungsfenster
        /// </summary>
        public GLWindow Window { get { return KWEngine.Window; } }
        /// <summary>
        /// Gibt an, ob das Objekt nicht gerendert werden soll
        /// </summary>
        public bool SkipRender { get; set; } = false;

        /// <summary>
        /// Setzt manuell fest, ob das Objekt Texturen aufweist, die einen Alpha-Kanal besitzen
        /// </summary>
        public bool HasTransparencyTexture { get; set; } = false;

        /// <summary>
        /// Gibt an, ob das Objekt Transparenzanteile besitzt
        /// </summary>
        public bool IsTransparent { 
            get
            {
                if (HasTransparencyTexture)
                    return true;
                if(_stateCurrent._opacity < 1f)
                    return true;
                foreach(GeoMaterial mat in _gModel.Material)
                {
                    if(mat.ColorAlbedo.W < 1f && mat.ColorAlbedo.W > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Mittelpunkt des Objekts
        /// </summary>
        public Vector3 Center { get { return _stateCurrent._center; } }
        /// <summary>
        /// Maße des Objekts (jeweils maximal)
        /// </summary>
        public Vector3 Dimensions { get { return _stateCurrent._dimensions; } }

        /// <summary>
        /// Position des Objekts
        /// </summary>
        public Vector3 Position { 
            get
            {
                return _stateCurrent._position;
            }
        }

        /// <summary>
        /// Rotation/Orientierung des Objekts
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return _stateCurrent._rotation;
            }
        }

        /// <summary>
        /// Größe des Objekts
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return _stateCurrent._scale;
            }
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">Position auf x-Achse</param>
        /// <param name="y">Position auf y-Achse</param>
        /// <param name="z">Position auf z-Achse</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }
        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="position">Position in 3D</param>
        public void SetPosition(Vector3 position)
        {
            _stateCurrent._position = position;
            UpdateModelMatrixAndHitboxes();
        }
        /// <summary>
        /// Setzt die x-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="x">Positionswert</param>
        public void SetPositionX(float x)
        {
            SetPosition(new Vector3(x, Position.Y, Position.Z));
        }

        /// <summary>
        /// Setzt die y-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="y">Positionswert</param>
        public void SetPositionY(float y)
        {
            SetPosition(new Vector3(Position.X, y, Position.Z));
        }

        /// <summary>
        /// Setzt die z-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="z">Positionswert</param>
        public void SetPositionZ(float z)
        {
            SetPosition(new Vector3(Position.X, Position.Y, z));
        }

        /// <summary>
        /// Setzt die Orientierung/Rotation des Objekts
        /// </summary>
        /// <param name="x">Rotation um lokale x-Achse in Grad</param>
        /// <param name="y">Rotation um lokale y-Achse in Grad</param>
        /// <param name="z">Rotation um lokale z-Achse in Grad</param>
        public void SetRotation(float x, float y, float z)
        {
            SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z)));
        }

        /// <summary>
        /// Erhöht die Rotation um die x-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationX(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Erhöht die Rotation um die y-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationY(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Erhöht die Rotation um die z-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationZ(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Setzt die Rotation mit Hilfe eines Quaternion-Objekts
        /// </summary>
        /// <param name="rotation">Rotation</param>
        public void SetRotation(Quaternion rotation)
        {
            _stateCurrent._rotation = rotation;
            UpdateModelMatrixAndHitboxes();
        }
        /// <summary>
        /// Setzt die Größenskalierung des Objekts entlang seiner lokalen drei Achsen
        /// </summary>
        /// <param name="x">Skalierung in x-Richtung</param>
        /// <param name="y">Skalierung in y-Richtung</param>
        /// <param name="z">Skalierung in z-Richtung</param>
        public void SetScale(float x, float y, float z)
        {
            _stateCurrent._scale = new Vector3(
                Math.Max(float.Epsilon, x) ,
                Math.Max(float.Epsilon, y),
                Math.Max(float.Epsilon, z));
            UpdateModelMatrixAndHitboxes();
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
        /// Setzt die Größenskalierung des Objekts (muss > 0 sein)
        /// </summary>
        /// <param name="s">Skalierung</param>
        public void SetScale(float s)
        {
            SetScale(s, s, s);
        }

        /// <summary>
        /// Bewegt das Objekt in seiner Blickrichtung
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void Move(float units)
        {
            MoveOffset(LookAtVector * units);
        }

        /// <summary>
        /// Bewegt das Objekt in seiner Blickrichtung (ohne Höhenunterschied)
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveXZ(float units)
        {
            Vector3 lavXZ = LookAtVector;
            lavXZ.Y = 0;
            Vector3.NormalizeFast(lavXZ);
            MoveOffset(lavXZ * units);
        }

        /// <summary>
        /// Bewegt das Objekt entlang seines lokalen "Oben"-Vektors
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveUp(float units)
        {
            MoveAlongVector(LookAtVectorLocalUp, units);
        }

        /// <summary>
        /// Bewegt das Objekt um die gegebenen Einheiten entlang eines Vektors
        /// </summary>
        /// <param name="v">Richtungsvektor</param>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveAlongVector(Vector3 v, float units)
        {
            MoveOffset(v * units);
        }

        /// <summary>
        /// Bewegt das Objekt entlang der drei Weltachsen
        /// </summary>
        /// <param name="x">Bewegungseinheiten in x-Richtung</param>
        /// <param name="y">Bewegungseinheiten in y-Richtung</param>
        /// <param name="z">Bewegungseinheiten in z-Richtung</param>
        public void MoveOffset(float x, float y, float z)
        {
            MoveOffset(new Vector3(x, y, z));
        }
        /// <summary>
        /// Bewegt das Objekt entlang der drei Weltachsen
        /// </summary>
        /// <param name="offset">Bewegungseinheiten in 3D</param>
        public void MoveOffset(Vector3 offset)
        {
            SetPosition(Position + offset);
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
            Vector3 tmpNoY = new Vector3(tmp.X, 0, tmp.Y);
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
        /// Gibt die ID und den Namen des Objekts zurück
        /// </summary>
        /// <returns>Informationen zum Objekt</returns>
        public override string ToString()
        {
            return ID.ToString().PadLeft(8, ' ') + ": " + Name;
        }

        /// <summary>
        /// Setzt die Animationsnummer des Objekts (muss >= 0 sein)
        /// </summary>
        /// <param name="id">ID</param>
        public void SetAnimationID(int id)
        {
            if (_gModel.ModelOriginal.Animations != null)
                _stateCurrent._animationID = MathHelper.Clamp(id, -1, _gModel.ModelOriginal.Animations.Count - 1);
            else
                _stateCurrent._animationID = -1;
        }

        /// <summary>
        /// Setzt den Stand der Animation zwischen 0% und 100% (0 bis 1)
        /// </summary>
        /// <param name="p">Stand (Werte zwischen 0 und 1)</param>
        public void SetAnimationPercentage(float p)
        {
            if (Math.Abs(p - _stateCurrent._animationPercentage) > 0.25f)
            {
                _stateCurrent._animationPercentage = MathHelper.Clamp(p, 0f, 1f);
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
            else
            {
                _stateCurrent._animationPercentage = MathHelper.Clamp(p, 0f, 1f);
            }
        }

        /// <summary>
        /// Führt die Animation um einen gegebenen Teil fort
        /// </summary>
        /// <param name="p">relativer Fortschritt der Animation</param>
        public void SetAnimationPercentageAdvance(float p)
        {
            _stateCurrent._animationPercentage = _stateCurrent._animationPercentage + p;
            if (_stateCurrent._animationPercentage > 1f)
            {
                _stateCurrent._animationPercentage = _stateCurrent._animationPercentage - 1f;
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Objekts (Standard: 1)
        /// </summary>
        /// <param name="o">Sichtbarkeit (0 bis 1)</param>
        public void SetOpacity(float o)
        {
            _stateCurrent._opacity = MathHelper.Clamp(o, 0f, 1f);
        }

        /// <summary>
        /// Setzt die Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._colorTint = new Vector3(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1));
        }

        /// <summary>
        /// Setzt die selbstleuchtende Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        /// <param name="intensity">Helligkeit (zwischen 0 und 2)</param>
        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1),
                MathHelper.Clamp(intensity, 0, 2));
        }

        /// <summary>
        /// Setzt fest, wie metallisch das Objekt ist
        /// </summary>
        /// <param name="m">Metallwert (zwischen 0 und 1)</param>
        /// <param name="meshId">ID des zu ändernden Meshs/Materials (Standard: 0)</param>
        public void SetMetallic(float m, int meshId = 0)
        {
            if(meshId < _gModel.Material.Length)
                _gModel.Material[meshId].Metallic = MathHelper.Clamp(m, 0f, 1f);
        }

        /// <summary>
        /// Setzt die Art des Metalls
        /// </summary>
        /// <param name="type">Metalltyp</param>
        public void SetMetallicType(MetallicType type)
        {
            _gModel._metallicType = type;
        }

        /// <summary>
        /// Setzt die Rauheit der Objektoberfläche (Standard: 1)
        /// </summary>
        /// <param name="r">Rauheit (zwischen 0 und 1)</param>
        /// <param name="meshId">ID des zu ändernden Meshs/Materials (Standard: 0)</param>
        public void SetRoughness(float r, int meshId = 0)
        {
            if (meshId < _gModel.Material.Length)
                _gModel.Material[meshId].Roughness = MathHelper.Clamp(r, 0.00001f, 1f);
        }

        /// <summary>
        /// Setzt die Textur des Objekts
        /// </summary>
        /// <param name="filename">Dateiname der Textur (inkl. relativem Pfad)</param>
        /// <param name="type">Art der Textur (Standard: Albedo)</param>
        /// <param name="meshId">ID des 3D-Modellanteils (Standard: 0)</param>
        public void SetTexture(string filename, TextureType type = TextureType.Albedo, int meshId = 0)
        {
            if (filename == null)
                filename = "";
            _gModel.SetTexture(filename.ToLower().Trim(), type, meshId);
        }

        /// <summary>
        /// Setzt die Texturverschiebung auf dem Objekt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureOffset(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(_stateCurrent._uvTransform.X, _stateCurrent._uvTransform.Y, x, y);
        }


        /// <summary>
        /// Setzt die Texturverschiebung auf dem Objekt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// /// <param name="meshId">ID des 3D-Modellanteils (Standard: 0)</param>
        public void SetTextureOffset(float x, float y, int meshId)
        {
            SetTextureOffsetForMaterial(x, y, meshId);
        }

        /// <summary>
        /// Setzt die Texturwiederholung auf dem Objekt (Standard: 1)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureRepeat(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(x, y, _stateCurrent._uvTransform.Z, _stateCurrent._uvTransform.W);
        }

        /// <summary>
        /// Setzt die Texturwiederholung auf einem einzelnen Mesh eines Objekts (Standard: 1)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="meshIndex">Nullbasierter Index des zu ändernden Meshs/Materials</param>
        public void SetTextureRepeat(float x, float y, int meshIndex)
        {
            SetTextureRepeatForMaterial(x, y, meshIndex);
        }
        
        internal void SetTextureRepeatForMaterial(float x, float y, int materialIndex)
        {
            if(_gModel.Material.Length > materialIndex && _gModel.Material[materialIndex].TextureAlbedo.IsTextureSet)
            {
                float clipX = _gModel.Material[materialIndex].TextureAlbedo.UVTransform.Z;
                float clipY = _gModel.Material[materialIndex].TextureAlbedo.UVTransform.W;
                _gModel.Material[materialIndex].TextureAlbedo.UVTransform = new Vector4(x, y, clipX, clipY);
            }
            else
            {
                if(_gModel.Material.Length <= materialIndex)
                    KWEngine.LogWriteLine("[GameObject] Texture repeat: invalid material");
            }
        }

        internal void SetTextureOffsetForMaterial(float x, float y, int materialIndex)
        {
            if (_gModel.Material.Length > materialIndex && _gModel.Material[materialIndex].TextureAlbedo.IsTextureSet)
            {
                float repeatX = _gModel.Material[materialIndex].TextureAlbedo.UVTransform.X;
                float repeatY = _gModel.Material[materialIndex].TextureAlbedo.UVTransform.Y;
                _gModel.Material[materialIndex].TextureAlbedo.UVTransform = new Vector4(repeatX, repeatY, x, y);
            }
            else
            {
                if (_gModel.Material.Length <= materialIndex)
                    KWEngine.LogWriteLine("[GameObject] Texture offset: invalid material");
            }
        }

        /// <summary>
        /// Gibt an, ob dieses Objekt an einem anderen Objekt angeheftet wurde
        /// </summary>
        public bool IsAttachedToGameObject { get { return _attachedTo != null; } }

        /// <summary>
        /// (Normalisierter) Blickrichtungsvektor des Objekts
        /// </summary>
        public Vector3 LookAtVector
        {
            get { return _stateCurrent._lookAtVector; }
        }

        /// <summary>
        /// (Normalisierter) Lokaler Oben-Vektor des Objekts
        /// </summary>
        public Vector3 LookAtVectorLocalUp
        {
            get { return _stateCurrent._lookAtVectorUp; }
        }
        /// <summary>
        /// (Normalisierter) Lokaler Rechts-Vektor des Objekts
        /// </summary>
        public Vector3 LookAtVectorLocalRight
        {
            get { return _stateCurrent._lookAtVectorRight; }
        }

        /// <summary>
        /// Konvertiert die aktuelle Rotation in Gradangaben für jede der drei Weltachsen
        /// </summary>
        /// <returns>Gradangaben für die aktuelle Rotation des Objekts um die x-, y- und z-Achse</returns>
        public Vector3 GetRotationEulerAngles()
        {
            return HelperRotation.ConvertQuaternionToEulerAngles(Rotation);
        }

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
            return IsLookingAt(new Vector3(x, y, z), diameter);
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
            Vector3[] aabb = new Vector3[] { new Vector3(-0.5f * diameter, -0.5f * diameter, -0.5f * diameter), new Vector3(0.5f * diameter, 0.5f * diameter, 0.5f * diameter) };

            Matrix4 matrix = Matrix4.CreateTranslation(target);
            Vector3 x = new Vector3(matrix.Row0);
            x.NormalizeFast();
            Vector3 y = new Vector3(matrix.Row1);
            y.NormalizeFast();
            Vector3 z = new Vector3(matrix.Row2);
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
        /// Setzt die Rotation passend zum übergebenen Ebenenvektor (surface normal), um z.B. das Objekt zu kippen, wenn es auf einer Schräge steht.
        /// </summary>
        /// <param name="surfaceNormal">Ebenenvektor</param>
        public void SetRotationToMatchSurfaceNormal(Vector3 surfaceNormal)
        {
            SetRotation(HelperVector.GetRotationToMatchSurfaceNormal(LookAtVector, surfaceNormal));
        }

        /// <summary>
        /// Bindet eine andere GameObject-Instanz an den jeweiligen Knochen des aktuell verwendeten Modells
        /// </summary>
        /// <param name="g">Anzuhängende Instanz</param>
        /// <param name="boneName">Name des Knochens, an den die Instanz angehängt werden soll</param>
        public void AttachGameObjectToBone(GameObject g, string boneName)
        {
            GeoModel modelToBeAttached = g._gModel.ModelOriginal;
            if (g.IsAttachedToGameObject == false && modelToBeAttached != null && _gModel.ModelOriginal.BoneNames.IndexOf(boneName) >= 0)
            {
                GeoNode node = GeoNode.FindChild(_gModel.ModelOriginal.Armature, boneName);
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
                KWEngine.LogWriteLine("[GameObject] Unknown model or bone name or already in use");
            }
        }

        /// <summary>
        /// Entfernt die Bindung (Attachment) einer GameObject-Instanz 
        /// </summary>
        /// <param name="boneName">Name des Knochens</param>
        public void DetachGameObjectFromBone(string boneName)
        {
            if (_gModel.ModelOriginal.BoneNames.IndexOf(boneName) >= 0)
            {
                GeoNode node = GeoNode.FindChild(_gModel.ModelOriginal.Armature, boneName);
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
            List<string> boneNames = new List<string>();
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
            GeoNode node = GeoNode.FindChild(_gModel.ModelOriginal.Armature, boneName);
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
            float distanceToCameraThis = (this.Center - KWEngine.CurrentWorld.CameraPosition).LengthSquared;
            float distanceToCameraOther = (other.Center - KWEngine.CurrentWorld.CameraPosition).LengthSquared;
            return distanceToCameraOther > distanceToCameraThis ? 1 : -1;
        }

        /// <summary>
        /// Ersetzt die eigentliche Hitbox-Form mit der für Spielfiguren gängigen Kapselform
        /// </summary>
        /// <param name="meshIndex">Index des 3D-Mesh, für das die Hitbox getauscht werden soll</param>
        /// <param name="mode">Modus zur Bestimmung der richtigen Hitbox-Orientierung</param>
        public void SetHitboxToCapsuleForMesh(int meshIndex = 0, CapsuleHitboxMode mode = CapsuleHitboxMode.Default)
        {
            if(_hitboxes.Count > meshIndex)
            {
                Matrix4 meshTransform = HelperIntersection.CalculateMeshTransformForGameObject(this, meshIndex, mode);

                Vector3 currentHitboxCenter = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.Center, 1.0f), meshTransform).Xyz;
                Vector3 frontbottomleft = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.minX, _hitboxes[meshIndex]._mesh.minY, _hitboxes[meshIndex]._mesh.maxZ, 1f), meshTransform).Xyz;
                Vector3 backtopright = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.maxX, _hitboxes[meshIndex]._mesh.maxY, _hitboxes[meshIndex]._mesh.minZ, 1f), meshTransform).Xyz
                    ;
                bool wasRemoved = CurrentWorld._gameObjectHitboxes.Remove(this._hitboxes[meshIndex]);
                if (wasRemoved)
                {
                    this._hitboxes[meshIndex] = new GameObjectHitbox(this, KWEngine.KWCapsule.MeshHitboxes[0], currentHitboxCenter, frontbottomleft, backtopright);
                    CurrentWorld._gameObjectHitboxes.Add(this._hitboxes[meshIndex]);
                    UpdateModelMatrixAndHitboxes();
                }
            }
        }

        #region Internals
        internal GameObjectState _statePrevious;
        internal GameObjectState _stateCurrent;
        internal GameObjectRenderState _stateRender;
        internal List<GameObjectHitbox> _hitboxes = new List<GameObjectHitbox>();
        internal string _name = "(no name)";
        internal GameObjectModel _gModel;
        internal List<GeoNode> _attachBoneNodes = new List<GeoNode>();
        //internal List<Matrix4> _attachBoneNodesOffsets = new List<Matrix4>();
        internal GameObject _attachedTo = null;
        internal Matrix4 _attachmentMatrix = Matrix4.Identity;
        internal Dictionary<GeoNode, GameObject> _gameObjectsAttached = new Dictionary<GeoNode, GameObject>();
        internal bool _isCollisionObject = false;
        internal bool _isShadowCaster = false;

        internal Vector2 LeftRightMost { get; set; } = new Vector2(0, 0);
        internal Vector2 BackFrontMost { get; set; } = new Vector2(0, 0);
        internal Vector2 BottomTopMost { get; set; } = new Vector2(0, 0);

        internal void SetScale(Vector3 s)
        {
            SetScale(s.X, s.Y, s.Z);
        }

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

        internal void AddRotationXFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void AddRotationYFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void AddRotationZFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation = _stateCurrent._rotation * tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal bool HasArmatureAndAnimations
        {
            get
            {
                return _gModel.ModelOriginal.HasBones && _gModel.ModelOriginal.Animations != null && _gModel.ModelOriginal.Animations.Count > 0;
            }
        }

        internal void InitHitboxes()
        {
            RemoveHitboxesFromWorldHitboxList();
            _hitboxes.Clear();
            foreach(GeoMeshHitbox gmh in _gModel.ModelOriginal.MeshHitboxes)
            {
                _hitboxes.Add(new GameObjectHitbox(this, gmh));
            }
            UpdateWorldHitboxList();
            UpdateModelMatrixAndHitboxes();
        }

        internal void RemoveHitboxesFromWorldHitboxList()
        {
            foreach(GameObjectHitbox hb in _hitboxes)
            {
                CurrentWorld._gameObjectHitboxes.Remove(hb);
            }
        }

        internal void UpdateWorldHitboxList()
        {
            foreach (GameObjectHitbox hb in _hitboxes)
            {
                if(hb.IsActive)
                    KWEngine.CurrentWorld._gameObjectHitboxes.Add(hb);
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

        internal void InitRenderStateMatrices()
        {
            _stateRender._modelMatrices = new Matrix4[_gModel.ModelOriginal.Meshes.Count];
            _stateRender._normalMatrices = new Matrix4[_gModel.ModelOriginal.Meshes.Count];

            if (HasArmatureAndAnimations)
            {
                _stateRender._boneTranslationMatrices = new Dictionary<string, Matrix4[]>();
                foreach (GeoMesh mesh in _gModel.ModelOriginal.Meshes.Values)
                {
                    _stateRender._boneTranslationMatrices[mesh.Name] = new Matrix4[mesh.BoneIndices.Count];
                    for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        _stateRender._boneTranslationMatrices[mesh.Name][i] = Matrix4.Identity;
                }
            }
        }
        internal void SetMetallicType(int typeIndex)
        {
            SetMetallicType((MetallicType)typeIndex);
        }

        internal void InitStates()
        {
            _stateCurrent = new GameObjectState(this);
            _stateRender = new GameObjectRenderState(this);
            UpdateModelMatrixAndHitboxes();
            _statePrevious = _stateCurrent;

        }
        
        internal bool HasEmissiveTexture
        {
            get
            {
                foreach(GeoMaterial m in _gModel.Material)
                {
                    if (m.TextureEmissive.IsTextureSet)
                        return true;
                }
                return false;
            }
        }

        internal void UpdateModelMatrixAndHitboxes()
        {
            _stateCurrent._modelMatrix = HelperMatrix.CreateModelMatrix(_stateCurrent);
            _stateCurrent._lookAtVector = Vector3.NormalizeFast(Vector3.TransformNormal(Vector3.UnitZ, _stateCurrent._modelMatrix));
            _stateCurrent._lookAtVectorRight = Vector3.NormalizeFast(Vector3.Cross(_stateCurrent._lookAtVector, KWEngine.WorldUp));
            _stateCurrent._lookAtVectorUp = Vector3.NormalizeFast(Vector3.Cross(_stateCurrent._lookAtVectorRight, _stateCurrent._lookAtVector));

            _stateCurrent._center = Vector3.Zero;
            Vector3 dimMax = new Vector3(float.MinValue);
            Vector3 dimMin = new Vector3(float.MaxValue);

            int activeHitboxes = 0;
            foreach (GameObjectHitbox hb in _hitboxes)
            {
                if (hb.Update(ref _stateCurrent._center))
                {
                    activeHitboxes++;
                    if (hb._left < dimMin.X) dimMin.X = hb._left;
                    if (hb._low < dimMin.Y) dimMin.Y = hb._low;
                    if (hb._back < dimMin.Z) dimMin.Z = hb._back;
                    if (hb._right > dimMax.X) dimMax.X = hb._right;
                    if (hb._high > dimMax.Y) dimMax.Y = hb._high;
                    if (hb._front > dimMax.Z) dimMax.Z = hb._front;
                }
            }
            _stateCurrent._center /= activeHitboxes;
            _stateCurrent._dimensions.X = dimMax.X - dimMin.X;
            _stateCurrent._dimensions.Y = dimMax.Y - dimMin.Y;
            _stateCurrent._dimensions.Z = dimMax.Z - dimMin.Z;

            LeftRightMost = new Vector2(_stateCurrent._center.X - _stateCurrent._dimensions.X / 2 - KWEngine.SweepAndPruneTolerance, _stateCurrent._center.X + _stateCurrent._dimensions.X / 2 + KWEngine.SweepAndPruneTolerance);
            BackFrontMost = new Vector2(_stateCurrent._center.Z - _stateCurrent._dimensions.Z / 2 - KWEngine.SweepAndPruneTolerance, _stateCurrent._center.Z + _stateCurrent._dimensions.Z / 2 + KWEngine.SweepAndPruneTolerance);
            BottomTopMost = new Vector2(_stateCurrent._center.Y - _stateCurrent._dimensions.Y / 2 - KWEngine.SweepAndPruneTolerance, _stateCurrent._center.Y + _stateCurrent._dimensions.Y / 2 + KWEngine.SweepAndPruneTolerance);
        }

        internal Vector2 GetExtentsForAxis(int a)
        {
            if (a == 1)
                return BottomTopMost;
            else if (a == 2)
                return BackFrontMost;
            else
                return LeftRightMost;

        }

        internal List<GameObjectHitbox> _collisionCandidates = new List<GameObjectHitbox>();

        internal void CollectPotentialCollidersFromParent<T>(List<GameObjectHitbox> colliders, OctreeNode node)
        {
            if (node.Parent != null)
            {
                foreach(GameObjectHitbox g in node.Parent.HitboxesInThisNode)
                {
                    if(g.Owner != this && g.Owner is T)
                        colliders.Add(g);
                }
                
                CollectPotentialIntersections<T>(colliders, node.Parent);
            }
        }

        internal void CollectPotentialCollidersFromChildren<T>(List<GameObjectHitbox> colliders, OctreeNode node)
        {
            foreach (OctreeNode child in node.ChildOctreeNodes)
            {
                foreach (GameObjectHitbox g in child.HitboxesInThisNode)
                {
                    if (g.Owner != this && g.Owner is T)
                        colliders.Add(g);
                }
                CollectPotentialCollidersFromChildren<T>(colliders, child);
            }
        }

        internal void CollectPotentialIntersections<T>(List<GameObjectHitbox> colliders, OctreeNode node)
        {
            CollectPotentialCollidersFromParent<T>(colliders, node);

            foreach(GameObjectHitbox g in node.HitboxesInThisNode)
            {
                if(g.Owner != this && g.Owner is T)
                    colliders.Add(g);
            }

            CollectPotentialCollidersFromChildren<T>(colliders, node);
        }

        internal void SetScaleRotationAndTranslation(Vector3 s, Quaternion r, Vector3 t)
        {
            _stateCurrent._rotation = r;
            _stateCurrent._scale = s;
            _stateCurrent._position = t;
            UpdateModelMatrixAndHitboxes();
        }

        internal Vector3 _positionOffsetForAttachment = Vector3.Zero;
        internal Vector3 _scaleOffsetForAttachment = Vector3.One;
        internal Quaternion _rotationOffsetForAttachment = Quaternion.Identity;

        internal bool IsAttachedToViewSpaceGameObject { get { return _attachedTo != null && KWEngine.CurrentWorld._viewSpaceGameObject != null && _attachedTo == KWEngine.CurrentWorld._viewSpaceGameObject._gameObject; } }

        internal string _modelNameInDB = "KWCube";
        internal int _importedID = -1;

        /// <summary>
        /// Erfragt, ob der Mauszeiger über dem (aus Sicht der Kamera) sichtbaren Teil des Objekts liegt.
        /// ACHTUNG: Funktioniert NICHT mit (halb-)transparenten Objekten oder Objekten mit Transparenzanteilen in der Textur
        /// </summary>
        /// <returns>true, wenn der Mauszeiger auf dem Objekt liegt</returns>
        internal bool IsMouseCursorOnMeFast()
        {
            return KWEngine.Window.MouseCursorGameObjectID == this.ID;
        }
        #endregion
    }
}
