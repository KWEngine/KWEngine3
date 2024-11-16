using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.EngineCamera
{
    internal struct CameraState
    {
        public Matrix4 ProjectionMatrix = Matrix4.Identity;
        public Matrix4 ViewMatrix = Matrix4.Identity;
        public Matrix4 ViewMatrixNoShake = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrixNoShake = Matrix4.Identity;
        public Vector3 LookAtVector = -Vector3.UnitZ;
        public Vector3 LookAtVectorLocalUp = Vector3.UnitY;
        public Vector3 LookAtVectorLocalRight = Vector3.UnitX;

        internal Vector3 _position = KWEngine.DefaultCameraPosition;
        internal Vector3 _target = Vector3.Zero;
        internal Vector3 _shakeOffset = Vector3.Zero;
        internal Vector2 _bobValue = Vector2.Zero;
        internal float _shakeDuration = 0f;
        internal ShakeMode _shakeMode = ShakeMode.Additive;
        internal float _shakeTimestamp = 0;
        internal Quaternion _rotation = Quaternion.Identity;
        internal float _fov = 45;

        public CameraState()
        {
        }

        internal void UpdateViewMatrixAndLookAtVectorRenderPass()
        {
            Vector3 noiseOffset = Vector3.Zero;

            //take shake offset into account?
            float delta = KWEngine.WorldTime - _shakeTimestamp;
            float deltaScaled = HelperGeneral.ScaleToRange(delta, 0f, _shakeDuration, 0f, 1f);
            if (!float.IsNaN(deltaScaled) && deltaScaled >= 0f && deltaScaled <= 1f)
            {
                float deltaScaledP = ((deltaScaled * 2f - 1f) * 100f) % 1f;

                noiseOffset = new Vector3(
                    HelperPerlinNoise.GradientNoise(deltaScaledP, deltaScaledP, 0) * _shakeOffset.X * (1f - deltaScaled),
                    HelperPerlinNoise.GradientNoise(deltaScaledP, deltaScaledP, 1) * _shakeOffset.Y * (1f - deltaScaled),
                    HelperPerlinNoise.GradientNoise(deltaScaledP, deltaScaledP, 2) * _shakeOffset.Z * (1f - deltaScaled)
                );
            }

            LookAtVector = Vector3.NormalizeFast(_target - _position);
            LookAtVectorLocalRight = Vector3.NormalizeFast(Vector3.Cross(LookAtVector, KWEngine.WorldUp));
            LookAtVectorLocalUp = Vector3.NormalizeFast(Vector3.Cross(LookAtVectorLocalRight, LookAtVector));

            

            ViewMatrix = Matrix4.LookAt(
                _position + noiseOffset,
                _target + noiseOffset,
                KWEngine.WorldUp);
            ViewMatrixNoShake = Matrix4.LookAt(
                _position + noiseOffset * KWEngine.ViewSpaceGameObjectShakeFactor,
                _target + noiseOffset * KWEngine.ViewSpaceGameObjectShakeFactor,
                KWEngine.WorldUp);
        }

        internal void UpdateViewMatrixAndLookAtVector()
        {
            ViewMatrix = Matrix4.LookAt(
                _position,
                _target,
                KWEngine.WorldUp);
            LookAtVector = Vector3.NormalizeFast(_target - _position);
            LookAtVectorLocalRight = Vector3.NormalizeFast(Vector3.Cross(LookAtVector, KWEngine.WorldUp));
            LookAtVectorLocalUp = Vector3.NormalizeFast(Vector3.Cross(LookAtVectorLocalRight, LookAtVector));
        }

        internal void UpdateViewProjectionMatrix(float zNear, float zFar, bool isGameCam = false)
        {
            SetProjectionMatrix(zNear, zFar);
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            ViewProjectionMatrixNoShake = ViewMatrixNoShake * ProjectionMatrix;
        }

        internal void SetProjectionMatrix(float zNear, float zFar)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(_fov),
                KWEngine.Window.ClientRectangle.Size.X / (float)KWEngine.Window.ClientRectangle.Size.Y,
                zNear,
                zFar);
        }
    }
}
