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
        private int mSectorLength = 2;
        private float mTriangleWidth = 0.5f;
        private float mTriangleDepth = 0.5f;
        internal int _texHeight = -1;
        internal Sector[,] mSectorMap;
        internal string _heightMapName;
        internal float[,] _pixelHeights;
        internal bool _collisionModeNew = true; // NEW!

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

        internal GeoMesh BuildTerrain(string terrainName, string heightMap, SKBitmap image, int pHeight, out int width, out int depth)
        {
            width = -1;
            depth = -1;
            GeoMesh mmp;
            mWidth = image.Width; // terrain width
            mDepth = image.Height; // terrain depth
            width = mWidth;
            depth = mDepth;

            mHeight = pHeight;

            byte[] pixelData = image.GetPixelSpan().ToArray();

            mTriangleWidth = 1;
            mTriangleDepth = 1;
            mSectorLength = 8;

            int startX = -mWidth / 2;
            int startZ = -mDepth / 2;

            int sectorCountX = mWidth / mSectorLength;
            int sectorCountZ = mDepth / mSectorLength;

            mSectorMap = new Sector[sectorCountX, sectorCountZ];
            for (int i = 0; i < sectorCountX; i++)
            {
                for(int j = 0; j < sectorCountZ; j++)
                {
                                
                    float sectorLeft = startX + i * mSectorLength;
                    float sectorRight = startX + (i + 1) * mSectorLength;
                    float sectorFront = startZ + (j + 1) * mSectorLength;
                    float sectorBack = startZ + j * mSectorLength;

                    float trianglesCountX = (sectorRight - sectorLeft) / mTriangleWidth;
                    float trianglesCountZ = (sectorFront - sectorBack) / mTriangleDepth;
                    int trianglesCountPerSector = (int)(trianglesCountX * trianglesCountZ) * 2;

                    Sector currentSector = new Sector(
                        sectorLeft,
                        sectorRight,
                        sectorBack,
                        sectorFront,
                        trianglesCountPerSector
                        );

                    // generate triangles for this sector:
                    float h = 0;
                    for (float x = sectorLeft; x < sectorRight; x += mTriangleWidth)
                    {
                        for(float z = sectorBack; z < sectorFront; z += mTriangleDepth)
                        {
                            h = GetHeightForVertex(x + mTriangleWidth, z, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t0 = new Vector3(x + mTriangleWidth, h, z);                  // right back

                            h = GetHeightForVertex(x, z, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t1 = new Vector3(x, h, z);                                  // left back

                            h = GetHeightForVertex(x, z + mTriangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t2 = new Vector3(x, h, z + mTriangleDepth);                  // left front

                            h = GetHeightForVertex(x, z + mTriangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t3 = new Vector3(x, h, z + mTriangleDepth);                  // left front

                            h = GetHeightForVertex(x + mTriangleWidth, z + mTriangleDepth, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t4 = new Vector3(x + mTriangleWidth, h, z + mTriangleDepth);  // right front

                            h = GetHeightForVertex(x + mTriangleWidth, z, mWidth, mDepth, mHeight, image, pixelData);
                            Vector3 t5 = new Vector3(x + mTriangleWidth, h, z);                  // right back

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
            
            _heightMapName = terrainName;
            if (KWEngine.CurrentWorld._customTextures.ContainsKey(_heightMapName))
            {
                _texHeight = KWEngine.CurrentWorld._customTextures[_heightMapName].ID;
            }
            else
            {
                _texHeight = HelperTexture.LoadTextureForHeightMap(image, out int tmpMips);
                GL.BindTexture(TextureTarget.Texture2D, _texHeight);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                KWEngine.CurrentWorld._customTextures.Add(_heightMapName, new KWTexture(_texHeight, TextureTarget.Texture2D));
            }

            mmp.Primitive = PrimitiveType.Patches;
            return mmp;
        }
        
        public bool GetSectorForUntranslatedPosition(Vector3 position, out Sector s)
        {
            float tmpF;
            tmpF = position.X + (mWidth / 2);
            int tmpIndexX = Math.Min((int)(tmpF / mSectorLength), mSectorMap.GetLength(0) - 1);
            tmpF = position.Z + (mDepth / 2);
            int tmpIndexZ = Math.Min((int)(tmpF / mSectorLength), mSectorMap.GetLength(1) - 1);
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

        internal float GetHeightForVertex(float x, float z, int terrainWidth, int terrainDepth, int terrainHeight, SKBitmap image, byte[] pixelData)
        {
            //Console.WriteLine(x + " | " + z);
            //float xScaledToImageSize = Math.Clamp(HelperGeneral.ScaleToRange(x, -terrainWidth / 2f + 0.5f, terrainWidth / 2f - 0.5f, 0, image.Width - 1), 0f, image.Width - 1f);
            //float zScaledToImageSize = Math.Clamp(HelperGeneral.ScaleToRange(z, -terrainDepth / 2f + 0.5f, terrainDepth / 2f - 0.5f, 0, image.Height - 1), 0f, image.Height - 1f);

            float inputLoX = -(terrainWidth - 0) / 2f;// - 0.5f;
            float inputHiX = +(terrainWidth - 0) / 2f;// + 0.5f;
            float inputLoZ = -(terrainDepth - 0) / 2f;// - 0.5f;
            float inputHiZ = +(terrainDepth - 0) / 2f;// + 0.5f;
            float xScaledToImageSize = HelperGeneral.ScaleToRange(x, inputLoX, inputHiX, 0, image.Width - 1);
            float zScaledToImageSize = HelperGeneral.ScaleToRange(z, inputLoZ, inputHiZ, 0, image.Height - 1);

            //Console.WriteLine(xScaledToImageSize + "|" + zScaledToImageSize);

            int ix = (int)(xScaledToImageSize);
            int iz = (int)(zScaledToImageSize);
            int offsetX = iz * image.Width * image.BytesPerPixel + ix * image.BytesPerPixel;
            
            /*
            int ixNeighbour = Math.Min(ix + 1, image.Width - 1);
            int izNeighbour = Math.Min(iz + 1, image.Height - 1);
            float blendX = xScaledToImageSize - ix;
            float blendZ = zScaledToImageSize - iz;
            int offsetNeighbourX = iz * image.Width * image.BytesPerPixel + ixNeighbour * image.BytesPerPixel;
            int offsetNeighbourZ = izNeighbour * image.Width * image.BytesPerPixel + ix * image.BytesPerPixel;
            */
            byte colorLoc = pixelData[offsetX];
            //byte colorNX = pixelData[offsetNeighbourX];
            //byte colorNZ = pixelData[offsetNeighbourZ];

            //float vertexColor = ((colorLoc * (1f - blendX) + colorNX * blendX) + (colorLoc * (1f - blendZ) + colorNZ * blendZ)) * 0.5f;
            float vertexColor = colorLoc;
            float normalizedRGB = vertexColor / 255f;
            return normalizedRGB * terrainHeight;
        }
    }
}

