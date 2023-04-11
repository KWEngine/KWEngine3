using KWEngine3.Editor;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.EngineCamera
{
    internal struct Camera
    {
        internal ProjectionType _projectionType = ProjectionType.Perspective;
        internal float _zFar = 500f;
        internal float _zNear = 0.1f;
        internal float degX = 0f;
        internal float degY = 0f;

        internal CameraState _statePrevious = new CameraState();
        internal CameraState _stateCurrent = new CameraState();
        internal CameraState _stateRender = new CameraState();

        internal Frustum _frustum = new Frustum();

        internal void BackupCameraState()
        {
            _statePrevious = _stateCurrent;
        }

        public Camera()
        {
            _stateCurrent.SetProjectionMatrix(_zNear, _zFar);
            _stateCurrent.UpdateViewMatrixAndLookAtVector();
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
        }

        internal void SetFOVForPerspectiveProjection(float fov)
        {
            if(_projectionType == ProjectionType.Perspective)
            {
                _stateCurrent._fov = Math.Max(Math.Min(fov / 2.0f, 89.9999f), 10f);
            }
            else
            {
                EngineLog.AddMessage("[Camera] FOV ignored because of orthographic projection.");
                _stateCurrent._fov = KWEngine.Window.ClientRectangle.Size.Y;
            }
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
            _frustum.UpdateFrustum(_stateCurrent.ProjectionMatrix, _stateCurrent.ViewMatrix);
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }
        public void SetPosition(Vector3 position)
        {
            _stateCurrent._position = position;
            _stateCurrent.UpdateViewMatrixAndLookAtVector();
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
            _frustum.UpdateFrustum(_stateCurrent.ProjectionMatrix, _stateCurrent.ViewMatrix);
        }

        internal void UpdatePitchYaw(bool invert = false)
        {
            Vector3 d = _stateCurrent.LookAtVector * (invert ? - 1 : 1);            
            degX = (float)MathHelper.RadiansToDegrees(Math.Atan2(d.X, d.Z));
            degY = (float)MathHelper.RadiansToDegrees(Math.Asin(d.Y));
        }

        public void SetTarget(float x, float y, float z)
        {
            SetTarget(new Vector3(x, y, z));
        }
        public void SetTarget(Vector3 target)
        {
            _stateCurrent._target = target + new Vector3(0, 0, 0.000001f);
            _stateCurrent.UpdateViewMatrixAndLookAtVector();
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
            _frustum.UpdateFrustum(_stateCurrent.ProjectionMatrix, _stateCurrent.ViewMatrix);
        }

        public void YawAndPitch(float yaw, float pitch)
        {
            YawAndPitch(new Vector2(yaw, pitch));
        }

        public void YawAndPitch(Vector2 yawPitch, float distance = 1f) // yaw = deltaX, pitch = deltaY
        {
            degX = (degX + yawPitch.X) % 360f;
            degY += yawPitch.Y;
            if (degY > 89.9f)
                degY = 89.9f;
            else if (degY < -89.9f)
                degY = -89.9f;
            Vector3 newCamTarget = HelperRotation.CalculateRotationForArcBallCamera(
                _stateCurrent._position,
                distance,
                degX,
                degY,
                false,
                false
                );
            SetTarget(newCamTarget);

        }

        public void AdjustToGameObject(GameObject g, float offsetY = 0f)
        {
            Vector3 centerAdjusted = g.Center + new Vector3(0f, offsetY, 0f);
            SetPositionAndTarget(centerAdjusted, centerAdjusted + g.LookAtVector);
            UpdatePitchYaw();
        }

        internal void SetPositionAndTarget(Vector3 position, Vector3 target)
        {
            _stateCurrent._position = position;
            _stateCurrent._target = target;
            _stateCurrent.UpdateViewMatrixAndLookAtVector();
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
            _frustum.UpdateFrustum(_stateCurrent.ProjectionMatrix, _stateCurrent.ViewMatrix);
        }

        public void SetNearFarBound(float zNear, float zFar)
        {
            _zNear = MathHelper.Clamp(zNear, 0.01f, 1000f);
            _zFar = MathHelper.Clamp(zFar, _zNear, 100000f);
            _stateCurrent.UpdateViewProjectionMatrix(_zNear, _zFar);
            _frustum.UpdateFrustum(_stateCurrent.ProjectionMatrix, _stateCurrent.ViewMatrix);
        }

        public void Move(float units)
        {
            SetPositionAndTarget(_stateCurrent._position + _stateCurrent.LookAtVector * units, _stateCurrent._target + _stateCurrent.LookAtVector * units);
        }

        public void Strafe(float units)
        {
            Vector3 lavStrafe = Vector3.NormalizeFast(HelperVector.RotateVector(new Vector3(_stateCurrent.LookAtVector.X, 0, _stateCurrent.LookAtVector.Z), -90, Plane.XZ));
            SetPositionAndTarget(_stateCurrent._position + lavStrafe * units, _stateCurrent._target + lavStrafe * units );
        }

        public void MoveUpDown(float units)
        {
            Vector3 lavStrafe = Vector3.NormalizeFast(Vector3.Cross(_stateCurrent.LookAtVector, KWEngine.WorldUp));
            Vector3 camUp = Vector3.NormalizeFast(Vector3.Cross(lavStrafe, _stateCurrent.LookAtVector));

            SetPositionAndTarget(
                _stateCurrent._position + camUp * units,
                _stateCurrent._target + camUp * units
                );
        }

        internal void ArcBallAroundSelf(Vector2 deltaXY)
        {
            degX = (degX + deltaXY.X * 0.25f) % 360f;
            degY += deltaXY.Y * 0.25f;
            if (degY > 89.9f)
                degY = 89.9f;
            else if (degY < -89.9f)
                degY = -89.9f;

            Vector3 newCamPos = HelperRotation.CalculateRotationForArcBallCamera(
                _stateCurrent._position,
                1f,
                180 + degX,
                degY,
                true,
                true
                );
            SetTarget(newCamPos);

            Console.WriteLine(degX + " | " + degY);
            Console.WriteLine(_stateCurrent._position);
            Console.WriteLine(newCamPos);
            Console.WriteLine("-----------");
        }

        internal void ArcBall(Vector2 deltaXY)
        {
            degX = (degX + deltaXY.X * 0.25f) % 360f;
            degY += deltaXY.Y * 0.25f;
            if (degY > 89.9f)
                degY = 89.9f;
            else if (degY < -89.9f)
                degY = -89.9f;

            Vector3 newCamPos = HelperRotation.CalculateRotationForArcBallCamera(
                _stateCurrent._target,
                (_stateCurrent._target - _stateCurrent._position).LengthFast, 
                degX, 
                degY, 
                true, 
                true
                );
            SetPosition(newCamPos);
        }

        internal void ResetArcBall()
        {
            degX = 0;
            degY = 45;
            SetPositionAndTarget(new Vector3(0, 25, 25), Vector3.Zero);
        }

        internal Vector3 Get3DMouseCoords()
        {
            float x = KWEngine.Window.MousePosition.X;
            float y = KWEngine.Window.MousePosition.Y;

            x = (2.0f * x) / KWEngine.Window.Size.X - 1.0f;
            y = 1.0f - (2.0f * y) / KWEngine.Window.Size.Y;

            Vector4 ray_clip = new Vector4(x, y, -1, 1);
            Matrix4 viewInv = Matrix4.Invert(_stateCurrent.ViewMatrix);

            Vector4.TransformRow(ray_clip, Matrix4.Invert(_stateCurrent.ProjectionMatrix), out Vector4 ray_eye);
            ray_eye.Z = -1;
            ray_eye.W = 0;
            Vector4.TransformRow(ray_eye, viewInv, out Vector4 ray_world);
            Vector3 ray_world3 = Vector3.Normalize(ray_world.Xyz);
            return ray_world3;
        }
    }
}
