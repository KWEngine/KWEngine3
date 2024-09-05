using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Reflection;
using SkiaSharp;

namespace KWEngine3.Model
{
    internal class GeoTerrain
    {
        private enum SideFaceType
        {
            Front,
            Back,
            Left,
            Right
        }

        private float mWidth = 0;
        private float mDepth = 0;
        private float mScaleFactor;
        private int mDots = -1;
        private int mSectorSize = -1;
        private float mSectorWidth;
        private float mSectorDepth;
        internal int _texHeight = -1;

        internal int _texBlend = KWEngine.TextureBlack;
        internal int _texR = KWEngine.TextureAlpha;
        internal int _texG = KWEngine.TextureAlpha;
        internal int _texB = KWEngine.TextureAlpha;

        private Sector[,] mSectorMap;

        private float mCompleteDiameter;
        internal float mTexX = 1;
        internal float mTexY = 1;

        public float GetScaleFactor()
        {
            return mScaleFactor;
        }

        public float GetWidth()
        {
            return mWidth;
        }

        public float GetDepth()
        {
            return mDepth;
        }

        internal GeoMesh BuildTerrain(string heightMap, float width, float height, float depth, bool isFile = true)
        {
            GeoMesh mmp;
            if (isFile)
            {
                if (!File.Exists(heightMap))
                {
                    KWEngine.LogWriteLine("[Terrain] Heightmap invalid.");
                    return null;
                }
                else
                {
                    string hmtmp = heightMap.Trim().ToLower();
                    if(!hmtmp.EndsWith("png") && !hmtmp.EndsWith("jpg") && !hmtmp.EndsWith("jpeg"))
                    {
                        KWEngine.LogWriteLine("[Terrain] Heightmap may be of type PNG or JPG only.");
                        return null;
                    }
                    else
                    {
                        if(KWEngine.CurrentWorld._customTextures.ContainsKey(heightMap))
                        {
                            _texHeight = KWEngine.CurrentWorld._customTextures[heightMap];
                        }
                        else
                        {
                            _texHeight = HelperTexture.LoadTextureForModelExternal(heightMap, out int tmpMips);
                            KWEngine.CurrentWorld._customTextures.Add(heightMap, _texHeight);
                        }
                    }
                }
            }

            try
            {
                Assembly a;
                if (heightMap == null)
                {
                    a = Assembly.GetExecutingAssembly();
                    isFile = false;
                    heightMap = "Assets.Textures.heightmap.png";
                }
                else
                {
                    a = Assembly.GetEntryAssembly();
                }
                string assemblyname = a.GetName().Name;
                using (Stream s = isFile ? File.Open(heightMap, FileMode.Open) : a.GetManifestResourceStream(assemblyname + "." + heightMap))
                {
                    if(!isFile)
                    {
                        _texHeight = KWEngine.TextureBlack;
                    }

                    using (SKBitmap image = SKBitmap.Decode(s))
                    {

                        mDots = image.Width * image.Height;

                        mWidth = width;
                        mDepth = depth;
                        mScaleFactor = height;
                        mTexX = 1;
                        mTexY = 1;

                        if (image.Width < 4 || image.Height < 4 || image.Height > 4096 || image.Width > 4096)
                        {
                            KWEngine.LogWriteLine("[Terrain] Height map too small or too big (4-4096px)");
                            DeleteOpenGLHeightMap(heightMap);
                            return null;
                        }

                        double mp = Math.Round(mDots / 1000000.0, 3);
                        long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                        float stepWidth = mWidth / (image.Width - 1);
                        float stepDepth = mDepth / (image.Height - 1);

                        float trisCountWidth = (mWidth / stepWidth) * 2;
                        float trisCountDepth= (mDepth / stepDepth) * 2;

                        int tmpSectorCountWidth = (int)Math.Round(trisCountWidth / 4);
                        int tmpSectorCountDepth = (int)Math.Round(trisCountDepth / 4);
                        mSectorSize = Math.Min(tmpSectorCountWidth, tmpSectorCountDepth);

                        mSectorMap = new Sector[mSectorSize, mSectorSize];
                        for (int i = 0; i < mSectorSize; i++)
                        {
                            for (int j = 0; j < mSectorSize; j++)
                            {
                                Sector se = new Sector();
                                se.Left = -mWidth / 2 + i * (mWidth / mSectorSize);
                                se.Right = -mWidth / 2 + (i + 1) * (mWidth / mSectorSize);
                                se.Back = -mDepth / 2 + (j + 1) * (mDepth / mSectorSize);
                                se.Front = -mDepth / 2 + j * (mDepth / mSectorSize);
                                se.Center = new Vector2((se.Left + se.Right) / 2, (se.Front + se.Back) / 2);
                                mSectorMap[i, j] = se;
                            }
                        }
                        mSectorWidth = mWidth / mSectorSize;
                        mSectorDepth = mDepth / mSectorSize;


                        float[,] mHeightMap = new float[image.Width, image.Height];
                        mCompleteDiameter = (float)Math.Sqrt(mWidth * mWidth + mDepth * mDepth + mScaleFactor * mScaleFactor);

                        
                        Vector3[] points = new Vector3[mDots];
                        int c = 0;
                        int cFBuffer = 0;
                        int cFBufferUV = 0;
                        
                        List<Vector3> frontFaces = new List<Vector3>();
                        List<Vector3> backFaces = new List<Vector3>();
                        List<Vector3> leftFaces = new List<Vector3>();
                        List<Vector3> rightFaces = new List<Vector3>();
                        
                        for (int i = 0; i < image.Width; i++)
                        {
                            for (int j = 0; j < image.Height; j++)
                            {
                                SKColor tmpColor = image.GetPixel(i, j);
                                float normalizedRGB = ((tmpColor.Red + tmpColor.Green + tmpColor.Blue) / 3f) / 255f;
                                mHeightMap[i, j] = normalizedRGB;

                                Vector3 tmp = new Vector3(
                                    i * stepWidth - mWidth / 2,
                                    mScaleFactor * normalizedRGB,
                                    -mDepth / 2 + j * stepDepth
                                    );
                                points[c] = tmp;

                                // left face:
                                if(i == 0)
                                {
                                    leftFaces.Add(tmp);
                                }
                                // right face
                                if (i == image.Width - 1)
                                {
                                    rightFaces.Add(tmp);
                                }
                                // back face:
                                if (j == 0)
                                {
                                    backFaces.Add(tmp);
                                }
                                // front face:
                                if(j == image.Height - 1)
                                {
                                    frontFaces.Add(tmp);
                                }

                                //increase counter:
                                c++;
                                cFBuffer += 3;
                                cFBufferUV += 2;
                            }
                        }

                        mmp = new GeoMesh();
                        mmp.Name = heightMap;

                        int triangles = 0;
                        int imageHeight = image.Height;
                        Vector3 normalT1 = new Vector3(0, 0, 0);
                        Vector3 normalT2 = new Vector3(0, 0, 0);

                        float deltaU1 = 0;
                        float deltaV1 = 0;
                        float deltaU2 = 0;
                        float deltaV2 = 0;
                        float f = 1.0f;
                        Vector3 tangent = new Vector3(0, 0, 0);
                        Vector3 bitangent = new Vector3(0, 0, 0);
                        for (int i = 0; i < points.Length - imageHeight - 1; i++)
                        {
                            Vector3 tmp;

                            if ((i + 1) % imageHeight == 0)
                            {
                                continue;
                            }

                            
                            // Generate Triangle objects:

                            // T1:
                            Vector3 v1 = new Vector3(points[i + imageHeight + 1]);
                            Vector3 v2 = new Vector3(points[i + imageHeight]);
                            Vector3 v3 = new Vector3(points[i]);
                            GeoTerrainTriangle t123 = new GeoTerrainTriangle(v1, v2, v3);
                            normalT1 = t123.Normal;

                            float xRight = Math.Max(Math.Max(v1.X, v2.X), v3.X);
                            float xLeft = Math.Min(Math.Min(v1.X, v2.X), v3.X);
                            float yTop = Math.Max(Math.Max(v1.Z, v2.Z), v3.Z);
                            float yBottom = Math.Min(Math.Min(v1.Z, v2.Z), v3.Z);

                            List<SectorTuple> st123 = GetSectorTuplesForTriangle(t123);
                            foreach (SectorTuple tuple in st123)
                            {
                                mSectorMap[tuple.X, tuple.Y].AddTriangle(t123);
                            }
                            /*
                            // tangents and bitangent generation
                            deltaU1 = VBOUVBuffer[(i + imageHeight) * 2] - VBOUVBuffer[(i + imageHeight + 1) * 2];
                            deltaV1 = VBOUVBuffer[(i + imageHeight) * 2 + 1] - VBOUVBuffer[(i + imageHeight + 1) * 2 + 1];
                            deltaU2 = VBOUVBuffer[(i + 0) * 2] - VBOUVBuffer[(i + imageHeight + 1) * 2];
                            deltaV2 = VBOUVBuffer[(i + 0) * 2 + 1] - VBOUVBuffer[(i + imageHeight + 1) * 2 + 1];
                            f = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);

                            tangent.X = f * (deltaV2 * t123.edge1.X - deltaV1 * t123.edge2.X);
                            tangent.Y = f * (deltaV2 * t123.edge1.Y - deltaV1 * t123.edge2.Y);
                            tangent.Z = f * (deltaV2 * t123.edge1.Z - deltaV1 * t123.edge2.Z);

                            bitangent.X = f * (-deltaU2 * t123.edge1.X - deltaU1 * t123.edge2.X);
                            bitangent.Y = f * (-deltaU2 * t123.edge1.Y - deltaU1 * t123.edge2.Y);
                            bitangent.Z = f * (-deltaU2 * t123.edge1.Z - deltaU1 * t123.edge2.Z);

                            // Generate their normals for VBO:
                            normalMapping.TryGetValue(i, out tmp);
                            tmp += normalT1;
                            normalMapping[i] = tmp;
                            normalMappingCount[i]++;

                            normalMapping.TryGetValue(i + imageHeight, out tmp);
                            tmp += normalT1;
                            normalMapping[i + imageHeight] = tmp;
                            normalMappingCount[i + imageHeight]++;

                            normalMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += normalT1;
                            normalMapping[i + imageHeight + 1] = tmp;
                            normalMappingCount[i + imageHeight + 1]++;

                            // map tangents & bitangent here:
                            tangentMapping.TryGetValue(i, out tmp);
                            tmp += tangent;
                            tangentMapping[i] = tmp;
                            tangentMapping.TryGetValue(i + imageHeight, out tmp);
                            tmp += tangent;
                            tangentMapping[i + imageHeight] = tmp;
                            tangentMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += tangent;
                            tangentMapping[i + imageHeight + 1] = tmp;

                            bitangentMapping.TryGetValue(i, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i] = tmp;
                            bitangentMapping.TryGetValue(i + imageHeight, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i + imageHeight] = tmp;
                            bitangentMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i + imageHeight + 1] = tmp;

                            */

                            // ============================ T2 ======================

                            Vector3 v4 = new Vector3(points[i]);
                            Vector3 v5 = new Vector3(points[i + 1]);
                            Vector3 v6 = new Vector3(points[i + imageHeight + 1]);
                            GeoTerrainTriangle t456 = new GeoTerrainTriangle(v4, v5, v6);
                            normalT2 = t456.Normal;

                            List<SectorTuple> st456 = GetSectorTuplesForTriangle(t456);
                            foreach (SectorTuple tuple in st456)
                            {
                                mSectorMap[tuple.X, tuple.Y].AddTriangle(t456);
                            }
                            /*
                            // tangents and bitangent generation
                            deltaU1 = VBOUVBuffer[(i + 1) * 2] - VBOUVBuffer[(i) * 2];
                            deltaV1 = VBOUVBuffer[(i + 1) * 2 + 1] - VBOUVBuffer[(i) * 2 + 1];
                            deltaU2 = VBOUVBuffer[(i + imageHeight + 1) * 2] - VBOUVBuffer[(i) * 2];
                            deltaV2 = VBOUVBuffer[(i + imageHeight + 1) * 2 + 1] - VBOUVBuffer[(i) * 2 + 1];
                            f = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);

                            tangent.X = f * (deltaV2 * t456.edge1.X - deltaV1 * t456.edge2.X);
                            tangent.Y = f * (deltaV2 * t456.edge1.Y - deltaV1 * t456.edge2.Y);
                            tangent.Z = f * (deltaV2 * t456.edge1.Z - deltaV1 * t456.edge2.Z);

                            bitangent.X = f * (-deltaU2 * t456.edge1.X - deltaU1 * t456.edge2.X);
                            bitangent.Y = f * (-deltaU2 * t456.edge1.Y - deltaU1 * t456.edge2.Y);
                            bitangent.Z = f * (-deltaU2 * t456.edge1.Z - deltaU1 * t456.edge2.Z);

                            normalMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += normalT2;
                            normalMapping[i + imageHeight + 1] = tmp;
                            normalMappingCount[i + imageHeight + 1]++;

                            normalMapping.TryGetValue(i + 1, out tmp);
                            tmp += normalT2;
                            normalMapping[i + 1] = tmp;
                            normalMappingCount[i + 1]++;

                            normalMapping.TryGetValue(i, out tmp);
                            tmp += normalT2;
                            normalMapping[i] = tmp;
                            normalMappingCount[i]++;

                            // map tangents & bitangent here:
                            tangentMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += tangent;
                            tangentMapping[i + imageHeight + 1] = tmp;
                            tangentMapping.TryGetValue(i + 1, out tmp);
                            tmp += tangent;
                            tangentMapping[i + 1] = tmp;
                            tangentMapping.TryGetValue(i, out tmp);
                            tmp += tangent;
                            tangentMapping[i] = tmp;

                            bitangentMapping.TryGetValue(i + imageHeight + 1, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i + imageHeight + 1] = tmp;
                            bitangentMapping.TryGetValue(i + 1, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i + 1] = tmp;
                            bitangentMapping.TryGetValue(i, out tmp);
                            tmp += bitangent;
                            bitangentMapping[i] = tmp;
                            */
                            triangles += 2;
                        }
                        //Debug.WriteLine("\tGenerated triangles:\t" + triangles);
                        cFBuffer = 0;

                        /*
                        for (int i = 0; i < points.Length; i++)
                        {
                            // Interpolate normals:
                            Vector3 tmp = new Vector3(normalMapping[i].X / normalMappingCount[i],
                                normalMapping[i].Y / normalMappingCount[i],
                                normalMapping[i].Z / normalMappingCount[i]);
                            tmp.Normalize();

                            Vector3 tTemp = new Vector3(tangentMapping[i].X / normalMappingCount[i],
                                tangentMapping[i].Y / normalMappingCount[i],
                                tangentMapping[i].Z / normalMappingCount[i]);
                            tTemp.Normalize();

                            Vector3 btTemp = new Vector3(bitangentMapping[i].X / normalMappingCount[i],
                                bitangentMapping[i].Y / normalMappingCount[i],
                                bitangentMapping[i].Z / normalMappingCount[i]);
                            btTemp.Normalize();

                            VBONormalsBuffer[cFBuffer + 0] = tmp.X;
                            VBONormalsBuffer[cFBuffer + 1] = tmp.Y;
                            VBONormalsBuffer[cFBuffer + 2] = tmp.Z;

                            // tangents and bitangents:
                            VBOTangentBuffer[cFBuffer + 0] = tTemp.X;
                            VBOTangentBuffer[cFBuffer + 1] = tTemp.Y;
                            VBOTangentBuffer[cFBuffer + 2] = tTemp.Z;

                            // tangents and bitangents:
                            VBOBiTangentBuffer[cFBuffer + 0] = btTemp.X;
                            VBOBiTangentBuffer[cFBuffer + 1] = btTemp.Y;
                            VBOBiTangentBuffer[cFBuffer + 2] = btTemp.Z;

                            cFBuffer += 3;
                        }

                        // Generate VBOs:
                        mmp.VBOPosition = GL.GenBuffer();
                        mmp.VBONormal = GL.GenBuffer();
                        mmp.VBOTexture1 = GL.GenBuffer();
                        mmp.VBOTangent = GL.GenBuffer();
                        mmp.VBOBiTangent = GL.GenBuffer();
                        mmp.VBOIndex = GL.GenBuffer();

                        // Vertices:
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mmp.VBOPosition);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOVerticesBuffer.Length * 4, VBOVerticesBuffer, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(0);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Normals
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mmp.VBONormal);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBONormalsBuffer.Length * 4, VBONormalsBuffer, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(2);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // UVs
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mmp.VBOTexture1);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOUVBuffer.Length * 4, VBOUVBuffer, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(1);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Tangents
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mmp.VBOTangent);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOTangentBuffer.Length * 4, VBOTangentBuffer, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(3);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Bitangents
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mmp.VBOBiTangent);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOBiTangentBuffer.Length * 4, VBOBiTangentBuffer, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(4);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        uint[] indices = mIndices.ToArray();
                        mmp.IndexCount = indices.Length;
                        // Indices:
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, mmp.VBOIndex);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                        */

                        mmp.Transform = Matrix4.Identity;

                        //GL.BindVertexArray(0);

                        //long diff = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - start;
                        //Debug.WriteLine("\t...done (" + Math.Round(diff / 1000f, 2) + " seconds)");

                        /*
                        // Generate side meshes:
                        float[] VBOVerticesBufferSideMesh = new float[(frontFaces.Count + backFaces.Count + leftFaces.Count + rightFaces.Count) * 2 * 3];
                        float[] VBONormalsBufferSideMesh = new float[VBOVerticesBufferSideMesh.Length];
                        float[] VBOUVBufferSideMesh = new float[(frontFaces.Count + backFaces.Count + leftFaces.Count + rightFaces.Count) * 2 * 2];
                        float[] VBOTangentBufferSideMesh = new float[VBONormalsBufferSideMesh.Length];
                        float[] VBOBiTangentBufferSideMesh = new float[VBONormalsBufferSideMesh.Length];
                        List<uint> indicesSideMesh = new List<uint>();

                        int indexVertices = 0, indexNormals = 0, indexUVs = 0;
                        uint indexIndices = 0;
                        rightFaces.Reverse();
                        backFaces.Reverse();
                        GenerateFaceVertices(frontFaces, SideFaceType.Front, ref indexVertices, ref indexNormals, ref indexUVs, ref indexIndices, VBOVerticesBufferSideMesh, VBONormalsBufferSideMesh, VBOUVBufferSideMesh, VBOTangentBufferSideMesh, VBOBiTangentBufferSideMesh, indicesSideMesh);
                        GenerateFaceVertices(leftFaces, SideFaceType.Left, ref indexVertices, ref indexNormals, ref indexUVs, ref indexIndices, VBOVerticesBufferSideMesh, VBONormalsBufferSideMesh, VBOUVBufferSideMesh, VBOTangentBufferSideMesh, VBOBiTangentBufferSideMesh, indicesSideMesh);
                        GenerateFaceVertices(backFaces, SideFaceType.Back, ref indexVertices, ref indexNormals, ref indexUVs, ref indexIndices, VBOVerticesBufferSideMesh, VBONormalsBufferSideMesh, VBOUVBufferSideMesh, VBOTangentBufferSideMesh, VBOBiTangentBufferSideMesh, indicesSideMesh);
                        GenerateFaceVertices(rightFaces, SideFaceType.Right, ref indexVertices, ref indexNormals, ref indexUVs, ref indexIndices, VBOVerticesBufferSideMesh, VBONormalsBufferSideMesh, VBOUVBufferSideMesh, VBOTangentBufferSideMesh, VBOBiTangentBufferSideMesh, indicesSideMesh);

                        // Generate VBOs:
                        sideMeshes.VAO = GL.GenVertexArray();
                        sideMeshes.VBOPosition = GL.GenBuffer();
                        sideMeshes.VBONormal = GL.GenBuffer();
                        sideMeshes.VBOTexture1 = GL.GenBuffer();
                        sideMeshes.VBOTangent = GL.GenBuffer();
                        sideMeshes.VBOBiTangent = GL.GenBuffer();
                        sideMeshes.VBOIndex = GL.GenBuffer();

                        GL.BindVertexArray(sideMeshes.VAO);
                        // Vertices:
                        GL.BindBuffer(BufferTarget.ArrayBuffer, sideMeshes.VBOPosition);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOVerticesBufferSideMesh.Length * 4, VBOVerticesBufferSideMesh, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(0);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Normals
                        GL.BindBuffer(BufferTarget.ArrayBuffer, sideMeshes.VBONormal);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBONormalsBufferSideMesh.Length * 4, VBONormalsBufferSideMesh, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(2);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // UVs
                        GL.BindBuffer(BufferTarget.ArrayBuffer, sideMeshes.VBOTexture1);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOUVBufferSideMesh.Length * 4, VBOUVBufferSideMesh, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(1);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Tangents
                        GL.BindBuffer(BufferTarget.ArrayBuffer, sideMeshes.VBOTangent);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOTangentBufferSideMesh.Length * 4, VBOTangentBufferSideMesh, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(3);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        // Bitangents
                        GL.BindBuffer(BufferTarget.ArrayBuffer, sideMeshes.VBOBiTangent);
                        GL.BufferData(BufferTarget.ArrayBuffer, VBOBiTangentBufferSideMesh.Length * 4, VBOBiTangentBufferSideMesh, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                        GL.EnableVertexAttribArray(4);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        uint[] indicesSideMeshArray = indicesSideMesh.ToArray();
                        sideMeshes.IndexCount = indicesSideMeshArray.Length;
                        // Indices:
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, sideMeshes.VBOIndex);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, indicesSideMeshArray.Length * 4, indicesSideMeshArray, BufferUsageHint.StaticDraw);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                        sideMeshes.Transform = Matrix4.Identity;
                        GL.BindVertexArray(0);
                        */
                    }
                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[Terrain] Terrain source material invalid");
                DeleteOpenGLHeightMap(heightMap);
                return null;
            }
            mmp.Primitive = PrimitiveType.Triangles;
            //sideMeshes.Primitive = PrimitiveType.Triangles;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            return mmp;
        }

        private List<SectorTuple> GetSectorTuplesForTriangle(GeoTerrainTriangle t)
        {
            List<SectorTuple> indices = new List<SectorTuple>();

            float dividerWidth = mWidth / mSectorMap.GetLength(0);
            float dividerDepth = mDepth / mSectorMap.GetLength(1);

            foreach (Vector3 v in t.Vertices)
            {
                float tmpF1, tmpF2;
                tmpF1 = v.X + (mWidth / 2);
                int tmpIndexX1 = Math.Min((int)(tmpF1 / dividerWidth), mSectorMap.GetLength(0) - 1);
                int tmpIndexX2 = Math.Min((int)Math.Round(tmpF1 / dividerWidth, 0), mSectorMap.GetLength(0) - 1);
                tmpF2 = v.Z + (mDepth / 2);
                int tmpIndexZ1 = Math.Min((int)(tmpF2 / dividerDepth), mSectorMap.GetLength(1) - 1);
                int tmpIndexZ2 = Math.Min((int)Math.Round(tmpF2 / dividerDepth, 0), mSectorMap.GetLength(1) - 1);
                SectorTuple st1 = new SectorTuple() { X = tmpIndexX1, Y = tmpIndexZ1 };
                SectorTuple st2 = new SectorTuple() { X = tmpIndexX1, Y = tmpIndexZ2 };
                SectorTuple st3 = new SectorTuple() { X = tmpIndexX2, Y = tmpIndexZ1 };
                SectorTuple st4 = new SectorTuple() { X = tmpIndexX2, Y = tmpIndexZ2 };

                if (!indices.Contains(st1))
                {
                    indices.Add(st1);
                }
                if (!indices.Contains(st2))
                {
                    indices.Add(st2);
                }
                if (!indices.Contains(st3))
                {
                    indices.Add(st3);
                }
                if (!indices.Contains(st4))
                {
                    indices.Add(st4);
                }
            }
            return indices;
        }

        public Sector GetSectorForUntranslatedPosition(Vector3 position)
        {
            float dividerWidth = mWidth / mSectorSize;
            float dividerDepth = mDepth / mSectorSize;

            float tmpF;
            tmpF = position.X + (mWidth / 2);
            int tmpIndexX = Math.Min((int)(tmpF / dividerWidth), mSectorMap.GetLength(0) - 1);
            tmpF = position.Z + (mDepth / 2);
            int tmpIndexZ = Math.Min((int)(tmpF / dividerDepth), mSectorMap.GetLength(1) - 1);

            if (tmpIndexX < 0 || tmpIndexX >= mSectorMap.GetLength(0)
                || tmpIndexZ < 0 || tmpIndexZ >= mSectorMap.GetLength(1))
                return null;

            return mSectorMap[tmpIndexX, tmpIndexZ];
        }

        private void GenerateFaceVertices(
            List<Vector3> pointList, 
            SideFaceType sideFaceType,
            ref int vboCounter,
            ref int normalCounter,
            ref int uvCounter,
            ref uint indexCounter,
            float[] VBOVerticesBufferSideMesh, 
            float[] VBONormalsBufferSideMesh,
            float[] VBOUVBufferSideMesh,
            float[] VBOTangentBufferSideMesh,
            float[] VBOBiTangentBufferSideMesh,
            List<uint> indicesSideMesh)
        {
            for (int i = 0; i < pointList.Count; i++, vboCounter += 6, normalCounter += 6, uvCounter += 4)
            {
                Vector3 tmpVertex = pointList[i];

                VBOVerticesBufferSideMesh[vboCounter + 0] = tmpVertex.X;
                VBOVerticesBufferSideMesh[vboCounter + 1] = tmpVertex.Y;
                VBOVerticesBufferSideMesh[vboCounter + 2] = tmpVertex.Z;
                VBOVerticesBufferSideMesh[vboCounter + 3] = tmpVertex.X;
                VBOVerticesBufferSideMesh[vboCounter + 4] = 0f;
                VBOVerticesBufferSideMesh[vboCounter + 5] = tmpVertex.Z;
                if (sideFaceType == SideFaceType.Front || sideFaceType == SideFaceType.Back)
                {
                    VBOUVBufferSideMesh[uvCounter + 0] = (tmpVertex.X + mWidth / 2) / mWidth;
                    VBOUVBufferSideMesh[uvCounter + 1] = tmpVertex.Y / mWidth;
                    VBOUVBufferSideMesh[uvCounter + 2] = (tmpVertex.X + mWidth / 2) / mWidth;
                    VBOUVBufferSideMesh[uvCounter + 3] = 0f;
                }
                else
                {
                    VBOUVBufferSideMesh[uvCounter + 0] = (tmpVertex.Z + mDepth / 2) / mDepth;
                    VBOUVBufferSideMesh[uvCounter + 1] = tmpVertex.Y  / mDepth;
                    VBOUVBufferSideMesh[uvCounter + 2] = (tmpVertex.Z + mDepth / 2) / mDepth;
                    VBOUVBufferSideMesh[uvCounter + 3] = 0f; 
                }

                Vector3 n;
                Vector3 t;
                Vector3 b;
                if(sideFaceType == SideFaceType.Front)
                {
                    n = Vector3.UnitZ;
                    t = Vector3.UnitX;
                    b = Vector3.UnitY;
                }
                else if(sideFaceType == SideFaceType.Back)
                {
                    n = -Vector3.UnitZ;
                    t = Vector3.UnitX;
                    b = Vector3.UnitY;
                }
                else if (sideFaceType == SideFaceType.Left)
                {
                    n = -Vector3.UnitX;
                    t = Vector3.UnitZ;
                    b = Vector3.UnitY;
                }
                else
                {
                    n = Vector3.UnitX;
                    t = Vector3.UnitZ;
                    b = Vector3.UnitY;
                }

                VBONormalsBufferSideMesh[normalCounter + 0] = n.X;
                VBONormalsBufferSideMesh[normalCounter + 1] = n.Y;
                VBONormalsBufferSideMesh[normalCounter + 2] = n.Z;
                VBONormalsBufferSideMesh[normalCounter + 3] = n.X;
                VBONormalsBufferSideMesh[normalCounter + 4] = n.Y;
                VBONormalsBufferSideMesh[normalCounter + 5] = n.Z;

                VBOTangentBufferSideMesh[normalCounter + 0] = t.X;
                VBOTangentBufferSideMesh[normalCounter + 1] = t.Y;
                VBOTangentBufferSideMesh[normalCounter + 2] = t.Z;
                VBOTangentBufferSideMesh[normalCounter + 3] = t.X;
                VBOTangentBufferSideMesh[normalCounter + 4] = t.Y;
                VBOTangentBufferSideMesh[normalCounter + 5] = t.Z;

                VBOBiTangentBufferSideMesh[normalCounter + 0] = b.X;
                VBOBiTangentBufferSideMesh[normalCounter + 1] = b.Y;
                VBOBiTangentBufferSideMesh[normalCounter + 2] = b.Z;
                VBOBiTangentBufferSideMesh[normalCounter + 3] = b.X;
                VBOBiTangentBufferSideMesh[normalCounter + 4] = b.Y;
                VBOBiTangentBufferSideMesh[normalCounter + 5] = b.Z;
            }

            for (uint i = 0; i < (pointList.Count * 2) - 3; i+=2)
            {
                indicesSideMesh.Add(indexCounter + i);
                indicesSideMesh.Add(indexCounter + i + 1);
                indicesSideMesh.Add(indexCounter + i + 2);

                indicesSideMesh.Add(indexCounter + i + 1);
                indicesSideMesh.Add(indexCounter + i + 3);
                indicesSideMesh.Add(indexCounter + i + 2);
            }
            indexCounter += (uint)pointList.Count * 2;
        }

        internal void Dispose()
        {
            for(int i = 0; i < mSectorMap.GetLength(0); i++)
            {
                for(int j = 0; j < mSectorMap.GetLength(1); j++)
                {
                    mSectorMap[i, j].Triangles.Clear();
                }
            }
            mSectorMap = null;
        }

        internal void DeleteOpenGLHeightMap(string entry)
        {
            if(_texHeight > 0 && _texHeight != KWEngine.TextureBlack)
            {
                GL.DeleteTextures(1, new int[] { _texHeight });
                _texHeight = -1;
            }

            if(KWEngine.CurrentWorld._customTextures.ContainsKey(entry))
            {
                KWEngine.CurrentWorld._customTextures.Remove(entry);
            }
        }
    }
}

