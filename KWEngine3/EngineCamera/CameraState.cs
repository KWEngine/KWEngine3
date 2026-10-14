using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.EngineCamera
{
    internal struct CameraState
    {
        public Matrix4 ProjectionMatrix = Matrix4.Identity;
        public Matrix4 ViewMatrix = Matrix4.Identity; // render state of the game camera: includes the camera shake rotation
        public Matrix4 ViewMatrixVSG = Matrix4.Identity; // render state of the game camera only: reduced shake for the ViewSpaceGameObject
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrixVSG = Matrix4.Identity; // render state of the game camera only (see ViewMatrixVSG)
        public Matrix4 ViewProjectionMatrixInverse = Matrix4.Identity; // only maintained for the render state (once per frame in HelperSimulation.BlendCameraStates)
        public Vector3 LookAtVector = -Vector3.UnitZ; // never includes the camera shake
        public Vector3 LookAtVectorLocalUp = Vector3.UnitY;
        public Vector3 LookAtVectorLocalRight = Vector3.UnitX;

        internal Vector3 _position = KWEngine.DefaultCameraPosition; // never includes the camera shake (the shake rotates around this position)
        internal Vector3 _target = Vector3.Zero;
        internal CameraShake _shake = new();
        internal Vector2 _bobValue = Vector2.Zero;
        internal Quaternion _rotation = Quaternion.Identity;
        internal float _fov = 45;

        public CameraState()
        {
        }

        internal void UpdateViewMatrixAndLookAtVectorRenderPass()
        {
            UpdateViewMatrixAndLookAtVector();

            Vector3 shakeAngles = _shake.GetAngles(KWEngine.WorldTime);
            if (shakeAngles == Vector3.Zero)
            {
                ViewMatrixVSG = ViewMatrix;
            }
            else
            {
                // rotate around the eye (view space origin): the camera position itself does not change
                Matrix4 viewWithoutShake = ViewMatrix;
                ViewMatrix = viewWithoutShake * CameraShake.CreateRotation(shakeAngles);
                ViewMatrixVSG = viewWithoutShake * CameraShake.CreateRotation(shakeAngles * KWEngine.ViewSpaceGameObjectShakeFactor);
            }
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

        internal void UpdateViewProjectionMatrix(float zNear, float zFar, bool includeViewSpaceGameObjectMatrix = false)
        {
            SetProjectionMatrix(zNear, zFar);
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            if (includeViewSpaceGameObjectMatrix)
            {
                ViewProjectionMatrixVSG = ViewMatrixVSG * ProjectionMatrix;
            }
        }

        internal void UpdateViewProjectionMatrixInverse()
        {
            ViewProjectionMatrixInverse = Matrix4.Invert(ViewProjectionMatrix);
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
