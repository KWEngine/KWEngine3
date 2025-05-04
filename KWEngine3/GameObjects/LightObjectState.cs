using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    internal struct LightObjectState
    {
        internal const float NEARDEFAULT = 0.1f;
        internal const float FARDEFAULT = 100.0f;
        internal const float FOVDEFAULT = 90;

        public LightObject _parent;
        public Vector3 _position;
        public Vector3 _target;
        public Vector3 _lookAtVector;
        public Vector4 _nearFarFOVType;
        public Vector4 _color;
        public Matrix4[] _viewProjectionMatrix;
        

        public LightObjectState(LightObject parent, LightType type)
        {
            _parent = parent;
            _position = Vector3.Zero;
            _target = Vector3.Zero;
            _color = new Vector4(1, 1, 1, 1);
            _nearFarFOVType = new Vector4(NEARDEFAULT, FARDEFAULT, FOVDEFAULT, type == LightType.Point ? 0 : type == LightType.Sun ? -1 : 1);
            _viewProjectionMatrix = new Matrix4[6];
            _lookAtVector = Vector3.Zero;
        }
    }
}
