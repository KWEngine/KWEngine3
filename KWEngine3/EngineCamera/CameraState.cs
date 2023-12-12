using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.EngineCamera
{
    internal struct CameraState
    {
        public Matrix4 ProjectionMatrix = Matrix4.Identity;
        public Matrix4 ViewMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Vector3 LookAtVector = -Vector3.UnitZ;
        public Vector3 LookAtVectorLocalUp = Vector3.UnitY;
        public Vector3 LookAtVectorLocalRight = Vector3.UnitX;

        internal Vector3 _position = KWEngine.DefaultCameraPosition;
        internal Vector3 _target = Vector3.Zero;
        internal Quaternion _rotation = Quaternion.Identity;
        internal float _fov = 45;

        public CameraState()
        {
        }

        internal void UpdateViewMatrixAndLookAtVector()
        {
            ViewMatrix = Matrix4.LookAt(
                _position,
                _target,
                KWEngine.WorldUp);
            while (float.IsNaN(ViewMatrix.M11))
            {
                _target.Z -= 0.000001f;
                ViewMatrix = Matrix4.LookAt(
                _position,
                _target,
                KWEngine.WorldUp);
            }
            LookAtVector = Vector3.NormalizeFast(_target - _position);
            LookAtVectorLocalRight = Vector3.NormalizeFast(Vector3.Cross(LookAtVector, KWEngine.WorldUp));
            LookAtVectorLocalUp = Vector3.NormalizeFast(Vector3.Cross(LookAtVectorLocalRight, LookAtVector));
        }

        internal void UpdateViewProjectionMatrix(float zNear, float zFar)
        {
            SetProjectionMatrix(zNear, zFar);
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
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
