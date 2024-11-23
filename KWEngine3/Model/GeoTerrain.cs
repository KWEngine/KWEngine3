using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using System.Diagnostics;

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
        private int mHeight = 0;
        //private int mSectorSize = 2;
        internal int _texHeight = -1;
        internal Sector[,] mSectorMap;
        private float mCompleteDiameter;
        internal string _heightMapName;

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
                            throw new Exception("[KWEngine] Height map too small or of invalid type");
                        }

                        if(image.Width < width || image.Height < depth)
                        {
                            KWEngine.LogWriteLine("[KWEngine] WARNING: height map resolution too low for the given width and depth - collision model might be inaccurate");
                        }

                        mWidth = width; // terrain width
                        mDepth = depth; // terrain depth
                        mHeight = pHeight;

                        byte[] pixelData = image.GetPixelSpan().ToArray();

                        float triangleWidth = 0.5f;
                        float triangleDepth = 0.5f;
                        int sectorLength = 2;

                        int startX = -mWidth / 2;
                        int startZ = -mDepth / 2;

                        mCompleteDiameter = MathF.Sqrt(mWidth * mWidth + mDepth * mDepth + mHeight * mHeight);

                        int sectorCountX = mWidth / sectorLength;
                        int sectorCountZ = mDepth / sectorLength;

                        mSectorMap = new Sector[sectorCountX, sectorCountZ];
                        for (int i = 0; i < sectorCountX; i++)
                        {
                            for(int j = 0; j < sectorCountZ; j++)
                            {
                                
                                float sectorLeft = startX + i * sectorLength;
                                float sectorRight = startX + (i + 1) * sectorLength;
                                float sectorFront = startZ + (j + 1) * sectorLength;
                                float sectorBack = startZ + j * sectorLength;

                                float trianglesCountX = (sectorRight - sectorLeft);
                                float trianglesCountZ = (sectorFront - sectorBack);
                                int trianglesCountPerSector = (int)((trianglesCountX * trianglesCountZ) / (triangleWidth * triangleDepth) * 2);

                                Sector currentSector = new Sector(
                                    sectorLeft,
                                    sectorRight,
                                    sectorBack,
                                    sectorFront,
                                    trianglesCountPerSector
                                    );

                                // generate triangles for this sector:
                                float h = 0;
                                for (float x = sectorLeft; x < sectorRight; x += triangleWidth)
                                {
                                    for(float z = sectorBack; z < sectorFront; z += triangleDepth)
                                    {
                                        h = GetHeightForVertex(x + triangleWidth, z, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t0 = new Vector3(x + triangleWidth, h, z);                  // right back

                                        h = GetHeightForVertex(x, z, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t1 = new Vector3(x, h, z);                                  // left back

                                        h = GetHeightForVertex(x, z + triangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t2 = new Vector3(x, h, z + triangleDepth);                  // left front

                                        h = GetHeightForVertex(x, z + triangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t3 = new Vector3(x, h, z + triangleDepth);                  // left front

                                        h = GetHeightForVertex(x + triangleWidth, z + triangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t4 = new Vector3(x + triangleWidth, h, z + triangleDepth);  // right front

                                        h = GetHeightForVertex(x + triangleWidth, z, mWidth, mDepth, mHeight, image, pixelData);
                                        Vector3 t5 = new Vector3(x + triangleWidth, h, z);                  // right back

                                        GeoTerrainTriangle tri0 = new GeoTerrainTriangle(t0, t1, t2);
                                        GeoTerrainTriangle tri1 = new GeoTerrainTriangle(t3, t4, t5);
                                        currentSector.AddTriangle(tri0);
                                        currentSector.AddTriangle(tri1);
                                    }
                                }
                                mSectorMap[i, j] = currentSector;
                            }
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
            _heightMapName = heightMap;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(heightMap))
            {
                _texHeight = KWEngine.CurrentWorld._customTextures[heightMap].ID;
            }
            else
            {
                _texHeight = HelperTexture.LoadTextureForModelExternal(heightMap, out int tmpMips);
                GL.BindTexture(TextureTarget.Texture2D, _texHeight);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                KWEngine.CurrentWorld._customTextures.Add(heightMap, new KWTexture(_texHeight, TextureTarget.Texture2D));
            }

            mmp.Primitive = PrimitiveType.Patches;
            return mmp;
        }

        public bool GetSectorForUntranslatedPosition(Vector3 position, out Sector s)
        {
            float tmpF;
            tmpF = position.X + (mWidth / 2);
            int tmpIndexX = Math.Min((int)(tmpF * 0.5f), mSectorMap.GetLength(0) - 1);
            tmpF = position.Z + (mDepth / 2);
            int tmpIndexZ = Math.Min((int)(tmpF * 0.5f), mSectorMap.GetLength(1) - 1);
            s = new Sector();
            if (tmpIndexX < 0 || tmpIndexX >= mSectorMap.GetLength(0) || tmpIndexZ < 0 || tmpIndexZ >= mSectorMap.GetLength(1))
            {
                return false;
            }
            s = mSectorMap[tmpIndexX, tmpIndexZ];
            return true;
        }

        internal void Dispose()
        {
            for (int i = 0; i < mSectorMap.GetLength(0); i++)
            {
                for (int j = 0; j < mSectorMap.GetLength(1); j++)
                {
                    mSectorMap[i, j].Triangles = null;
                }
            }
            mSectorMap = null;
            DeleteOpenGLHeightMap(_heightMapName);
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

        internal static float GetHeightForVertex(float x, float z, int terrainWidth, int terrainDepth, int terrainHeight, SKBitmap image, byte[] pixelData)
        {
            float xScaledToImageSize = HelperGeneral.ScaleToRange(x + terrainWidth / 2, 0, terrainWidth, 0, image.Width - 1);
            float zScaledToImageSize = HelperGeneral.ScaleToRange(z + terrainDepth / 2, 0, terrainDepth, 0, image.Height - 1);
            int ix = (int)xScaledToImageSize;
            int iz = (int)zScaledToImageSize;

            int ixNeighbour = Math.Min(ix + 1, image.Width - 1);
            int izNeighbour = Math.Min(iz + 1, image.Height - 1);

            float blendX = xScaledToImageSize - ix;
            float blendZ = zScaledToImageSize - iz;

            int offsetX = iz * image.Width * image.BytesPerPixel + ix * image.BytesPerPixel;
            int offsetNeighbourX = iz * image.Width * image.BytesPerPixel + ixNeighbour * image.BytesPerPixel;
            int offsetNeighbourZ = izNeighbour * image.Width * image.BytesPerPixel + ix * image.BytesPerPixel;

            byte colorLoc = pixelData[offsetX];
            byte colorNX = pixelData[offsetNeighbourX];
            byte colorNZ = pixelData[offsetNeighbourZ];

            float vertexColor = ((colorLoc * (1f - blendX) + colorNX * blendX) + (colorLoc * (1f - blendZ) + colorNZ * blendZ)) * 0.5f;

            float normalizedRGB = vertexColor / 255f;
            return normalizedRGB * terrainHeight;
        }
    }
}

