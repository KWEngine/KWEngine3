using KWEngine3.Helper;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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

            this._pixelHeights = new float[image.Width, image.Height];
            // fill float-2dim-array for new collision detection
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    SKColor clr = image.GetPixel(x, y);
                    this._pixelHeights[x, y] = (clr.Red + clr.Green + clr.Blue) / 3f / 255f;
                }
            }

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
                            h = GetHeightForVertex(x + mTriangleWidth, z);
                            Vector3 t0 = new Vector3(x + mTriangleWidth, h, z);                  // right back

                            h = GetHeightForVertex(x, z);
                            Vector3 t1 = new Vector3(x, h, z);                                  // left back

                            h = GetHeightForVertex(x, z + mTriangleDepth);
                            Vector3 t2 = new Vector3(x, h, z + mTriangleDepth);                  // left front

                            h = GetHeightForVertex(x, z + mTriangleDepth);
                            Vector3 t3 = new Vector3(x, h, z + mTriangleDepth);                  // left front

                            h = GetHeightForVertex(x + mTriangleWidth, z + mTriangleDepth);
                            Vector3 t4 = new Vector3(x + mTriangleWidth, h, z + mTriangleDepth);  // right front

                            h = GetHeightForVertex(x + mTriangleWidth, z);
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

        internal float GetHeightForVertex(float x, float z)
        {
            Vector3 ray = new Vector3(x, this.mHeight + 1, z);

            float rayXOffset = ray.X + this.mWidth / 2f;
            float rayZOffset = ray.Z + this.mDepth / 2f;
            float pxX = rayXOffset - 0.5f;
            float pxZ = rayZOffset - 0.5f;

            float weightX = 1f - pxX % 1f;
            float weightZ = 1f - pxZ % 1f;
            float weightXOther = 1f - weightX;
            float weightZOther = 1f - weightZ;

            int pxX0 = (int)(pxX + 0.0f);
            int pxX1 = MathHelper.Clamp(pxX0 + 1, 0, this.mWidth - 1);

            int pxZ0 = (int)(pxZ + 0.0f);
            int pxZ1 = MathHelper.Clamp(pxZ0 + 1, 0, this.mDepth - 1);

            float i00 = this._pixelHeights[pxX0, pxZ0];
            float i01 = this._pixelHeights[pxX0, pxZ1];
            float i10 = this._pixelHeights[pxX1, pxZ0];
            float i11 = this._pixelHeights[pxX1, pxZ1];

            Vector3 v00 = new Vector3(pxX0, i00 * this.mHeight, pxZ0);
            Vector3 v01 = new Vector3(pxX0, i01 * this.mHeight, pxZ1);
            Vector3 v10 = new Vector3(pxX1, i10 * this.mHeight, pxZ0);
            Vector3 v11 = new Vector3(pxX1, i11 * this.mHeight, pxZ1);

            // bilinear interpolation:
            float result =
                i00 * weightX * weightZ +
                i01 * weightX * weightZOther +
                i10 * weightXOther * weightZ +
                i11 * weightXOther * weightZOther;

            return result * this.mHeight;
        }
    }
}

