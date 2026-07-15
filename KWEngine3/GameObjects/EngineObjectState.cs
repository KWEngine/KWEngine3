using OpenTK.Mathematics;

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
        internal Vector3 _colorTint = Vector3.One;
        internal Vector3 _lookAtVector = Vector3.UnitZ;
        internal Vector3 _lookAtVectorUp = Vector3.UnitY;
        internal Vector3 _lookAtVectorRight = Vector3.UnitX;
        internal Vector4 _uvTransform = new(1, 1, 0, 0); //zw = offset.xy
        internal Vector2 _uvClip = new Vector2(0, 0);
        internal float _expansionFactorXZ = 0f;

        // Animation: bis zu 4 gleichzeitige Layer als einzelne Struct-Felder,
        // damit Struct-Kopien (z.B. _statePrevious = _stateCurrent) vollständige Wert-Kopien erzeugen.
        internal AnimationLayer _animationLayer0;
        internal AnimationLayer _animationLayer1;
        internal AnimationLayer _animationLayer2;
        internal AnimationLayer _animationLayer3;
        internal const int MAX_ANIMATION_LAYERS = 4;

        // Zugriff per Index für Schleifen in HelperSimulation
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
        internal int AnimationIDDefault
        {
            get => _animationLayer0.AnimationID;
            set => _animationLayer0.AnimationID = value;
        }
        internal float AnimationPercentageDefault
        {
            get => _animationLayer0.Percentage;
            set => _animationLayer0.Percentage = value;
        }

        internal Vector3 _position;
        internal Quaternion _rotation;
        internal Vector3 _scale;

        internal Dictionary<int, Vector3> _rotationPre;


        public EngineObjectState():this(null)
        {
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _position = Vector3.Zero;
        }

        public EngineObjectState(EngineObject gameObject)
        {
            this._engineObject = gameObject ?? throw new ArgumentNullException("Invalid EngineObject instance.");

            _animationLayer0 = AnimationLayer.CreateDefault();
            _animationLayer1 = AnimationLayer.CreateDefault();
            _animationLayer2 = AnimationLayer.CreateDefault();
            _animationLayer3 = AnimationLayer.CreateDefault();

            _expansionFactorXZ = 0f;
            _rotationPre = new();
            _rotation = Quaternion.Identity;
            _scale = Vector3.One;
            _position = Vector3.Zero;
        }
    }
}
