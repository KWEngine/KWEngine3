using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    public abstract class ViewSpaceGameObject
    {
        internal GameObject _gameObject;
        internal Vector3 _offset = Vector3.Zero;
        internal Vector3 _right = Vector3.UnitX;
        internal Vector3 _up = Vector3.UnitY;
        internal Quaternion _rotation = Quaternion.Identity;
        internal readonly static Quaternion ROT180 = Quaternion.FromEulerAngles(0, (float)Math.PI, 0);

        internal bool IsValid { get { return _gameObject != null; } }

        public ViewSpaceGameObject()
            :this("KWCube")
        {
        }

        public ViewSpaceGameObject(string modelName)
        {
            if (modelName != null && KWEngine.Models.ContainsKey(modelName.Trim()))
            {
                _gameObject = new GameObjectFPV(modelName.Trim());
            }
            else
            { 
                _gameObject = null;
            }
        }

        public abstract void Act();

        public void SetModel(string modelName)
        {
            if(IsValid)
            {
                if(!_gameObject.SetModel(modelName))
                {
                    KWEngine.LogWriteLine("[ViewSpaceGameObject] Invalid model");
                }
                else
                {
                    UpdatePosition();
                }
            }
        }

        public void UpdatePosition()
        {
            if (IsValid)
            {
                Vector3 pos = KWEngine.CurrentWorld.CameraPosition;

                _right = Vector3.NormalizeFast(Vector3.Cross(KWEngine.CurrentWorld.CameraLookAtVector, KWEngine.WorldUp));
                _up = Vector3.NormalizeFast(Vector3.Cross(KWEngine.CurrentWorld.CameraLookAtVector, _right));
                Vector3 pos2 = pos + _right * _offset.X + _up * _offset.Y + KWEngine.CurrentWorld.CameraLookAtVector * _offset.Z;

                _gameObject.SetPosition(pos2);
                Quaternion invertedViewRotation = Quaternion.Invert(KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewMatrix.ExtractRotation(false));
                _gameObject.SetRotation(invertedViewRotation * ROT180 * _rotation);
            }
        }

        public void SetRotation(float x, float y, float z)
        {
            _rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z));
        }

        public List<Intersection> GetIntersections()
        {
            List<Intersection> intersections = new List<Intersection>();
            if(IsValid)
            {
                intersections.AddRange(_gameObject.GetIntersections());
            }
            return intersections;
        }

        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }

        public bool IsCollisionObject { 
            get 
            { 
                return IsValid ? _gameObject.IsCollisionObject : false; 
            }
            set
            {
                if(IsValid)
                {
                    _gameObject.IsCollisionObject = value;
                }
            }
        }

        public bool IsShadowCaster
        {
            get
            {
                return IsValid ? _gameObject.IsShadowCaster : false;
            }
            set
            {
                if (IsValid)
                {
                    _gameObject.IsShadowCaster = value;
                }
            }
        }

        public void SetOffset(float horizontal, float vertical, float nearFar)
        {
            SetOffset(new Vector3(horizontal, -vertical, nearFar));
        }

        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
        }

        public void SetScale(float s)
        {
            if (IsValid)
            {
                s = MathHelper.Max(0.000001f, s);
                _gameObject.SetScale(s);
            }
        }

        public void SetAnimationID(int id)
        {
            if (IsValid && _gameObject._gModel.ModelOriginal.Animations != null)
                _gameObject._stateCurrent._animationID = MathHelper.Clamp(id, -1, _gameObject._gModel.ModelOriginal.Animations.Count - 1);
            else
            {
                if(IsValid)
                    _gameObject._stateCurrent._animationID = -1;
            }
        }

        public void SetAnimationPercentage(float p)
        {
            if (IsValid)
                _gameObject.SetAnimationPercentage(p);// _stateCurrent._animationPercentage = MathHelper.Clamp(p, 0f, 1f);
        }

        public void SetAnimationPercentageAdvance(float p)
        {
            if (IsValid)
                _gameObject.SetAnimationPercentageAdvance(p);
                //_gameObject._stateCurrent._animationPercentage = (_gameObject._stateCurrent._animationPercentage + p * KWEngine.DeltaTimeCurrentNibbleSize) % 1f;
        }
    }
}
