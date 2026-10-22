using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für ein nicht-animiertes, statisches Objekt ohne Teilnahme an der Spielmechanik
    /// </summary>
    public abstract class RenderObject : EngineObject, IComparable<RenderObject>
    {
        /// <summary>
        /// Gibt die beim Erstellen des Objekts festgelegte Anzahl an Instanzen an
        /// </summary>
        public int InstanceCount { get; internal set; } = -1;

        /// <summary>
        /// Gibt an, ob jede zusätzliche Instanz absolut oder relativ zur Hauptinstanz positioniert wird (Standard: absolut)
        /// </summary>
        public InstanceMode Mode { get; internal set; } = InstanceMode.Absolute;

        /// <summary>
        /// Standardkonstruktor (erzeugt mit einem Würfel als 3D-Modell)
        /// </summary>
        public RenderObject()
            : this("KWCube")
        {

        }
        /// <summary>
        /// Konstruktormethode, der der 3D-Modellname mitgegeben werden kann
        /// </summary>
        /// <param name="modelname">Name des zu verwendenden 3D-Modells</param>
        public RenderObject(string modelname)
        {
            bool modelSetSuccessfully = SetModel(modelname);
            if (!modelSetSuccessfully)
            {
                KWEngine.LogWriteLine("[RenderObject] Cannot set " + (modelname == null ? "" : modelname.Trim()));
            }

            SetAdditionalInstanceCount(0);
            InitStates();
        }

        /// <summary>
        /// Konfiguriert die Anzahl der gewünschten Instanzen (Kopien) für das Objekt. 
        /// Es sind maximal 1023 zusätzliche Instanzen möglich.
        /// </summary>
        /// <param name="instanceCount">Anzahl zusätzlich zu erstellender Instanzen (0 für keine zusätzlichen Instanzen)</param>
        /// <param name="mode">Legt fest, ob jede zusätzliche Instanz absolut oder relativ zur Hauptinstanz positioniert wird (Standard: absolut)</param>
        public void SetAdditionalInstanceCount(int instanceCount, InstanceMode mode = InstanceMode.Absolute)
        {
            if (instanceCount > KWEngine.MAXADDITIONALINSTANCECOUNT)
            {
                KWEngine.LogWriteLine("[RenderObject] Max. additional instance count per object is " + KWEngine.MAXADDITIONALINSTANCECOUNT);
            }
            Mode = mode;
            instanceCount = Math.Clamp(instanceCount, 0, KWEngine.MAXADDITIONALINSTANCECOUNT);
            InstanceCount = instanceCount + 1;
            InitInstancesData();
        }

        /// <summary>
        /// Vergleicht das Objekt bzgl. seiner Entfernung zur Kamera mit einem anderen Objekt
        /// </summary>
        /// <param name="other">anderes RenderObject</param>
        /// <returns>1, wenn das aufrufende Objekt näher an der Kamera ist, sonst -1</returns>
        public int CompareTo(RenderObject other)
        {
            if (other._projZ > this._projZ)
                return 1;
            else
                return -1;
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
                if (model.HasTransparencyTexture && HasTransparencyTexture == false)
                {
                    HasTransparencyTexture = model.HasTransparencyTexture;
                }
                if (!model.IsTerrain)
                {
                    _modelNameInDB = modelname;
                    _model = new EngineObjectModel(model);
                    GeoMesh[] modelMeshesArray = model.MeshesArray;
                    for (int i = 0; i < _model.Material.Length; i++)
                    {
                        _model.Material[i] = modelMeshesArray[i].Material;
                    }
                    _hues = new float[model.Meshes.Count];
                    InitHitboxes();
                    InitRenderStateMatrices();
                    InitPreRotationQuaternions();
                }
                else
                {
                    KWEngine.LogWriteLine("[RenderObject] Cannot set a terrain model (" + modelname + ") as RenderObject model.");
                }
            }
            else
            {
                KWEngine.LogWriteLine("[RenderObject] Cannot find model '" + modelname + "'.");
            }
            return modelFound;
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="position">Position in 3D</param>
        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);
            WriteDataToUBOForInstance(0);
        }

        /// <summary>
        /// Setzt die Größenskalierung des Objekts entlang seiner lokalen drei Achsen
        /// </summary>
        /// <param name="x">Skalierung in x-Richtung</param>
        /// <param name="y">Skalierung in y-Richtung</param>
        /// <param name="z">Skalierung in z-Richtung</param>
        public override void SetScale(float x, float y, float z)
        {
            base.SetScale(x, y, z);
            WriteDataToUBOForInstance(0);
        }

        /// <summary>
        /// Setzt die Rotation mit Hilfe eines Quaternion-Objekts
        /// </summary>
        /// <param name="rotation">Rotation</param>
        public override void SetRotation(Quaternion rotation)
        {
            base.SetRotation(rotation);
            WriteDataToUBOForInstance(0);
        }

        /// <summary>
        /// Setzt die Position für eine bestimmte Instanz. Die Rotation und Skalierung der angegebenen Instanz werden zurück auf die Standardwerte gesetzt.
        /// </summary>
        /// <param name="instanceIndex">Index der Instanz (muss zwischen 1 und 1023 liegen)</param>
        /// <param name="position">Position der Instanz</param>
        public void SetPositionForInstance(int instanceIndex, Vector3 position)
        {
            SetPositionRotationScaleForInstance(instanceIndex, position, Quaternion.Identity, Vector3.One);
        }

        /// <summary>
        /// Setzt die Position für eine bestimmte Instanz. Die Rotation und Skalierung der angegebenen Instanz werden zurück auf die Standardwerte gesetzt.
        /// </summary>
        /// <param name="instanceIndex">Index der Instanz (muss zwischen 1 und 1023 liegen)</param>
        /// <param name="x">X-Position der Instanz</param>
        /// <param name="y">Y-Position der Instanz</param>
        /// <param name="z">Z-Position der Instanz</param>
        public void SetPositionForInstance(int instanceIndex, float x, float y, float z)
        {
            SetPositionRotationScaleForInstance(instanceIndex, new Vector3(x, y, z), Quaternion.Identity, Vector3.One);
        }

        /// <summary>
        /// Setzt die Position/Rotation/Skalierung für eine bestimmte Instanz.
        /// </summary>
        /// <param name="instanceIndex">Index der Instanz (muss zwischen 1 und 1023 liegen)</param>
        /// <param name="position">Position der Instanz</param>
        /// <param name="rotation">Rotation der Instanz</param>
        /// <param name="scale">Skalierung der Instanz</param>
        public void SetPositionRotationScaleForInstance(int instanceIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (instanceIndex <= 0)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index must be >= 1");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index must be >= 1");
            }
            else if (instanceIndex > InstanceCount - 1)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index must be below instance count");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index must be < instance count");
            }
            else if (scale.X <= 0 || scale.Y <= 0 || scale.Z <= 0)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index has invalid scale value");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index has invalid scale value");
            }
            else
            {
                _instancePositions[instanceIndex] = position;
                _instanceScales[instanceIndex] = scale;
                _instanceRotations[instanceIndex] = rotation;

                WriteDataToUBOForInstance(instanceIndex);
                UpdateModelMatrixAndHitboxes();
            }
        }

        #region internals
        internal const int BYTESPERINSTANCE = 16 * 4; // 64 Bytes for 1 matrix
        internal int _ubo = -1;
        internal static float[] _uboTempData;

        internal Vector3[] _instancePositions;
        internal Vector3[] _instanceScales;
        internal Quaternion[] _instanceRotations;

        // copy of the matrices in the instance ubo (same slots), used to build the sorted ubo for the forward pass.
        // [only valid after WriteAllDataToUBO()]
        internal Matrix4[] _instanceUBOMatrices;
        internal bool _instanceUBOMatricesValid = false;
        internal int _instanceUBOVersion = 0;

        // second ubo with the instances sorted back to front, only used by the forward pass.
        // The original ubo keeps its order, because WriteDataToUBOForInstance() writes to slot i.
        internal int _uboSorted = -1;
        private int[] _sortedOrder;
        private int[] _sortedOrderUploaded;
        private float[] _sortedDistances;
        private int _sortedVersionUploaded = -1;

        // Base matrix (and its inverse) that the additional instances in the ubo were written with.
        // The instanced shaders always multiply with the model matrix of the base instance, so the ubo holds
        // Inverse(base) * instance (absolute mode) or Inverse(base) * instance * base (relative mode).
        internal Matrix4 _uboBaseMatrix = Matrix4.Identity;
        internal Matrix4 _uboBaseMatrixInverse = Matrix4.Identity;

        internal void InitInstancesData()
        {
            _instancePositions = new Vector3[InstanceCount];
            _instanceScales = new Vector3[InstanceCount];
            _instanceRotations = new Quaternion[InstanceCount];
            _instanceUBOMatrices = new Matrix4[InstanceCount];
            _instanceUBOMatricesValid = false;

            _uboTempData = new float[BYTESPERINSTANCE / 4];

            for (int i = 0; i < _uboTempData.Length; i += 16)
            {
                _uboTempData[i + 00] = 1f;
                _uboTempData[i + 01] = 0f;
                _uboTempData[i + 02] = 0f;
                _uboTempData[i + 03] = 0f;
                _uboTempData[i + 04] = 0f;
                _uboTempData[i + 05] = 1f;
                _uboTempData[i + 06] = 0f;
                _uboTempData[i + 07] = 0f;
                _uboTempData[i + 08] = 0f;
                _uboTempData[i + 09] = 0f;
                _uboTempData[i + 10] = 1f;
                _uboTempData[i + 11] = 0f;
                _uboTempData[i + 12] = 0f;
                _uboTempData[i + 13] = 0f;
                _uboTempData[i + 14] = 0f;
                _uboTempData[i + 15] = 1f;
            }
        }

        internal void InitUBO(bool deletePreviousUBO)
        {
            if (_ubo > 0 && deletePreviousUBO)
            {
                this.DeleteUBO();
            }

            _ubo = GL.GenBuffer();
            _uboBaseMatrix = _stateCurrent._modelMatrix;
            _uboBaseMatrixInverse = _stateCurrent._modelMatrixInverse;
            WriteAllDataToUBO();
        }

        internal void WriteDataToUBOForInstance(int i)
        {
            if (_ubo > 0 && IsInCurrentWorld)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, _ubo);

                Matrix4 tmp;
                if (i == 0)
                {
                    tmp = Matrix4.Identity;
                }
                else
                {
                    tmp = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instanceRotations[i], ref _instancePositions[i]);
                    if (Mode == InstanceMode.Absolute)
                        tmp = _uboBaseMatrixInverse * tmp;
                    else
                        tmp = _uboBaseMatrixInverse * tmp * _uboBaseMatrix; // relative: instance lives in the coordinate system of the base instance
                }
                _instanceUBOMatrices[i] = tmp;
                _instanceUBOVersion++;

                // MODEL MATRIX
                _uboTempData[00] = tmp.M11;
                _uboTempData[01] = tmp.M12;
                _uboTempData[02] = tmp.M13;
                _uboTempData[03] = tmp.M14;

                _uboTempData[04] = tmp.M21;
                _uboTempData[05] = tmp.M22;
                _uboTempData[06] = tmp.M23;
                _uboTempData[07] = tmp.M24;

                _uboTempData[08] = tmp.M31;
                _uboTempData[09] = tmp.M32;
                _uboTempData[10] = tmp.M33;
                _uboTempData[11] = tmp.M34;

                _uboTempData[12] = tmp.M41;
                _uboTempData[13] = tmp.M42;
                _uboTempData[14] = tmp.M43;
                _uboTempData[15] = tmp.M44;

                GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)(i * BYTESPERINSTANCE), BYTESPERINSTANCE, _uboTempData);
            }
        }

        internal void WriteAllDataToUBO()
        {
            int floatsPerInstance = BYTESPERINSTANCE / 4;
            int totalFloats = InstanceCount * floatsPerInstance;
            if (_uboTempData == null || _uboTempData.Length < totalFloats)
            {
                _uboTempData = new float[totalFloats];
            }

            for (int i = 0; i < InstanceCount; i++)
            {
                Matrix4 tmp;
                if (i == 0)
                {
                    tmp = Matrix4.Identity;
                }
                else
                {
                    tmp = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instanceRotations[i], ref _instancePositions[i]);
                    if (Mode == InstanceMode.Absolute)
                        tmp = _uboBaseMatrixInverse * tmp;
                    else
                        tmp = _uboBaseMatrixInverse * tmp * _uboBaseMatrix; // relative: instance lives in the coordinate system of the base instance
                }

                int offset = i * floatsPerInstance;
                _instanceUBOMatrices[i] = tmp;

                // MODEL MATRIX
                _uboTempData[offset + 00] = tmp.M11;
                _uboTempData[offset + 01] = tmp.M12;
                _uboTempData[offset + 02] = tmp.M13;
                _uboTempData[offset + 03] = tmp.M14;

                _uboTempData[offset + 04] = tmp.M21;
                _uboTempData[offset + 05] = tmp.M22;
                _uboTempData[offset + 06] = tmp.M23;
                _uboTempData[offset + 07] = tmp.M24;

                _uboTempData[offset + 08] = tmp.M31;
                _uboTempData[offset + 09] = tmp.M32;
                _uboTempData[offset + 10] = tmp.M33;
                _uboTempData[offset + 11] = tmp.M34;

                _uboTempData[offset + 12] = tmp.M41;
                _uboTempData[offset + 13] = tmp.M42;
                _uboTempData[offset + 14] = tmp.M43;
                _uboTempData[offset + 15] = tmp.M44;
            }

            // Single BufferData call: orphans the previous store (avoids a GPU sync/stall on objects
            // still in flight) and uploads all instance matrices in one driver call instead of InstanceCount calls.
            GL.BindBuffer(BufferTarget.UniformBuffer, _ubo);
            GL.BufferData(BufferTarget.UniformBuffer, totalFloats * 4, _uboTempData, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            _instanceUBOVersion++;
            _instanceUBOMatricesValid = true;
        }

        // Rewrites the additional instances if the base matrix used for rendering changed (absolute mode: they keep their
        // own position, relative mode: they follow the base instance). Called once per frame after the render state was blended.
        internal void UpdateInstancesForRenderPass()
        {
            if (InstanceCount <= 1 || _ubo <= 0 || !IsInCurrentWorld)
                return;

            // base at rest: use the exact simulation matrix (the blended render matrix may differ in the last bits every frame)
            bool baseAtRest = _statePrevious._modelMatrix == _stateCurrent._modelMatrix;
            Matrix4 baseMatrix = baseAtRest ? _stateCurrent._modelMatrix : _stateRender._modelMatrix;
            if (baseMatrix == _uboBaseMatrix)
                return;

            _uboBaseMatrix = baseMatrix;
            _uboBaseMatrixInverse = baseAtRest ? _stateCurrent._modelMatrixInverse : Matrix4.Invert(baseMatrix);
            WriteAllDataToUBO();
        }

        internal void DeleteUBO()
        {
            if (_ubo > 0)
            {
                // unbind this ubo from all shaders that might use it:
                RenderManager.UnbindUBOFromAllInstanceShaders(_ubo);

                GL.DeleteBuffer(_ubo);
                _ubo = -1;
            }
            if (_uboSorted > 0)
            {
                RenderManager.UnbindUBOFromAllInstanceShaders(_uboSorted);

                GL.DeleteBuffer(_uboSorted);
                _uboSorted = -1;
                _sortedVersionUploaded = -1;
            }
        }

        // Returns the ubo for the forward pass. With more than one instance, the instances are sorted back to front
        // by the distance of the model center to the camera.
        internal int GetUBOSortedForForwardPass()
        {
            if (InstanceCount <= 1 || _ubo <= 0 || !_instanceUBOMatricesValid)
                return _ubo;

            if (_sortedOrder == null || _sortedOrder.Length != InstanceCount)
            {
                _sortedOrder = new int[InstanceCount];
                _sortedOrderUploaded = new int[InstanceCount];
                _sortedDistances = new float[InstanceCount];
                _sortedVersionUploaded = -1;
            }

            Vector3 camPos = KWEngine.Mode == EngineMode.Play ? KWEngine.CurrentWorld._cameraGame._stateRender._position : KWEngine.CurrentWorld._cameraEditor._stateRender._position;
            // same order of transformations as in the instanced vertex shaders: instance matrix * model matrix * position
            Vector3 centerBase = Vector3.TransformPosition(_model.Center.Xyz, _stateRender._modelMatrix);
            for (int k = 0; k < InstanceCount; k++)
            {
                Vector3 center = Vector3.TransformPosition(centerBase, _instanceUBOMatrices[k]);
                _sortedOrder[k] = k;
                // negative value, so that the instance with the largest distance comes first after sorting
                _sortedDistances[k] = -(center - camPos).LengthSquared;
            }
            Array.Sort(_sortedDistances, _sortedOrder, 0, InstanceCount);

            bool upload = _uboSorted <= 0 || _sortedVersionUploaded != _instanceUBOVersion;
            for (int k = 0; !upload && k < InstanceCount; k++)
            {
                if (_sortedOrder[k] != _sortedOrderUploaded[k])
                    upload = true;
            }

            if (upload)
            {
                int floatsPerInstance = BYTESPERINSTANCE / 4;
                int totalFloats = InstanceCount * floatsPerInstance;
                if (_uboTempData == null || _uboTempData.Length < totalFloats)
                {
                    _uboTempData = new float[totalFloats];
                }

                for (int k = 0; k < InstanceCount; k++)
                {
                    Matrix4 m = _instanceUBOMatrices[_sortedOrder[k]];
                    int offset = k * floatsPerInstance;
                    _uboTempData[offset + 00] = m.M11;
                    _uboTempData[offset + 01] = m.M12;
                    _uboTempData[offset + 02] = m.M13;
                    _uboTempData[offset + 03] = m.M14;
                    _uboTempData[offset + 04] = m.M21;
                    _uboTempData[offset + 05] = m.M22;
                    _uboTempData[offset + 06] = m.M23;
                    _uboTempData[offset + 07] = m.M24;
                    _uboTempData[offset + 08] = m.M31;
                    _uboTempData[offset + 09] = m.M32;
                    _uboTempData[offset + 10] = m.M33;
                    _uboTempData[offset + 11] = m.M34;
                    _uboTempData[offset + 12] = m.M41;
                    _uboTempData[offset + 13] = m.M42;
                    _uboTempData[offset + 14] = m.M43;
                    _uboTempData[offset + 15] = m.M44;
                }

                if (_uboSorted <= 0)
                {
                    _uboSorted = GL.GenBuffer();
                }
                GL.BindBuffer(BufferTarget.UniformBuffer, _uboSorted);
                GL.BufferData(BufferTarget.UniformBuffer, totalFloats * 4, _uboTempData, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                Array.Copy(_sortedOrder, _sortedOrderUploaded, InstanceCount);
                _sortedVersionUploaded = _instanceUBOVersion;
            }
            return _uboSorted;
        }

        internal bool IsConfigured { get { return InstanceCount >= 0; } }

        internal override void InitHitboxes()
        {
            UpdateModelMatrixAndHitboxes();
        }

        internal override void UpdateModelMatrixAndHitboxes()
        {
            if (_stateCurrent._rotation == new Quaternion(0, 0, 0, 0))
            {
                return;
            }

            _stateCurrent._modelMatrix = HelperMatrix.CreateModelMatrix(ref _stateCurrent);
            _stateCurrent._modelMatrixInverse = Matrix4.Invert(_stateCurrent._modelMatrix);
            _stateCurrent._lookAtVector = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitZ, _stateCurrent._modelMatrixInverse));
            _stateCurrent._lookAtVectorRight = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitX, _stateCurrent._modelMatrixInverse));
            _stateCurrent._lookAtVectorUp = Vector3.NormalizeFast(Vector3.TransformNormalInverse(Vector3.UnitY, _stateCurrent._modelMatrixInverse));

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // Get min/max for x/y/z for base instance (0):
            GetMinMaxForBaseInstance(ref min, ref max);

            // Get min/max for x/y/z for all the other instances (> 0):
            GetMinMaxForInstances(ref min, ref max);

            _stateCurrent._dimensions.X = max.X - min.X;
            _stateCurrent._dimensions.Y = max.Y - min.Y;
            _stateCurrent._dimensions.Z = max.Z - min.Z;
            _stateCurrent._center = (max + min) / 2f;

            AABBLeft = _stateCurrent._center.X - _stateCurrent._dimensions.X * 0.5f;
            AABBRight = _stateCurrent._center.X + _stateCurrent._dimensions.X * 0.5f;
            AABBLow = _stateCurrent._center.Y - _stateCurrent._dimensions.Y * 0.5f;
            AABBHigh = _stateCurrent._center.Y + _stateCurrent._dimensions.Y * 0.5f;
            AABBBack = _stateCurrent._center.Z - _stateCurrent._dimensions.Z * 0.5f;
            AABBFront = _stateCurrent._center.Z + _stateCurrent._dimensions.Z * 0.5f;
        }

        internal void GetMinMaxForBaseInstance(ref Vector3 min, ref Vector3 max)
        {
            // does the mesh have hitboxes?
            if (_model.ModelOriginal.MeshCollider.MeshHitboxes != null && _model.ModelOriginal.MeshCollider.MeshHitboxes.Count > 0)
            {
                // if so, iterate through them and check their boundaries
                foreach (GeoMeshHitbox hb in _model.ModelOriginal.MeshCollider.MeshHitboxes)
                {
                    ExtendMinMaxByTransformedBox(new Vector3(hb.minX, hb.minY, hb.minZ), new Vector3(hb.maxX, hb.maxY, hb.maxZ), ref _stateCurrent._modelMatrix, ref min, ref max);
                }
            }
            else
            {
                // no hitboxes: use the bounds of all meshes, determined once by EngineObjectModel when the model was set
                // (same vbo data as before, so the vbo does not have to be read back from the gpu on every update)
                Vector3 boxMin = _model.DimensionsMin.Xyz;
                Vector3 boxMax = _model.DimensionsMax.Xyz;
                if (boxMin.X > boxMax.X)
                {
                    KWEngine.LogWriteLine("[RenderObject] Cannot determine volume for frustum culling for " + this.Name);
                }
                ExtendMinMaxByTransformedBox(boxMin, boxMax, ref _stateCurrent._modelMatrix, ref min, ref max);
            }
        }

        internal void GetMinMaxForInstances(ref Vector3 min, ref Vector3 max)
        {
            // does the mesh have hitboxes?
            if (_model.ModelOriginal.MeshCollider.MeshHitboxes != null && _model.ModelOriginal.MeshCollider.MeshHitboxes.Count > 0)
            {
                // if so, iterate through them and check their boundaries
                Vector4 meshMin = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, 1);
                Vector4 meshMax = new Vector4(float.MinValue, float.MinValue, float.MinValue, 1);

                foreach (GeoMeshHitbox hb in _model.ModelOriginal.MeshCollider.MeshHitboxes)
                {
                    if (meshMin.X > hb.minX) meshMin.X = hb.minX;
                    if (meshMax.X < hb.maxX) meshMax.X = hb.maxX;

                    if (meshMin.Y > hb.minY) meshMin.Y = hb.minY;
                    if (meshMax.Y < hb.maxY) meshMax.Y = hb.maxY;

                    if (meshMin.Z > hb.minZ) meshMin.Z = hb.minZ;
                    if (meshMax.Z < hb.maxZ) meshMax.Z = hb.maxZ;
                }

                for (int i = 1; i < InstanceCount; i++)
                {
                    if (_instanceScales[i] == Vector3.Zero)
                        continue; // instance has not been set yet
                    Matrix4 iModel = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instanceRotations[i], ref _instancePositions[i]);
                    if (Mode == InstanceMode.Relative)
                        iModel = iModel * _stateCurrent._modelMatrix;
                    ExtendMinMaxByTransformedBox(meshMin.Xyz, meshMax.Xyz, ref iModel, ref min, ref max);
                }
            }
            else
            {
                // no hitboxes: use the bounds of all meshes, determined once by EngineObjectModel when the model was set
                Vector4 meshMin = _model.DimensionsMin;
                Vector4 meshMax = _model.DimensionsMax;

                for (int i = 1; i < InstanceCount; i++)
                {
                    if (_instanceScales[i] == Vector3.Zero)
                        continue; // instance has not been set yet
                    Matrix4 iModel = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instanceRotations[i], ref _instancePositions[i]);
                    if (Mode == InstanceMode.Relative)
                        iModel = iModel * _stateCurrent._modelMatrix;
                    ExtendMinMaxByTransformedBox(meshMin.Xyz, meshMax.Xyz, ref iModel, ref min, ref max);
                }
            }
        }

        // extends min/max by the 8 corners of the box (boxMin, boxMax) after transforming them with m
        private static void ExtendMinMaxByTransformedBox(Vector3 boxMin, Vector3 boxMax, ref Matrix4 m, ref Vector3 min, ref Vector3 max)
        {
            if (boxMin.X > boxMax.X || boxMin.Y > boxMax.Y || boxMin.Z > boxMax.Z)
                return; // no valid box

            for (int c = 0; c < 8; c++)
            {
                Vector3 corner = new Vector3((c & 1) == 0 ? boxMin.X : boxMax.X, (c & 2) == 0 ? boxMin.Y : boxMax.Y, (c & 4) == 0 ? boxMin.Z : boxMax.Z);
                Vector3 p = Vector3.TransformPosition(corner, m);
                min = Vector3.ComponentMin(min, p);
                max = Vector3.ComponentMax(max, p);
            }
        }
        #endregion
    }
}

