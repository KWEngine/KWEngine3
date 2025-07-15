using Assimp;
using KWEngine3.Helper;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KWEngine3.Model
{
    internal class GeoMesh
    {
        public override string ToString()
        {
            return Name;
        }

        public List<string> BoneNames = new List<string>();
        public List<int> BoneIndices = new List<int>();
        public List<Matrix4> BoneOffset = new List<Matrix4>();
        public List<Matrix4> BoneOffsetInverse = new List<Matrix4>();

        internal int BoneTranslationMatrixCount;

        public int VAO { get; internal set; }
        public int VBOPosition { get; internal set; }
        public int VBONormal { get; internal set; }
        public int VBOTexture1 { get; internal set; }
        public int VBOBoneIDs { get; internal set; }
        public int VBOBoneWeights { get; internal set; }
        public int VBOTangent { get; internal set; }
        public int VBOBiTangent { get; internal set; }
        public int VBOIndex { get; internal set; }
        public string Name { get; internal set; }
        public string NameOrg { get; internal set; }

        internal Matrix4 Transform;
        public GeoVertex[] Vertices { get; internal set; }
        public OpenTK.Graphics.OpenGL4.PrimitiveType Primitive;
        public int IndexCount
        {
            get;
            internal set;
        }

        public GeoMaterial Material { get; internal set; }

        internal void VAOGenerateAndBind()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
        }

        internal void VAOUnbind()
        {
            GL.BindVertexArray(0);
        }

        internal void VBOGenerateVerticesAndBones(bool hasBones)
        {
            float[] verticesF = new float[Vertices.Length * 3];
            uint[] boneIds = new uint[Vertices.Length * 3];
            float[] boneWeights = new float[Vertices.Length * 3];

            for (int i = 0, arrayIndex = 0; i < Vertices.Length; i++, arrayIndex += 3)
            {
                verticesF[arrayIndex] = Vertices[i].X;
                verticesF[arrayIndex + 1] = Vertices[i].Y;
                verticesF[arrayIndex + 2] = Vertices[i].Z;

                if (hasBones)
                {
                    boneIds[arrayIndex] = Vertices[i].BoneIDs[0];
                    boneIds[arrayIndex + 1] = Vertices[i].BoneIDs[1];
                    boneIds[arrayIndex + 2] = Vertices[i].BoneIDs[2];

                    boneWeights[arrayIndex] = Vertices[i].Weights[0];
                    boneWeights[arrayIndex + 1] = Vertices[i].Weights[1];
                    boneWeights[arrayIndex + 2] = Vertices[i].Weights[2];
                }
            }
            VBOPosition = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOPosition);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * 3 * 4, verticesF, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            if (hasBones)
            {
                VBOBoneIDs = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBoneIDs);
                GL.BufferData(BufferTarget.ArrayBuffer, boneIds.Length * 4, boneIds, BufferUsageHint.StaticDraw);
                GL.VertexAttribIPointer(5, 3, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
                GL.EnableVertexAttribArray(5);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                VBOBoneWeights = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBoneWeights);
                GL.BufferData(BufferTarget.ArrayBuffer, boneWeights.Length * 4, boneWeights, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(6, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(6);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal List<Vector3> VBOGenerateNormals(Mesh mesh)
        {
            List<Vector3> normals = new List<Vector3>();
            if (mesh.HasNormals)
            {
                float[] values = new float[mesh.Normals.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.Normals.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.Normals[i].X;
                    values[arrayIndex + 1] = mesh.Normals[i].Y;
                    values[arrayIndex + 2] = mesh.Normals[i].Z;

                    Vector3 tmpNormal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                    normals.Add(tmpNormal);
                }
                VBONormal = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormal);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Normals.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(2);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            return normals;
        }

        internal void VBOGenerateNormals(float[] normals)
        {
            if (normals.Length > 0)
            {
                VBONormal = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormal);
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * 4, normals, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(2);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateTextureCoords1(Mesh mesh)
        {
            if (mesh.HasTextureCoords(0))
            {
                float[] values = new float[mesh.TextureCoordinateChannels[0].Count * 2];

                for (int i = 0, arrayIndex = 0; i < mesh.TextureCoordinateChannels[0].Count; i++, arrayIndex += 2)
                {
                    values[arrayIndex] = mesh.TextureCoordinateChannels[0][i].X;
                    values[arrayIndex + 1] = mesh.TextureCoordinateChannels[0][i].Y;
                }

                VBOTexture1 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexture1);
                GL.BufferData(BufferTarget.ArrayBuffer, values.Length * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

        }

        internal void VBOGenerateTextureCoords1(float[] uvs)
        {
            if (uvs != null && uvs.Length > 0)
            {
                VBOTexture1 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexture1);
                GL.BufferData(BufferTarget.ArrayBuffer, uvs.Length * 4, uvs, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateTangents(Mesh mesh)
        {
            
            if (mesh.HasTextureCoords(0) && mesh.HasVertices && mesh.HasNormals)
            {
                int[] indices = mesh.GetIndices();

                // Zwischenspeicher für aufsummierte Tangenten und Bitangenten
                Vector3[] tanSum = new Vector3[mesh.Vertices.Count];
                Vector3[] bitanSum = new Vector3[mesh.Vertices.Count];

                // 1. Pro-Dreieck Tangenten und Bitangenten berechnen
                for (int i = 0; i < indices.Length; i += 3)
                {
                    int i0 = indices[i + 0];
                    int i1 = indices[i + 1];
                    int i2 = indices[i + 2];

                    Vector3 p0 = HelperVector.ConvertVector3DToOpenTK(mesh.Vertices[i0]);
                    Vector3 p1 = HelperVector.ConvertVector3DToOpenTK(mesh.Vertices[i1]);
                    Vector3 p2 = HelperVector.ConvertVector3DToOpenTK(mesh.Vertices[i2]);

                    Vector2 uv0 = HelperVector.ConvertVector3DToOpenTK(mesh.TextureCoordinateChannels[0][i0]).Xy;
                    Vector2 uv1 = HelperVector.ConvertVector3DToOpenTK(mesh.TextureCoordinateChannels[0][i1]).Xy;
                    Vector2 uv2 = HelperVector.ConvertVector3DToOpenTK(mesh.TextureCoordinateChannels[0][i2]).Xy;

                    Vector3 e1 = p1 - p0;
                    Vector3 e2 = p2 - p0;

                    Vector2 duv1 = uv1 - uv0;
                    Vector2 duv2 = uv2 - uv0;

                    float det = duv1.X * duv2.Y - duv2.X * duv1.Y;
                    float f = det == 0f ? 0f : 1f / det;

                    Vector3 tangent = f * (duv2.Y * e1 - duv1.Y * e2);
                    Vector3 bitangent = f * (-duv2.X * e1 + duv1.X * e2);

                    tanSum[i0] += tangent;
                    tanSum[i1] += tangent;
                    tanSum[i2] += tangent;

                    bitanSum[i0] += bitangent;
                    bitanSum[i1] += bitangent;
                    bitanSum[i2] += bitangent;
                }

                // 2. Orthonormalisierung & Handedness pro Vertex
                float[] tangents_f = new float[mesh.Vertices.Count * 3];
                float[] bitangents_f = new float[mesh.Vertices.Count * 3];
                for (int i = 0, arrayIndex = 0; i < mesh.Vertices.Count; i++, arrayIndex += 3)
                {
                    Vector3 n = HelperVector.ConvertVector3DToOpenTK(mesh.Normals[i]);
                    Vector3 t = tanSum[i];

                    // Gram-Schmidt-Orthonormalisierung
                    t = Vector3.Normalize(t - n * Vector3.Dot(n, t));

                    // Handedness bestimmen
                    Vector3 bOrig = bitanSum[i];
                    Vector3 bCross = Vector3.Cross(n, t);
                    float h = Vector3.Dot(bCross, bOrig) < 0f ? -1f : 1f;

                    tangents_f[arrayIndex + 0] = t.X;
                    tangents_f[arrayIndex + 1] = t.Y;
                    tangents_f[arrayIndex + 2] = t.Z;

                    bitangents_f[arrayIndex + 0] = bCross.X * h;
                    bitangents_f[arrayIndex + 1] = bCross.Y * h;
                    bitangents_f[arrayIndex + 2] = bCross.Z * h;
                }

                VBOTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, tangents_f.Length * 4, tangents_f, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(3);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                VBOBiTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBiTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, bitangents_f.Length * 4, bitangents_f, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(4);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            
            /*
            if (mesh.HasTangentBasis)
            {
                //Tangents
                float[] values = new float[mesh.Tangents.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.Tangents.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.Tangents[i].X;
                    values[arrayIndex + 1] = mesh.Tangents[i].Y;
                    values[arrayIndex + 2] = mesh.Tangents[i].Z;
                }
                VBOTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Tangents.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(3);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                //BiTangents
                values = new float[mesh.BiTangents.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.BiTangents.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.BiTangents[i].X;
                    values[arrayIndex + 1] = mesh.BiTangents[i].Y;
                    values[arrayIndex + 2] = mesh.BiTangents[i].Z;
                }
                VBOBiTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBiTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.BiTangents.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(4);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }    
            */
        }

        internal static void GenerateVerticesFromVBO(int vbo, ref float xmin, ref float xmax, ref float ymin, ref float ymax, ref float zmin, ref float zmax)
        {
            if (vbo >= 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int size);
                if(size > 0)
                {
                    float[] vbodata = new float[size / sizeof(float)];
                    GL.GetBufferSubData<float>(BufferTarget.ArrayBuffer, IntPtr.Zero, size, vbodata);
                    for (int i = 0; i < vbodata.Length; i += 3)
                    {
                        GeoVertex newVertex = new GeoVertex(i / 3, vbodata[i], vbodata[i + 1], vbodata[i + 2]);
                        if (newVertex.X < xmin) xmin = newVertex.X;
                        if (newVertex.X > xmax) xmax = newVertex.X;
                        if (newVertex.Y < ymin) ymin = newVertex.Y;
                        if (newVertex.Y > ymax) ymax = newVertex.Y;
                        if (newVertex.Z < zmin) zmin = newVertex.Z;
                        if (newVertex.Z > zmax) zmax = newVertex.Z;
                    }
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }
            }
        }

        internal void VBOGenerateTangents(float[] normals, float[] tangents)
        {
            if (tangents != null && tangents.Length > 0)
            {
                VBOTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, tangents.Length * 4, tangents, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(3);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                if (tangents != null && tangents.Length > 0)
                {
                    //BiTangents
                    float[] bitangents = new float[tangents.Length];
                    for (int i = 0; i < normals.Length; i += 3)
                    {
                        Vector3 n = new Vector3(normals[i], normals[i + 1], normals[i + 2]);
                        Vector3 t = new Vector3(tangents[i], tangents[i + 1], tangents[i + 2]);
                        Vector3 bt = Vector3.Cross(n, t);
                        bitangents[i] = bt.X;
                        bitangents[i + 1] = bt.Y;
                        bitangents[i + 2] = bt.Z;
                    }

                    VBOBiTangent = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBiTangent);
                    GL.BufferData(BufferTarget.ArrayBuffer, bitangents.Length * 4, bitangents, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(4);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }
            }
        }

        internal void VBOGenerateIndices(uint[] indices)
        {
            VBOIndex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndex);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        internal void Dispose()
        {
            // Dispose VAO and VBOS:
            if (VBOIndex > 0)
                GL.DeleteBuffer(VBOIndex);
            if (VBOPosition > 0)
                GL.DeleteBuffer(VBOPosition);
            if (VBONormal > 0)
                GL.DeleteBuffer(VBONormal);
            if (VBOTangent > 0)
                GL.DeleteBuffer(VBOTangent);
            if (VBOBiTangent > 0)
                GL.DeleteBuffer(VBOBiTangent);
            if (VBOTexture1 > 0)
                GL.DeleteBuffer(VBOTexture1);
            if (VBOBoneIDs > 0)
                GL.DeleteBuffer(VBOBoneIDs);
            if (VBOBoneWeights > 0)
                GL.DeleteBuffer(VBOBoneWeights);

            if (VAO > 0)
                GL.DeleteVertexArray(VAO);
        }

        public GeoTerrain Terrain { get; internal set; }
    }
}
