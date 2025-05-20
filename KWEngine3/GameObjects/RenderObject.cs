using KWEngine3.Helper;
using KWEngine3.Model;
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
            if(instanceCount > KWEngine.MAXADDITIONALINSTANCECOUNT)
            {
                KWEngine.LogWriteLine("[RenderObject] Max. additional instance count per object is " + KWEngine.MAXADDITIONALINSTANCECOUNT);
            }
            Mode = mode;
            instanceCount = Math.Clamp(instanceCount, 0, KWEngine.MAXADDITIONALINSTANCECOUNT);
            InstanceCount = instanceCount + 1;
            InitUBO();
        }

        /// <summary>
        /// Vergleicht das Objekt bzgl. seiner Entfernung zur Kamera mit einem anderen Objekt
        /// </summary>
        /// <param name="other">anderes RenderObject</param>
        /// <returns>1, wenn das aufrufende Objekt näher an der Kamera ist, sonst -1</returns>
        public int CompareTo(RenderObject other)
        {
            Vector3 camPos = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateCurrent._position : KWEngine.CurrentWorld.CameraPosition;

            float distanceToCameraThis = (this.Position - camPos).LengthSquared;
            float distanceToCameraOther = (other.Position - camPos).LengthSquared;
            return distanceToCameraOther > distanceToCameraThis ? 1 : -1;
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
                    for (int i = 0; i < _model.Material.Length; i++)
                    {
                        _model.Material[i] = model.Meshes.Values.ToArray()[i].Material;
                    }
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
            if(instanceIndex <= 0)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index must be >= 1");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index must be >= 1");
            }

            if(instanceIndex > InstanceCount - 1)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index must be below instance count");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index must be < instance count");
            }
            if(scale.X <= 0 || scale.Y <= 0 || scale.Z <= 0)
            {
                KWEngine.LogWriteLine("[RenderObject] Instance index has invalid scale value");
                throw new Exceptions.GameObjectException("[RenderObject] Instance index has invalid scale value");
            }

            Matrix4 tmp = HelperMatrix.CreateModelMatrix(ref scale, ref rotation, ref position);
            if(Mode == InstanceMode.Absolute)
                tmp = _stateCurrent._modelMatrixInverse * tmp;

            // MODEL MATRIX
            _uboData[00] = tmp.M11;
            _uboData[01] = tmp.M12;
            _uboData[02] = tmp.M13;
            _uboData[03] = tmp.M14;

            _uboData[04] = tmp.M21;
            _uboData[05] = tmp.M22;
            _uboData[06] = tmp.M23;
            _uboData[07] = tmp.M24;

            _uboData[08] = tmp.M31;
            _uboData[09] = tmp.M32;
            _uboData[10] = tmp.M33;
            _uboData[11] = tmp.M34;

            _uboData[12] = tmp.M41;
            _uboData[13] = tmp.M42;
            _uboData[14] = tmp.M43;
            _uboData[15] = tmp.M44;

            GL.BindBuffer(BufferTarget.UniformBuffer, _ubo);
            IntPtr ptr = IntPtr.Add(IntPtr.Zero, instanceIndex * BYTESPERINSTANCE);
            GL.BufferSubData(BufferTarget.UniformBuffer, ptr, BYTESPERINSTANCE, _uboData);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            _instancePositions[instanceIndex] = position;
            _instanceScales[instanceIndex] = scale;

            UpdateModelMatrixAndHitboxes();
        }

        #region internals
        internal const int BYTESPERINSTANCE = 16 * 4; // 64 Bytes for 1 matrix
        internal int _ubo = -1;
        internal static float[] _uboData;

        internal Vector3[] _instancePositions;
        internal Vector3[] _instanceScales;

        internal void InitUBO()
        {
            _instancePositions = new Vector3[InstanceCount];
            _instanceScales = new Vector3[InstanceCount];

            _uboData = new float[BYTESPERINSTANCE / 4];
            for(int i = 0; i < _uboData.Length; i+=16)
            {
                _uboData[i + 00] = 1f;
                _uboData[i + 01] = 0f;
                _uboData[i + 02] = 0f;
                _uboData[i + 03] = 0f;
                _uboData[i + 04] = 0f;
                _uboData[i + 05] = 1f;
                _uboData[i + 06] = 0f;
                _uboData[i + 07] = 0f;
                _uboData[i + 08] = 0f;
                _uboData[i + 09] = 0f;
                _uboData[i + 10] = 1f;
                _uboData[i + 11] = 0f;
                _uboData[i + 12] = 0f;
                _uboData[i + 13] = 0f;
                _uboData[i + 14] = 0f;
                _uboData[i + 15] = 1f;
            }
            
            if(_ubo > 0)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);
                GL.DeleteBuffers(1, new int[] { _ubo });
            }

            _ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _ubo);
            GL.BufferData(BufferTarget.UniformBuffer, InstanceCount * BYTESPERINSTANCE, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            for (int i = 0; i < InstanceCount; i++)
            {
                GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)(i * BYTESPERINSTANCE), BYTESPERINSTANCE, _uboData);
            }
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
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

            _stateCurrent._modelMatrix = HelperMatrix.CreateModelMatrix(_stateCurrent);
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
        }

        internal void GetMinMaxForBaseInstance(ref Vector3 min, ref Vector3 max)
        {
            // does the mesh have hitboxes?
            if (_model.ModelOriginal.MeshCollider.MeshHitboxes != null && _model.ModelOriginal.MeshCollider.MeshHitboxes.Count > 0)
            {
                // if so, iterate through them and check their boundaries
                foreach(GeoMeshHitbox hb in _model.ModelOriginal.MeshCollider.MeshHitboxes)
                {
                    Vector4 currentMin = Vector4.TransformRow(new Vector4(hb.minX, hb.minY, hb.minZ, 1.0f), _stateCurrent._modelMatrix);
                    Vector4 currentMax = Vector4.TransformRow(new Vector4(hb.maxX, hb.maxY, hb.maxZ, 1.0f), _stateCurrent._modelMatrix);
                    if (currentMin.X < min.X) min.X = currentMin.X;
                    if (currentMin.Y < min.Y) min.Y = currentMin.Y;
                    if (currentMin.Z < min.Z) min.Z = currentMin.Z;
                    if (currentMax.X > max.X) max.X = currentMax.X;
                    if (currentMax.Y > max.Y) max.Y = currentMax.Y;
                    if (currentMax.Z > max.Z) max.Z = currentMax.Z;
                }
            }
            else
            {
                KWEngine.LogWriteLine("[RenderObject] WARNING: using slow volume determination for " + this.Name);

                // otherwise, scan through all the vertices of the vbo and determine the dimensions (slow!)
                foreach (GeoMesh mesh in _model.ModelOriginal.Meshes.Values)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VBOPosition);
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int sizeInBytes);
                    if(sizeInBytes > 0)
                    {
                        float[] buffer = new float[sizeInBytes / sizeof(float)];
                        GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeInBytes, buffer);

                        Vector3 currentMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                        Vector3 currentMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                        for (int i = 0; i < buffer.Length; i += 3)
                        {
                            if (buffer[i + 0] < min.X) currentMin.X = buffer[i + 0];
                            if (buffer[i + 1] < min.Y) currentMin.Y = buffer[i + 1];
                            if (buffer[i + 2] < min.Z) currentMin.Z = buffer[i + 2];

                            if (buffer[i + 0] > max.X) currentMax.X = buffer[i + 0];
                            if (buffer[i + 1] > max.Y) currentMax.Y = buffer[i + 1];
                            if (buffer[i + 2] > max.Z) currentMax.Z = buffer[i + 2];
                        }
                        currentMin = Vector4.TransformRow(new Vector4(currentMin, 1.0f), _stateCurrent._modelMatrix).Xyz;
                        currentMax = Vector4.TransformRow(new Vector4(currentMax, 1.0f), _stateCurrent._modelMatrix).Xyz;

                        if (currentMin.X < min.X) min.X = currentMin.X;
                        if (currentMin.Y < min.Y) min.Y = currentMin.Y;
                        if (currentMin.Z < min.Z) min.Z = currentMin.Z;
                        if (currentMax.X > max.X) max.X = currentMax.X;
                        if (currentMax.Y > max.Y) max.Y = currentMax.Y;
                        if (currentMax.Z > max.Z) max.Z = currentMax.Z;
                    }
                    else
                    {
                        // maybe: make default size? but should never happen anyway...
                        KWEngine.LogWriteLine("[RenderObject] Cannot determine volume for frustum culling for " + this.Name + " (" + mesh.Name + ")");
                    }
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }
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

                for(int i = 1; i < InstanceCount; i++)
                {
                    Matrix4 iModel = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instancePositions[i]);
                    Vector3 currentMin = Vector4.TransformRow(meshMin, iModel).Xyz;
                    Vector3 currentMax = Vector4.TransformRow(meshMax, iModel).Xyz;

                    if (currentMin.X < min.X) min.X = currentMin.X;
                    if (currentMin.Y < min.Y) min.Y = currentMin.Y;
                    if (currentMin.Z < min.Z) min.Z = currentMin.Z;
                    if (currentMax.X > max.X) max.X = currentMax.X;
                    if (currentMax.Y > max.Y) max.Y = currentMax.Y;
                    if (currentMax.Z > max.Z) max.Z = currentMax.Z;
                }
            }
            else
            {
                // otherwise, scan through all the vertices of the vbo and determine the dimensions (slow!)
                KWEngine.LogWriteLine("[RenderObject] WARNING: using slow volume determination for " + this.Name);

                Vector4 meshMin = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, 1);
                Vector4 meshMax = new Vector4(float.MinValue, float.MinValue, float.MinValue, 1);

                foreach (GeoMesh mesh in _model.ModelOriginal.Meshes.Values)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VBOPosition);
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int sizeInBytes);
                    if (sizeInBytes > 0)
                    {
                        float[] buffer = new float[sizeInBytes / sizeof(float)];
                        GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeInBytes, buffer);

                        for (int i = 0; i < buffer.Length; i += 3)
                        {
                            if (buffer[i + 0] < min.X) meshMin.X = buffer[i + 0];
                            if (buffer[i + 1] < min.Y) meshMin.Y = buffer[i + 1];
                            if (buffer[i + 2] < min.Z) meshMin.Z = buffer[i + 2];

                            if (buffer[i + 0] > max.X) meshMax.X = buffer[i + 0];
                            if (buffer[i + 1] > max.Y) meshMax.Y = buffer[i + 1];
                            if (buffer[i + 2] > max.Z) meshMax.Z = buffer[i + 2];
                        }
                    }
                    else
                    {
                        // maybe: make default size? but should never happen anyway...
                        KWEngine.LogWriteLine("[RenderObject] Cannot determine volume for frustum culling for " + this.Name + " (" + mesh.Name + ")");
                    }
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                for (int i = 1; i < InstanceCount; i++)
                {
                    Matrix4 iModel = HelperMatrix.CreateModelMatrix(ref _instanceScales[i], ref _instancePositions[i]);
                    Vector3 currentMin = Vector4.TransformRow(meshMin, iModel).Xyz;
                    Vector3 currentMax = Vector4.TransformRow(meshMax, iModel).Xyz;

                    if (currentMin.X < min.X) min.X = currentMin.X;
                    if (currentMin.Y < min.Y) min.Y = currentMin.Y;
                    if (currentMin.Z < min.Z) min.Z = currentMin.Z;
                    if (currentMax.X > max.X) max.X = currentMax.X;
                    if (currentMax.Y > max.Y) max.Y = currentMax.Y;
                    if (currentMax.Z > max.Z) max.Z = currentMax.Z;
                }
            }
        }
        #endregion
    }
}

