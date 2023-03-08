using KWEngine3.EngineCamera;
using KWEngine3.Exceptions;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KWEngine3
{
    public abstract class World
    {
        #region Internals
        internal ViewSpaceGameObject _viewSpaceGameObject = null;

        internal List<GameObject> _gameObjects = new List<GameObject>();
        internal List<GameObject> _gameObjectsToBeAdded = new List<GameObject>();
        internal List<GameObject> _gameObjectsToBeRemoved = new List<GameObject>();

        internal List<TerrainObject> _terrainObjects = new List<TerrainObject>();
        internal List<TerrainObject> _terrainObjectsToBeAdded = new List<TerrainObject>();
        internal List<TerrainObject> _terrainObjectsToBeRemoved = new List<TerrainObject>();

        internal List<LightObject> _lightObjects = new List<LightObject>();
        internal List<LightObject> _lightObjectsToBeAdded = new List<LightObject>();
        internal List<LightObject> _lightObjectsToBeRemoved = new List<LightObject>();

        internal List<HUDObject> _hudObjects = new List<HUDObject>();
        internal List<HUDObject> _hudObjectsToBeAdded = new List<HUDObject>();
        internal List<HUDObject> _hudObjectsToBeRemoved = new List<HUDObject>();

        internal List<TimeBasedObject> _particleAndExplosionObjects = new List<TimeBasedObject>();

        internal Queue<ushort> _availableGameObjectIDs = new Queue<ushort>();
        internal Queue<ushort> _availableLightObjectIDs = new Queue<ushort>();

        internal Dictionary<string, int> _customTextures = new Dictionary<string, int>();

        internal Camera _cameraGame;
        internal Camera _cameraEditor;
        internal Vector2 _mouseGrabDelta = Vector2.Zero;

        internal Vector2 _xMinMax = new Vector2(float.MaxValue, float.MinValue);
        internal Vector2 _yMinMax = new Vector2(float.MaxValue, float.MinValue);
        internal Vector2 _zMinMax = new Vector2(float.MaxValue, float.MinValue);
        internal Vector3 _worldCenter = new Vector3(0);

        internal Vector3 _colorAmbient = new Vector3(0.75f, 0.75f, 0.75f);
        internal WorldBackground _background = new WorldBackground();


        internal void ResetWorldDimensions()
        {
            _xMinMax = new Vector2(float.MaxValue, float.MinValue);
            _yMinMax = new Vector2(float.MaxValue, float.MinValue);
            _zMinMax = new Vector2(float.MaxValue, float.MinValue);
        }

        internal GameObject BuildAndAddDefaultGameObject(string classname)
        {
            GameObject g = (GameObject)Assembly.GetEntryAssembly().CreateInstance(classname);
            AddGameObject(g);
            return g;
        }

        internal void UpdateWorldDimensions(Vector3 center, Vector3 dimensions)
        {
            if (center.X - dimensions.X / 2 < _xMinMax.X)
                _xMinMax.X = center.X - dimensions.X / 2;
            if (center.X + dimensions.X / 2 > _xMinMax.Y)
                _xMinMax.Y = center.X + dimensions.X / 2;

            if (center.Y - dimensions.Y / 2 < _yMinMax.X)
                _yMinMax.X = center.Y - dimensions.Y / 2;
            if (center.Y + dimensions.Y / 2 > _yMinMax.Y)
                _yMinMax.Y = center.Y + dimensions.Y / 2;

            if (center.Z - dimensions.Z / 2 < _zMinMax.X)
                _zMinMax.X = center.Z - dimensions.Z / 2;
            if (center.Z + dimensions.Z / 2 > _zMinMax.Y)
                _zMinMax.Y = center.Z + dimensions.Z / 2;

            _worldCenter = new Vector3((_xMinMax.X + _xMinMax.Y) / 2f, (_yMinMax.X + _yMinMax.Y) / 2f, (_zMinMax.X + _zMinMax.Y) / 2f);
        }

        internal void Init()
        {
            _availableGameObjectIDs.Clear();
            _availableLightObjectIDs.Clear();
            for (ushort i = 1; i < ushort.MaxValue; i++)
            {
                _availableLightObjectIDs.Enqueue(i);
                _availableGameObjectIDs.Enqueue(i);
            }
            Window.MousePosition = Window.ClientSize / 2;
            _cameraGame = new Camera();
            _cameraEditor = _cameraGame;
        }
        internal BackgroundType BackgroundTextureType { get { return _background.Type; } }

        internal int _preparedLightsCount = 0;
        internal float[] _preparedLightsArray = new float[KWEngine.MAX_LIGHTS * KWEngine.LIGHTINDEXDIVIDER];
        internal List<int> _preparedTex2DIndices = new List<int>();
        internal List<int> _preparedCubeMapIndices = new List<int>();
        internal List<LightObject> _currentShadowLights = new List<LightObject>();

        internal void Export()
        {
            HelperImportExport.ExportWorld(this);
        }

        internal void AddRemoveTerrainObjects()
        {
            foreach (TerrainObject g in _terrainObjectsToBeRemoved)
            {
                _availableGameObjectIDs.Enqueue((ushort)g.ID);
                g.ID = 0;

                _terrainObjects.Remove(g);
            }
            _terrainObjectsToBeRemoved.Clear();

            foreach (TerrainObject g in _terrainObjectsToBeAdded)
            {
                _terrainObjects.Add(g);
            }
            _terrainObjectsToBeAdded.Clear();
        }

        internal List<TerrainObject> GetTerrainObjects()
        {
            return _terrainObjects;
        }

        internal LightObject BuildAndAddDefaultLightObject(string lightType, ShadowQuality quality)
        {
            //"Point light", "Directional light", "Sun light"
            LightType t;
            if (lightType == "Point light")
            {
                t = LightType.Point;
            }
            else if (lightType == "Directional light")
            {
                t = LightType.Directional;
            }
            else
            {
                t = LightType.Sun;
            }
            LightObject l = new LightObject(t, quality);
            l.SetPosition(0, 0, 0);
            l.SetTarget(0, -1, 0);
            AddLightObject(l);
            return l;
        }

        internal void AddRemoveHUDObjects()
        {
            foreach (HUDObject h in _hudObjectsToBeRemoved)
            {
                _hudObjects.Remove(h);
            }
            _hudObjectsToBeRemoved.Clear();

            foreach (HUDObject h in _hudObjectsToBeAdded)
            {
                _hudObjects.Add(h);
            }
            _hudObjectsToBeAdded.Clear();
        }

        internal void AddRemoveGameObjects()
        {
            foreach (GameObject g in _gameObjectsToBeRemoved)
            {
                _availableGameObjectIDs.Enqueue((ushort)g.ID);
                g.ID = 0;

                _gameObjects.Remove(g);
            }
            _gameObjectsToBeRemoved.Clear();

            foreach (GameObject g in _gameObjectsToBeAdded)
            {
                _gameObjects.Add(g);
            }
            _gameObjectsToBeAdded.Clear();
        }

        internal void Dispose()
        {
            HelperGeneral.CheckGLErrors();
            foreach (LightObject l in _lightObjectsToBeAdded)
            {
                l.DeleteShadowMap();
            }
            _lightObjectsToBeAdded.Clear();

            foreach (LightObject l in _lightObjects)
            {
                RemoveLightObject(l);
            }
            AddRemoveLightObjects();
            Framebuffer._fbShadowMapCounter = 0;

            foreach(GameObject g in _gameObjects)
            {
                RemoveGameObject(g);
            }
            AddRemoveGameObjects();

            foreach(TerrainObject t in _terrainObjects)
            {
                RemoveTerrainObject(t);
            }
            AddRemoveTerrainObjects();

            foreach (HUDObject h in _hudObjects)
            {
                RemoveHUDObject(h);
            }
            AddRemoveHUDObjects();

            KWEngine.DeleteCustomModelsAndTextures(this);
        }

        internal List<GameObject> GetTransparentGameObjects()
        {
            List<GameObject> list = new List<GameObject>();
            foreach (GameObject g in _gameObjects)
            {
                if (g.IsTransparent)
                    list.Add(g);
            }
            return list;
        }

        internal void PrepareLightObjectsForRenderPass()
        {
            int offset = 0;
            int cubemapIndex = -1;
            int tex2dIndex = 1;
            _preparedTex2DIndices.Clear();
            _preparedCubeMapIndices.Clear();
            _currentShadowLights.Clear();
            _preparedLightsCount = 0;
            foreach (LightObject l in _lightObjects)
            {
                if (!l.IsInsideScreenSpace)
                    continue;

                // 00-03 = position and shadow map texture index (vec4)
                _preparedLightsArray[offset + 00] = l._stateRender._position.X;
                _preparedLightsArray[offset + 01] = l._stateRender._position.Y;
                _preparedLightsArray[offset + 02] = l._stateRender._position.Z;
                if (l.ShadowCasterType == ShadowQuality.NoShadow)
                {
                    _preparedLightsArray[offset + 03] = 0f;
                }
                else if (l.Type == LightType.Point)
                {
                    _currentShadowLights.Add(l);
                    _preparedCubeMapIndices.Add(offset / KWEngine.LIGHTINDEXDIVIDER);
                    _preparedLightsArray[offset + 03] = cubemapIndex--;
                }
                else
                {
                    _currentShadowLights.Add(l);
                    _preparedTex2DIndices.Add(offset / KWEngine.LIGHTINDEXDIVIDER);
                    _preparedLightsArray[offset + 03] = tex2dIndex++;
                }
                // 04-06 = lookatvector (vec3)
                _preparedLightsArray[offset + 04] = l._stateRender._lookAtVector.X;
                _preparedLightsArray[offset + 05] = l._stateRender._lookAtVector.Y;
                _preparedLightsArray[offset + 06] = l._stateRender._lookAtVector.Z;
                // 07-10 = color (rgb, intensity) (vec4)
                _preparedLightsArray[offset + 07] = l._stateRender._color.X;
                _preparedLightsArray[offset + 08] = l._stateRender._color.Y;
                _preparedLightsArray[offset + 09] = l._stateRender._color.Z;
                _preparedLightsArray[offset + 10] = l._stateRender._color.W;
                // 11-14 = near, far, fov, type (vec4)
                _preparedLightsArray[offset + 11] = l._stateRender._nearFarFOVType.X;
                _preparedLightsArray[offset + 12] = l._stateRender._nearFarFOVType.Y;
                _preparedLightsArray[offset + 13] = l._stateRender._nearFarFOVType.Z;
                _preparedLightsArray[offset + 14] = l._stateRender._nearFarFOVType.W;

                _preparedLightsArray[offset + 15] = l._shadowBias;
                _preparedLightsArray[offset + 16] = l._shadowOffset;

                offset += KWEngine.LIGHTINDEXDIVIDER;
                _preparedLightsCount++;
            }
        }

        internal void AddRemoveLightObjects()
        {
            foreach (LightObject l in _lightObjectsToBeRemoved)
            {
                _availableLightObjectIDs.Enqueue((ushort)Math.Abs(l.ID));
                l.ID = 0;
                l.DeleteShadowMap();

                _lightObjects.Remove(l);
            }
            _lightObjectsToBeRemoved.Clear();

            foreach (LightObject l in _lightObjectsToBeAdded)
            {
                _lightObjects.Add(l);
            }
            _lightObjectsToBeAdded.Clear();
        }
        #endregion

        public void SetColorAmbient(float r, float g, float b)
        {
            SetColorAmbient(new Vector3(r, g, b));
        }

        public void SetColorAmbient(Vector3 a)
        {
            _colorAmbient = new Vector3(
                    MathHelper.Clamp(a.X, 0f, 1f),
                    MathHelper.Clamp(a.Y, 0f, 1f),
                    MathHelper.Clamp(a.Z, 0f, 1f)
                );
        }

        public bool IsViewSpaceGameObjectAttached { get { return _viewSpaceGameObject != null && _viewSpaceGameObject.IsValid; } }

        public void SetViewSpaceGameObject(ViewSpaceGameObject vsg)
        {
            if(vsg == null || !vsg.IsValid)
            {
                KWEngine.LogWriteLine("[World] view space game object now unset");
            }
            else
            {
                _viewSpaceGameObject = vsg;
            }
        }

        public Vector3 GetViewSpaceGameObjectPosition()
        {
            if(IsViewSpaceGameObjectAttached)
            {
                return _viewSpaceGameObject._gameObject.Center;
            }
            KWEngine.LogWriteLine("[World] No view space game object found");
            return Vector3.Zero;
        }

        public void SetBackgroundSkybox(string filename, float rotationY = 0f)
        {
            if (filename == null || filename.Length == 0)
            {
                _background.Unset();
                return;
            }
            _background.SetSkybox(filename, rotationY);
        }

        public void SetBackgroundBrightnessMultiplier(float m)
        {
            _background.SetBrightnessMultiplier(m);
        }
        public void SetBackground2D(string filename)
        {
            if (filename == null || filename.Length == 0)
            {
                _background.Unset();
                return;
            }
            _background.SetStandard(filename);
        }

        public void SetBackground2DOffset(float x, float y)
        {
            _background.SetOffset(x, y);
        }

        public void SetBackground2DRepeat(float x, float y)
        {
            _background.SetRepeat(x, y);
        }

        public void SetBackground2DClip(float x, float y)
        {
            _background.SetClip(x, y);
        }

        public void AddExplosionObject(ExplosionObject ex)
        {
            if (!_particleAndExplosionObjects.Contains(ex))
            {
                ex._starttime = WorldTime;
                _particleAndExplosionObjects.Add(ex);
            }
        }

        public void AddParticleObject(ParticleObject po)
        {
            if (!_particleAndExplosionObjects.Contains(po))
            {
                po._starttime = WorldTime;
                _particleAndExplosionObjects.Add(po);
            }
        }

        public void AddGameObject(GameObject g)
        {
            if (!_gameObjects.Contains(g) && !_gameObjectsToBeAdded.Contains(g))
            {
                g.ID = _availableGameObjectIDs.Dequeue();
                _gameObjectsToBeAdded.Add(g);
            }
        }

        public void RemoveGameObject(GameObject g)
        {
            if (!_gameObjectsToBeRemoved.Contains(g))
                _gameObjectsToBeRemoved.Add(g);
        }

        public void AddLightObject(LightObject l)
        {
            if (_lightObjects.Count + _lightObjectsToBeAdded.Count < KWEngine.MAX_LIGHTS)
            {
                if (!_lightObjects.Contains(l) && !_lightObjectsToBeAdded.Contains(l))
                {
                    l.ID = _availableLightObjectIDs.Dequeue() * -1;
                    _lightObjectsToBeAdded.Add(l);
                    if(l.ShadowCasterType != ShadowQuality.NoShadow)
                        l.AttachShadowMap();
                }
            }
            else
            {
                KWEngine.LogWriteLine("LightObject ignored: Max. concurrent limit of " + KWEngine.MAX_LIGHTS + " lights per world has been reached.");
            }
        }

        public void RemoveLightObject(LightObject l)
        {
            if (!_lightObjectsToBeRemoved.Contains(l))
                _lightObjectsToBeRemoved.Add(l);
        }

        public void AddTerrainObject(TerrainObject t)
        {
            if (!_terrainObjects.Contains(t) && !_terrainObjectsToBeAdded.Contains(t))
            {
                t.ID = _availableGameObjectIDs.Dequeue();
                _terrainObjectsToBeAdded.Add(t);
            }
        }

        public void RemoveTerrainObject(TerrainObject t)
        {
            if (!_terrainObjectsToBeRemoved.Contains(t))
                _terrainObjectsToBeRemoved.Add(t);
        }

        public void AddHUDObject(HUDObject h)
        {
            if (!_hudObjects.Contains(h) && !_hudObjectsToBeAdded.Contains(h))
                _hudObjectsToBeAdded.Add(h);
        }

        public void RemoveHUDObject(HUDObject h)
        {
            if (!_hudObjectsToBeRemoved.Contains(h))
                _hudObjectsToBeRemoved.Add(h);
        }

        public void SetCameraPosition(float x, float y, float z, float offsetY = 0f)
        {
            SetCameraPosition(new Vector3(x, y, z), offsetY);
        }

        public void SetCameraPosition(Vector3 position, float offsetY = 0f)
        {
            if (KWEngine.Mode == EngineMode.Play)
            {
                _cameraGame.SetPosition(position + new Vector3(0, offsetY, 0));
            }
            else
            {
                _cameraEditor.SetPosition(position);
            }
        }

        public void SetCameraTarget(float x, float y, float z)
        {
            SetCameraTarget(new Vector3(x, y, z));
        }

        public void SetCameraTarget(Vector3 target)
        {
            if (KWEngine.Mode == EngineMode.Play)
            {
                _cameraGame.SetTarget(target);
            }
            else
            {
                _cameraEditor.SetTarget(target);
            }
        }

        public void SetCameraToFirstPersonGameObject(GameObject g, float offsetY = 0f)
        {
            _cameraGame.degX = 0;
            _cameraGame.degY = 0;
            _cameraGame.AdjustToGameObject(g, offsetY);
        }

        public void AddCameraRotation(Vector2 yawPitch)
        {
            if(_mouseGrabDelta != Vector2.Zero)
            {
                if (Mouse.Delta != Vector2.Zero)
                {
                    return;
                }
                else
                {
                    _mouseGrabDelta = Vector2.Zero;
                    return;
                }
            }
            _cameraGame.YawAndPitch(yawPitch * KWEngine.MouseSensitivity);
        }

        /// <summary>
        /// Erstellt eine Liste aller GameObject-Instanzen mit einem bestimmten Namen
        /// </summary>
        /// <param name="name">gesuchter Name</param>
        /// <returns>Liste der gefundenen Instanzen</returns>
        public List<GameObject> GetGameObjectsByName(string name)
        {
            name = name.Trim();
            List<GameObject> os = _gameObjects.FindAll(go => go.Name == name);
            return os;
        }

        /// <summary>
        /// Durchsucht die Liste der GameObject-Instanzen nach Objekten des gegebenen Typs mit dem gegebenen Namen
        /// </summary>
        /// <typeparam name="T">Klassenname</typeparam>
        /// <param name="name">Name der gesuchten Objekte</param>
        /// <returns>Liste der gefundenen Objekte</returns>
        public List<T> GetGameObjectsByName<T>(string name) where T : class
        {
            name = name.Trim();
            List<T> os = new List<T>();
            var list = _gameObjects.FindAll(go => go is T && go.Name == name);
            if (list.Count > 0)
            {
                foreach (object o in list)
                {
                    os.Add((T)o);
                }
            }
            return os;
        }

        /// <summary>
        /// Durchsucht die Liste der GameObject-Instanzen nach Objekten des gegebenen Typs
        /// </summary>
        /// <typeparam name="T">Klassenname</typeparam>
        /// <returns>Liste der gefundenen Objekte</returns>
        public List<T> GetGameObjectsByType<T>()
        {
            List<T> os = new List<T>();
            var list = _gameObjects.FindAll(go => go is T);
            if (list.Count > 0)
            {
                foreach (object o in list)
                {
                    os.Add((T)o);
                }
            }
            return os;
        }

        /// <summary>
        /// Durchsucht die Liste der GameObject-Instanzen nach einem Objekt des gegebenen Typs mit dem gegebenen Namen
        /// </summary>
        /// <typeparam name="T">Klasse des gesuchten Objekts</typeparam>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public T GetGameObjectByName<T>(string name) where T : class
        {
            name = name.Trim();
            GameObject g = _gameObjects.FirstOrDefault(go => go is T && go.Name == name);
            if (g != null)
            {
                return (T)(object)g;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Durchsucht die Liste der GameObject-Instanzen nach einem Objekt mit dem gegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public GameObject GetGameObjectByName(string name)
        {
            name = name.Trim();
            GameObject g = _gameObjects.FirstOrDefault(go => go.Name == name);
            return g;
        }

        /// <summary>
        /// Durchsucht die Liste der LightObject-Instanzen nach einem Objekt mit dem gegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public LightObject GetLightObjectByName(string name)
        {
            name = name.Trim();
            LightObject l = _lightObjects.FirstOrDefault(lo => lo.Name == name);
            return l;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach einem Objekt mit dem gegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public HUDObject GetHUDObjectByName(string name)
        {
            name = name.Trim();
            HUDObject h = _hudObjects.FirstOrDefault(ho => ho.Name == name);
            return h;
        }

        public float GameTime { get { return KWEngine.ApplicationTime; } }
        public float WorldTime { get { return KWEngine.WorldTime; } }

        public KeyboardState Keyboard { get { return KWEngine.Window.KeyboardState; } }
        public MouseState Mouse { get { return KWEngine.Window.MouseState; } }
        public GLWindow Window { get { return KWEngine.Window; } }

        public Vector3 CameraLookAtVector { get { return _cameraGame._stateCurrent.LookAtVector; } }
        public Vector3 CameraLookAtVectorLocalUp { get { return _cameraGame._stateCurrent.LookAtVectorLocalUp; } }
        public Vector3 CameraLookAtVectorLocalRight { get { return _cameraGame._stateCurrent.LookAtVectorLocalRight; } }
        public Vector3 CameraPosition { get { return _cameraGame._stateCurrent._position; } }
        public Vector3 CameraTarget { get { return _cameraGame._stateCurrent._target; } }

        public void MouseCursorGrab()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;
            Window.MousePosition = Window.ClientSize / 2;
            _mouseGrabDelta = Mouse.Delta;
        }
        public void MouseCursorHide()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Hidden;
        }
        public void MouseCursorReset()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
        }
        public void MouseCursorResetPosition()
        {
            Window.MousePosition = Window.ClientRectangle.HalfSize;
        }

        public void SetFOV(int fov)
        {
            _cameraGame.SetFOVForPerspectiveProjection(fov);
        }

        public IReadOnlyCollection<GameObject> GetGameObjects()
        {
            return _gameObjects.AsReadOnly();
        }

        public IReadOnlyCollection<LightObject> GetLightObjects()
        {
            return _lightObjects.AsReadOnly();
        }

        public void LoadJSON(string filename, [CallerMemberName] string callerName = "")
        {
            if(callerName != "Prepare")
            {
                KWEngine.LogWriteLine("[World] LoadJSON only allowed in Prepare()");
                return;
            }
            if (filename == null)
                filename = "";
            else
                filename = filename.Trim();

            if (File.Exists(filename))
            {
                HelperImportExport.ImportWorld(this, filename);
            }
            else
            {
                KWEngine.LogWriteLine("[World] " + filename + " does not exist");
            }
        }

        public abstract void Prepare();
        public abstract void Act();
    }
}
