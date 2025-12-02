using Assimp;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace KWEngine3.Helper
{
    internal static class HelperMatrix
    {
        internal enum MainCamDirection
        {
            MinusX,
            PlusX,
            MinusY,
            PlusY,
            MinusZ,
            PlusZ
        }

        internal static Vector3[] aabbpoints = new Vector3[8];

        public static MainCamDirection FindMainCamDirection()
        {
            Vector3 lav = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender.LookAtVector : KWEngine.CurrentWorld._cameraGame._stateRender.LookAtVector;

            if(MathF.Abs(lav.X) >= MathF.Abs(lav.Y) && MathF.Abs(lav.X) >= MathF.Abs(lav.Z))
            {
                return lav.X > 0 ? MainCamDirection.PlusX : MainCamDirection.MinusX;
            }
            else if(MathF.Abs(lav.Y) >= MathF.Abs(lav.X) && MathF.Abs(lav.Y) >= MathF.Abs(lav.Z))
            {
                return lav.Y > 0 ? MainCamDirection.PlusY : MainCamDirection.MinusY;
            }
            else
            {
                return lav.Z > 0 ? MainCamDirection.PlusZ : MainCamDirection.MinusZ;
            }
        }

        public static float GetProjZForCameraMainPlane(EngineObject e, MainCamDirection camDir, ref Vector3 camPos)
        {
            switch(camDir)
            {
                case MainCamDirection.MinusX:
                    return MathF.Abs(camPos.X - e.AABBRight);
                case MainCamDirection.PlusX:
                    return MathF.Abs(camPos.X - e.AABBLeft);
                case MainCamDirection.MinusY:
                    return MathF.Abs(camPos.Y - e.AABBHigh);
                case MainCamDirection.PlusY:
                    return MathF.Abs(camPos.Y - e.AABBLow);
                case MainCamDirection.MinusZ:
                    return MathF.Abs(camPos.Z - e.AABBFront);
                case MainCamDirection.PlusZ:
                    return MathF.Abs(camPos.Z - e.AABBBack);
                default:
                    return MathF.Abs(camPos.Z - e.AABBFront);
            }
        }

        public static void SortByZ(List<RenderObject> transparentObjects)
        {
            if (KWEngine.ZOderMode == ZOrderMode.CameraMainPlane)
            {
                Vector3 camPos = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender._position : KWEngine.CurrentWorld._cameraGame._stateRender._position;
                MainCamDirection camDirMain = FindMainCamDirection();

                foreach (RenderObject g in transparentObjects)
                {
                    g._projZ = GetProjZForCameraMainPlane(g, camDirMain, ref camPos);
                }
            }
            else
            {
                Matrix4 m = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix;
                foreach (RenderObject g in transparentObjects)
                {
                    g._projZ = HelperMatrix.FindCenterZOfAABB(g, ref m);
                }
            }
            transparentObjects.Sort();
        }

        public static void SortByZ(List<GameObject> transparentObjects)
        {
            if (KWEngine.ZOderMode == ZOrderMode.CameraMainPlane)
            {
                Vector3 camPos = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender._position : KWEngine.CurrentWorld._cameraGame._stateRender._position;
                MainCamDirection camDirMain = FindMainCamDirection();

                foreach (GameObject g in transparentObjects)
                {
                    g._projZ = GetProjZForCameraMainPlane(g, camDirMain, ref camPos);
                }
            }
            else
            {
                Matrix4 m = KWEngine.EditModeActive ? KWEngine.CurrentWorld._cameraEditor._stateRender.ViewProjectionMatrix : KWEngine.CurrentWorld._cameraGame._stateRender.ViewProjectionMatrix;
                foreach (GameObject g in transparentObjects)
                {
                    if (g._fullDiameter > KWEngine.ZOrderModeThreshold)
                    {
                        g._projZ = HelperMatrix.FindNearestZOfAABB(g, ref m);
                    }
                    else
                    {
                        g._projZ = HelperMatrix.FindCenterZOfAABB(g, ref m);
                    }
                }
            }
            transparentObjects.Sort();
        }

        public static float FindCenterZOfAABB(EngineObject e, ref Matrix4 matrix)
        {
            Vector4 projected = new Vector4(e.Center, 1.0f) * matrix;
            float z = (projected.Xyz / projected.W).Z;
            return z;
        }

        public static float FindNearestZOfAABB(EngineObject e, ref Matrix4 matrix)
        {
            aabbpoints[0] = new Vector3(e.AABBLeft, e.AABBLow, e.AABBBack);
            aabbpoints[1] = new Vector3(e.AABBRight, e.AABBLow, e.AABBBack);
            aabbpoints[2] = new Vector3(e.AABBLeft, e.AABBHigh, e.AABBBack);
            aabbpoints[3] = new Vector3(e.AABBRight, e.AABBHigh, e.AABBBack);

            aabbpoints[4] = new Vector3(e.AABBLeft, e.AABBLow, e.AABBFront);
            aabbpoints[5] = new Vector3(e.AABBRight, e.AABBLow, e.AABBFront);
            aabbpoints[6] = new Vector3(e.AABBLeft, e.AABBHigh, e.AABBFront);
            aabbpoints[7] = new Vector3(e.AABBRight, e.AABBHigh, e.AABBFront);

            float closest = float.MaxValue;
            foreach(Vector3 v in aabbpoints)
            {
                Vector4 projected = new Vector4(v, 1.0f) * matrix;
                float z = (projected.Xyz / projected.W).Z;
                if (z < closest)
                    closest = z;
            }
            return closest;
        }

        public static float FindFarestZOfAABB(EngineObject e, ref Matrix4 matrix)
        {
            aabbpoints[0] = new Vector3(e.AABBLeft, e.AABBLow, e.AABBBack);
            aabbpoints[1] = new Vector3(e.AABBRight, e.AABBLow, e.AABBBack);
            aabbpoints[2] = new Vector3(e.AABBLeft, e.AABBHigh, e.AABBBack);
            aabbpoints[3] = new Vector3(e.AABBRight, e.AABBHigh, e.AABBBack);

            aabbpoints[4] = new Vector3(e.AABBLeft, e.AABBLow, e.AABBFront);
            aabbpoints[5] = new Vector3(e.AABBRight, e.AABBLow, e.AABBFront);
            aabbpoints[6] = new Vector3(e.AABBLeft, e.AABBHigh, e.AABBFront);
            aabbpoints[7] = new Vector3(e.AABBRight, e.AABBHigh, e.AABBFront);

            float farest = float.MinValue;
            foreach (Vector3 v in aabbpoints)
            {
                Vector4 projected = new Vector4(v, 1.0f) * matrix;
                float z = (projected.Xyz / projected.W).Z;
                if (z > farest)
                    farest = z;
            }
            return farest;
        }

        public static Matrix4 CreateModelMatrix(ref Vector3 s, ref Vector3 t)
        {
            Matrix4 m = Matrix4.Identity;

            m.Row0 *= s.X;
            m.Row1 *= s.Y;
            m.Row2 *= s.Z;

            m.Row3.X = t.X;
            m.Row3.Y = t.Y;
            m.Row3.Z = t.Z;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrix(ref Vector3 s, ref Quaternion r, ref Vector3 t)
        {
            Matrix4 m = Matrix4.CreateFromQuaternion(r);

            m.Row0 *= s.X;
            m.Row1 *= s.Y;
            m.Row2 *= s.Z;

            m.Row3.X = t.X;
            m.Row3.Y = t.Y;
            m.Row3.Z = t.Z;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrix(float sX, float sY, float sZ, ref Quaternion r, float tX, float tY, float tZ)
        {
            Matrix4 m = Matrix4.CreateFromQuaternion(r);

            m.Row0 *= sX;
            m.Row1 *= sY;
            m.Row2 *= sZ;

            m.Row3.X = tX;
            m.Row3.Y = tY;
            m.Row3.Z = tZ;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrixForHUD(ref Vector3 s, ref Vector3 t)
        {
            Matrix4 m = Matrix4.Identity;

            m.Row0 *= s.X;
            m.Row1 *= s.Y;
            m.Row2 *= s.Z;

            m.Row3.X = t.X;
            m.Row3.Y = t.Y;
            m.Row3.Z = t.Z;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrixForHUD(float sX, float sY, float sZ, float tX, float tY, float tZ)
        {
            Matrix4 m = Matrix4.Identity;

            m.Row0 *= sX;
            m.Row1 *= sY;
            m.Row2 *= sZ;

            m.Row3.X = tX;
            m.Row3.Y = tY;
            m.Row3.Z = tZ;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrix(ref Vector3 s)
        {
            return Matrix4.CreateTranslation(s);
        }

        public static Matrix4 CreateModelMatrix(EngineObjectState state)
        {
            Matrix4 m = Matrix4.CreateFromQuaternion(state._rotation);

            m.Row0 *= state._scale.X;
            m.Row1 *= state._scale.Y;
            m.Row2 *= state._scale.Z;

            m.Row3.X = state._position.X;
            m.Row3.Y = state._position.Y;
            m.Row3.Z = state._position.Z;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 CreateModelMatrix(EngineObjectRenderState state)
        {
            Matrix4 m = Matrix4.CreateFromQuaternion(state._rotation);

            m.Row0 *= state._scale.X;
            m.Row1 *= state._scale.Y;
            m.Row2 *= state._scale.Z;

            m.Row3.X = state._position.X;
            m.Row3.Y = state._position.Y;
            m.Row3.Z = state._position.Z;
            m.Row3.W = 1.0f;

            return m;
        }

        public static Matrix4 BiasedMatrixForShadowMapping = new Matrix4(
            0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f);


        public static Matrix4 ConvertGLTFTRSToOpenTKMatrix(float[] scale, float[] rotation, float[] translation)
        {
            OpenTK.Mathematics.Quaternion r = new OpenTK.Mathematics.Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(r);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale[0], scale[1], scale[2]);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(translation[0], translation[1], translation[2]);

            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public static Matrix4 ConvertGLTFFloatArraytoOpenTKMatrix(float[] source)
        {
            Matrix4 convertedMatrix = new Matrix4(
                source[0], source[1], source[2], source[3],
                source[4], source[5], source[6], source[7],
                source[8], source[9], source[10], source[11],
                source[12], source[13], source[14], source[15]
                );
            return convertedMatrix;
        }

        public static void ConvertAssimpToOpenTKMatrix(Matrix4x4 source, out Matrix4 convertedMatrix)
        {
            convertedMatrix = new Matrix4(source.A1, source.A2, source.A3, source.A4,
                                                            source.B1, source.B2, source.B3, source.B4,
                                                            source.C1, source.C2, source.C3, source.C4,
                                                            source.D1, source.D2, source.D3, source.D4);
            convertedMatrix.Transpose();
        }

        public static Matrix4 ConvertAssimpToOpenTKMatrix(Matrix4x4 source)
        {
            Matrix4 convertedMatrix = new Matrix4(source.A1, source.A2, source.A3, source.A4,
                                                            source.B1, source.B2, source.B3, source.B4,
                                                            source.C1, source.C2, source.C3, source.C4,
                                                            source.D1, source.D2, source.D3, source.D4);
            convertedMatrix.Transpose();
            return convertedMatrix;
        }

        public static void ConvertAssimpToOpenTKMatrix(Matrix3x3 source, out Matrix3 convertedMatrix)
        {
            convertedMatrix = new Matrix3(source.A1, source.A2, source.A3,
                                                            source.B1, source.B2, source.B3,
                                                            source.C1, source.C2, source.C3);
            convertedMatrix.Transpose();
        }

        public static Matrix4 CreateRotationMatrixForAxisAngle(ref Vector3 axis, ref float angle)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);
            float oc = 1f - c;

            return new Matrix4(oc * axis.X * axis.X + c, oc * axis.X * axis.Y - axis.Z * s, oc * axis.Z * axis.X + axis.Y * s, 0f,
                        oc * axis.X * axis.Y + axis.Z * s, oc * axis.Y * axis.Y + c, oc * axis.Y * axis.Z - axis.X * s, 0f,
                        oc * axis.Z * axis.X - axis.Y * s, oc * axis.Y * axis.Z + axis.X * s, oc * axis.Z * axis.Z + c, 0f,
                        0f, 0f, 0f, 1f);
        }

        internal static Matrix4 Scale105 = Matrix4.CreateScale(1.05f);
        internal static Matrix4 Scale110 = Matrix4.CreateScale(1.10f);
        internal static Matrix4 Scale100 = Matrix4.Identity;

        internal static float[] Matrix4ToArray(Matrix4[] matrices)
        {
            float[] data = new float[16 * matrices.Length];
            for(int h = 0, i = 0; i < matrices.Length; i++, h += 16)
            {
                data[h + 00] = matrices[i].Row0.X;
                data[h + 01] = matrices[i].Row0.Y;
                data[h + 02] = matrices[i].Row0.Z;
                data[h + 03] = matrices[i].Row0.W;

                data[h + 04] = matrices[i].Row1.X;
                data[h + 05] = matrices[i].Row1.Y;
                data[h + 06] = matrices[i].Row1.Z;
                data[h + 07] = matrices[i].Row1.W;

                data[h + 08] = matrices[i].Row2.X;
                data[h + 09] = matrices[i].Row2.Y;
                data[h + 10] = matrices[i].Row2.Z;
                data[h + 11] = matrices[i].Row2.W;

                data[h + 12] = matrices[i].Row3.X;
                data[h + 13] = matrices[i].Row3.Y;
                data[h + 14] = matrices[i].Row3.Z;
                data[h + 15] = matrices[i].Row3.W;
            }
            
            return data;
        }
    }
}
