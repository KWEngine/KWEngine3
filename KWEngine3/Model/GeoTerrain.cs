using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
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

        private int mWidth = 0;
        private int mDepth = 0;
        private int mHeight;
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

        public int GetHeight()
        {
            return mHeight;
        }

        public int GetWidth()
        {
            return mWidth;
        }

        public int GetDepth()
        {
            return mDepth;
        }

        /*
        internal GeoMesh BuildTerrain(string terrainName, string heightMap, int width, int pHeight, int depth)
        {
            GeoMesh mmp;
            try
            {
                using (Stream s = File.Open(heightMap, FileMode.Open))
                {
                    using (SKBitmap image = SKBitmap.Decode(s))
                    {
                        if (image == null || image.Width % 16 != 0 || image.Height % 16 != 0 || image.Width < 16 || image.Height < 16 || image.Height > 1024 || image.Width > 1024)
                        {
                            throw new Exception();
                        }
                            
                        mDots = image.Width * image.Height;
                        mWidth = width;
                        mDepth = depth;
                        mHeight = pHeight;

                        float stepWidth = (float)mWidth / (image.Width);
                        float stepDepth = (float)mDepth / (image.Height);
                        float trisCountWidth = (mWidth / stepWidth) * 2;
                        float trisCountDepth = (mDepth / stepDepth) * 2;
                        int tmpSectorCountWidth = (int)Math.Round(trisCountWidth / 4);
                        int tmpSectorCountDepth = (int)Math.Round(trisCountDepth / 4);
                        mSectorSize = Math.Min(tmpSectorCountWidth, tmpSectorCountDepth);

                        mSectorMap = new Sector[mSectorSize, mSectorSize];
                        for (int i = 0; i < mSectorSize; i++)
                        {
                            for (int j = 0; j < mSectorSize; j++)
                            {
                                Sector se = new Sector();
                                se.Left = -mWidth / 2 + i * ((float)mWidth / mSectorSize);
                                se.Right = -mWidth / 2 + (i + 1) * ((float)mWidth / mSectorSize);
                                se.Back = -mDepth / 2 + (j + 1) * ((float)mDepth / mSectorSize);
                                se.Front = -mDepth / 2 + j * ((float)mDepth / mSectorSize);
                                se.Center = new Vector2((se.Left + se.Right) / 2, (se.Front + se.Back) / 2);
                                mSectorMap[i, j] = se;
                            }
                        }
                        mSectorWidth = (float)mWidth / mSectorSize;
                        mSectorDepth = (float)mDepth / mSectorSize;

                        float[,] mHeightMap = new float[image.Width, image.Height];
                        mCompleteDiameter = MathF.Sqrt(mWidth * mWidth + mDepth * mDepth + mHeight * mHeight);


                        Vector3[] points = new Vector3[mDots];
                        int c = 0;
                        for (int i = 0; i < image.Width; i++)
                        {
                            for (int j = 0; j < image.Height; j++)
                            {
                                SKColor tmpColor = image.GetPixel(i, j);
                                float normalizedRGB = ((tmpColor.Red + tmpColor.Green + tmpColor.Blue) / 3f) / 255f;
                                mHeightMap[i, j] = normalizedRGB;

                                Vector3 tmp = new Vector3(
                                    i * stepWidth - mWidth / 2,
                                    mHeight * normalizedRGB,
                                    -mDepth / 2 + j * stepDepth
                                    );
                                points[c] = tmp;

                                //increase 2nd counter:
                                c++;
                            }
                        }

                        mmp = new GeoMesh();
                        mmp.Name = terrainName;

                        int imageHeight = image.Height;
                        Vector3 normalT1 = new Vector3(0, 0, 0);
                        Vector3 normalT2 = new Vector3(0, 0, 0);
                        for (int i = 0; i < points.Length - imageHeight - 1; i++)
                        {
                            if ((i + 1) % imageHeight == 0)
                            {
                                continue;
                            }

                            // Generate Triangle objects:
                            // ============================ T1 ======================
                            Vector3 v1 = new Vector3(points[i + imageHeight + 1]);
                            Vector3 v2 = new Vector3(points[i + imageHeight]);
                            Vector3 v3 = new Vector3(points[i]);
                            GeoTerrainTriangle t123 = new GeoTerrainTriangle(v1, v2, v3);
                            normalT1 = t123.Normal;

                            List<SectorTuple> st123 = GetSectorTuplesForTriangle(t123);
                            foreach (SectorTuple tuple in st123)
                            {
                                mSectorMap[tuple.X, tuple.Y].AddTriangle(t123);
                            }

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
                        }
                        mmp.Transform = Matrix4.Identity;
                    }
                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[KWEngine] Height map image invalid or its width/height is not a multiple of 16 (range is from 16 - 1024px)");
                //DeleteOpenGLHeightMap(heightMap);
                return null;
            }

            if (KWEngine.CurrentWorld._customTextures.ContainsKey(heightMap))
            {
                _texHeight = KWEngine.CurrentWorld._customTextures[heightMap];
            }
            else
            {
                _texHeight = HelperTexture.LoadTextureForModelExternal(heightMap, out int tmpMips);
                KWEngine.CurrentWorld._customTextures.Add(heightMap, _texHeight);
            }

            mmp.Primitive = PrimitiveType.Patches;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            return mmp;
        }
        */

        internal GeoMesh BuildTerrain(string terrainName, string heightMap, int width, int pHeight, int depth)
        {
            GeoMesh mmp;
            try
            {
                using (Stream s = File.Open(heightMap, FileMode.Open))
                {
                    using (SKBitmap image = SKBitmap.Decode(s))
                    {
                        if (image == null || image.Width % 16 != 0 || image.Height % 16 != 0 || image.Width < 16 || image.Height < 16 || image.Height > 1024 || image.Width > 1024)
                        {
                            throw new Exception();
                        }

                        mDots = image.Width * image.Height;
                        mWidth = width; // terrain width
                        mDepth = depth; // terrain depth
                        mHeight = pHeight;

                        float triangleWidth = 1f;
                        float triangleDepth = 1f;
                        int sectorLength = 4;

                        int startX = -mWidth / 2;
                        int startZ = mDepth / 2;

                        List<Vector3> vertices = new();

                        int sectorCountX = mWidth / sectorLength;
                        int sectorCountZ = mWidth / sectorLength;

                        mSectorMap = new Sector[sectorCountX, sectorCountZ];

                        for (int i = 0; i < mSectorMap.GetLength(0) - 1; i++)
                        {
                            for(int j = 0; j < mSectorMap.GetLength(1) - 1; j++)
                            {
                                float sectorLeft = startX + i * sectorLength;
                                float sectorRight = startX + (i + 1) * sectorLength;
                                float sectorBack = startZ - (j + 1) * sectorLength;
                                float sectorFront = startZ - j * sectorLength;

                                Sector currentSector = new Sector(
                                    sectorLeft,
                                    sectorRight,
                                    sectorBack,
                                    sectorFront
                                    );


                                // generate triangles for this sector:
                                float h = 0;
                                for (float x = sectorLeft; x < sectorRight; x += triangleWidth)
                                {
                                    for(float z = sectorBack; z < sectorFront; z += triangleDepth)
                                    {
                                        h = GetHeightForVertex(x + triangleWidth, z, mWidth, mDepth, mHeight, image);
                                        Vector3 t0 = new Vector3(x + triangleWidth, h, z);                  // right back

                                        h = GetHeightForVertex(x, z, mWidth, mDepth, mHeight, image);
                                        Vector3 t1 = new Vector3(x, h, z);                                  // left back

                                        h = GetHeightForVertex(x, z + triangleDepth, mWidth, mDepth, mHeight, image);
                                        Vector3 t2 = new Vector3(x, h, z + triangleDepth);                  // left front


                                        h = GetHeightForVertex(x, z + triangleDepth, mWidth, mDepth, mHeight, image);
                                        Vector3 t3 = new Vector3(x, h, z + triangleDepth);                  // left front

                                        h = GetHeightForVertex(x + triangleWidth, z + triangleDepth, mWidth, mDepth, mHeight, image);
                                        Vector3 t4 = new Vector3(x + triangleWidth, h, z + triangleDepth);  // right front

                                        h = GetHeightForVertex(x + triangleWidth, z, mWidth, mDepth, mHeight, image);
                                        Vector3 t5 = new Vector3(x + triangleWidth, h, z);                  // right back

                                        GeoTerrainTriangle tri0 = new GeoTerrainTriangle(t0, t1, t2);
                                        GeoTerrainTriangle tri1 = new GeoTerrainTriangle(t3, t4, t5);
                                        currentSector.AddTriangle(tri0);
                                        currentSector.AddTriangle(tri1);
                                    }
                                    
                                    
                                    /*float z = i * triangleDepth;

                                    float xScaledToImageSize = HelperGeneral.ScaleToRange(x, 0, mWidth, 0, image.Width);
                                    float zScaledToImageSize = HelperGeneral.ScaleToRange(z, 0, mDepth, 0, image.Height);

                                    SKColor tmpColor = image.GetPixel((int)xScaledToImageSize, (int)zScaledToImageSize);
                                    float normalizedRGB = ((tmpColor.Red + tmpColor.Green + tmpColor.Blue) / 3f) / 255f;
                                    Vector3 tmp = new Vector3(
                                            x,
                                            mHeight * normalizedRGB,
                                            z
                                            );
                                    vertices.Add(tmp);
                                    */
                                }

                                mSectorMap[i, j] = currentSector;
                            }
                        }




                        for (int i = 0; i < mWidth / triangleWidth; i++)
                        {
                            float x = i * triangleWidth;
                            float z = i * triangleDepth;

                            float xScaledToImageSize = HelperGeneral.ScaleToRange(x, 0, mWidth, 0, image.Width);
                            float zScaledToImageSize = HelperGeneral.ScaleToRange(z, 0, mDepth, 0, image.Height);

                            SKColor tmpColor = image.GetPixel((int)xScaledToImageSize, (int)zScaledToImageSize);
                            float normalizedRGB = ((tmpColor.Red + tmpColor.Green + tmpColor.Blue) / 3f) / 255f;
                            Vector3 tmp = new Vector3(
                                    x,
                                    mHeight * normalizedRGB,
                                    z
                                    );
                            vertices.Add(tmp);
                        }

                        mmp = new GeoMesh();
                        mmp.Name = terrainName;
                        mmp.Transform = Matrix4.Identity;
                    }
                }
            }
            catch (Exception)
            {
                KWEngine.LogWriteLine("[KWEngine] Height map image invalid or its width/height is not a multiple of 16 (range is from 16 - 1024px)");
                //DeleteOpenGLHeightMap(heightMap);
                return null;
            }

            if (KWEngine.CurrentWorld._customTextures.ContainsKey(heightMap))
            {
                _texHeight = KWEngine.CurrentWorld._customTextures[heightMap];
            }
            else
            {
                _texHeight = HelperTexture.LoadTextureForModelExternal(heightMap, out int tmpMips);
                KWEngine.CurrentWorld._customTextures.Add(heightMap, _texHeight);
            }

            mmp.Primitive = PrimitiveType.Patches;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            return mmp;
        }
        private List<SectorTuple> GetSectorTuplesForTriangle(GeoTerrainTriangle t)
        {
            List<SectorTuple> indices = new List<SectorTuple>();

            float dividerWidth = MathF.Max(mWidth / mSectorMap.GetLength(0), 1f);
            float dividerDepth = MathF.Max(mDepth / mSectorMap.GetLength(1), 1f);

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
                    VBOUVBufferSideMesh[uvCounter + 1] = tmpVertex.Y / mDepth;
                    VBOUVBufferSideMesh[uvCounter + 2] = (tmpVertex.Z + mDepth / 2) / mDepth;
                    VBOUVBufferSideMesh[uvCounter + 3] = 0f;
                }

                Vector3 n;
                Vector3 t;
                Vector3 b;
                if (sideFaceType == SideFaceType.Front)
                {
                    n = Vector3.UnitZ;
                    t = Vector3.UnitX;
                    b = Vector3.UnitY;
                }
                else if (sideFaceType == SideFaceType.Back)
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

            for (uint i = 0; i < (pointList.Count * 2) - 3; i += 2)
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
            for (int i = 0; i < mSectorMap.GetLength(0); i++)
            {
                for (int j = 0; j < mSectorMap.GetLength(1); j++)
                {
                    mSectorMap[i, j].Triangles.Clear();
                }
            }
            mSectorMap = null;
        }

        internal void DeleteOpenGLHeightMap(string entry)
        {
            if (_texHeight > 0 && _texHeight != KWEngine.TextureBlack)
            {
                GL.DeleteTextures(1, new int[] { _texHeight });
                _texHeight = -1;
            }

            if (KWEngine.CurrentWorld._customTextures.ContainsKey(entry))
            {
                KWEngine.CurrentWorld._customTextures.Remove(entry);
            }
        }

        internal static float GetHeightForVertex(float x, float z, int terrainWidth, int terrainDepth, int terrainHeight, SKBitmap image)
        {
            float xScaledToImageSize = HelperGeneral.ScaleToRange(x + terrainWidth / 2, 0, terrainWidth, 0, image.Width);
            float zScaledToImageSize = HelperGeneral.ScaleToRange(z - terrainDepth / 2, 0, terrainDepth, 0, image.Height);

            SKColor tmpColor = image.GetPixel((int)xScaledToImageSize, (int)zScaledToImageSize);
            float normalizedRGB = ((tmpColor.Red + tmpColor.Green + tmpColor.Blue) / 3f) / 255f;

            return normalizedRGB * terrainHeight;
        }
    }
}

