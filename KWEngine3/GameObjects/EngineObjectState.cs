﻿using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal struct EngineObjectState
    {
        internal Vector3 _dimensions = Vector3.Zero;
        internal Vector3 _center = Vector3.Zero;
        internal float _opacity = 1f;
        internal EngineObject _engineObject = null;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Matrix4 _modelMatrixInverse = Matrix4.Identity;
        internal Vector4 _colorEmissive = new(0, 0, 0, 0);
        internal Vector3 _colorTint = Vector3.One;
        internal Vector3 _lookAtVector = Vector3.UnitZ;
        internal Vector3 _lookAtVectorUp = Vector3.UnitY;
        internal Vector3 _lookAtVectorRight = Vector3.UnitX;
        internal Vector4 _uvTransform = new(1, 1, 0, 0); //zw = offset.xy
        internal float _animationPercentage = 0f;
        internal int _animationID = -1;
        
        internal Vector3 _scaleHitbox;
        internal Vector3 _position;
        internal Quaternion _rotation;
        internal Vector3 _scale;
        

        public EngineObjectState():this(null)
        {
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _scaleHitbox = Vector3.One;
            _position = Vector3.Zero;
        }

        public EngineObjectState(EngineObject gameObject)
        {
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _scaleHitbox = Vector3.One;
            _position = Vector3.Zero;

            this._engineObject = gameObject ?? throw new ArgumentNullException("Invalid EngineObject instance.");
        }
    }
}