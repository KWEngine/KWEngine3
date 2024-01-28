using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.GameObjects
{
    internal struct EngineObjectRenderState
    {
        internal Vector3 _dimensions = Vector3.Zero;
        internal Vector3 _center = Vector3.Zero;
        internal float _opacity = 1f;
        internal EngineObject _engineObject = null;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Matrix4[] _modelMatrices;
        internal Matrix4 _normalMatrix = Matrix4.Identity;
        internal Matrix4[] _normalMatrices;
        internal Vector4 _colorEmissive = Vector4.Zero;
        internal Vector3 _colorTint = Vector3.One;
        internal Vector3 _lookAtVector = Vector3.UnitZ;
        internal float _animationPercentage = 0f;
        internal int _animationID = -1;
        internal Vector4 _uvTransform = new(1, 1, 0, 0);
        
        internal Vector3 _scaleHitbox;
        internal Vector3 _position;
        internal Quaternion _rotation;
        internal Vector3 _scale;

        internal Dictionary<string, Matrix4[]> _boneTranslationMatrices;

        public EngineObjectRenderState():this(null)
        {
        }

        public EngineObjectRenderState(EngineObject engineObject)
        {
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _scaleHitbox = Vector3.One;
            _position = Vector3.Zero;
            this._engineObject = engineObject ?? throw new ArgumentNullException("invalid game object for creating render state");
            _boneTranslationMatrices = new Dictionary<string, Matrix4[]>();
            _modelMatrices = new Matrix4[engineObject._model.ModelOriginal.Meshes.Values.Count];
            _normalMatrices = new Matrix4[engineObject._model.ModelOriginal.Meshes.Values.Count];
        }
    }
}
