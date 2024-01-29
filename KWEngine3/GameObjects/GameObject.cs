using KWEngine3.Helper;
using KWEngine3.Model;
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
                bool valueBefore = _isCollisionObject;
                _isCollisionObject = value;
               
                if(valueBefore != _isCollisionObject && this.ID > 0)
                {
                    KWEngine.CurrentWorld._gameObjectsColliderChange.Add(this);
                    if(valueBefore == true)
                    {
                        _addRemoveHitboxes = AddRemoveHitboxMode.Remove;
                    }
                    else
                    {
                        _addRemoveHitboxes = AddRemoveHitboxMode.Add;
                    }
                }
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
        /// Gibt an, ob das Objekt gerade eine Animation ausgewählt hat
        /// </summary>
        public bool IsAnimated
        {
            get
            {
                return _model.ModelOriginal.HasBones && _statePrevious._animationID >= 0;
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt über Animationen verfügt
        /// </summary>
        public bool HasAnimations
        {
            get
            {
                return _model.ModelOriginal.HasBones && _model.ModelOriginal.Animations != null && _model.ModelOriginal.Animations.Count > 0;
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
                GameObjectHitbox lowestHitbox = null;
                float y = float.MaxValue;
                foreach (GameObjectHitbox hbcaller in _hitboxes)
                {
                    if (hbcaller.IsActive && hbcaller._low < y)
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

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if (hbother.Owner.ID > 0)
                {
                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
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
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if ((hbother.Owner is T) == false || hbother.Owner.ID <= 0)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
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
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if (hbother.Owner.ID > 0)
                {
                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
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
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }

            foreach (GameObjectHitbox hbother in _collisionCandidates)
            {
                if ((hbother.Owner is T) == false || hbother.Owner.ID <= 0)
                {
                    continue;
                }
                foreach (GameObjectHitbox hbcaller in this._hitboxes)
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
        /// Setzt die Animationsnummer des Objekts (muss >= 0 sein)
        /// </summary>
        /// <param name="id">ID</param>
        public void SetAnimationID(int id)
        {
            if (_model.ModelOriginal.Animations != null)
                _stateCurrent._animationID = MathHelper.Clamp(id, -1, _model.ModelOriginal.Animations.Count - 1);
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
            _stateCurrent._animationPercentage += p;
            if (_stateCurrent._animationPercentage > 1f)
            {
                _stateCurrent._animationPercentage--;
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
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
        public void SetHitboxToCapsuleForMesh(int meshIndex = 0, CapsuleHitboxMode mode = CapsuleHitboxMode.Default)
        {
            if(_hitboxes.Count > meshIndex)
            {
                Matrix4 meshTransform = HelperIntersection.CalculateMeshTransformForGameObject(this, meshIndex, mode);

                Vector3 currentHitboxCenter = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.Center, 1.0f), meshTransform).Xyz;
                Vector3 frontbottomleft = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.minX, _hitboxes[meshIndex]._mesh.minY, _hitboxes[meshIndex]._mesh.maxZ, 1f), meshTransform).Xyz;
                Vector3 backtopright = Vector4.TransformRow(new Vector4(_hitboxes[meshIndex]._mesh.maxX, _hitboxes[meshIndex]._mesh.maxY, _hitboxes[meshIndex]._mesh.minZ, 1f), meshTransform).Xyz;
                lock (CurrentWorld._gameObjectHitboxes)
                {
                    if (CurrentWorld._gameObjectHitboxes.Contains(this._hitboxes[meshIndex]))
                    {
                        CurrentWorld._gameObjectHitboxes.Remove(this._hitboxes[meshIndex]);
                    }
                    this._hitboxes[meshIndex] = new GameObjectHitbox(this, KWEngine.KWCapsule.MeshHitboxes[0], currentHitboxCenter, frontbottomleft, backtopright);
                    CurrentWorld._gameObjectHitboxes.Add(this._hitboxes[meshIndex]);
                    UpdateModelMatrixAndHitboxes();
                    
                }
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
            List<RayIntersectionExt> list = new();

            foreach (GameObjectHitbox hb in _collisionCandidates)
            {
                if (hb.IsActive && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, hb.Owner))
                {
                    bool result = HelperIntersection.RaytraceHitbox(hb, rayOrigin, rayDirectionNormalized, out Vector3 intersectionPoint, out Vector3 faceNormal);
                    if (result == true)
                    {
                        RayIntersectionExt gd = new()
                        {
                            Distance = (intersectionPoint - rayOrigin).LengthFast,
                            Object = hb.Owner,
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
            List<RayIntersection> list = new();

            foreach (GameObjectHitbox hb in _collisionCandidates)
            {
                if (hb.IsActive && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, hb.Owner))
                {
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
        internal Vector3 _positionOffsetForAttachment = Vector3.Zero;
        internal Vector3 _scaleOffsetForAttachment = Vector3.One;
        internal Quaternion _rotationOffsetForAttachment = Quaternion.Identity;
        internal bool IsAttachedToViewSpaceGameObject { get { return _attachedTo != null && KWEngine.CurrentWorld._viewSpaceGameObject != null && _attachedTo == KWEngine.CurrentWorld._viewSpaceGameObject._gameObject; } }
        internal AddRemoveHitboxMode _addRemoveHitboxes = AddRemoveHitboxMode.None;

        internal bool HasArmatureAndAnimations
        {
            get
            {
                return _model.ModelOriginal.HasBones && _model.ModelOriginal.Animations != null && _model.ModelOriginal.Animations.Count > 0;
            }
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

        
        internal override void InitHitboxes()
        {
            if (this.ID > 0)
            {
                RemoveHitboxesFromWorldHitboxList();
            }
            _hitboxes.Clear();
            foreach(GeoMeshHitbox gmh in _model.ModelOriginal.MeshHitboxes)
            {
                _hitboxes.Add(new GameObjectHitbox(this, gmh));
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
                foreach (GameObjectHitbox hb in _hitboxes)
                {
                    CurrentWorld._gameObjectHitboxes.Remove(hb);
                }
            }
        }

        internal void UpdateWorldHitboxList()
        {
            lock (KWEngine.CurrentWorld._gameObjectHitboxes)
            {
                foreach (GameObjectHitbox hb in _hitboxes)
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

        internal override void InitRenderStateMatrices()
        {
            _stateRender._modelMatrices = new Matrix4[_model.ModelOriginal.Meshes.Count];
            _stateRender._normalMatrices = new Matrix4[_model.ModelOriginal.Meshes.Count];

            if (HasArmatureAndAnimations)
            {
                _stateRender._boneTranslationMatrices = new Dictionary<string, Matrix4[]>();
                foreach (GeoMesh mesh in _model.ModelOriginal.Meshes.Values)
                {
                    _stateRender._boneTranslationMatrices[mesh.Name] = new Matrix4[mesh.BoneIndices.Count];
                    for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        _stateRender._boneTranslationMatrices[mesh.Name][i] = Matrix4.Identity;
                }
            }
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

            foreach (GameObjectHitbox hb in _hitboxes)
            {
                if (hb.Update(ref _stateCurrent._center))
                {
                    if (hb._left < dimMin.X) dimMin.X = hb._left;
                    if (hb._low < dimMin.Y) dimMin.Y = hb._low;
                    if (hb._back < dimMin.Z) dimMin.Z = hb._back;
                    if (hb._right > dimMax.X) dimMax.X = hb._right;
                    if (hb._high > dimMax.Y) dimMax.Y = hb._high;
                    if (hb._front > dimMax.Z) dimMax.Z = hb._front;
                }
            }

            _stateCurrent._dimensions.X = dimMax.X - dimMin.X;
            _stateCurrent._dimensions.Y = dimMax.Y - dimMin.Y;
            _stateCurrent._dimensions.Z = dimMax.Z - dimMin.Z;
            _stateCurrent._center = (dimMax + dimMin) / 2f;
        }

        internal List<GameObjectHitbox> _collisionCandidates = new();

        internal void SetScaleRotationAndTranslation(Vector3 s, Quaternion r, Vector3 t)
        {
            _stateCurrent._rotation = r;
            _stateCurrent._scale = s;
            _stateCurrent._position = t;
            UpdateModelMatrixAndHitboxes();
        }
        #endregion
    }
}
