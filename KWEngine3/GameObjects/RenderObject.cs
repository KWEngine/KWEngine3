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
        /// Gibt an, ob die Instanz vollständig konfiguriert wurde
        /// </summary>
        public bool IsConfigured { get { return InstanceCount >= 0; } }

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
        public void SetAdditionalInstanceCount(int instanceCount)
        {
            if(instanceCount > KWEngine.MAXADDITIONALINSTANCECOUNT)
            {
                KWEngine.LogWriteLine("[RenderObject] Max. additional instance count per object is " + KWEngine.MAXADDITIONALINSTANCECOUNT);
            }
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
        /// Setzt die Position/Rotation/Skalierung für eine bestimmte Instanz. Alle Angaben sind in Relation zur Position/Rotation/Skalierung der Hauptinstanz zu verstehen.
        /// </summary>
        /// <param name="instanceIndex">Index der Instanz</param>
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

            Matrix4 tmp = HelperMatrix.CreateModelMatrix(ref scale, ref rotation, ref position);
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

            UpdateModelMatrixAndHitboxes();
        }

        #region internals
        internal const int BYTESPERINSTANCE = 16 * 4; // 64 Bytes for 1 matrix
        internal int _ubo = -1;
        internal static float[] _uboData;


        internal void InitUBO()
        {
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

            Vector3 dimsMax = new Vector3(float.MinValue);
            Vector3 dimsMin = new Vector3(float.MaxValue);

            if (InstanceCount > 1)
            {
                // loop through all instances to get an estimate of the outer dimensions:
                GL.BindBuffer(BufferTarget.UniformBuffer, _ubo);
                float[] tmp = new float[InstanceCount * 16];
                GL.GetBufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, InstanceCount * BYTESPERINSTANCE, tmp);
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                for (int i = 16; i < tmp.Length; i += 16)
                {
                    // get translation part for rough estimation of final screen space size:
                    float tX = tmp[i + 12];
                    float tY = tmp[i + 13];
                    float tZ = tmp[i + 14];

                    if (tX > dimsMax.X)
                        dimsMax.X = tX;
                    if (tX < dimsMin.X)
                        dimsMin.X = tX;

                    if (tY > dimsMax.Y)
                        dimsMax.Y = tY;
                    if (tY < dimsMin.Y)
                        dimsMin.Y = tY;

                    if (tZ > dimsMax.Z)
                        dimsMax.Z = tZ;
                    if (tZ < dimsMin.Z)
                        dimsMin.Z = tZ;
                }
            }
            else
            {
                dimsMin.X = 0; dimsMin.Y = 0; dimsMin.Z = 0;
                dimsMax.X = 0; dimsMax.Y = 0; dimsMax.Z = 0;
            }

            Vector4 dimMinTransformed = Vector4.TransformRow(_model.DimensionsMin + new Vector4(dimsMin, 0f), _stateCurrent._modelMatrix);
            Vector4 dimMaxTransformed = Vector4.TransformRow(_model.DimensionsMax + new Vector4(dimsMax, 0f), _stateCurrent._modelMatrix);

            _stateCurrent._dimensions.X = dimMaxTransformed.X - dimMinTransformed.X;
            _stateCurrent._dimensions.Y = dimMaxTransformed.Y - dimMinTransformed.Y;
            _stateCurrent._dimensions.Z = dimMaxTransformed.Z - dimMinTransformed.Z;
            _stateCurrent._center = (dimMaxTransformed.Xyz + dimMinTransformed.Xyz) / 2f;
        }
        #endregion
    }
}
