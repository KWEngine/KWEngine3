using KWEngine3.Exceptions;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3.GameObjects
{
    public abstract class GameObject : IComparable<GameObject>
    {
        public abstract void Act();
        public bool IsCollisionObject { get { return _isCollisionObject; } set { _isCollisionObject = value; } }
        public bool IsShadowCaster { get { return _isShadowCaster; } set { _isShadowCaster = value; } }
        public bool IsInsideScreenSpace { get; internal set; } = true;
        public int ID { get; internal set; } = -1;
        public bool UpdateLast { get; set; } = false;
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        public GameObject()
            : this("KWCube")
        {

        }
        public GameObject(string modelname)
        {
            bool modelSetSuccessfully = SetModel(modelname);
            if(!modelSetSuccessfully)
            {
                KWEngine.LogWriteLine("[GameObject] Cannot set " + (modelname == null ? "" : modelname.Trim()));
            }

            InitStates();
        }

        public bool SetModel(string modelname)
        {
            if (modelname == null || modelname.Trim().Length == 0)
                return false;

            modelname = modelname.Trim();
            bool modelFound = KWEngine.Models.TryGetValue(modelname, out GeoModel model);
            if (modelFound)
            {
                _modelNameInDB = modelname;
                _gModel = new GameObjectModel(model);
                for(int i = 0; i < _gModel.Material.Length; i++)
                {
                    _gModel.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                }
                InitHitboxes();
                InitRenderStateMatrices();
                ResetBoneAttachments();
            }
            return modelFound;
        }

        public bool IsAnimated
        {
            get
            {
                return _gModel.ModelOriginal.HasBones && _statePrevious._animationID >= 0;
            }
        }

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

        public Intersection GetIntersection()
        {
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }
            else if (_currentOctreeNode == null)
            {
                return null;
            }
            List<GameObject> potentialColliders = new List<GameObject>();
            CollectPotentialIntersections<GameObject>(potentialColliders, _currentOctreeNode);

            foreach (GameObject collider in potentialColliders)
            {
                foreach (GameObjectHitbox hbother in collider._hitboxes)
                {
                    if (!hbother.IsActive)
                        continue;

                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                        if (i != null)
                            return i;
                    }
                }
            }
            return null;
        }

        public Intersection GetIntersection<T>() where T : GameObject
        {
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return null;
            }
            else if (_currentOctreeNode == null)
            {
                return null;
            }
            List<GameObject> potentialColliders = new List<GameObject>();
            CollectPotentialIntersections<T>(potentialColliders, _currentOctreeNode);

            foreach (GameObject collider in potentialColliders)
            {
                foreach (GameObjectHitbox hbother in collider._hitboxes)
                {
                    if (!hbother.IsActive)
                        continue;

                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                        if (i != null)
                            return i;
                    }
                }
            }
            return null;
        }

        public List<Intersection> GetIntersections()
        {
            List<Intersection> intersections = new List<Intersection>();
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }
            else if (_currentOctreeNode == null)
            {
                return intersections;
            }
            List<GameObject> potentialColliders = new List<GameObject>();
            CollectPotentialIntersections<GameObject>(potentialColliders, _currentOctreeNode);

            foreach (GameObject collider in potentialColliders)
            {
                foreach (GameObjectHitbox hbother in collider._hitboxes)
                {
                    if (!hbother.IsActive)
                        continue;

                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                        if (i != null)
                            intersections.Add(i);
                    }
                }
            }
            return intersections;
        }

        public List<Intersection> GetIntersections<T>() where T : GameObject
        {
            List<Intersection> intersections = new List<Intersection>();
            if (!IsCollisionObject)
            {
                KWEngine.LogWriteLine("GameObject " + ID + " not a collision object.");
                return intersections;
            }
            else if (_currentOctreeNode == null)
            {
                return intersections;
            }
            List<GameObject> potentialColliders = new List<GameObject>();
            CollectPotentialIntersections<T>(potentialColliders, _currentOctreeNode);

            foreach (GameObject collider in potentialColliders)
            {
                foreach (GameObjectHitbox hbother in collider._hitboxes)
                {
                    if (!hbother.IsActive)
                        continue;

                    foreach (GameObjectHitbox hbcaller in this._hitboxes)
                    {
                        if (!hbcaller.IsActive)
                            continue;
                        Intersection i = HelperIntersection.TestIntersection(hbcaller, hbother);
                        if (i != null)
                            intersections.Add(i);
                    }
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
            target.Z += 0.000001f;
            Matrix4 lookat = Matrix4.LookAt(target, Position, KWEngine.WorldUp);
            lookat.Transpose();
            Quaternion q = Quaternion.FromMatrix(new Matrix3(lookat));
            q.Invert();
            return q;
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="target">Zielkoordinate</param>
        public void TurnTowardsXYZ(Vector3 target)
        {
            _stateCurrent._rotation = GetRotationToTarget(target);
        }

        /// <summary>
        /// Gleicht die Rotation der Instanz an die der Kamera an
        /// </summary>
        public void AdjustRotationToCameraRotation()
        {
            _stateCurrent._rotation = HelperRotation.GetRotationTowardsCamera();
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
                _stateCurrent._rotation = Quaternion.FromMatrix(new Matrix3(lookat));
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
                _stateCurrent._rotation = Quaternion.FromMatrix(new Matrix3(lookat));
            }
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger (näherungsweise) auf dem Objekt liegt 
        /// </summary>
        /// <returns>true, wenn der Mauszeiger auf dem Objekt liegt</returns>
        public bool IsMouseCursorInsideMyHitbox()
        {
            Vector2 mc;
            if (KWEngine.Window.CursorState == OpenTK.Windowing.Common.CursorState.Grabbed)
            {
                mc = KWEngine.Window.ClientRectangle.HalfSize;
            }
            else
            {
                mc = KWEngine.Window.MousePosition;
            }

            Vector3 rayDirection = HelperGeneral.Get3DMouseCoords(mc);
            Vector3 rayOrigin = KWEngine.CurrentWorld._cameraGame._stateCurrent._position;
            foreach (GameObjectHitbox h in this._hitboxes)
            {
                if (HelperIntersection.RayBoxIntersection(ref rayOrigin, ref rayDirection, h) == true)
                {
                    return true;
                }
            }
            return false;
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

        public float GameTime { get { return KWEngine.ApplicationTime; } }
        public float WorldTime { get { return KWEngine.WorldTime; } }
        public KeyboardState Keyboard { get { return KWEngine.Window.KeyboardState; } }
        public MouseState Mouse { get { return KWEngine.Window.MouseState; } }
        public Vector2 MouseMovement
        {
            get
            {
                if (Window.CursorState == OpenTK.Windowing.Common.CursorState.Grabbed)
                {
                    return Mouse.Delta;
                }
                else
                {
                    return Vector2.Zero;
                }
            }
        }
        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }
        public GLWindow Window { get { return KWEngine.Window; } }
        public bool SkipRender { get; set; } = false;

        public bool HasTransparencyTexture { get; set; } = false;

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

        public Vector3 Center { get { return _stateCurrent._center; } }
        public Vector3 Dimensions { get { return _stateCurrent._dimensions; } }

        public Vector3 Position { 
            get
            {
                return _stateCurrent._position;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return _stateCurrent._rotation;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return _stateCurrent._scale;
            }
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }
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

        public void SetRotation(float x, float y, float z)
        {
            SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z)));
        }

        public void AddRotationX(float r, bool worldSpace = false)
        {
            //Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(r * KWEngine.DeltaTimeCurrentNibbleSize));
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

        public void AddRotationY(float r, bool worldSpace = false)
        {
            //Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(r * KWEngine.DeltaTimeCurrentNibbleSize));
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

        public void AddRotationZ(float r, bool worldSpace = false)
        {
            //Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(r * KWEngine.DeltaTimeCurrentNibbleSize));
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

        public void SetRotation(Quaternion rotation)
        {
            _stateCurrent._rotation = rotation;
            UpdateModelMatrixAndHitboxes();
        }
        public void SetScale(float x, float y, float z)
        {
            if (x > float.Epsilon && y > float.Epsilon && z > float.Epsilon)
            {
                _stateCurrent._scale = new Vector3(x, y, z);
                UpdateModelMatrixAndHitboxes();
            }
            else
            {
                _stateCurrent._scale = new Vector3(1, 1, 1);
                UpdateModelMatrixAndHitboxes();
            }
        }

        public void SetHitboxScale(float s)
        {
            SetHitboxScale(s, s, s);
        }

        public void SetHitboxScale(float x, float y, float z)
        {
            if (x > float.Epsilon && y > float.Epsilon && z > float.Epsilon)
            {
                _stateCurrent._scaleHitbox = new Vector3(x, y, z);
            }
            else
            {
                throw new GameObjectException("Invalid scale values.");
            }
        }

        public void SetScale(float s)
        {
            SetScale(s, s, s);
        }

        public void Move(float units)
        {
            MoveOffset(LookAtVector * units);
        }

        public void MoveXZ(float units)
        {
            Vector3 lavXZ = LookAtVector;
            lavXZ.Y = 0;
            Vector3.NormalizeFast(lavXZ);
            MoveOffset(lavXZ * units);
        }

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
        /*
        public void MoveOffset(float mtvX, float mtvY, float mtvZ)
        {
            MoveOffset(new Vector3(mtvX, mtvY, mtvZ));
        }

        public void MoveOffset(Vector3 mtv)
        {
            SetPosition(Position + mtv);
        }
        */
        public void MoveOffset(float x, float y, float z)
        {
            MoveOffset(new Vector3(x, y, z));
        }

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

        public void MoveOffset(Vector3 offset)
        {
            //SetPosition(Position + offset * KWEngine.DeltaTimeCurrentNibbleSize);
            SetPosition(Position + offset);
        }

        public override string ToString()
        {
            return ID.ToString().PadLeft(8, ' ') + ": " + Name;
        }

        public void SetAnimationID(int id)
        {
            if (_gModel.ModelOriginal.Animations != null)
                _stateCurrent._animationID = MathHelper.Clamp(id, -1, _gModel.ModelOriginal.Animations.Count - 1);
            else
                _stateCurrent._animationID = -1;
        }

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

        public void SetAnimationPercentageAdvance(float p)
        {
            //_stateCurrent._animationPercentage = _stateCurrent._animationPercentage + p * KWEngine.DeltaTimeCurrentNibbleSize;
            _stateCurrent._animationPercentage = _stateCurrent._animationPercentage + p;
            if (_stateCurrent._animationPercentage > 1f)
            {
                _stateCurrent._animationPercentage = _stateCurrent._animationPercentage - 1f;
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
        }

        public void SetOpacity(float o)
        {
            _stateCurrent._opacity = MathHelper.Clamp(o, 0f, 1f);
        }

        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._colorTint = new Vector3(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1));
        }

        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1),
                MathHelper.Clamp(intensity, 0, 2));
        }

        public void SetMetallic(float m)
        {
            _gModel._metallic = MathHelper.Clamp(m, 0f, 1f);
        }

        public void SetMetallicType(MetallicType type)
        {
            _gModel._metallicType = type;
        }

        public void SetRoughness(float r)
        {
            _gModel._roughness = MathHelper.Clamp(r, 0.00001f, 1f);
        }

        public void SetTexture(string filename, TextureType type = TextureType.Albedo, int meshId = 0)
        {
            if (filename == null)
                filename = "";
            _gModel.SetTexture(filename.ToLower().Trim(), type, meshId);
        }

        public void SetTextureOffset(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(_stateCurrent._uvTransform.X, _stateCurrent._uvTransform.Y, x, y);
        }

        public void SetTextureRepeat(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(x, y, _stateCurrent._uvTransform.Z, _stateCurrent._uvTransform.W);
        }

        public bool IsAttachedToGameObject { get { return _attachedTo != null; } }

        public Vector3 LookAtVector
        {
            get { return _stateCurrent._lookAtVector; }
        }

        public Vector3 LookAtVectorLocalUp
        {
            get { return _stateCurrent._lookAtVectorUp; }
        }
        public Vector3 LookAtVectorLocalRight
        {
            get { return _stateCurrent._lookAtVectorRight; }
        }

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
        /// <returns></returns>
        public bool IsLookingAt(float x, float y, float z, float diameter)
        {
            return IsLookingAt(new Vector3(x, y, z), diameter);
        }

        /// <summary>
        /// Prüft, ob das Objekt in Richtung des gegebenen Punkts blickt
        /// </summary>
        /// <param name="target">Zielposition</param>
        /// <param name="diameter">Durchmesser um das Ziel</param>
        /// <returns></returns>
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
                float t1 = (e + aabb[0][i]) / f; // Intersection with the "left" plane
                float t2 = (e + aabb[1][i]) / f; // Intersection with the "right" plane
                if (t1 > t2)
                {
                    float w = t1;
                    t1 = t2;
                    t2 = w;
                }
                // tMax is the nearest "far" intersection (amongst the X,Y and Z planes pairs)
                if (t2 < tMax) tMax = t2;
                // tMin is the farthest "near" intersection (amongst the X,Y and Z planes pairs)
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
            _stateCurrent._rotation = HelperVector.GetRotationToMatchSurfaceNormal(LookAtVector, surfaceNormal);
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
                        g.SetPosition(0, 0, 0);
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
        /// <param name="boneName"></param>
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
                        attachment._attachmentMatrixUpdatedByBone = false;
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

        public int CompareTo(GameObject other)
        {
            float distanceToCameraThis = (this.Center - KWEngine.CurrentWorld.CameraPosition).LengthSquared;
            float distanceToCameraOther = (other.Center - KWEngine.CurrentWorld.CameraPosition).LengthSquared;
            return distanceToCameraOther > distanceToCameraThis ? 1 : -1;
        }

        #region Internals
        internal GameObjectState _statePrevious;
        internal GameObjectState _stateCurrent;
        internal GameObjectRenderState _stateRender;
        internal List<GameObjectHitbox> _hitboxes = new List<GameObjectHitbox>();
        internal string _name = "(no name)";
        internal GameObjectModel _gModel;
        internal List<GeoNode> _attachBoneNodes = new List<GeoNode>();
        internal List<Matrix4> _attachBoneNodesOffsets = new List<Matrix4>();
        internal bool _attachmentMatrixUpdatedByBone = false;
        internal GameObject _attachedTo = null;
        internal Matrix4 _attachmentMatrix = Matrix4.Identity;
        internal Dictionary<GeoNode, GameObject> _gameObjectsAttached = new Dictionary<GeoNode, GameObject>();
        internal OctreeNode _currentOctreeNode = null;
        internal bool _isCollisionObject = false;
        internal bool _isShadowCaster = false;

        internal void SetScale(Vector3 s)
        {
            SetScale(s.X, s.Y, s.Z);
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
            _hitboxes.Clear();
            foreach(GeoMeshHitbox gmh in _gModel.ModelOriginal.MeshHitboxes)
            {
                _hitboxes.Add(new GameObjectHitbox(this, gmh));
            }
            UpdateModelMatrixAndHitboxes();
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
            _attachBoneNodesOffsets.Clear();

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
        }

        internal void TurnTowardsViewForFPObject(Vector3 target)
        {
            _stateCurrent._rotation = GetRotationToTarget(target);
            _stateRender._rotation = _stateCurrent._rotation;
            _stateRender._modelMatrix = HelperMatrix.CreateModelMatrix(_stateRender);
            _stateRender._normalMatrix = Matrix4.Transpose(Matrix4.Invert(_stateRender._modelMatrix));
        }

        internal bool IsQuad
        {
            get
            {
                return _gModel.ModelOriginal.Name == "kwquad.obj";
            }
        }

        internal void CollectPotentialCollidersFromParent<T>(List<GameObject> colliders, OctreeNode node)
        {
            if (node.Parent != null)
            {
                foreach(GameObject g in node.Parent.GameObjectsInThisNode)
                {
                    if(g != this && g is T)
                        colliders.Add(g);
                }
                
                CollectPotentialIntersections<T>(colliders, node.Parent);
            }
        }

        internal void CollectPotentialCollidersFromChildren<T>(List<GameObject> colliders, OctreeNode node)
        {
            foreach (OctreeNode child in node.ChildOctreeNodes)
            {
                foreach (GameObject g in child.GameObjectsInThisNode)
                {
                    if (g != this && g is T)
                        colliders.Add(g);
                }
                CollectPotentialCollidersFromChildren<T>(colliders, child);
            }
        }

        internal void CollectPotentialIntersections<T>(List<GameObject> colliders, OctreeNode node)
        {
            CollectPotentialCollidersFromParent<T>(colliders, node);

            foreach(GameObject g in node.GameObjectsInThisNode)
            {
                if(g != this && g is T)
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

        internal string _modelNameInDB = "KWCube";
        #endregion
    }
}
