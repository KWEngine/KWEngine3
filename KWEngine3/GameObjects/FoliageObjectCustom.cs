using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für benutzerdefinierte Bodengewächse (maximal 1024 pro Instanz)
    /// </summary>
    public sealed class FoliageObjectCustom : FoliageBase
    {
        /// <summary>
        /// Konstruktormethode für ein Custom-Bodengewächs mit mehreren Instanzen
        /// </summary>
        /// <param name="instanceCount">Anzahl der Instanzen (Werte zwischen 1 und 256)</param>
        public FoliageObjectCustom(int instanceCount)
        {
            if(instanceCount < 1 || instanceCount > 256)
            {
                KWEngine.LogWriteLine("[FoliageObject] Invalid instance count detected");
            }
            SetInstanceCount(Math.Clamp(instanceCount, 1, 256));
        }

        /// <summary>
        /// Setzt die (Albedo-)Textur
        /// </summary>
        /// <param name="filename">Dateiname der Textur (inkl. relativem oder absolutem Pfad)</param>
        public void SetTexture(string filename)
        {
            filename = HelperGeneral.EqualizePathDividers(filename);
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(filename))
            {
                _textureId = KWEngine.CurrentWorld._customTextures[filename].ID;
            }
            else
            {
                _textureId = HelperTexture.LoadTextureForModelExternal(filename, out int mipMaps);
                if (_textureId < 0)
                {
                    KWEngine.LogWriteLine("[FoliageObject] Invalid texture file (" + filename + ")");
                }
                else
                {
                    KWEngine.CurrentWorld._customTextures.Add(filename, new KWTexture(_textureId, TextureTarget.Texture2D));
                }
            }
        }

        /// <summary>
        /// Name des Objekts
        /// </summary>
        public string Name { get { return _name; } set { _name = value == null ? "(no name)" : value; } }

        /// <summary>
        /// Gibt die Tönung und Leuchtkraft des Bodengewächses an
        /// </summary>
        public Vector4 Color { get { return _color; } }

        /// <summary>
        /// Gibt die aktuelle Rauheit der Gewächsoberfläche an
        /// </summary>
        public float Roughness { get { return _roughness; } }

        /// <summary>
        /// Gibt die Anzahl der für das Gewächs gerenderten Instanzen zurück
        /// </summary>
        public int FoliageCount { get { return _instanceCount; } }
        
        /// <summary>
        /// Gibt an, ob Schatten auf das Objekt geworfen werden können (Standard: true)
        /// </summary>
        public bool IsShadowReceiver { get; set; } = true;

        /// <summary>
        /// Gibt an, ob dieses Objekt auf Licht reagiert (Standard: true)
        /// </summary>
        public bool IsAffectedByLight { get; set; } = true;

        /// <summary>
        /// Gibt an, wie stark das Gras sich im Wind wiegt
        /// </summary>
        /// <param name="swayFactor">Verstärkungsfaktor (Werte zwischen 0 und 1, Standard: 0.05f)</param>
        public void SetSwayFactor(float swayFactor)
        {
            _swayFactor = Math.Clamp(swayFactor, 0f, 1f);
        }

        /// <summary>
        /// Gibt an, wie rau die Oberfläche des Gewächses ist (0f = glatt, 1f = rau)
        /// </summary>
        /// <param name="r">Grad der Rauheit</param>
        public void SetRoughness(float r)
        {
            _roughness = MathHelper.Clamp(r, 0.01f, 1.0f);
        }

        /// <summary>
        /// Setzt die Tönung und Leuchtkraft des Bodengewächses
        /// </summary>
        /// <param name="color">Tönung (RGB zwischen 0 und 1) und Leuchtkraft (zwischen 0 und 8, Standardwert: 0)</param>
        public void SetColor(Vector4 color)
        {
            _color.X = Math.Clamp(color.X, 0f, 1f);
            _color.Y = Math.Clamp(color.Y, 0f, 1f);
            _color.Z = Math.Clamp(color.Z, 0f, 1f);
            _color.W = Math.Clamp(color.W, 0f, 8f);
        }

        /// <summary>
        /// Setzt die Tönung und Leuchtkraft des Bodengewächses
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="b">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="intensity">Leuchtkraft (zwischen 0 und 8, Standardwert: 0)</param>
        public void SetColor(float r, float g, float b, float intensity = 0)
        {
            SetColor(new Vector4(r, g, b, intensity));
        }

        /// <summary>
        /// Setzt die Position des Gewächsmittelpunkts
        /// </summary>
        /// <param name="instanceIndex">Nullbasierter Index der zu positionierenden Instanz</param>
        /// <param name="position">neue Position</param>
        public void SetPosition(int instanceIndex, Vector3 position)
        {
            if (_isInWorld != null)
            {
                KWEngine.LogWriteLine("[FoliageObject] Instance may not change position, scale or texture settings after being added to the world");
                return;
            }

            if (instanceIndex < _instanceCount)
            {
                _posScaleArray1[instanceIndex * 4 + 0] = position.X;
                _posScaleArray1[instanceIndex * 4 + 1] = position.Y;
                _posScaleArray1[instanceIndex * 4 + 2] = position.Z;

                Update();
            }
        }

        /// <summary>
        /// Setzt die Position des Gewächsmittelpunkts
        /// </summary>
        /// <param name="instanceIndex">Nullbasierter Index der zu positionierenden Instanz</param>
        /// <param name="x">neue X-Position</param>
        /// <param name="y">neue Y-Position</param>
        /// <param name="z">neue Z-Position</param>
        public void SetPosition(int instanceIndex, float x, float y, float z)
        {
            SetPosition(instanceIndex, new Vector3(x, y, z));
        }

        /// <summary>
        /// Gibt die Größe für ein bestimmtes Gewächsteil an (Standardwert: 1)
        /// </summary>
        /// <param name="instanceIndex">Nullbasierter Index der zu skalierenden Instanz</param>
        /// <param name="scale">Achsengleiche Skalierung (muss positiv sein)</param>
        public void SetScale(int instanceIndex, float scale)
        {
            SetScale(instanceIndex, new Vector3(scale));
        }

        /// <summary>
        /// Gibt die Größe für ein bestimmtes Gewächsteil an (Standardwert: 1)
        /// </summary>
        /// <param name="instanceIndex">Nullbasierter Index der zu skalierenden Instanz</param>
        /// <param name="scaleX">Skalierung der x-Achse (muss positiv sein)</param>
        /// <param name="scaleY">Skalierung der y-Achse (muss positiv sein)</param>
        /// <param name="scaleZ">Skalierung der z-Achse (muss positiv sein)</param>
        public void SetScale(int instanceIndex, float scaleX, float scaleY, float scaleZ)
        {
            SetScale(instanceIndex, new Vector3(scaleX, scaleY, scaleZ));
        }

        /// <summary>
        /// Gibt die Größe für ein bestimmtes Gewächsteil an (Standardwert: 1)
        /// </summary>
        /// <param name="instanceIndex">Nullbasierter Index der zu skalierenden Instanz</param>
        /// <param name="scale">Skalierung der drei Achsen (muss positiv sein)</param>
        public void SetScale(int instanceIndex, Vector3 scale)
        {
            if(_isInWorld != null)
            {
                KWEngine.LogWriteLine("[FoliageObject] Instance may not change position, scale or texture settings after being added to the world");
                return;
            }

            if (instanceIndex < _instanceCount)
            {
                _posScaleArray1[instanceIndex * 4 + 3] = Math.Clamp(scale.X, 0.001f, 4096f);
                _posScaleArray2[instanceIndex * 4 + 0] = Math.Clamp(scale.Y, 0.001f, 4096f);
                _posScaleArray2[instanceIndex * 4 + 1] = Math.Clamp(scale.Z, 0.001f, 4096f);

                Update();
            }
        }

        /// <summary>
        /// Richtet das Gewächs bzgl. seiner Höhe am übergebenen Terrain aus
        /// </summary>
        /// <param name="t">Terrain-Objekt, an dem sich das Gewächs orientieren soll</param>
        public void AttachToTerrain(TerrainObject t)
        {
            if(t != null && t.ID > 0)
            {
                _terrainObject = t;
            }
        }

        /// <summary>
        /// Löst die Bindung zum aktuell festgelegten Terrain-Objekt
        /// </summary>
        public void DetachFromTerrain()
        {
            _terrainObject = null;
        }


        /// <summary>
        /// Gibt an, wie weit die Entfernung der Kamera zum Mittelpunkt des Objekts sein darf bevor das Objekt nicht mehr gezeichnet wird (Standardwert: 20 Einheiten)
        /// </summary>
        /// <param name="distance">Renderdistanz (positive Werte ab 1.0f)</param>
        public void SetCameraDrawDistance(float distance)
        {
            distance = MathHelper.Clamp(distance, 1f, 8096f);
            _camDistanceMax = distance * distance;
        }

        #region internals
        internal Vector4 _color = new(1, 1, 1, 1);
        internal float _roughness = 1.0f;
        internal Vector3 _patchSizeMax = Vector3.Zero;
        internal Vector3 _patchSizeMin = Vector3.Zero;
        internal int _instanceCount;
        internal string _name = "(no name)";
        internal int _textureId = -1;
        internal float _swayFactor = 0.05f;
        internal float _camDistanceMax = 20 * 20;
        internal TerrainObject _terrainObject = null;
        internal float[] _posScaleArray1;
        internal float[] _posScaleArray2;
        internal float[] _noise = new float[2];
        internal World _isInWorld = null;
        internal bool IsInsideScreenSpaceForRenderPass { get; set; } = true;

        /// <summary>
        /// Gibt an, ob das Objekt gerade in auf dem Bild zu sehen ist
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;


        internal void Update()
        {
            UpdateArrayAndPatchSize();   
        }

        internal void SetInstanceCount(int instanceCount)
        {
            _instanceCount = instanceCount;
            
            CreateArray();
            Update();
        }

        internal void UpdateArrayAndPatchSize()
        {
            float xmin = float.MaxValue;
            float ymin = float.MaxValue;
            float zmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymax = float.MinValue;
            float zmax = float.MinValue;
            for (int i = 0; i < _posScaleArray1.Length; i += 4)
            {
                if (_posScaleArray1[i + 0] - _posScaleArray1[i + 3] * 0.5f < xmin)
                    xmin = _posScaleArray1[i + 0] - _posScaleArray1[i + 3] * 0.5f;
                if (_posScaleArray1[i + 1] - _posScaleArray2[i + 0] * 0.5f < ymin)
                    ymin = _posScaleArray1[i + 1] - _posScaleArray2[i + 0] * 0.5f;
                if (_posScaleArray1[i + 2] - _posScaleArray2[i + 1] * 0.5f < zmin)
                    zmin = _posScaleArray1[i + 2] - _posScaleArray2[i + 1] * 0.5f;

                if (_posScaleArray1[i + 0] - _posScaleArray1[i + 3] * 0.5f > xmax)
                    xmax = _posScaleArray1[i + 0] + _posScaleArray1[i + 3] * 0.5f;
                if (_posScaleArray1[i + 1] - _posScaleArray2[i + 0] * 0.5f > ymax)
                    ymax = _posScaleArray1[i + 1] + _posScaleArray2[i + 0] * 0.5f;
                if (_posScaleArray1[i + 2] - _posScaleArray2[i + 1] * 0.5f > zmax)
                    zmax = _posScaleArray1[i + 2] + _posScaleArray2[i + 1] * 0.5f;
            }

            _patchSizeMin.X = xmin;
            _patchSizeMin.Y = ymin;
            _patchSizeMin.Z = zmin;

            _patchSizeMax.X = xmax;
            _patchSizeMax.Y = ymax;
            _patchSizeMax.Z = zmax;
        }

        internal void CreateArray()
        {
            _posScaleArray1 = new float[_instanceCount * 4];
            _posScaleArray2 = new float[_instanceCount * 4];
            for (int i = 0; i < _posScaleArray1.Length; i += 4)
            {
                _posScaleArray1[i + 0] = 0; // pos X
                _posScaleArray1[i + 1] = 0; // pos Y
                _posScaleArray1[i + 2] = 0; // pos Z
                _posScaleArray1[i + 3] = 1; // sca X

                _posScaleArray2[i + 0] = 1; // sca Y
                _posScaleArray2[i + 1] = 1; // sca Z
                _posScaleArray2[i + 2] = 0; // uv #1
                _posScaleArray2[i + 3] = 0; // uv #2
            }
        }

        #endregion
    }
}
