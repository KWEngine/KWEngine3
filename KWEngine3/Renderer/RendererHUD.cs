using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace KWEngine3.Renderer
{
    internal static class RendererHUD
    {
        public static int ProgramID { get; private set; } = -1;
        public static int UModelMatrix { get; private set; } = -1;
        public static int UViewProjectionMatrix { get; private set; } = -1;
        public static int UTexture { get; private set; } = -1;
        public static int UColorTint { get; private set; } = -1;
        public static int UColorGlow { get; private set; } = -1;
        public static int UOffsets { get; private set; } = -1;
        public static int UOffsetCount { get; private set; } = -1;
        public static int UTextAlign { get; private set; } = -1;
        public static int UCursorInfo { get; private set; } = -1;
        public static int UCharacterDistance { get; private set; } = -1;
        public static int UMode { get; private set; } = -1;
        public static int UId { get; private set; } = -1;
        public static int UCharacterWidth { get; private set; } = -1;
        public static int UTextureRepeat { get; private set; } = -1;
        public static int UOptions { get; private set; } = -1;

        public static void Init()
        {
            if (ProgramID < 0)
            {
                ProgramID = GL.CreateProgram();

                string resourceNameVertexShader = "KWEngine3.Shaders.shader.hud.vert";
                string resourceNameFragmentShader = "KWEngine3.Shaders.shader.hud.frag";

                int vertexShader;
                int fragmentShader;
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
                {
                    vertexShader = HelperShader.LoadCompileAttachShader(s, ShaderType.VertexShader, ProgramID);
                }

                using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
                {
                    fragmentShader = HelperShader.LoadCompileAttachShader(s, ShaderType.FragmentShader, ProgramID);
                }

                GL.LinkProgram(ProgramID);
                UModelMatrix = GL.GetUniformLocation(ProgramID, "uModelMatrix");
                UCharacterDistance = GL.GetUniformLocation(ProgramID, "uCharacterDistance");
                UViewProjectionMatrix = GL.GetUniformLocation(ProgramID, "uViewProjectionMatrix");
                UTexture = GL.GetUniformLocation(ProgramID, "uTexture");
                UColorTint = GL.GetUniformLocation(ProgramID, "uColorTint");
                UColorGlow = GL.GetUniformLocation(ProgramID, "uColorGlow");
                UOffsets = GL.GetUniformLocation(ProgramID, "uOffsets");
                UOffsetCount = GL.GetUniformLocation(ProgramID, "uOffsetCount");
                UCharacterWidth = GL.GetUniformLocation(ProgramID, "uCharacterWidth");
                UTextAlign = GL.GetUniformLocation(ProgramID, "uTextAlign");
                UMode = GL.GetUniformLocation(ProgramID, "uMode"); // 0 = text, 1 = image, 2 = textInput, etc.
                UTextureRepeat = GL.GetUniformLocation(ProgramID, "uTextureRepeat");
                UOptions = GL.GetUniformLocation(ProgramID, "uOptions");
                UCursorInfo = GL.GetUniformLocation(ProgramID, "uCursorInfo");
            }
        }

        public static void Bind()
        {
            GL.UseProgram(ProgramID);
        }


        public static void SetGlobals()
        {
            // ?
        }

        public static int RenderHUDObjects(int index, bool back)
        {
            GL.Viewport(0, 0, KWEngine.Window.ClientSize.X, KWEngine.Window.ClientSize.Y);
            GeoMesh mesh = KWEngine.GetModel("KWQuad").Meshes.Values.ElementAt(0);
            if (back)
            {
                for (int i = 0; i < KWEngine.CurrentWorld._hudObjects.Count; i++)
                {
                    HUDObject h = KWEngine.CurrentWorld._hudObjects[i];
                    if (h._zIndex > -2f)
                    {
                        break;
                    }
                    Draw(h, mesh);
                    index++;
                }
                return index;
            }
            else
            {
                // Render the rest of the HUDObject instances:
                for (int i = index; i < KWEngine.CurrentWorld._hudObjects.Count; i++)
                {
                    HUDObject h = KWEngine.CurrentWorld._hudObjects[i];
                    Draw(h, mesh);
                    index++;
                }
                return -1;
            }
        }

        public static void DrawMap()
        {
            GL.Viewport(
                KWEngine.CurrentWorld.Map._targetCenter.X - KWEngine.CurrentWorld.Map._targetDimensions.X / 2,
                (KWEngine.Window.ClientSize.Y - KWEngine.CurrentWorld.Map._targetCenter.Y) - KWEngine.CurrentWorld.Map._targetDimensions.Y / 2,
                KWEngine.CurrentWorld.Map._targetDimensions.X,
                KWEngine.CurrentWorld.Map._targetDimensions.Y
                );

            // Render map entries:
            GeoMesh meshMap = KWEngine.CurrentWorld.Map._direction == ProjectionDirection.NegativeY ? KWEngine.KWMapItemXZ.Meshes.Values.ElementAt(0) : KWEngine.KWMapItemXY.Meshes.Values.ElementAt(0);
            Array.Sort(KWEngine.CurrentWorld.Map._items);
            if(KWEngine.CurrentWorld.Map._background != null)
            {
                DrawMapBackground(KWEngine.CurrentWorld.Map._background, meshMap);
            }
            for (int i = 0; i < KWEngine.CurrentWorld.Map._indexFree; i++)
            {
                if (KWEngine.CurrentWorld.Map._items[i]._go != null)
                {
                    DrawMapModel(KWEngine.CurrentWorld.Map._items[i]);
                }
                else
                {
                    DrawMapItem(KWEngine.CurrentWorld.Map._items[i], meshMap);
                }
                KWEngine.CurrentWorld.Map._items[i]._go = null;
            }
        }

        public static void DrawMapItem(HUDObjectMap ho, GeoMesh mesh)
        {
            if (ho == null || ho._color.W <= 0)
                return;

            GL.Uniform4(UColorTint, ho._color);
            GL.Uniform4(UColorGlow, ho._colorE);
            GL.Uniform2(UTextureRepeat, ho._textureRepeat);
            GL.Uniform1(UMode, 2);
            GL.Uniform1(UOptions, KWEngine.CurrentWorld.Map._drawAsCircle ? 1 : 0);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            Matrix4 tmp = KWEngine.CurrentWorld.Map._camView * KWEngine.CurrentWorld.Map._camPreRotation * KWEngine.CurrentWorld.Map._camProjection;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref tmp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);

            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public static void DrawMapModel(HUDObjectMap ho)
        {
            if (ho == null || ho._color.W <= 0)
                return;

            GL.Uniform4(UColorTint, ho._color);
            GL.Uniform4(UColorGlow, ho._colorE);
            GL.Uniform2(UTextureRepeat, ho._textureRepeat);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);

            GeoMesh[] meshes = ho._go._model.ModelOriginal.Meshes.Values.ToArray();
            for (int i = 0; i < meshes.Length; i++)
            {
                GeoMesh mesh = meshes[i];
                GeoMaterial material = ho._go._model.Material[i];

                if (material.ColorAlbedo.W == 0)
                    continue;

                GL.UniformMatrix4(UModelMatrix, false, ref ho._go._stateRender._modelMatrices[i]);
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
        }

        public static void DrawMapBackground(HUDObjectMap ho, GeoMesh mesh)
        {
            GL.Uniform4(UColorTint, ho._color);
            GL.Uniform4(UColorGlow, ho._colorE);
            GL.Uniform2(UTextureRepeat, ho._textureRepeat);
            GL.Uniform1(UMode, 2);
            GL.Uniform1(UOptions, KWEngine.CurrentWorld.Map._drawAsCircle ? 1 : 0);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            Matrix4 tmp = KWEngine.CurrentWorld.Map._camView * KWEngine.CurrentWorld.Map._camPreRotation * KWEngine.CurrentWorld.Map._camProjection;
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref tmp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);

            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public static void Draw(HUDObject ho, GeoMesh mesh)
        {
            if (ho == null || !ho.IsVisible || !ho.IsInsideScreenSpace())
                return;

            Vector2 txR = (ho is HUDObjectImage) ? (ho as HUDObjectImage)._textureRepeat : Vector2.One;

            GL.Uniform4(UColorTint, ho._tint);
            GL.Uniform4(UColorGlow, ho._glow);
            GL.Uniform2(UTextureRepeat, txR);
            GL.Uniform1(UOptions, 0);
            GL.Uniform3(UCursorInfo, 0f, 0f, 0f);
            GL.UniformMatrix4(UModelMatrix, false, ref ho._modelMatrix);
            GL.UniformMatrix4(UViewProjectionMatrix, false, ref KWEngine.Window._viewProjectionMatrixHUDNew);
            GL.BindVertexArray(mesh.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);

            if (ho is HUDObjectText)
            {
                DrawText(ho as HUDObjectText, mesh);
            }
            else if(ho is HUDObjectImage)
            {
                DrawImage(ho as HUDObjectImage, mesh);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        

        internal static void DrawText(HUDObjectText ho, GeoMesh mesh)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._font.Texture);
            GL.Uniform1(UTexture, 0);
            GL.Uniform1(UMode, 0);
            GL.Uniform1(UOptions, 0);
            GL.Uniform1(UOffsets, ho._offsets.Length, ho._offsets);
            GL.Uniform1(UOffsetCount, ho._offsets.Length);
            GL.Uniform1(UTextAlign, (int)ho.TextAlignment);
            GL.Uniform1(UCharacterWidth, ho._scale.X);
            GL.Uniform1(UCharacterDistance, ho._spread);

            GL.DrawElementsInstanced(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, ho._offsets.Length);

            if(ho is HUDObjectTextInput)
            {
                HUDObjectTextInput i = (HUDObjectTextInput)ho;
                if(i.HasFocus && i._offsetsCursor != null)
                {
                    GL.Uniform1(UMode, i.CursorType == KeyboardCursorType.Pipe ? 3 : i.CursorType == KeyboardCursorType.Underscore ? 4 : 5);
                    GL.Uniform3(UCursorInfo, (float)i.CursorBehaviour, KWEngine.CurrentWorld.WorldTime, i.CursorBlinkSpeed);
                    GL.Uniform1(UOffsets, i._offsetsCursor.Length, i._offsetsCursor);
                    GL.Uniform1(UOffsetCount, i._offsetsCursor.Length);
                    GL.DrawElementsInstanced(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, i._offsetsCursor.Length);
                }
            }
        }

        internal static void DrawImage(HUDObjectImage ho, GeoMesh mesh)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ho._textureId);
            GL.Uniform1(UTexture, 0);
            GL.Uniform1(UMode, 1);
            GL.Uniform1(UOptions, 0);
            GL.Uniform1(UOffsets, 0, _arrayEmptyInt32);
            GL.Uniform1(UOffsetCount, 0);
            GL.Uniform1(UTextAlign, 0);
            GL.Uniform1(UCharacterWidth, ho._scale.X);
            GL.Uniform1(UCharacterDistance, 1f);

            GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        internal static int[] _arrayEmptyInt32 = new int[0];
    }
}
