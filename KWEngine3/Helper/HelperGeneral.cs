using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using KWEngine3.GameObjects;
using System.Diagnostics;
using System.Reflection;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Mathefunktionen
    /// </summary>
    public static class HelperGeneral
    {

        internal static int GetTextureStorageSize()
        {
            int bytes = 0;

            foreach(KeyValuePair<string, KWTexture> entry in KWEngine.CurrentWorld._customTextures)
            {
                int currentSizeInBytes = 0;
                GL.BindTexture(entry.Value.Target, entry.Value.ID);
                CheckGLErrors();


                // mipmaps?
                GL.GetTexParameter(entry.Value.Target, GetTextureParameter.TextureMaxLevel, out int mipmapCount);
                CheckGLErrors();
                if (entry.Value.Target == TextureTarget.Texture2D)
                {
                    // compression?
                    GL.GetTexLevelParameter(entry.Value.Target, 0, GetTextureParameter.TextureCompressed, out int compressed);
                    CheckGLErrors();
                    if (compressed > 0)
                    {
                        GL.GetTexLevelParameter(entry.Value.Target, 0, GetTextureParameter.TextureCompressedImageSize, out currentSizeInBytes);
                        CheckGLErrors();
                    }
                    else
                    {
                        // get resolution:
                        GL.GetTexLevelParameter(entry.Value.Target, 0, GetTextureParameter.TextureWidth, out int width);
                        CheckGLErrors();
                        GL.GetTexLevelParameter(entry.Value.Target, 0, GetTextureParameter.TextureHeight, out int height);
                        CheckGLErrors();
                        int pixels = width * height;

                        // get number of color channels:
                        GL.GetTexParameter(entry.Value.Target, GetTextureParameter.TextureInternalFormat, out int format);
                        CheckGLErrors();
                    }
                }
                else if(entry.Value.Target == TextureTarget.TextureCubeMap)
                {

                }
                GL.BindTexture(entry.Value.Target, 0);
            }

            return bytes;
        }
        internal static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                var debuggableAttribute = attribute as DebuggableAttribute;
                if (debuggableAttribute != null)
                {
                    return debuggableAttribute.IsJITTrackingEnabled;
                }
            }
            return false;
        }

        internal static string EqualizePathDividers(string s)
        {
            if (s != null)
            {
                s = s.Replace(@"\\", @"\");
                s = s.Replace(@"\", "/");
            }
            else
            {
                s = "";
            }
            return s;
        }

        internal static bool IsObjectClassOfType<T>(GameObject g)
        {
            Type tType = typeof(T);
            Type gType = g.GetType();
            bool result = tType == gType;
            return result;
        }

        internal static bool IsObjectClassOrSubclassOfType<T>(GameObject g)
        {
            Type gType = g.GetType();
            Type tType = typeof(T);
            return tType == gType || gType.IsSubclassOf(tType);
        }

        internal static bool IsObjectClassOrSubclassOfTypes(Type[] typelist, GameObject g)
        {
            Type gt = g.GetType();
            foreach(Type t in typelist)
            {
                if (t == gt || gt.IsSubclassOf(t))
                    return true;
            }
            return false;
        }

        internal static bool IsTypeClassOrSubclassOfGameObject(Type t)
        {
            Type gt = typeof(GameObject);
            return t == gt || t.IsSubclassOf(gt);
        }

        /// <summary>
        /// Prüft, ob der Dateiname darauf hindeutet, dass ein 3D-Modell in der Datei gespeichert ist
        /// </summary>
        /// <param name="file">Dateiname (inkl. Dateiendung)</param>
        /// <returns>true, wenn es sich um eine Datei mit 3D-Modell handelt</returns>
        public static bool IsModelFile(string file)
        {
            string ext = file.Substring(file.LastIndexOf('.') + 1).ToLower();
            switch(ext)
            {
                case "gltf":
                    return true;
                case "glb":
                    return true;
                case "fbx":
                    return true;
                case "obj":
                    return true;
                case "dae":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Prüft, ob der Dateiname darauf hindeutet, dass ein 3D-Modell in der Datei gespeichert ist
        /// </summary>
        /// <param name="file">Datei als FileInfo-Objekt</param>
        /// <returns>true, wenn es sich um eine Datei mit 3D-Modell handelt</returns>
        public static bool IsModelFile(FileInfo file)
        {
            string ext = file.Extension.ToLower();
            switch (ext)
            {
                case "gltf":
                    return true;
                case "glb":
                    return true;
                case "fbx":
                    return true;
                case "obj":
                    return true;
                case "dae":
                    return true;
                default:
                    return false;
            }
        }

        internal static Vector3 ToVector3(byte[] ar, int start, int length)
        {

            byte[] data = ar;//= { 1, 0, 0, 0, 9, 8, 7 }; // IntValue = 1, Array = {9,8,7}
            IntPtr ptPoit = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, start, ptPoit, length);

            
            Vector3 x = (Vector3)Marshal.PtrToStructure(ptPoit, typeof(Vector3));
            Marshal.FreeHGlobal(ptPoit);

            return x;
        }

        internal static Quaternion ToQuaternion(byte[] ar, int start, int length)
        {

            byte[] data = ar;
            IntPtr ptPoit = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, start, ptPoit, length);


            Quaternion x = (Quaternion)Marshal.PtrToStructure(ptPoit, typeof(Quaternion));
            Marshal.FreeHGlobal(ptPoit);

            return x;
        }

        internal static Vector3[] ConvertByteArrayToVector3List(Span<byte> myArray)
        {
            return MemoryMarshal.Cast<byte, Vector3>(myArray).ToArray();
        }

        internal static Quaternion[] ConvertByteArrayToQuaternionList(Span<byte> myArray)
        {
            return MemoryMarshal.Cast<byte, Quaternion>(myArray).ToArray();
        }

        internal static float[] ConvertByteArrayToFloatList(Span<byte> myArray)
        {
            return MemoryMarshal.Cast<byte, float>(myArray).ToArray();
        }

        internal static T CastToStruct<T>(this byte[] data) where T : struct
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
            pData.Free();
            return result;
        }

        internal static Vector3 Get3DMouseCoords(Vector2 mc)
        {
            mc.X = (2.0f * mc.X) / KWEngine.Window.ClientRectangle.Size.X - 1.0f;
            mc.Y = 1.0f - (2.0f * mc.Y) / KWEngine.Window.ClientRectangle.Size.Y;
            Vector4 ray_clip = new Vector4(mc.X, mc.Y, -1, 1);
            Matrix4 viewInv = Matrix4.Invert(KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewMatrix);

            Vector4.TransformRow(ray_clip, Matrix4.Invert(KWEngine.CurrentWorld._cameraGame._stateCurrent.ProjectionMatrix), out Vector4 ray_eye);
            ray_eye.Z = -1;
            ray_eye.W = 0;
            Vector4.TransformRow(ray_eye, viewInv, out Vector4 ray_world);
            Vector3 ray_world3 = Vector3.NormalizeFast(ray_world.Xyz);
            return ray_world3;
        }

        internal static void FlushAndFinish()
        {
            GL.Flush();
            GL.Finish();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        }

        internal static void SwitchToBufferAndClear(int id, ClearColorMode mode = ClearColorMode.ZeroZeroZeroOne)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            if (mode == ClearColorMode.ZeroZeroZeroOne)
            {
                GL.ClearColor(0f, 0f, 0f, 1f);
            }
            else if(mode == ClearColorMode.OneZeroZeroZero)
            {
                GL.ClearColor(1f, 0f, 0f, 0f);
            }
            else if(mode == ClearColorMode.AllZero)
            {
                GL.ClearColor(0f, 0f, 0f, 0f);
            }
            else if (mode == ClearColorMode.AllOne)
            {
                GL.ClearColor(1f, 1f, 1f, 1f);
            }
            else if(mode == ClearColorMode.OneOneOneZero)
            {
                GL.ClearColor(1f, 1f, 1f, 0f);
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Normalisiert einen Wert innerhalb eines Bereichs auf den Wertebereich 0 bis 1
        /// </summary>
        /// <param name="value">zu normalisierender Wert</param>
        /// <param name="rangeMin">Bereichsminimum</param>
        /// <param name="rangeMax">Bereichsmaximum</param>
        /// <returns></returns>
        public static float NormalizeValueToRange(float value, float rangeMin, float rangeMax)
        {
            float normalizedValue = (value - rangeMin) / (rangeMax - rangeMin);
            return normalizedValue;
        }

        /// <summary>
        /// Skaliert die Werte innerhalb eines float-Arrays, so dass sie in den durch lowerBound und upperBound angegebenen Bereich passen
        /// </summary>
        /// <param name="lowerBound">Untergrenze</param>
        /// <param name="upperBound">Obergrenze</param>
        /// <param name="array">Werte (Achtung: Werden in-place manipuliert!)</param>
        public static void ScaleToRange(int lowerBound, int upperBound, float[] array)
        {
            float max = array.Max();
            float min = array.Min();
            if (min < max)
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (upperBound - lowerBound) * ((array[i] - min) / (max - min)) + lowerBound;
                }
        }

        /// <summary>
        /// Skaliert einen Wert dahingehend, dass er in den durch lowerBound und upperBound angegebenen Bereich passt
        /// </summary>
        /// <param name="inputValue">zu skalierender Wert</param>
        /// <param name="inputLow">Minimalwert des nicht-skalierten Werts</param>
        /// <param name="inputHigh">Maximalwert des nicht-skalierten Werts</param>
        /// <param name="outputLow">Minimalwert des Ausgabewertebereichs</param>
        /// <param name="outputHigh">Maximalwert des Ausgabewertebereichs</param>
        /// <returns>skalierter Wert</returns>
        public static float ScaleToRange(float inputValue, float inputLow, float inputHigh, float outputLow, float outputHigh)
        {
            float val = (outputHigh - outputLow) * ((inputValue - inputLow) / (inputHigh - inputLow)) + outputLow;
            return val;
        }

        /// <summary>
        /// Beschneidet Werte
        /// </summary>
        /// <param name="v">Wert</param>
        /// <param name="l">Untergrenze</param>
        /// <param name="u">Obergrenze</param>
        /// <returns></returns>
        public static float Clamp(float v, float l, float u)
        {
            if (v < l)
                return l;
            else if (v > u)
                return u;
            else
                return v;
        }

        internal static bool CheckGLErrors()
        {
            bool hasError = false;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                hasError = true;
                KWEngine.LogWriteLine("[GL] " + c.ToString());
            }
            return hasError;
        }

        #region internals
        /*
        internal static Vector3 GetRayOriginForOrthographicProjection(Vector2 normalizedMouseCoords, GameWindow window)
        {
            float aspectRatio = (float)window.ClientRectangle.Size.X / (float)window.ClientRectangle.Size.Y;
            float xOffset = (2.0f * normalizedMouseCoords.X / (float)window.ClientRectangle.Size.X - 1) * (KWEngine.CurrentWorld.FOV / 2 * aspectRatio);
            float yOffset = (-(2.0f * normalizedMouseCoords.Y / (float)window.ClientRectangle.Size.Y - 1)) * (KWEngine.CurrentWorld.FOV / 2);
            Vector3 lookAt;
            Vector3 position;
            if (KWEngine.CurrentWorld.IsFirstPersonMode && KWEngine.CurrentWorld.GetFirstPersonObject() != null)
            {
                position = KWEngine.CurrentWorld.GetFirstPersonObject().Position + KWEngine.WorldUp * KWEngine.CurrentWorld.GetFirstPersonObject().FPSEyeOffset;
                throw new Exception("LAV");// lookAt = HelperCamera.GetLookAtVector();
            }
            else
            {
                position = KWEngine.CurrentWorld.GetCameraPosition();
                lookAt = KWEngine.CurrentWorld.GetCameraLookAtVector();
            }
            Vector3 cameraRight = Vector3.NormalizeFast(Vector3.Cross(lookAt, KWEngine.WorldUp));
            Vector3 cameraUp = Vector3.NormalizeFast(Vector3.Cross(cameraRight, lookAt));

            Vector3 rayOrigin = position + cameraRight * xOffset + cameraUp * yOffset;
            return rayOrigin;
        }
        */
        internal static Vector3 UnProject(this Vector3 mouse, Matrix4 projection, Matrix4 view, int width, int height)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float)width - 1;
            vec.Y = -(2.0f * mouse.Y / (float)height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;
            Matrix4 viewInv;
            Matrix4 projInv;
            try
            {
                viewInv = Matrix4.Invert(view);
                projInv = Matrix4.Invert(projection);
            }
            catch (Exception)
            {
                return Vector3.Zero;
            }

            Vector4.TransformRow(vec, projInv, out vec);
            Vector4.TransformRow(vec, viewInv, out vec);

            if (vec.W == 0)
            {
                vec.W += 0.000001f;
            }
            vec.X /= vec.W;
            vec.Y /= vec.W;
            vec.Z /= vec.W;
            return vec.Xyz;
        }
        #endregion
    }
   
}
