using KWEngine3.Assets;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererGBufferFoliage
    {
        // uniform locations of one of the two foliage programs
        private sealed class FoliageProgram
        {
            public int ProgramID = -1;
            public int UViewProjectionMatrix = -1;
            public int UModelMatrix = -1;
            public int UNormalMatrix = -1;
            public int UColorTintAndEmissive = -1;
            public int UHue = -1;
            public int UTextureAlbedo = -1;
            public int UTextureNormal = -1;
            public int UTextureNoise = -1;
            public int UPatchSizeTime = -1;
            public int UNXNZ = -1;
            public int UDXDZSwayRound = -1;
            public int UNoise = -1;
            public int UTerrainPosition = -1;
            public int UTerrainScale = -1;
            public int UTerrainHeightMap = -1;
            public int ULightConfig = -1;
            public int URoughnessMetallic = -1;
            public int UCameraPosition = -1;
            // FoliageObject only:
            public int UCell = -1;
            public int UCellLength = -1;
            public int ULOD = -1;
            public int UAlignToTerrain = -1;
            // FoliageObjectCustom only:
            public int UPosScaleUV1 = -1;
            public int UPosScaleUV2 = -1;
            public int UFade = -1;
            public int UAlphaMipScales = -1;
        }

        // one draw call of a FoliageObject: a rectangular block of its plant grid
        private struct FoliageCell
        {
            public int StartX;
            public int StartZ;
            public int Columns;       // edge cells are cut off by the patch border
            public int Rows;
            public int InstanceCount; // first part of the bit reversed instance range that can still be visible
        }

        private static readonly FoliageProgram _programGrid = new FoliageProgram();   // FoliageObject (opaque, no discard)
        private static readonly FoliageProgram _programCustom = new FoliageProgram(); // FoliageObjectCustom (alpha tested cards)

        public static int ProgramID { get { return _programGrid.ProgramID; } }

        private const float CELL_SIZE = 8f;           // approximate edge length of a cell in world units
        private const int CELLS_PER_AXIS_MAX = 32;
        // Plants are widened (and thinned out) below this width on screen, measured at the reference width of a plant
        // (grass: upper part of the blade). Higher values flicker less and draw fewer instances, but blades look coarser
        // in the distance. Measured with 1280x720 (flicker vs. a 3x3 supersampled reference): 1.25 hardly helps, 2 is a
        // good compromise, 3 is close to the reference at mid distance.
        private const float MIN_PIXEL_WIDTH = 2.0f;
        private const float LOD_KEEP_MIN = 1f / 16f;  // must match LOD_KEEP_MIN in shader.gbufferfoliage.vert
        private const float LOD_KEEP_FADE = 0.3f;     // must match LOD_KEEP_FADE in shader.gbufferfoliage.vert
        private const float PLANT_SIZE_MIN = 0.47f;   // range of the random size factor of a plant (heightFactor2 in the vertex shader)
        private const float PLANT_SIZE_MAX = 1.06f;
        private const float LOD_FADE_BAND = 0.15f;    // plants shrink to zero within this fraction of the draw distance before it
        private const float JITTER_MAX = 1f;          // max. random offset of a plant from its grid position (noise * random in the vertex shader)
        private const float CUSTOM_FADE_BAND = 0.15f;

        private static readonly Frustum _frustum = new Frustum();
        private static readonly FoliageCell[] _cells = new FoliageCell[CELLS_PER_AXIS_MAX * CELLS_PER_AXIS_MAX];
        private static readonly float[] _cellDistances = new float[CELLS_PER_AXIS_MAX * CELLS_PER_AXIS_MAX];
        private static FoliageObject[] _visibleObjects = new FoliageObject[16];
        private static float[] _visibleObjectDistances = new float[16];

        public static void Init()
        {
            if (_programGrid.ProgramID < 0)
            {
                CreateProgram(_programGrid, "KWEngine3.Shaders.shader.gbufferfoliage.vert", "KWEngine3.Shaders.shader.gbufferfoliage.frag");
                CreateProgram(_programCustom, "KWEngine3.Shaders.shader.gbufferfoliagecustom.vert", "KWEngine3.Shaders.shader.gbufferfoliagecustom.frag");
            }
        }

        private static void CreateProgram(FoliageProgram p, string resourceNameVertexShader, string resourceNameFragmentShader)
        {
            p.ProgramID = GL.CreateProgram();

            int vertexShader;
            int fragmentShader;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
            {
                vertexShader = RenderManager.LoadCompileAttachShader(s, ShaderType.VertexShader, p.ProgramID);
            }

            using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
            {
                fragmentShader = RenderManager.LoadCompileAttachShader(s, ShaderType.FragmentShader, p.ProgramID);
            }

            GL.LinkProgram(p.ProgramID);
            RenderManager.CheckShaderStatus(p.ProgramID, vertexShader, fragmentShader);

            p.UViewProjectionMatrix = GL.GetUniformLocation(p.ProgramID, "uViewProjectionMatrix");
            p.UModelMatrix = GL.GetUniformLocation(p.ProgramID, "uModelMatrix");
            p.UNormalMatrix = GL.GetUniformLocation(p.ProgramID, "uNormalMatrix");
            p.UColorTintAndEmissive = GL.GetUniformLocation(p.ProgramID, "uColorTintEmissive");
            p.UHue = GL.GetUniformLocation(p.ProgramID, "uHue");
            p.UTextureAlbedo = GL.GetUniformLocation(p.ProgramID, "uTextureAlbedo");
            p.UTextureNormal = GL.GetUniformLocation(p.ProgramID, "uTextureNormal");
            p.UTextureNoise = GL.GetUniformLocation(p.ProgramID, "uNoiseMap");
            p.UPatchSizeTime = GL.GetUniformLocation(p.ProgramID, "uPatchSizeTime");
            p.UNXNZ = GL.GetUniformLocation(p.ProgramID, "uNXNZ");
            p.UDXDZSwayRound = GL.GetUniformLocation(p.ProgramID, "uDXDZSwayRound");
            p.UNoise = GL.GetUniformLocation(p.ProgramID, "uNoise");
            p.UTerrainPosition = GL.GetUniformLocation(p.ProgramID, "uTerrainPosition");
            p.UTerrainScale = GL.GetUniformLocation(p.ProgramID, "uTerrainScale");
            p.UTerrainHeightMap = GL.GetUniformLocation(p.ProgramID, "uTerrainHeightMap");
            p.ULightConfig = GL.GetUniformLocation(p.ProgramID, "uLightConfig");
            p.URoughnessMetallic = GL.GetUniformLocation(p.ProgramID, "uRoughnessMetallic");
            p.UCameraPosition = GL.GetUniformLocation(p.ProgramID, "uCameraPosition");
            p.UCell = GL.GetUniformLocation(p.ProgramID, "uCell");
            p.UCellLength = GL.GetUniformLocation(p.ProgramID, "uCellLength");
            p.ULOD = GL.GetUniformLocation(p.ProgramID, "uLOD");
            p.UAlignToTerrain = GL.GetUniformLocation(p.ProgramID, "uAlignToTerrain");
            p.UPosScaleUV1 = GL.GetUniformLocation(p.ProgramID, "uPosScaleUV1");
            p.UPosScaleUV2 = GL.GetUniformLocation(p.ProgramID, "uPosScaleUV2");
            p.UFade = GL.GetUniformLocation(p.ProgramID, "uFade");
            p.UAlphaMipScales = GL.GetUniformLocation(p.ProgramID, "uAlphaMipScales");
        }

        public static void Bind()
        {
            GL.UseProgram(_programGrid.ProgramID);
        }

        public static void RenderScene()
        {
            World w = KWEngine.CurrentWorld;
            if (w == null || w._foliageObjects.Count == 0)
                return;

            // culling uses the interpolated camera state that is rendered in this frame
            // (the frustum of the camera object itself belongs to the current simulation state)
            Vector3 cameraPosition;
            Matrix4 viewProjection;
            float projectionScaleY; // M22 of the projection matrix = 1 / tan(fovY / 2)
            if (KWEngine.Mode == EngineMode.Play)
            {
                cameraPosition = w._cameraGame._stateRender._position;
                viewProjection = w._cameraGame._stateRender.ViewProjectionMatrix;
                projectionScaleY = w._cameraGame._stateRender.ProjectionMatrix.M22;
                _frustum.UpdateFrustum(w._cameraGame._stateRender.ProjectionMatrix, w._cameraGame._stateRender.ViewMatrix);
            }
            else
            {
                cameraPosition = w._cameraEditor._stateRender._position;
                viewProjection = w._cameraEditor._stateRender.ViewProjectionMatrix;
                projectionScaleY = w._cameraEditor._stateRender.ProjectionMatrix.M22;
                _frustum.UpdateFrustum(w._cameraEditor._stateRender.ProjectionMatrix, w._cameraEditor._stateRender.ViewMatrix);
            }
            // pixels per world unit at a distance of 1 (g-buffer resolution)
            float pixelsPerUnit = projectionScaleY * RenderManager.FramebufferDeferred.Height * 0.5f;

            // FoliageObject instances, nearest patch first:
            int visibleCount = 0;
            foreach (FoliageBase fb in w._foliageObjects)
            {
                if (fb is FoliageObject f && f._nXZ.X > 0 && f._nXZ.Y > 0)
                {
                    GetGridBounds(f, GetMinWidthDistance(f, pixelsPerUnit), out float originX, out float originZ, out float margin, out float minY, out float maxY);
                    Vector3 boundsMin = new Vector3(originX - margin, minY, originZ - margin);
                    Vector3 boundsMax = new Vector3(originX + f._dXZ.X * (f._nXZ.X - 1) + margin, maxY, originZ + f._dXZ.Y * (f._nXZ.Y - 1) + margin);
                    float distance = DistanceToBox(cameraPosition, boundsMin, boundsMax);
                    if (distance >= f._drawDistance || !IsBoxInFrustum(boundsMin, boundsMax))
                        continue;

                    if (visibleCount == _visibleObjects.Length)
                    {
                        Array.Resize(ref _visibleObjects, visibleCount * 2);
                        Array.Resize(ref _visibleObjectDistances, visibleCount * 2);
                    }
                    _visibleObjects[visibleCount] = f;
                    _visibleObjectDistances[visibleCount] = distance;
                    visibleCount++;
                }
            }

            if (visibleCount > 0)
            {
                Array.Sort(_visibleObjectDistances, _visibleObjects, 0, visibleCount);
                GL.UseProgram(_programGrid.ProgramID);
                SetGlobals(_programGrid, ref viewProjection, cameraPosition);
                for (int i = 0; i < visibleCount; i++)
                {
                    DrawGrid(_visibleObjects[i], cameraPosition, pixelsPerUnit);
                    _visibleObjects[i] = null; // do not keep removed objects alive
                }
            }

            // FoliageObjectCustom instances:
            bool customProgramBound = false;
            foreach (FoliageBase fb in w._foliageObjects)
            {
                if (fb is FoliageObjectCustom fc && fc._instanceCount > 0)
                {
                    float drawDistance = MathF.Sqrt(fc._camDistanceMax);
                    GetCustomBounds(fc, out Vector3 boundsMin, out Vector3 boundsMax);
                    if (DistanceToBox(cameraPosition, boundsMin, boundsMax) >= drawDistance || !IsBoxInFrustum(boundsMin, boundsMax))
                        continue;

                    if (!customProgramBound)
                    {
                        GL.UseProgram(_programCustom.ProgramID);
                        SetGlobals(_programCustom, ref viewProjection, cameraPosition);
                        GL.Disable(EnableCap.CullFace); // cards are visible from both sides
                        customProgramBound = true;
                    }
                    DrawCustom(fc, drawDistance);
                }
            }
            if (customProgramBound)
                GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private static void SetGlobals(FoliageProgram p, ref Matrix4 viewProjection, Vector3 cameraPosition)
        {
            GL.UniformMatrix4(p.UViewProjectionMatrix, false, ref viewProjection);
            GL.Uniform3(p.UCameraPosition, cameraPosition);
        }

        private static void DrawGrid(FoliageObject f, Vector3 cameraPosition, float pixelsPerUnit)
        {
            int nx = f._nXZ.X;
            int nz = f._nXZ.Y;
            float dx = f._dXZ.X;
            float dz = f._dXZ.Y;
            float minWidthDistance = GetMinWidthDistance(f, pixelsPerUnit);
            GetGridBounds(f, minWidthDistance, out float originX, out float originZ, out float margin, out float minY, out float maxY);

            float drawDistance = f._drawDistance;
            float fadeBand = drawDistance * LOD_FADE_BAND;

            // collect the visible cells:
            int cellLength = GetCellLength(nx, nz, dx, dz, out int cellLengthLog2);
            int cellInstances = cellLength * cellLength;
            int cellCount = 0;
            for (int sz = 0; sz < nz; sz += cellLength)
            {
                int ez = Math.Min(sz + cellLength, nz);
                for (int sx = 0; sx < nx; sx += cellLength)
                {
                    int ex = Math.Min(sx + cellLength, nx);
                    Vector3 cellMin = new Vector3(originX + dx * sx - margin, minY, originZ + dz * sz - margin);
                    Vector3 cellMax = new Vector3(originX + dx * (ex - 1) + margin, maxY, originZ + dz * (ez - 1) + margin);
                    float distance = DistanceToBox(cameraPosition, cellMin, cellMax);
                    if (distance >= drawDistance || !IsBoxInFrustum(cellMin, cellMax))
                        continue;

                    // keep is largest at the nearest point of the cell (and for the largest plants). Plants whose rank in
                    // the cell is beyond keepMax * (1 + LOD_KEEP_FADE) have shrunk to zero everywhere in the cell.
                    float keepMax = distance > 0f ? Math.Clamp(minWidthDistance * PLANT_SIZE_MAX / distance, LOD_KEEP_MIN, 1f) : 1f;
                    float visibleFraction = Math.Min(keepMax * (1f + LOD_KEEP_FADE), 1f);
                    FoliageCell c;
                    c.StartX = sx;
                    c.StartZ = sz;
                    c.Columns = ex - sx;
                    c.Rows = ez - sz;
                    c.InstanceCount = Math.Clamp((int)MathF.Ceiling(visibleFraction * cellInstances), 1, cellInstances);
                    _cells[cellCount] = c;
                    _cellDistances[cellCount] = distance;
                    cellCount++;
                }
            }
            if (cellCount == 0)
                return;
            Array.Sort(_cellDistances, _cells, 0, cellCount);

            FoliageProgram p = _programGrid;

            // Bind uniforms
            GL.UniformMatrix4(p.UModelMatrix, false, ref f._modelMatrix);
            GL.UniformMatrix4(p.UNormalMatrix, false, ref f._normalMatrix);
            GL.Uniform4(p.UColorTintAndEmissive, f._color);
            GL.Uniform1(p.UHue, f._hue);
            GL.Uniform3(p.UPatchSizeTime, new Vector3(f._patchSize.X, f._patchSize.Y, KWEngine.CurrentWorld.WorldTime));
            GL.Uniform2(p.UNXNZ, f._nXZ);
            GL.Uniform4(p.UDXDZSwayRound, f._dXZ.X, f._dXZ.Y, f._swayFactor, f.IsSizeReducedAtCorners ? 0.05f : 1.0f);
            GL.Uniform2(p.UNoise, f._noise.Length / 2, f._noise);
            GL.Uniform2(p.URoughnessMetallic, f._roughness, 0.0f);
            GL.Uniform3(p.ULOD, minWidthDistance, drawDistance, fadeBand);

            int val = f.IsShadowReceiver ? 1 : -1;
            val *= f.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(p.ULightConfig, val);

            // Bind textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, f._textureId != -1 ? f._textureId : KWEngine.TextureWhite);
            GL.Uniform1(p.UTextureAlbedo, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, f.Type == FoliageType.GrassMinecraft || f.Type == FoliageType.Fern || f.Type == FoliageType.GroundCover ? KWEngine.TextureNormalEmpty : KWEngine.TextureFoliageGrassNormal);
            GL.Uniform1(p.UTextureNormal, 1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureNoise);
            GL.Uniform1(p.UTextureNoise, 2);

            BindTerrain(p, f._terrainObject);

            // Draw calls (one per visible cell)
            int vao;
            int indexBuffer;
            int indexCount;
            if (f.Type == FoliageType.GrassMinecraft)
            {
                GeoMesh m = KWEngine.KWFoliageMinecraft.MeshesArray[0];
                vao = m.VAO;
                indexBuffer = m.VBOIndex;
                indexCount = m.IndexCount;
            }
            else if (f.Type == FoliageType.Fern)
            {
                GeoMesh m = KWEngine.KWFoliageFern.MeshesArray[0];
                vao = m.VAO;
                indexBuffer = m.VBOIndex;
                indexCount = m.IndexCount;
            }
            else if (f.Type == FoliageType.GroundCover)
            {
                GeoMesh m = KWEngine.KWFoliageGroundCover.MeshesArray[0];
                vao = m.VAO;
                indexBuffer = m.VBOIndex;
                indexCount = m.IndexCount;
            }
            else
            {
                vao = KWFoliageGrass.VAO;
                indexBuffer = KWFoliageGrass.EBO;
                indexCount = KWFoliageGrass.IndexCount;
            }

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.Uniform2(p.UCellLength, cellLength, cellLengthLog2);
            GL.Uniform1(p.UAlignToTerrain, f.Type == FoliageType.GroundCover ? 1 : 0); // ground cover is tilted with the terrain
            for (int i = 0; i < cellCount; i++)
            {
                FoliageCell c = _cells[i];
                GL.Uniform4(p.UCell, c.StartX, c.StartZ, c.Columns, c.Rows);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, c.InstanceCount);
            }
            GL.BindVertexArray(0);
        }

        private static void DrawCustom(FoliageObjectCustom f, float drawDistance)
        {
            FoliageProgram p = _programCustom;

            GL.Uniform4(p.UPosScaleUV1, f._instanceCount, f._posScaleArray1);
            GL.Uniform4(p.UPosScaleUV2, f._instanceCount, f._posScaleArray2);

            // Bind uniforms
            GL.Uniform4(p.UColorTintAndEmissive, f._color);
            GL.Uniform1(p.UHue, f._hue);
            GL.Uniform3(p.UPatchSizeTime, new Vector3(0f, 0f, KWEngine.CurrentWorld.WorldTime));
            GL.Uniform2(p.UNoise, f._noise.Length / 2, f._noise); // TODO: check if /2 really cuts it here ;-)
            GL.Uniform2(p.URoughnessMetallic, f._roughness, 0.0f);
            GL.Uniform2(p.UFade, drawDistance * (1f - CUSTOM_FADE_BAND), drawDistance);
            GL.Uniform1(p.UAlphaMipScales, f._alphaMipScales.Length, f._alphaMipScales);

            int val = f.IsShadowReceiver ? 1 : -1;
            val *= f.IsAffectedByLight ? 1 : 10;
            GL.Uniform1(p.ULightConfig, val);

            // Bind textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, f._textureId != -1 ? f._textureId : KWEngine.TextureWhite);
            GL.Uniform1(p.UTextureAlbedo, 0);

            BindTerrain(p, f._terrainObject);

            GeoMesh m = (f.ImposterType == FoliageImposterType.Cross ? KWEngine.KWFoliageImposterCross : KWEngine.KWFoliageImposter).MeshesArray[0];
            GL.BindVertexArray(m.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m.VBOIndex);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, m.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, f._instanceCount);
            GL.BindVertexArray(0);
        }

        private static void BindTerrain(FoliageProgram p, TerrainObject terrain)
        {
            GL.ActiveTexture(TextureUnit.Texture3);
            if (terrain != null && terrain.ID != 0)
            {
                GeoTerrain t = terrain._gModel.ModelOriginal.MeshesArray[0].Terrain;
                GL.Uniform3(p.UTerrainScale, (float)t.GetWidth(), (float)t.GetHeight(), (float)t.GetDepth());
                GL.Uniform3(p.UTerrainPosition, terrain._stateRender._position);
                GL.BindTexture(TextureTarget.Texture2D, t._texHeight);
            }
            else
            {
                GL.Uniform3(p.UTerrainScale, 0f, 0f, 0f);
                GL.Uniform3(p.UTerrainPosition, 0f, 0f, 0f);
                GL.BindTexture(TextureTarget.Texture2D, KWEngine.TextureBlack);
            }
            GL.Uniform1(p.UTerrainHeightMap, 3);
        }

        // Distance up to which a plant of nominal size (random size factor 1) is at least MIN_PIXEL_WIDTH wide on screen.
        // Reference width per type: grass = upper part of the blade, fern = a leaf, minecraft grass = one quad, ground cover = a leaflet.
        private static float GetMinWidthDistance(FoliageObject f, float pixelsPerUnit)
        {
            float referenceWidth;
            if (f.Type == FoliageType.Fern)
                referenceWidth = 0.04f;
            else if (f.Type == FoliageType.GrassMinecraft)
                referenceWidth = 0.124f;
            else if (f.Type == FoliageType.GroundCover)
                referenceWidth = 0.06f;
            else
                referenceWidth = KWFoliageGrass.XH * 2f;
            return pixelsPerUnit * referenceWidth * Math.Min(f._scale.X, f._scale.Z) / MIN_PIXEL_WIDTH;
        }

        // Grid origin (position of plant 0/0 before its random offset) and the vertical range of a patch.
        // margin = how far a plant can reach beyond its grid position (random offset, width incl. lod widening, bending in the wind).
        private static void GetGridBounds(FoliageObject f, float minWidthDistance, out float originX, out float originZ, out float margin, out float minY, out float maxY)
        {
            originX = f._position.X - f._dXZ.X * (f._nXZ.X - 1) * 0.5f;
            originZ = f._position.Z - f._dXZ.Y * (f._nXZ.Y - 1) * 0.5f;

            float plantRadius = f.Type == FoliageType.Fern ? 0.45f : (f.Type == FoliageType.GrassMinecraft ? 0.1f : (f.Type == FoliageType.GroundCover ? 0.4f : 0.03f));
            // model height 1 (ground cover: 0.1), the random height factor in the vertex shader is about 1.06 at most
            float plantHeight = (f.Type == FoliageType.GroundCover ? 0.12f : 1.1f) * f._scale.Y;
            float bend = plantHeight * MathF.Sin(f._swayFactor * MathHelper.PiOver2);
            float widenMax = Math.Clamp(f._drawDistance / (minWidthDistance * PLANT_SIZE_MIN), 1f, 1f / LOD_KEEP_MIN);
            margin = JITTER_MAX + plantRadius * widenMax * 1.1f * Math.Max(f._scale.X, f._scale.Z) + bend;

            minY = f._position.Y - 0.05f * f._scale.Y;
            maxY = f._position.Y + plantHeight;
            if (f._terrainObject != null && f._terrainObject.ID != 0)
            {
                GeoTerrain t = f._terrainObject._gModel.ModelOriginal.MeshesArray[0].Terrain;
                float terrainY = f._terrainObject._stateRender._position.Y;
                minY += terrainY;
                maxY += terrainY + (float)t.GetHeight();
            }
        }

        private static void GetCustomBounds(FoliageObjectCustom f, out Vector3 boundsMin, out Vector3 boundsMax)
        {
            boundsMin = f._renderBoundsMin;
            boundsMax = f._renderBoundsMax;
            if (f._terrainObject != null && f._terrainObject.ID != 0)
            {
                GeoTerrain t = f._terrainObject._gModel.ModelOriginal.MeshesArray[0].Terrain;
                float terrainY = f._terrainObject._stateRender._position.Y;
                boundsMin.Y += terrainY;
                boundsMax.Y += terrainY + (float)t.GetHeight();
            }
        }

        // Edge length of the square cells in plants: a power of two (needed for the bit reversed instance order),
        // close to CELL_SIZE world units, not larger than the patch (edge cells waste instances outside of it)
        // and large enough for at most CELLS_PER_AXIS_MAX cells per axis.
        private static int GetCellLength(int nx, int nz, float dx, float dz, out int log2Length)
        {
            float target = CELL_SIZE / Math.Max(Math.Max(dx, dz), 0.0001f);
            int length = 4;
            while (length * 2 <= target * 1.4142f)
                length <<= 1;

            int patchLimit = 4;
            while (patchLimit * 2 <= Math.Min(nx, nz))
                patchLimit <<= 1;
            length = Math.Min(length, patchLimit);

            while ((Math.Max(nx, nz) + length - 1) / length > CELLS_PER_AXIS_MAX)
                length <<= 1;

            log2Length = 0;
            while ((1 << log2Length) < length)
                log2Length++;
            return length;
        }

        private static float DistanceToBox(Vector3 p, Vector3 boxMin, Vector3 boxMax)
        {
            float dx = Math.Max(Math.Max(boxMin.X - p.X, 0f), p.X - boxMax.X);
            float dy = Math.Max(Math.Max(boxMin.Y - p.Y, 0f), p.Y - boxMax.Y);
            float dz = Math.Max(Math.Max(boxMin.Z - p.Z, 0f), p.Z - boxMax.Z);
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static bool IsBoxInFrustum(Vector3 boxMin, Vector3 boxMax)
        {
            Vector3 halfSize = (boxMax - boxMin) * 0.5f;
            return _frustum.VolumeVsFrustum(boxMin + halfSize, halfSize.X, halfSize.Y, halfSize.Z);
        }
    }
}
