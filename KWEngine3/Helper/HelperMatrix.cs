using Assimp;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace KWEngine3.Helper
{
    internal static class HelperMatrix
    {
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

        public static Matrix4 CreateModelMatrix(ref Vector3 s)
        {
            return Matrix4.CreateTranslation(s);
        }

        public static Matrix4 CreateModelMatrix(GameObjectState state)
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

        public static Matrix4 CreateModelMatrix(GameObjectRenderState state)
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
