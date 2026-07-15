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
        internal Vector3 _colorTint = Vector3.One;
        internal Vector3 _lookAtVector = Vector3.UnitZ;
        // Animation: bis zu 4 gleichzeitige Layer als einzelne Struct-Felder (Wert-Semantik, sicher bei Struct-Kopien)
        internal AnimationLayer _animationLayer0;
        internal AnimationLayer _animationLayer1;
        internal AnimationLayer _animationLayer2;
        internal AnimationLayer _animationLayer3;

        internal AnimationLayer GetAnimationLayer(int index)
        {
            switch (index)
            {
                case 0: return _animationLayer0;
                case 1: return _animationLayer1;
                case 2: return _animationLayer2;
                default: return _animationLayer3;
            }
        }
        internal void SetAnimationLayer(int index, AnimationLayer layer)
        {
            switch (index)
            {
                case 0: _animationLayer0 = layer; break;
                case 1: _animationLayer1 = layer; break;
                case 2: _animationLayer2 = layer; break;
                default: _animationLayer3 = layer; break;
            }
        }

        // Kompatibilitäts-Hilfseigenschaften, die auf Layer 0 zeigen
        internal int _animationID
        {
            get => _animationLayer0.AnimationID;
            set => _animationLayer0.AnimationID = value;
        }
        internal float _animationPercentage
        {
            get => _animationLayer0.Percentage;
            set => _animationLayer0.Percentage = value;
        }
        internal Vector4 _uvTransform = new(1, 1, 0, 0);
        internal Vector2 _uvClip = new Vector2(0, 0);

        internal Vector3 _scaleHitbox;
        internal Vector3 _position;
        internal Quaternion _rotation;
        internal Vector3 _scale;
        internal float _expansionFactorXZ;
        internal Dictionary<int, Quaternion> _rotationPre;

        internal Dictionary<string, Matrix4[]> _boneTranslationMatrices;

        public EngineObjectRenderState():this(null)
        {
        }

        public EngineObjectRenderState(EngineObject engineObject)
        {
            _rotationPre = new();
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _scaleHitbox = Vector3.One;
            _position = Vector3.Zero;
            this._engineObject = engineObject ?? throw new ArgumentNullException("invalid game object for creating render state");
            _animationLayer0 = AnimationLayer.CreateDefault();
            _animationLayer1 = AnimationLayer.CreateDefault();
            _animationLayer2 = AnimationLayer.CreateDefault();
            _animationLayer3 = AnimationLayer.CreateDefault();
            _expansionFactorXZ = 0f;
            _boneTranslationMatrices = new Dictionary<string, Matrix4[]>();
            _modelMatrices = new Matrix4[engineObject._model.ModelOriginal.Meshes.Values.Count];
            _normalMatrices = new Matrix4[engineObject._model.ModelOriginal.Meshes.Values.Count];
        }
    }
}
