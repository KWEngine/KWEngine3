using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal class Frustum
    {
        internal readonly float[] _clipMatrix = new float[16];
        internal readonly float[,] _frustum = new float[6, 4];

        internal const int A = 0;
        internal const int B = 1;
        internal const int C = 2;
        internal const int D = 3;

        public Frustum()
        {

        }
        internal void UpdateScreenSpaceStatus(TextObject t)
        {
            t.IsInsideScreenSpace = SphereVsFrustum(t.Position, t._stateCurrent._width / 2f);
            t.IsInsideScreenSpaceForRenderPass = SphereVsFrustum(t.Position, t._stateCurrent._width / 2f * 1.25f);
        }
        internal void UpdateScreenSpaceStatus(EngineObject e)
        {
            if (e is GameObject)
            {
                e.IsInsideScreenSpace = VolumeVsFrustum(e.Center, e._stateCurrent._dimensions.X / 2f, e._stateCurrent._dimensions.Y / 2f, e._stateCurrent._dimensions.Z / 2f);
                e.IsInsideScreenSpaceForRenderPass = SphereVsFrustum(e.Center, e._stateCurrent._dimensions.LengthFast * 1.25f);
            }
            else if(e is RenderObject)
            {
                e.IsInsideScreenSpace = SphereVsFrustum(e.Center, e._stateCurrent._dimensions.LengthFast / 2f);
                /*if (e.IsInsideScreenSpace)
                {
                    Console.WriteLine("+");
                }
                else
                {
                    Console.WriteLine("-");
                }*/
                e.IsInsideScreenSpaceForRenderPass = e.IsInsideScreenSpace;
            }
        }
        internal void UpdateScreenSpaceStatus(TerrainObject t)
        {
            t.IsInsideScreenSpace = VolumeVsFrustum(t._stateCurrent._center, t._stateCurrent._dimensions.X / 2, t._stateCurrent._dimensions.Y / 2, t._stateCurrent._dimensions.Z / 2);
            t.IsInsideScreenSpaceForRenderPass = VolumeVsFrustum(t._stateCurrent._center, t._stateCurrent._dimensions.X * 1.25f / 2, t._stateCurrent._dimensions.Y * 1.25f / 2, t._stateCurrent._dimensions.Z * 1.25f / 2);
        }

        internal void UpdateScreenSpaceStatus(FoliageObject f)
        {
            if(f._terrainObject != null)
            {
                f.IsInsideScreenSpace = VolumeVsFrustum(new Vector3(f._position.X, f._terrainObject._stateCurrent._center.Y, f._position.Z), f._patchSize.X * 0.5f, f._terrainObject._stateCurrent._dimensions.Y * 0.5f, f._patchSize.Y * 0.5f);
                f.IsInsideScreenSpaceForRenderPass = VolumeVsFrustum(new Vector3(f._position.X, f._terrainObject._stateCurrent._center.Y, f._position.Z), f._patchSize.X * 0.5f * 1.25f, f._terrainObject._stateCurrent._dimensions.Y * 0.5f * 1.25f, f._patchSize.Y * 0.5f * 1.25f);
            }
            else
            {
                f.IsInsideScreenSpace = VolumeVsFrustum(f._position + new Vector3(0, f._scale.Y * 0.5f, 0), f._patchSize.X, f._scale.Y, f._patchSize.Y);
                f.IsInsideScreenSpaceForRenderPass = VolumeVsFrustum(f._position + new Vector3(0, f._scale.Y * 0.5f, 0), f._patchSize.X * 1.25f, f._scale.Y * 1.25f, f._patchSize.Y * 1.25f);
            }
        }
        /*
        internal void UpdateScreenSpaceStatus(LightObject l)
        {
            l.GetVolume(out Vector3 center, out Vector3 dimensions);
            l.IsInsideScreenSpace = VolumeVsFrustum(center, dimensions.X, dimensions.Y, dimensions.Z);
            l.IsInsideScreenSpaceForRenderPass = VolumeVsFrustum(center, dimensions.X * 1.25f, dimensions.Y * 1.25f, dimensions.Z * 1.25f);
        }
        */
        internal enum ClippingPlane : int
        {
            Right = 0,
            Left = 1,
            Bottom = 2,
            Top = 3,
            Back = 4,
            Front = 5
        }

        internal void NormalizePlane(float[,] frustum, int side)
        {
            float magnitude = new Vector3(frustum[side, 0], frustum[side, 1], frustum[side, 2]).LengthFast;
            frustum[side, 0] /= magnitude;
            frustum[side, 1] /= magnitude;
            frustum[side, 2] /= magnitude;
            frustum[side, 3] /= magnitude;
        }

        internal bool PointVsFrustum(float x, float y, float z)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_frustum[i, 0] * x + _frustum[i, 1] * y + _frustum[i, 2] * z + _frustum[i, 3] <= 0.0f)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool PointVsFrustum(Vector3 location)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_frustum[i, 0] * location.X + _frustum[i, 1] * location.Y + _frustum[i, 2] * location.Z + _frustum[i, 3] <= 0.0f)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool SphereVsFrustum(float x, float y, float z, float radius)
        {
            for (int p = 0; p < 6; p++)
            {
                float d = _frustum[p, 0] * x + _frustum[p, 1] * y + _frustum[p, 2] * z + _frustum[p, 3];
                if (d <= -radius)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool SphereVsFrustum(Vector3 location, float radius)
        {
            for (int p = 0; p < 6; p++)
            {
                float d = _frustum[p, 0] * location.X + _frustum[p, 1] * location.Y + _frustum[p, 2] * location.Z + _frustum[p, 3];
                if (d <= -radius)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool VolumeVsFrustum(Vector3 location, float width, float height, float length)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
                    continue;
                return false;
            }
            return true;
        }

        public bool CubeVsFrustum(float x, float y, float z, float size)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
                    continue;
                if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
                    continue;
                return false;
            }
            return true;
        }


        internal void UpdateFrustum(Matrix4 projectionMatrix, Matrix4 viewMatrix)
        {
            _clipMatrix[0] = (viewMatrix.M11 * projectionMatrix.M11) + (viewMatrix.M12 * projectionMatrix.M21) + (viewMatrix.M13 * projectionMatrix.M31) + (viewMatrix.M14 * projectionMatrix.M41);
            _clipMatrix[1] = (viewMatrix.M11 * projectionMatrix.M12) + (viewMatrix.M12 * projectionMatrix.M22) + (viewMatrix.M13 * projectionMatrix.M32) + (viewMatrix.M14 * projectionMatrix.M42);
            _clipMatrix[2] = (viewMatrix.M11 * projectionMatrix.M13) + (viewMatrix.M12 * projectionMatrix.M23) + (viewMatrix.M13 * projectionMatrix.M33) + (viewMatrix.M14 * projectionMatrix.M43);
            _clipMatrix[3] = (viewMatrix.M11 * projectionMatrix.M14) + (viewMatrix.M12 * projectionMatrix.M24) + (viewMatrix.M13 * projectionMatrix.M34) + (viewMatrix.M14 * projectionMatrix.M44);

            _clipMatrix[4] = (viewMatrix.M21 * projectionMatrix.M11) + (viewMatrix.M22 * projectionMatrix.M21) + (viewMatrix.M23 * projectionMatrix.M31) + (viewMatrix.M24 * projectionMatrix.M41);
            _clipMatrix[5] = (viewMatrix.M21 * projectionMatrix.M12) + (viewMatrix.M22 * projectionMatrix.M22) + (viewMatrix.M23 * projectionMatrix.M32) + (viewMatrix.M24 * projectionMatrix.M42);
            _clipMatrix[6] = (viewMatrix.M21 * projectionMatrix.M13) + (viewMatrix.M22 * projectionMatrix.M23) + (viewMatrix.M23 * projectionMatrix.M33) + (viewMatrix.M24 * projectionMatrix.M43);
            _clipMatrix[7] = (viewMatrix.M21 * projectionMatrix.M14) + (viewMatrix.M22 * projectionMatrix.M24) + (viewMatrix.M23 * projectionMatrix.M34) + (viewMatrix.M24 * projectionMatrix.M44);

            _clipMatrix[8] = (viewMatrix.M31 * projectionMatrix.M11) + (viewMatrix.M32 * projectionMatrix.M21) + (viewMatrix.M33 * projectionMatrix.M31) + (viewMatrix.M34 * projectionMatrix.M41);
            _clipMatrix[9] = (viewMatrix.M31 * projectionMatrix.M12) + (viewMatrix.M32 * projectionMatrix.M22) + (viewMatrix.M33 * projectionMatrix.M32) + (viewMatrix.M34 * projectionMatrix.M42);
            _clipMatrix[10] = (viewMatrix.M31 * projectionMatrix.M13) + (viewMatrix.M32 * projectionMatrix.M23) + (viewMatrix.M33 * projectionMatrix.M33) + (viewMatrix.M34 * projectionMatrix.M43);
            _clipMatrix[11] = (viewMatrix.M31 * projectionMatrix.M14) + (viewMatrix.M32 * projectionMatrix.M24) + (viewMatrix.M33 * projectionMatrix.M34) + (viewMatrix.M34 * projectionMatrix.M44);

            _clipMatrix[12] = (viewMatrix.M41 * projectionMatrix.M11) + (viewMatrix.M42 * projectionMatrix.M21) + (viewMatrix.M43 * projectionMatrix.M31) + (viewMatrix.M44 * projectionMatrix.M41);
            _clipMatrix[13] = (viewMatrix.M41 * projectionMatrix.M12) + (viewMatrix.M42 * projectionMatrix.M22) + (viewMatrix.M43 * projectionMatrix.M32) + (viewMatrix.M44 * projectionMatrix.M42);
            _clipMatrix[14] = (viewMatrix.M41 * projectionMatrix.M13) + (viewMatrix.M42 * projectionMatrix.M23) + (viewMatrix.M43 * projectionMatrix.M33) + (viewMatrix.M44 * projectionMatrix.M43);
            _clipMatrix[15] = (viewMatrix.M41 * projectionMatrix.M14) + (viewMatrix.M42 * projectionMatrix.M24) + (viewMatrix.M43 * projectionMatrix.M34) + (viewMatrix.M44 * projectionMatrix.M44);

            _frustum[(int)ClippingPlane.Right, 0] = _clipMatrix[3] - _clipMatrix[0];
            _frustum[(int)ClippingPlane.Right, 1] = _clipMatrix[7] - _clipMatrix[4];
            _frustum[(int)ClippingPlane.Right, 2] = _clipMatrix[11] - _clipMatrix[8];
            _frustum[(int)ClippingPlane.Right, 3] = _clipMatrix[15] - _clipMatrix[12];
            NormalizePlane(_frustum, (int)ClippingPlane.Right);

            _frustum[(int)ClippingPlane.Left, 0] = _clipMatrix[3] + _clipMatrix[0];
            _frustum[(int)ClippingPlane.Left, 1] = _clipMatrix[7] + _clipMatrix[4];
            _frustum[(int)ClippingPlane.Left, 2] = _clipMatrix[11] + _clipMatrix[8];
            _frustum[(int)ClippingPlane.Left, 3] = _clipMatrix[15] + _clipMatrix[12];
            NormalizePlane(_frustum, (int)ClippingPlane.Left);

            _frustum[(int)ClippingPlane.Bottom, 0] = _clipMatrix[3] + _clipMatrix[1];
            _frustum[(int)ClippingPlane.Bottom, 1] = _clipMatrix[7] + _clipMatrix[5];
            _frustum[(int)ClippingPlane.Bottom, 2] = _clipMatrix[11] + _clipMatrix[9];
            _frustum[(int)ClippingPlane.Bottom, 3] = _clipMatrix[15] + _clipMatrix[13];
            NormalizePlane(_frustum, (int)ClippingPlane.Bottom);

            _frustum[(int)ClippingPlane.Top, 0] = _clipMatrix[3] - _clipMatrix[1];
            _frustum[(int)ClippingPlane.Top, 1] = _clipMatrix[7] - _clipMatrix[5];
            _frustum[(int)ClippingPlane.Top, 2] = _clipMatrix[11] - _clipMatrix[9];
            _frustum[(int)ClippingPlane.Top, 3] = _clipMatrix[15] - _clipMatrix[13];
            NormalizePlane(_frustum, (int)ClippingPlane.Top);

            _frustum[(int)ClippingPlane.Back, 0] = _clipMatrix[3] - _clipMatrix[2];
            _frustum[(int)ClippingPlane.Back, 1] = _clipMatrix[7] - _clipMatrix[6];
            _frustum[(int)ClippingPlane.Back, 2] = _clipMatrix[11] - _clipMatrix[10];
            _frustum[(int)ClippingPlane.Back, 3] = _clipMatrix[15] - _clipMatrix[14];
            NormalizePlane(_frustum, (int)ClippingPlane.Back);

            _frustum[(int)ClippingPlane.Front, 0] = _clipMatrix[3] + _clipMatrix[2];
            _frustum[(int)ClippingPlane.Front, 1] = _clipMatrix[7] + _clipMatrix[6];
            _frustum[(int)ClippingPlane.Front, 2] = _clipMatrix[11] + _clipMatrix[10];
            _frustum[(int)ClippingPlane.Front, 3] = _clipMatrix[15] + _clipMatrix[14];
            NormalizePlane(_frustum, (int)ClippingPlane.Front);
        }
    }
}
