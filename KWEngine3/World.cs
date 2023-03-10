using KWEngine3.EngineCamera;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KWEngine3
{
    /// <summary>
    /// Basisklasse für Spielwelten
    /// </summary>
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

        internal IReadOnlyCollection<GameObject> GetGameObjectsSortedByType()
        {
            IReadOnlyCollection<GameObject> myTempList = _gameObjects
                .OrderBy(item => item.GetType().Name)
                .ToList().AsReadOnly();
            return myTempList;
        }

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

        /// <summary>
        /// Setzt die Farbe des Umgebungslichts (dort wo kein Licht scheint)
        /// </summary>
        /// <param name="r">Rotanteil (0 bis 1)</param>
        /// <param name="g">Grünanteil (0 bis 1)</param>
        /// <param name="b">Blauanteil (0 bis 1)</param>
        public void SetColorAmbient(float r, float g, float b)
        {
            SetColorAmbient(new Vector3(r, g, b));
        }

        /// <summary>
        /// Setzt die Farbe des Umgebungslichts (dort wo kein Licht scheint)
        /// </summary>
        /// <param name="a">Rot-/Grün-/Blauanteil</param>
        public void SetColorAmbient(Vector3 a)
        {
            _colorAmbient = new Vector3(
                    MathHelper.Clamp(a.X, 0f, 1f),
                    MathHelper.Clamp(a.Y, 0f, 1f),
                    MathHelper.Clamp(a.Z, 0f, 1f)
                );
        }

        /// <summary>
        /// Gibt an, ob aktuell ein ViewSpaceGameObject verwendet wird
        /// </summary>
        public bool IsViewSpaceGameObjectAttached { get { return _viewSpaceGameObject != null && _viewSpaceGameObject.IsValid; } }

        /// <summary>
        /// Heftet ein Objekt als ViewSpaceGameObject an die aktuelle Welt bzw. dessen Kamera an
        /// </summary>
        /// <param name="vsg">Anzuheftende Instanz</param>
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

        /// <summary>
        /// Erfragt die Position des aktuell angehefteten ViewSpaceGameObject
        /// </summary>
        /// <returns>Position (aber falls kein Objekt angeheftet ist: (0|0|0))</returns>
        public Vector3 GetViewSpaceGameObjectPosition()
        {
            if(IsViewSpaceGameObjectAttached)
            {
                return _viewSpaceGameObject._gameObject.Center;
            }
            KWEngine.LogWriteLine("[World] No view space game object found");
            return Vector3.Zero;
        }

        /// <summary>
        /// Setzt die Skybox für den 3D-Hintergrund
        /// </summary>
        /// <param name="filename">Dateiname inkl. relativem Pfad</param>
        /// <param name="rotationY">Startrotation des Hintergrunds um die Y-Achse (Standard: 0)</param>
        public void SetBackgroundSkybox(string filename, float rotationY = 0f)
        {
            if (filename == null || filename.Length == 0)
            {
                _background.Unset();
                return;
            }
            _background.SetSkybox(filename, rotationY);
        }

        /// <summary>
        /// Setzt den Helligkeitsverstärker für einen Hintergrund
        /// </summary>
        /// <param name="m">Verstärkerwert (muss >= 0 sein)</param>
        public void SetBackgroundBrightnessMultiplier(float m)
        {
            _background.SetBrightnessMultiplier(m);
        }
        /// <summary>
        /// Setzt ein 2D-Hintergrundbild
        /// </summary>
        /// <param name="filename">Dateiname inkl. relativem Pfad</param>
        public void SetBackground2D(string filename)
        {
            if (filename == null || filename.Length == 0)
            {
                _background.Unset();
                return;
            }
            _background.SetStandard(filename);
        }

        /// <summary>
        /// Verschiebt das 2D-Hintergrundbild um die angegebenen Werte
        /// </summary>
        /// <param name="x">x-Verschiebung</param>
        /// <param name="y">y-Verschiebung</param>
        public void SetBackground2DOffset(float x, float y)
        {
            _background.SetOffset(x, y);
        }

        /// <summary>
        /// Setzt die Texturwiederholung des 2D-Hintergrundbilds
        /// </summary>
        /// <param name="x">x-Wiederholung</param>
        /// <param name="y">y-Wiederholung</param>
        public void SetBackground2DRepeat(float x, float y)
        {
            _background.SetRepeat(x, y);
        }

        /// <summary>
        /// Beschneidet die 2D-Hintergrundtextur
        /// </summary>
        /// <param name="x">Beschneidung in x-Richtung</param>
        /// <param name="y">Beschneidung in y-Richtung</param>
        public void SetBackground2DClip(float x, float y)
        {
            _background.SetClip(x, y);
        }

        /// <summary>
        /// Fügt ein Explosionsobjekt hinzu
        /// </summary>
        /// <param name="ex">Objekt</param>
        public void AddExplosionObject(ExplosionObject ex)
        {
            if (!_particleAndExplosionObjects.Contains(ex))
            {
                ex._starttime = WorldTime;
                _particleAndExplosionObjects.Add(ex);
            }
        }

        /// <summary>
        /// Fügt ein Partikelobjekt hinzu
        /// </summary>
        /// <param name="po">Objekt</param>
        public void AddParticleObject(ParticleObject po)
        {
            if (!_particleAndExplosionObjects.Contains(po))
            {
                po._starttime = WorldTime;
                _particleAndExplosionObjects.Add(po);
            }
        }

        /// <summary>
        /// Fügt ein GameObject der Welt hinzu
        /// </summary>
        /// <param name="g">Objekt</param>
        public void AddGameObject(GameObject g)
        {
            if (!_gameObjects.Contains(g) && !_gameObjectsToBeAdded.Contains(g))
            {
                g.ID = _availableGameObjectIDs.Dequeue();
                _gameObjectsToBeAdded.Add(g);
            }
        }

        /// <summary>
        /// Löscht das angegebene Objekt aus der Welt
        /// </summary>
        /// <param name="g">Objekt</param>
        public void RemoveGameObject(GameObject g)
        {
            if (!_gameObjectsToBeRemoved.Contains(g))
                _gameObjectsToBeRemoved.Add(g);
        }

        /// <summary>
        /// Fügt das angegebene Lichtobjekt der Welt hinzu
        /// </summary>
        /// <param name="l">Objekt</param>
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
                KWEngine.LogWriteLine("[World] LightObject ignored: limit of " + KWEngine.MAX_LIGHTS + " concurrent lights per world has been reached");
            }
        }

        /// <summary>
        /// Löscht das angegebene Licht-Objekt aus der Welt
        /// </summary>
        /// <param name="l">Objekt</param>
        public void RemoveLightObject(LightObject l)
        {
            if (!_lightObjectsToBeRemoved.Contains(l))
                _lightObjectsToBeRemoved.Add(l);
        }

        /// <summary>
        /// Fügt das angegebene Terrain-Objekt der Welt hinzu
        /// </summary>
        /// <param name="t">Objekt</param>
        public void AddTerrainObject(TerrainObject t)
        {
            if (!_terrainObjects.Contains(t) && !_terrainObjectsToBeAdded.Contains(t))
            {
                t.ID = _availableGameObjectIDs.Dequeue();
                _terrainObjectsToBeAdded.Add(t);
            }
        }

        /// <summary>
        /// Löscht das angegebene Terrain-Objekt aus der Welt
        /// </summary>
        /// <param name="t">Objekt</param>
        public void RemoveTerrainObject(TerrainObject t)
        {
            if (!_terrainObjectsToBeRemoved.Contains(t))
                _terrainObjectsToBeRemoved.Add(t);
        }

        /// <summary>
        /// Fügt ein HUD-Objekt der Welt hinzu
        /// </summary>
        /// <param name="h">Objekt</param>
        public void AddHUDObject(HUDObject h)
        {
            if (!_hudObjects.Contains(h) && !_hudObjectsToBeAdded.Contains(h))
                _hudObjectsToBeAdded.Add(h);
        }

        /// <summary>
        /// Löscht das angegebene HUD-Objekt aus der Welt
        /// </summary>
        /// <param name="h">Objekt</param>
        public void RemoveHUDObject(HUDObject h)
        {
            if (!_hudObjectsToBeRemoved.Contains(h))
                _hudObjectsToBeRemoved.Add(h);
        }

        /// <summary>
        /// Setzt die Kameraposition
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <param name="offsetY">(optional: Verschiebung in y-Richtung)</param>
        public void SetCameraPosition(float x, float y, float z, float offsetY = 0f)
        {
            SetCameraPosition(new Vector3(x, y, z), offsetY);
        }

        /// <summary>
        /// Setzt die Kameraposition
        /// </summary>
        /// <param name="position">Positionswert in 3D</param>
        /// <param name="offsetY">(optional: Verschiebung in y-Richtung)</param>
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

        /// <summary>
        /// Aktualisiert die Kameraperspektive für den First-Person-Modus
        /// </summary>
        /// <param name="position">Zielposition</param>
        /// <param name="offsetY">optionaler vertikaler Offset</param>
        public void UpdateCameraPositionForFirstPersonView(Vector3 position, float offsetY = 0)
        {
            Vector3 newPos = position + new Vector3(0f, offsetY, 0f);
            _cameraGame.SetPositionAndTarget(newPos, newPos + CameraLookAtVector);
        }

        /// <summary>
        /// Setzt das Ziel der Kamera
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetCameraTarget(float x, float y, float z)
        {
            SetCameraTarget(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt das Ziel der Kamera
        /// </summary>
        /// <param name="target">Zielposition</param>
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

        /// <summary>
        /// Verschiebt die Kamera einmalig auf die Position des angegebenen GameObject
        /// </summary>
        /// <param name="g">GameObject-Instanz</param>
        /// <param name="offsetY">Relative Verschiebung der Kamera in y-Richtung</param>
        public void SetCameraToFirstPersonGameObject(GameObject g, float offsetY = 0f)
        {
            _cameraGame.degX = 0;
            _cameraGame.degY = 0;
            _cameraGame.AdjustToGameObject(g, offsetY);
        }

        /// <summary>
        /// Rotiert die Kamera gemäß der gegebenen Bewegung
        /// </summary>
        /// <param name="yawPitch">Bewegung in x-/y-Richtung</param>
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

        /// <summary>
        /// Zeit in Sekunden, die die Applikation bereits geöffnet ist
        /// </summary>
        public float ApplicationTime { get { return KWEngine.ApplicationTime; } }
        /// <summary>
        /// Zeit in Sekunden, die die aktuelle Welt bereits läuft
        /// </summary>
        public float WorldTime { get { return KWEngine.WorldTime; } }

        /// <summary>
        /// Verweis auf Keyboardeingaben
        /// </summary>
        public KeyboardState Keyboard { get { return KWEngine.Window.KeyboardState; } }
        /// <summary>
        /// Verweis auf Mauseingaben
        /// </summary>
        public MouseState Mouse { get { return KWEngine.Window.MouseState; } }
        /// <summary>
        /// Verweis auf das aktuelle Programmfenster
        /// </summary>
        public GLWindow Window { get { return KWEngine.Window; } }

        /// <summary>
        /// Blickrichtung der Kamera
        /// </summary>
        public Vector3 CameraLookAtVector { get { return _cameraGame._stateCurrent.LookAtVector; } }
        /// <summary>
        /// Blickrichtung der Kamera nach oben
        /// </summary>
        public Vector3 CameraLookAtVectorLocalUp { get { return _cameraGame._stateCurrent.LookAtVectorLocalUp; } }
        /// <summary>
        /// Blickrichtung der Kamera nach rechts
        /// </summary>
        public Vector3 CameraLookAtVectorLocalRight { get { return _cameraGame._stateCurrent.LookAtVectorLocalRight; } }
        /// <summary>
        /// Kameraposition
        /// </summary>
        public Vector3 CameraPosition { get { return _cameraGame._stateCurrent._position; } }
        /// <summary>
        /// Kameraziel
        /// </summary>
        public Vector3 CameraTarget { get { return _cameraGame._stateCurrent._target; } }

        /// <summary>
        /// Fange den Mauszeiger und blende ihn aus (für First-Person-Modus)
        /// </summary>
        public void MouseCursorGrab()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;
            Window.MousePosition = Window.ClientSize / 2;
            _mouseGrabDelta = Mouse.Delta;
        }
        /// <summary>
        /// Verstecke den Mauszeiger
        /// </summary>
        public void MouseCursorHide()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Hidden;
        }
        /// <summary>
        /// Setzt den Mauszeiger wieder auf seinen Normalzustand (sichtbar) zurück
        /// </summary>
        public void MouseCursorReset()
        {
            Window.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
        }
        /// <summary>
        /// Setze den Mauszeiger in die Mitte des Fensters
        /// </summary>
        public void MouseCursorResetPosition()
        {
            Window.MousePosition = Window.ClientRectangle.HalfSize;
        }

        /// <summary>
        /// Setze den Blickwinkel der Kamera
        /// </summary>
        /// <param name="fov">Blickwinkel in Grad (zwischen 10 und 180)</param>
        public void SetCameraFOV(int fov)
        {
            _cameraGame.SetFOVForPerspectiveProjection(fov);
        }

        /// <summary>
        /// Erfragt die Liste der aktuellen GameObject-Instanzen der Welt
        /// </summary>
        /// <returns>Liste</returns>
        public IReadOnlyCollection<GameObject> GetGameObjects()
        {
            return _gameObjects.AsReadOnly();
        }

        /// <summary>
        /// Erfragt die Liste der aktuellen LightObject-Instanzen der Welt
        /// </summary>
        /// <returns>Liste</returns>
        public IReadOnlyCollection<LightObject> GetLightObjects()
        {
            return _lightObjects.AsReadOnly();
        }

        /// <summary>
        /// Lade eine Weltkonfiguation aus der angegebenen JSON-Datei
        /// </summary>
        /// <param name="filename">Dateiname (inkl. relativem Pfad)</param>
        /// <param name="callerName">(nicht verwenden!)</param>
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

        /// <summary>
        /// Vorbereitungsmethode der Welt
        /// </summary>
        public abstract void Prepare();
        
        /// <summary>
        /// Act-Methode der Welt
        /// </summary>
        public abstract void Act();
    }
}
