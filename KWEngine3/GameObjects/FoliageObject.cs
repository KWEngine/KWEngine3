﻿using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Bodengewächse (z.B. Gras)
    /// </summary>
    public sealed class FoliageObject
    {
        /// <summary>
        /// Konstruktormethode für das Bodengewächs mit einer Instanz
        /// </summary>
        public FoliageObject()
            : this(1)
        {

        }

        /// <summary>
        /// Konstruktormethode für das Bodengewächs mit mehreren Instanzen
        /// </summary>
        /// <param name="instanceCount">Anzahl der Instanzen</param>
        public FoliageObject(int instanceCount)
        {
            _instanceCount = instanceCount;
        }

        /// <summary>
        /// Name des Objekts
        /// </summary>
        public string Name { get { return _name; } set { _name = value == null ? "(no name)" : value; } }

        /// <summary>
        /// Gibt an, ob sich das Objekt im Sichtbereich der Kamera befindet
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;

        /// <summary>
        /// Gibt die Tönung und Leuchtkraft des Bodengewächses an
        /// </summary>
        public Vector4 Color { get { return _color; } }

        /// <summary>
        /// Gibt die Position des Bodengewächses an
        /// </summary>
        public Vector3 Position { get { return _position; } }

        /// <summary>
        /// Gibt die Ausweitung des Gewächses an
        /// </summary>
        public Vector2 PatchSize { get { return _patchSize; } }

        /// <summary>
        /// Gibt die Anzahl der für das Gewächs gerenderten Instanzen zurück
        /// </summary>
        public int FoliageCount { get { return _instanceCount; } }
        
        /// <summary>
        /// Gibt die aktuelle Rotation als Quaternion an
        /// </summary>
        public Quaternion Rotation { get { return _rotation; } }

        /// <summary>
        /// Setzt die (ungefähre) Anzahl an benötigten Instanzen für das Gewächs
        /// </summary>
        /// <param name="instanceCount">Anzahl der gewünschten Instanzen</param>
        public void SetInstanceCount(int instanceCount)
        {
            _instanceCount = instanceCount;
            UpdateInstanceCountForPatchSize();
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
        /// Setzt die Position des Gewächsmittelpunkts
        /// </summary>
        /// <param name="position">neue Position</param>
        public void SetPosition(Vector3 position)
        {
            _position = position;
            UpdateModelMatrix();
        }

        /// <summary>
        /// Setzt die Position des Gewächsmittelpunkts
        /// </summary>
        /// <param name="x">neue X-Position</param>
        /// <param name="y">neue Y-Position</param>
        /// <param name="z">neue Z-Position</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
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
        /// Setzt die Fläche um die Position, auf die Bodengewächsinstanzen verteilt werden
        /// </summary>
        /// <param name="x">Ausbreitung in der Breite (Minimum: 1f, Maximum 1024f)</param>
        /// <param name="z">Ausbreitung in der Tiefe (Minimum: 1f,  Maximum 1024f)</param>
        public void SetPatchSize(float x, float z)
        {
            _patchSize.X = Math.Clamp(x, 1f, 1024f);
            _patchSize.Y = Math.Clamp(z, 1f, 1024f);
            UpdateInstanceCountForPatchSize();
        }

        /// <summary>
        /// Gibt die Größe je Gewächsteil (Standardwert: 1)
        /// </summary>
        /// <param name="width">Breite eines einzelnes Gewächsteils (muss positiv sein)</param>
        /// <param name="height">Höhe eines einzelnes Gewächsteils (muss positiv sein)</param>
        /// <param name="depth">Tiefe eines einzelnes Gewächsteils (muss positiv sein)</param>
        public void SetScale(float width, float height, float depth)
        {
            SetScale(new Vector3(width, height, depth));
        }


        /// <summary>
        /// Gibt die Größe je Gewächsteil (Standardwert: 1)
        /// </summary>
        /// <param name="s">Skalierung (jede Vektorkomponente muss positiv sein)</param>
        public void SetScale(Vector3 s)
        {
            _scale.X = Math.Clamp(s.X, 0.001f, 4096f);
            _scale.Y = Math.Clamp(s.Y, 0.001f, 4096f);
            _scale.Z = Math.Clamp(s.Z, 0.001f, 4096f);
            UpdateModelMatrix();
        }

        /// <summary>
        /// Setzt die Rotation des Gewächsfeldes (in Grad, Rotationsreihenfolge: Z -> Y -> X)
        /// </summary>
        /// <param name="x">x-Rotation (in Grad)</param>
        /// <param name="y">y-Rotation (in Grad)</param>
        /// <param name="z">z-Rotation (in Grad)</param>
        public void SetRotation(float x, float y, float z)
        {
            SetRotation(HelperRotation.GetQuaternionForEulerDegrees(x, y, z));
        }

        /// <summary>
        /// Setzt die Rotation des Gewächsfeldes
        /// </summary>
        /// <param name="r">Rotation als Quaternion</param>
        public void SetRotation(Quaternion r)
        {
            _rotation = r;
            UpdateModelMatrix();
        }

        #region internals
        internal Vector4 _color = new(1, 1, 1, 1);
        internal Vector3 _position = new();
        internal Vector3 _scale = Vector3.One;
        internal Quaternion _rotation = Quaternion.Identity;
        internal Vector2 _patchSize = Vector2.One;
        internal int _instanceCount;
        internal string _name = "(no name)";
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Matrix4 _normalMatrix = Matrix4.Identity;
        internal Vector2i _nXZ = Vector2i.One;
        internal Vector2 _dXZ = Vector2.Zero;
        internal float[] _noise;
        

        internal void UpdateModelMatrix()
        {
            _modelMatrix = HelperMatrix.CreateModelMatrix(ref _scale, ref _rotation, ref _position);
            _normalMatrix = Matrix4.Transpose(Matrix4.Invert(_modelMatrix));
        }

        internal void UpdateInstanceCountForPatchSize()
        {
            float nxtmp;
            nxtmp = (float)Math.Sqrt(
                    (_patchSize.X / _patchSize.Y) * _instanceCount
                    +
                    Math.Pow(_patchSize.X - _patchSize.Y, 2) / (4 * Math.Pow(_patchSize.Y, 2))
                )
                - (_patchSize.X - _patchSize.Y) / (2f * _patchSize.Y);

            _nXZ.X = (int)Math.Round(nxtmp);
            _nXZ.Y = (int)Math.Round(_instanceCount / nxtmp);

            _dXZ.X = _patchSize.X / (_nXZ.X - 1);
            _dXZ.Y = _patchSize.Y / (_nXZ.Y - 1);

            _instanceCount = _nXZ.X * _nXZ.Y;

            _noise = new float[512];
            for (int i = 0; i < _noise.Length; i+=2)
            {
                _noise[i + 0] = HelperRandom.GetRandomNumber(-_dXZ.X / 2.25f, _dXZ.X / 2.25f);
                _noise[i + 1] = HelperRandom.GetRandomNumber(-_dXZ.Y / 2.25f, _dXZ.Y / 2.25f);
            }
        }
        #endregion
    }
}