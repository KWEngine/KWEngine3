using KWEngine3.EngineCamera;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Reflection;

namespace KWEngine3
{
    /// <summary>
    /// Basisklasse für Spielwelten
    /// </summary>
    public abstract class World
    {
        #region Internals
        internal HUDObjectTextInput _hudObjectInputWithFocus = null;
        internal ViewSpaceGameObject _viewSpaceGameObject = null;

        internal List<WorldEvent> _eventQueue = new();

        internal WorldMap _map = null;

        internal List<FoliageObject> _foliageObjects = new();
        internal List<FoliageObject> _foliageObjectsToBeAdded = new();
        internal List<FoliageObject> _foliageObjectsToBeRemoved = new();

        internal List<GameObject> _gameObjects = new();
        internal List<GameObject> _gameObjectsToBeAdded = new();
        internal List<GameObject> _gameObjectsToBeRemoved = new();

        internal List<RenderObject> _renderObjects = new();
        internal List<RenderObject> _renderObjectsToBeAdded = new();
        internal List<RenderObject> _renderObjectsToBeRemoved = new();

        internal List<ColliderChangeIntent> _gameObjectsColliderChange = new();
        internal List<GameObjectHitbox> _gameObjectHitboxes = new();

        internal List<TerrainObject> _terrainObjects = new();
        internal List<TerrainObject> _terrainObjectsToBeAdded = new();
        internal List<TerrainObject> _terrainObjectsToBeRemoved = new();

        internal List<LightObject> _lightObjects = new();
        internal List<LightObject> _lightObjectsToBeAdded = new();
        internal List<LightObject> _lightObjectsToBeRemoved = new();

        internal List<HUDObject> _hudObjects = new();
        internal List<HUDObject> _hudObjectsToBeAdded = new();
        internal List<HUDObject> _hudObjectsToBeRemoved = new();

        internal List<TextObject> _textObjects = new();
        internal List<TextObject> _textObjectsToBeAdded = new();
        internal List<TextObject> _textObjectsToBeRemoved = new();

        internal List<FlowField> _flowFields = new List<FlowField>();

        internal List<TimeBasedObject> _particleAndExplosionObjects = new();

        internal Queue<ushort> _availableGameObjectIDs = new();
        internal Queue<ushort> _availableLightObjectIDs = new();

        internal Dictionary<string, KWTexture> _customTextures = new();

        internal Camera _cameraGame;
        internal Camera _cameraEditor;
        internal bool _startingFrameActive = false;
        internal bool _mouseCursorJustGrabbed = false;

        internal Vector2 _xMinMax = new(float.MaxValue, float.MinValue);
        internal Vector2 _yMinMax = new(float.MaxValue, float.MinValue);
        internal Vector2 _zMinMax = new(float.MaxValue, float.MinValue);
        internal Vector3 _worldCenter = new(0);

        internal Vector3 _colorAmbient = new(0.75f, 0.75f, 0.75f);
        internal Vector3 _backgroundFillColor = new(0.0f, 0.0f, 0.0f);
        internal WorldBackground _background = new();

        internal WorldFadeState _fadeStateCurrent = new WorldFadeState();
        internal WorldFadeState _fadeStatePrevious = new WorldFadeState();

        internal IReadOnlyCollection<GameObject> GetGameObjectsSortedByType()
        {
            IReadOnlyCollection<GameObject> myTempList = _gameObjects
                .OrderBy(item => item.GetType().Name)
                .ToList().AsReadOnly();
            return myTempList;
        }

        internal void ProcessWorldEventQueue()
        {
            for (int i = _eventQueue.Count - 1; i >= 0; i--)
            {
                WorldEvent e = _eventQueue[i];
                if (e.Owner == KWEngine.CurrentWorld)
                {
                    if (e.Timestamp <= WorldTime)
                    {
                        OnWorldEvent(e);
                        _eventQueue.Remove(e);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    _eventQueue.Remove(e);
                }
            }
        }

        internal GameObject GetGameObjectByID(int id)
        {
            int index = _gameObjects.FindIndex(g => g.ID == id);
            if(index >= 0)
            {
                return _gameObjects[index];
            }
            else
            {
                return null;
            }
        }

        internal T GetGameObjectByID<T>(int id) where T : GameObject
        {
            int index = _gameObjects.FindIndex(g => g.ID == id && g is T);
            if (index >= 0)
            {
                return _gameObjects[index] as T;
            }
            else
            {
                return null;
            }
        }

        internal void ResetWorldDimensions()
        {
            _xMinMax = new Vector2(float.MaxValue, float.MinValue);
            _yMinMax = new Vector2(float.MaxValue, float.MinValue);
            _zMinMax = new Vector2(float.MaxValue, float.MinValue);
        }


        internal GameObject BuildAndAddDefaultGameObjectForEditor(string classname)
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

            _map = new WorldMap();

            for (ushort i = 1; i < ushort.MaxValue - 1; i++)
            {
                _availableLightObjectIDs.Enqueue(i);
                _availableGameObjectIDs.Enqueue(i);
            }
            _cameraGame = new Camera();
            _cameraEditor = _cameraGame;
        }
        internal BackgroundType BackgroundTextureType { get { return _background.Type; } }

        internal int _preparedLightsCount = 0;
        internal float[] _preparedLightsArray = new float[KWEngine.MAX_LIGHTS * KWEngine.LIGHTINDEXDIVIDER];
        internal List<int> _preparedTex2DIndices = new();
        internal List<int> _preparedCubeMapIndices = new();
        internal List<LightObject> _currentShadowLights = new();

        internal static void Export()
        {
            if(KWEngine.CurrentWorld != null)
            {
                HelperImportExport.ExportWorld(KWEngine.CurrentWorld);
            }
        }

        internal void AddRemoveTerrainObjects()
        {
            foreach (TerrainObject t in _terrainObjectsToBeRemoved)
            {
                _availableGameObjectIDs.Enqueue((ushort)t.ID);
                t.ID = 0;
                t._myWorld = null;

                _terrainObjects.Remove(t);
                foreach (FoliageObject f in _foliageObjects)
                {
                    if (f._terrainObject == t)
                    {
                        f.DetachFromTerrain();
                    }
                }
            }
            _terrainObjectsToBeRemoved.Clear();

            foreach (TerrainObject t in _terrainObjectsToBeAdded)
            {
                _terrainObjects.Add(t);
                t._myWorld = this;
            }
            _terrainObjectsToBeAdded.Clear();
        }

        internal List<TerrainObject> GetTerrainObjects()
        {
            return _terrainObjects;
        }

        internal LightObject BuildAndAddDefaultLightObjectForEditor(string lightType, ShadowQuality quality)
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
            LightObject l = new(t, quality);
            l.SetPosition(0, 0, 0);
            l.SetTarget(0, -1, 0);
            AddLightObject(l);
            return l;
        }

        internal void AddRemoveHUDObjects()
        {
            foreach (HUDObject h in _hudObjectsToBeRemoved)
            {
                if(h is HUDObjectTextInput && (h as HUDObjectTextInput).HasFocus)
                {
                    _hudObjectInputWithFocus = null;
                }
                h._currentWorld = null;
                _hudObjects.Remove(h);
            }
            _hudObjectsToBeRemoved.Clear();

            foreach (HUDObject h in _hudObjectsToBeAdded)
            {
                _hudObjects.Add(h);
                h._currentWorld = this;
            }
            _hudObjectsToBeAdded.Clear();
            Map.Reset();
        }

        internal void AddRemoveTextObjects()
        {
            foreach (TextObject t in _textObjectsToBeRemoved)
            {
                _textObjects.Remove(t);
            }
            _textObjectsToBeRemoved.Clear();

            foreach (TextObject t in _textObjectsToBeAdded)
            {
                _textObjects.Add(t);
            }
            _textObjectsToBeAdded.Clear();
        }

        internal void AddRemoveFoliageObjects()
        {
            for (int i = _foliageObjectsToBeRemoved.Count - 1; i >= 0; i--)
            {
                if (_foliageObjectsToBeAdded.Contains(_foliageObjectsToBeRemoved[i]))
                {
                    _foliageObjectsToBeAdded.Remove(_foliageObjectsToBeRemoved[i]);
                    _foliageObjectsToBeRemoved.RemoveAt(i);
                }
            }

            foreach (FoliageObject f in _foliageObjectsToBeRemoved)
            {
                _foliageObjects.Remove(f);
            }
            _foliageObjectsToBeRemoved.Clear();

            foreach (FoliageObject f in _foliageObjectsToBeAdded)
            {
                _foliageObjects.Add(f);
            }
            _foliageObjectsToBeAdded.Clear();
        }

        internal void AddRemoveRenderObjects()
        {
            for (int i = _renderObjectsToBeRemoved.Count - 1; i >= 0; i--)
            {
                if (_renderObjectsToBeAdded.Contains(_renderObjectsToBeRemoved[i]))
                {
                    _renderObjectsToBeAdded.Remove(_renderObjectsToBeRemoved[i]);
                    _renderObjectsToBeRemoved.RemoveAt(i);
                }
            }

            foreach (RenderObject r in _renderObjectsToBeRemoved)
            {
                _renderObjects.Remove(r);
                r._myWorld = null;
            }
            _gameObjectsToBeRemoved.Clear();

            foreach (RenderObject r in _renderObjectsToBeAdded)
            {
                _renderObjects.Add(r);
                r._myWorld = this;
            }
            _renderObjectsToBeAdded.Clear();
        }

        internal void AddRemoveGameObjects()
        {
            for(int i = _gameObjectsToBeRemoved.Count - 1; i>= 0; i--)
            {
                if (_gameObjectsToBeAdded.Contains(_gameObjectsToBeRemoved[i]))
                {
                    _gameObjectsToBeAdded.Remove(_gameObjectsToBeRemoved[i]);
                    _gameObjectsToBeRemoved.RemoveAt(i);
                }
            }

            lock (_gameObjectHitboxes)
            {
                foreach (ColliderChangeIntent g in _gameObjectsColliderChange)
                {
                    if (g._mode == AddRemoveHitboxMode.Add)
                    {
                        foreach (GameObjectHitbox hb in g._objectToChange._colliderModel._hitboxes)
                        {
                            if (!_gameObjectHitboxes.Contains(hb))
                            {
                                _gameObjectHitboxes.Add(hb);
                            }
                        }
                    }
                    else if (g._mode == AddRemoveHitboxMode.Remove)
                    {
                        foreach (GameObjectHitbox hb in g._objectToChange._colliderModel._hitboxes)
                        {
                            _gameObjectHitboxes.Remove(hb);
                        }
                    }
                    else if(g._mode == AddRemoveHitboxMode.AddCustomRemoveDefault)
                    {
                        foreach (GameObjectHitbox hb in g._objectToChange._colliderModel._hitboxes)
                        {
                            _gameObjectHitboxes.Remove(hb);
                        }
                        g._objectToChange._colliderModel._hitboxes.Clear();
                        g._objectToChange._colliderModel._hitboxes.AddRange(g._hitboxesNew);

                        if (g._objectToChange._isCollisionObject)
                        {
                            foreach (GameObjectHitbox gmh in g._objectToChange._colliderModel._hitboxes)
                            {
                                if (!_gameObjectHitboxes.Contains(gmh))
                                    _gameObjectHitboxes.Add(gmh);
                            }
                        }
                        g._objectToChange._colliderModel._customColliderName = g._customColliderName;
                        g._objectToChange._colliderModel._customColliderFilename = g._customColliderFilename;
                        g._objectToChange.UpdateModelMatrixAndHitboxes();
                    }
                    else if(g._mode == AddRemoveHitboxMode.AddDefaultRemoveCustom)
                    {
                        foreach (GameObjectHitbox hb in g._objectToChange._colliderModel._hitboxes)
                        {
                            _gameObjectHitboxes.Remove(hb);
                        }

                        g._objectToChange._colliderModel._hitboxes.Clear();
                        g._objectToChange._colliderModel._hitboxes.AddRange(g._hitboxesNew);
                        g._objectToChange._colliderModel._customColliderName = "";
                        g._objectToChange._colliderModel._customColliderFilename = "";
                        if (g._objectToChange._isCollisionObject)
                        {
                            foreach (GameObjectHitbox gmh in g._objectToChange._colliderModel._hitboxes)
                            {
                                if (!_gameObjectHitboxes.Contains(gmh))
                                {
                                    _gameObjectHitboxes.Add(gmh);
                                }
                            }
                        }
                        g._objectToChange.UpdateModelMatrixAndHitboxes();
                    }
                }
                _gameObjectsColliderChange.Clear();

                foreach (GameObject g in _gameObjectsToBeRemoved)
                {
                    _availableGameObjectIDs.Enqueue((ushort)g.ID);
                    g.ID = 0;
                    g._myWorld = null;

                    HelperGameObjectAttachment.CleanAttachments(g);

                    _gameObjects.Remove(g);
                    foreach(GameObjectHitbox hb in g._colliderModel._hitboxes)
                    {
                        _gameObjectHitboxes.Remove(hb);
                    }
                }
                _gameObjectsToBeRemoved.Clear();

                foreach (GameObject g in _gameObjectsToBeAdded)
                {
                    _gameObjects.Add(g);
                    g._myWorld = this;
                    if (g.IsCollisionObject)
                    {
                        foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
                        {
                            if (!_gameObjectHitboxes.Contains(hb))
                            {
                                _gameObjectHitboxes.Add(hb);
                            }
                        }
                    }
                }
                _gameObjectsToBeAdded.Clear();
            }
        }

        internal void Dispose()
        {
            lock (_flowFields)
            {
                _flowFields.Clear();
            }

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

            foreach (FoliageObject f in _foliageObjects)
            {
                RemoveFoliageObject(f);
            }
            AddRemoveFoliageObjects();

            foreach (RenderObject r in _renderObjects)
            {
                RemoveRenderObject(r);
            }
            AddRemoveRenderObjects();

            foreach (TerrainObject t in _terrainObjects)
            {
                RemoveTerrainObject(t);
            }
            AddRemoveTerrainObjects();

            foreach (HUDObject h in _hudObjects)
            {
                RemoveHUDObject(h);
            }
            AddRemoveHUDObjects();
            _map.Reset();

            foreach (TextObject t in _textObjects)
            {
                RemoveTextObject(t);
            }
            AddRemoveTextObjects();

            KWEngine.DeleteCustomModelsAndTexturesFromCurrentWorld();

            _eventQueue.Clear();
        }

        internal List<GameObject> GetTransparentGameObjectsForEditor()
        {
            List<GameObject> list = new();
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
            int offsetTex = 0;
            int cubemapIndex = -1;
            int tex2dIndex = 1;
            _preparedTex2DIndices.Clear();
            _preparedCubeMapIndices.Clear();
            _currentShadowLights.Clear();
            _preparedLightsCount = 0;
            foreach (LightObject l in _lightObjects)
            {
                if (KWEngine.Mode == EngineMode.Play && !l.IsInsideScreenSpaceForRenderPass)
                {
                    offsetTex += KWEngine.LIGHTINDEXDIVIDER;
                    continue;
                }

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
                    _preparedCubeMapIndices.Add(offsetTex / KWEngine.LIGHTINDEXDIVIDER);
                    _preparedLightsArray[offset + 03] = cubemapIndex--;
                }
                else
                {
                    _currentShadowLights.Add(l);
                    _preparedTex2DIndices.Add(offsetTex / KWEngine.LIGHTINDEXDIVIDER);
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

                _preparedLightsArray[offset + 15] = l._shadowBias * ((int)KWEngine.Window._ppQuality >= 1 ? 1 : 4);
                _preparedLightsArray[offset + 16] = (int)KWEngine.Window._ppQuality >= 1 ? l._shadowOffset : 0;

                offset += KWEngine.LIGHTINDEXDIVIDER;
                offsetTex += KWEngine.LIGHTINDEXDIVIDER;
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
        /// Gibt die Referenz auf die (optionale) Karte zurück
        /// </summary>
        public WorldMap Map { get { return _map;} }

        /// <summary>
        /// Gibt die globale Mischfarbe an, mit der der Bildschirminhalt gemischt wird, wenn FadeFactor kleiner als 1.0 ist
        /// </summary>
        public Vector3 FadeColor { 
            get
            {
                return _fadeStateCurrent.Color;
            }
        }

        /// <summary>
        /// Setzt die globale Mischfarbe, mit der der Bildschirminhalt gemischt wird, wenn FadeFactor kleiner als 1.0 ist
        /// </summary>
        /// <param name="r">Rotanteil (von 0 bis 1)</param>
        /// <param name="g">Grünanteil (von 0 bis 1)</param>
        /// <param name="b">Blauanteil (von 0 bis 1)</param>
        public void SetFadeColor(float r, float g, float b)
        {
            r = Math.Clamp(r, 0f, 1f);
            g = Math.Clamp(g, 0f, 1f);
            b = Math.Clamp(b, 0f, 1f);
            _fadeStateCurrent.Color = new Vector3(r, g, b);
        }

        /// <summary>
        /// Gibt den aktuellen Mischfaktor an (Standard: 1.0 für vollständige Darstellung des Bildschirminhalts)
        /// </summary>
        public float FadeFactor { get { return _fadeStateCurrent.Factor; } }

        /// <summary>
        /// Setzt den Mischfaktor zwischen globaler Mischfarbe und dem Bildschirminhalt. Ist f == 1.0, wird der Bildschirminhalt vollständig angezeigt.
        /// </summary>
        /// <param name="f">Mischfaktor (muss zwischen 0.0 und 1.0 liegen)</param>
        public void SetFadeFactor(float f)
        {
            _fadeStateCurrent.Factor = Math.Clamp(f, 0f, 1f);
        }

        /// <summary>
        /// Gibt an, ob die Welt bereits via Prepare()-Methode abschließend vorbereitet wurde
        /// </summary>
        public bool IsPrepared { get; internal set; } = false;

        /// <summary>
        /// Gibt die Strecke an, die der Mauszeiger seit der letzten Überprüfung zurückgelegt hat
        /// </summary>
        public static Vector2 MouseMovement
        {
            get
            {
                return KWEngine.Window._mouseDeltaToUse;
            }
        }

        /// <summary>
        /// Gibt Auskunft über das aktuell gewählte Umgebungslicht
        /// </summary>
        public Vector3 ColorAmbient { get { return  _colorAmbient; } }

        /// <summary>
        /// Gibt Auskunft über die aktuell gewählte Hintergrundfarbe
        /// </summary>
        public Vector3 ColorBackground { get { return _backgroundFillColor; } }

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
        /// Gibt das aktuell verwendete ViewSpaceGameObject zurück
        /// </summary>
        /// <returns>ViewSpaceGameObject-Instanz</returns>
        public ViewSpaceGameObject GetViewSpaceGameObject()
        {
            if (_viewSpaceGameObject == null || !_viewSpaceGameObject.IsValid)
            {
                return null;
            }
            else
            {
                return _viewSpaceGameObject;
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
        /// Setzt die Hintergrundfarbe der Welt
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        public void SetBackgroundFillColor(float r, float g, float b)
        {
            SetBackgroundFillColor(new Vector3(r, g, b));
        }

        /// <summary>
        /// Setzt die Hintergrundfarbe der Welt
        /// </summary>
        /// <param name="color">Farbwerte (RGB, jeweils zwischen 0 und 1)</param>
        public void SetBackgroundFillColor(Vector3 color)
        {
            _backgroundFillColor = new(Math.Clamp(color.X, 0f, 1f), Math.Clamp(color.Y, 0f, 1f), Math.Clamp(color.Z, 0f, 1f));

            RenderManager.UpdateFramebufferClearColor(_backgroundFillColor);
        }

        /// <summary>
        /// Erfragt die Auflösung der aktuell gewählten Hintergrundtextur
        /// </summary>
        /// <returns>Auflösung der Hintergrundtextur in Pixeln</returns>
        /// <remarks>Wenn kein Hintergrundbild festgelegt wurde, wird der Nullvektor (0|0) zurückgegeben</remarks>
        public Vector2 GetBackgroundImageSize()
        {
            if(_background.Type != BackgroundType.None)
            {
                if(_background.Type == BackgroundType.Standard)
                {
                    HelperTexture.GetTextureDimensions(_background._standardId, out int width, out int height);
                    return new Vector2(width, height);
                }
                else
                {
                    HelperTexture.GetTextureDimensions(_background._skyboxId, out int width, out int height);
                    return new Vector2(width, height);
                }
            }
            else
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Setzt die Skybox für den 3D-Hintergrund
        /// </summary>
        /// <param name="filename">Dateiname inkl. relativem Pfad</param>
        /// <param name="rotationY">Startrotation des Hintergrunds um die Y-Achse (Standard: 0)</param>
        /// <param name="type">Typ der Skybox [CubeMap oder Gleichwinkelbild (Equirectangular), Standard: CubeMap]</param>
        public void SetBackgroundSkybox(string filename, float rotationY = 0f, SkyboxType type = SkyboxType.CubeMap)
        {
            if (filename == null || filename.Length == 0)
            {
                _background.Unset();
                return;
            }
            _background.SetSkybox(filename, rotationY, type);
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
        /// Erfragt die aktuelle Texturverschiebung des 2D-Hintergrundbilds
        /// </summary>
        /// <returns>Verschiebungswerte</returns>
        public Vector2 GetBackground2DOffset()
        {
            if (_background != null && _background.Type == BackgroundType.Standard)
            {
                return _background._stateCurrent.Offset;
            }
            return Vector2.Zero;
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
        /// Erfragt die aktuelle Texturwiederholung des 2D-Hintergrundbilds
        /// </summary>
        /// <returns>Wiederholungswerte</returns>
        public Vector2 GetBackground2DRepeat()
        {
            if(_background != null && _background.Type == BackgroundType.Standard)
            {
                return _background._stateCurrent.Scale;
            }
            return Vector2.One;
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
        /// Erfragt die aktuelle Texturbeschneidung des 2D-Hintergrundbilds
        /// </summary>
        /// <returns>Beschneidungswerte</returns>
        public Vector2 GetBackground2DClip()
        {
            if (_background != null && _background.Type == BackgroundType.Standard)
            {
                return _background._stateCurrent.Clip;
            }
            return Vector2.One;
        }

        /// <summary>
        /// Setzt das FlowField-Objekt (es kann aktuell immer nur ein FlowField pro Welt existieren)
        /// </summary>
        /// <param name="f">FlowField-Instanz</param>
        [Obsolete("SetFlowField is deprecated, please use AddFlowField instead.")]
        public void SetFlowField(FlowField f)
        {
            AddFlowField(f);
        }

        /// <summary>
        /// Fügt der aktuellen Welt die angegebene FlowField-Instanz hinzu
        /// </summary>
        /// <param name="f">FlowField-Instanz</param>
        public void AddFlowField(FlowField f)
        {
            lock (_flowFields)
            {
                if (!_flowFields.Contains(f))
                {
                    _flowFields.Add(f);
                }
            }
        }

        /// <summary>
        /// Entfernt die angegebene FlowField-Instanz aus der Welt
        /// </summary>
        /// <param name="f">zu löschende Instanz</param>
        public void RemoveFlowField(FlowField f)
        {
            lock (_flowFields)
            {
                _flowFields.Remove(f);
            }
        }

        /// <summary>
        /// Erfragt die Referenz auf das aktuelle FlowField mit dem angegebenen Index (Standardwert: 0)
        /// </summary>
        /// <returns>FlowField-Instanz - wenn keine gefunden wird: null</returns>
        public FlowField GetFlowField(int index = 0)
        {
            if (_flowFields.Count > 0 && _flowFields.Count > index)
                return _flowFields[index];
            else
                return null;
        }

        /// <summary>
        /// Erfragt die aktuelle Anzahl an aktiven FlowField-Instanzen in der Welt
        /// </summary>
        /// <returns>Anzahl der FlowField-Instanzen</returns>
        public int GetFlowFieldCount()
        {
            return _flowFields.Count;
        }

        /// <summary>
        /// Durchsucht die Liste der aktuell aktiven FlowField-Instanzen nach einem Namen und gibt die erste Instanz, die mit diesem Namen übereinstimmt zurück
        /// </summary>
        /// <param name="name">gesuchter Name</param>
        /// <returns>gefundene FlowField-Instanz (null, falls keine Instanz mit dem angegebenen Namen gefunden werden kann)</returns>
        public FlowField GetFlowFieldByName(string name)
        {
            return _flowFields.Find(ff => ff.Name == name);
        }

        /// <summary>
        /// Fügt ein Explosionsobjekt hinzu
        /// </summary>
        /// <param name="ex">Objekt</param>
        public void AddExplosionObject(ExplosionObject ex)
        {
            if (ex == null)
                return;

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
            if (po == null)
                return;

            if (!_particleAndExplosionObjects.Contains(po))
            {
                po._starttime = WorldTime;
                _particleAndExplosionObjects.Add(po);
            }
        }

        /// <summary>
        /// Fügt ein Gewächsobjekt (z.B. Gras) hinzu
        /// </summary>
        /// <param name="f">hinzuzufügendes Objekt</param>
        public void AddFoliageObject(FoliageObject f)
        {
            if (f == null)
                return;

            if (IsPrepared == false)
            {
                if (!_foliageObjects.Contains(f))
                {
                    _foliageObjects.Add(f);
                }
                else
                {
                    KWEngine.LogWriteLine("[FoliageObject] Object " + f.Name + " already in world.");
                }
            }
            else
            {
                if (!_foliageObjects.Contains(f) && !_foliageObjectsToBeAdded.Contains(f))
                {
                    _foliageObjectsToBeAdded.Add(f);
                }
                else
                {
                    KWEngine.LogWriteLine("[FoliageObject] Object " + f.Name + " already in world.");
                }
            }
        }

        /// <summary>
        /// Löscht ein Gewächsobjekt (z.B. Gras) aus der Welt
        /// </summary>
        /// <param name="f">zu entfernendes Objekt</param>
        public void RemoveFoliageObject(FoliageObject f)
        {
            if (f == null)
                return;

            if (IsPrepared == false)
            {
                _foliageObjects.Remove(f);
            }
            else
            {
                if (!_foliageObjectsToBeRemoved.Contains(f))
                {
                    _foliageObjectsToBeRemoved.Add(f);
                }
            }
        }

        /// <summary>
        /// Fügt ein GameObject der Welt hinzu
        /// </summary>
        /// <param name="g">Objekt</param>
        public void AddGameObject(GameObject g)
        {
            if (g == null)
                return;

            if(IsPrepared == false)
            {
                if (!_gameObjects.Contains(g))
                {
                    g.ID = _availableGameObjectIDs.Dequeue();
                    _gameObjects.Add(g);
                    if (g.IsCollisionObject)
                    {
                        foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
                        {
                            if (!_gameObjectHitboxes.Contains(hb))
                            {
                                _gameObjectHitboxes.Add(hb);
                            }
                        }
                    }
                }
                else
                {
                    KWEngine.LogWriteLine("[GameObject] Object " + g.Name + " already in world.");
                }

                HelperSweepAndPrune.SweepAndPrune();
                return;
            }


            if (!_gameObjects.Contains(g) && !_gameObjectsToBeAdded.Contains(g))
            {
                g.ID = _availableGameObjectIDs.Dequeue();
                _gameObjectsToBeAdded.Add(g);
            }
            else
            {
                KWEngine.LogWriteLine("[GameObject] Object " + g.Name + " already in world.");
            }
        }

        /// <summary>
        /// Löscht das angegebene Objekt aus der Welt
        /// </summary>
        /// <param name="g">Objekt</param>
        public void RemoveGameObject(GameObject g)
        {
            if (g == null)
                return;

            if(IsPrepared == false)
            {
                if(_gameObjects.Remove(g))
                {
                    _availableGameObjectIDs.Enqueue((ushort)g.ID);
                    g.ID = 0;
                    foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
                    {
                        _gameObjectHitboxes.Remove(hb);
                    }
                }

                HelperGameObjectAttachment.CleanAttachments(g);

                HelperSweepAndPrune.SweepAndPrune();
                return;
            }

            if (!_gameObjectsToBeRemoved.Contains(g))
                _gameObjectsToBeRemoved.Add(g);
        }

        /// <summary>
        /// Löscht alle aktuell in der Welt befindlichen GameObject-Instanzen des angegebenen Typs
        /// </summary>
        /// <typeparam name="T">Zu suchender Datentyp (Klasse)</typeparam>
        /// <param name="includeSubtypes">wenn true, werden auch Unterklassen des angegebenen Typs berücksichtigt (Standard: true)</param>
        public void RemoveGameObjectsOfType<T>(bool includeSubtypes = true)
        {
            foreach(GameObject g in _gameObjects)
            {
                if(includeSubtypes)
                {
                    if (HelperGeneral.IsObjectClassOrSubclassOfType<T>(g))
                    {
                        RemoveGameObject(g);
                    }
                }
                else
                {
                    if(HelperGeneral.IsObjectClassOfType<T>(g))
                    {
                        RemoveGameObject(g);
                    }
                }
            }
        }

        /// <summary>
        /// Löscht das angegebene Objekt aus der Welt
        /// </summary>
        /// <param name="r">Hinzuzufügendes Objekt</param>
        public void AddRenderObject(RenderObject r)
        {
            if (r == null)
                return;

            if (r.IsConfigured == false)
            {
                KWEngine.LogWriteLine("[World] RenderObject instance '" + r.Name + "' incomplete. Set additional instance count first.");
            }

            if (IsPrepared == false)
            {
                if (!_renderObjects.Contains(r))
                {
                    _renderObjects.Add(r);
                }
                else
                {
                    KWEngine.LogWriteLine("[RenderObject] Object " + r.Name + " already in world.");
                }
            }
            else
            {
                if (!_renderObjects.Contains(r) && !_renderObjectsToBeAdded.Contains(r))
                {
                    _renderObjectsToBeAdded.Add(r);
                }
                else
                {
                    KWEngine.LogWriteLine("[RenderObject] Object " + r.Name + " already in world.");
                }
            }
        }

        /// <summary>
        /// Löscht das angegebene Objekt aus der Welt
        /// </summary>
        /// <param name="r">Zu löschendes Objekt</param>
        public void RemoveRenderObject(RenderObject r)
        {
            if (r == null)
                return;
            if (IsPrepared == false)
            {
                _renderObjects.Remove(r);
            }
            else
            {
                if (!_renderObjectsToBeRemoved.Contains(r))
                {
                    _renderObjectsToBeRemoved.Add(r);
                }
            }
        }

        /// <summary>
        /// Fügt das angegebene Lichtobjekt der Welt hinzu
        /// </summary>
        /// <param name="l">Objekt</param>
        public void AddLightObject(LightObject l)
        {
            if (l == null)
                return;

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
            if (l == null)
                return;

            if (!_lightObjectsToBeRemoved.Contains(l))
                _lightObjectsToBeRemoved.Add(l);
        }

        /// <summary>
        /// Fügt das angegebene Terrain-Objekt der Welt hinzu
        /// </summary>
        /// <param name="t">Objekt</param>
        public void AddTerrainObject(TerrainObject t)
        {
            if (t == null)
                return;

            if (IsPrepared == false)
            {
                if (!_terrainObjects.Contains(t))
                {
                    t.ID = _availableGameObjectIDs.Dequeue();
                    _terrainObjects.Add(t);
                }
            }
            else
            {
                if (!_terrainObjects.Contains(t) && !_terrainObjectsToBeAdded.Contains(t))
                {
                    t.ID = _availableGameObjectIDs.Dequeue();
                    _terrainObjectsToBeAdded.Add(t);
                }
            }
        }

        /// <summary>
        /// Löscht das angegebene Terrain-Objekt aus der Welt
        /// </summary>
        /// <param name="t">Objekt</param>
        public void RemoveTerrainObject(TerrainObject t)
        {
            if (t == null)
                return;

            if (IsPrepared == false)
            {
                _terrainObjects.Remove(t);
                foreach(FoliageObject f in _foliageObjects)
                {
                    if(f._terrainObject == t)
                    {
                        f.DetachFromTerrain();
                    }
                }
            }
            else
            {
                if (!_terrainObjectsToBeRemoved.Contains(t))
                    _terrainObjectsToBeRemoved.Add(t);
            }
        }

        /// <summary>
        /// Fügt ein HUD-Objekt der Welt hinzu
        /// </summary>
        /// <param name="h">Objekt</param>
        public void AddHUDObject(HUDObject h)
        {
            if (h != null && !_hudObjects.Contains(h) && !_hudObjectsToBeAdded.Contains(h))
                _hudObjectsToBeAdded.Add(h);
        }

        /// <summary>
        /// Löscht das angegebene HUD-Objekt aus der Welt
        /// </summary>
        /// <param name="h">Objekt</param>
        public void RemoveHUDObject(HUDObject h)
        {
            if (h != null && !_hudObjectsToBeRemoved.Contains(h))
                _hudObjectsToBeRemoved.Add(h);
        }


        /// <summary>
        /// Fügt ein Textobjekt der Welt hinzu
        /// </summary>
        /// <param name="t">Objekt</param>
        public void AddTextObject(TextObject t)
        {
            if (t != null && !_textObjects.Contains(t) && !_textObjectsToBeAdded.Contains(t))
                _textObjectsToBeAdded.Add(t);
        }
        /// <summary>
        /// Löscht das angegebene Textobjekt aus der Welt
        /// </summary>
        /// <param name="t">Objekt</param>
        public void RemoveTextObject(TextObject t)
        {
            if(t != null && !_textObjectsToBeRemoved.Contains(t))
                _textObjectsToBeRemoved.Add(t);
        }

        /// <summary>
        /// Ändert die Renderdistanz der Kamera (Standard: 500, Maximal 10000)
        /// </summary>
        /// <param name="renderDistance">neue Renderdistanz (muss größer oder gleich 1 sein)</param>
        public void SetCameraRenderDistance(float renderDistance)
        {
            if (renderDistance <= 0.1f)
            {
                KWEngine.LogWriteLine("[World] Camera render distance adjusted to be at least 1f.");
                renderDistance  = 1f;
            }
            _cameraGame.SetNearFarBound(0.1f, Math.Clamp(renderDistance, 1, 10000));
        }

        /// <summary>
        /// Erzwing die sofortige Aktualisierung der Kamera hinsichtlich Position und Ziel
        /// </summary>
        /// <remarks>Sollte nur dann verwendet werden, wenn die Kamera direkt durch den Mauscursor verändert wird (ausgenommen: First-Person-Mode)</remarks>
        public void ForceCameraUpdate()
        {
            if (KWEngine.Mode == EngineMode.Play)
            {
                _cameraGame._statePrevious = _cameraGame._stateCurrent;
            }
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
        /// Startet ein zeitbegrenztes Schütteln der Kamera
        /// </summary>
        /// <param name="shakeX">Stärke der Verschiebung relativ zur lokalen x-Achse der Kamera</param>
        /// <param name="shakeY">Stärke der Verschiebung relativ zur lokalen y-Achse der Kamera</param>
        /// <param name="shakeZ">Stärke der Verschiebung relativ zur lokalen z-Achse der Kamera</param>
        /// <param name="duration">Dauer des Effekts (in Sekunden, Standard: 1f)</param>
        /// <param name="speed">Geschwindigkeit des Effekts (Standard: 100f)</param>
        /// <param name="mode">Modus, der festlegt, wie mit den neuen Werten umgegangen wird</param>
        public void StartCameraShake(float shakeX, float shakeY, float shakeZ, float duration = 1f, float speed = 100f, ShakeMode mode = ShakeMode.Additive)
        {
            shakeX = Math.Max(0f, shakeX);
            shakeY = Math.Max(0f, shakeY);
            shakeZ = Math.Max(0f, shakeZ);
            _cameraGame._stateCurrent._shakeTimestamp = WorldTime;
            if (mode == ShakeMode.Additive)
            {
                _cameraGame._stateCurrent._shakeOffset = new Vector3(
                    Math.Max(_cameraGame._stateCurrent._shakeOffset.X ,shakeX),
                    Math.Max(_cameraGame._stateCurrent._shakeOffset.Y, shakeY),
                    Math.Max(_cameraGame._stateCurrent._shakeOffset.Z, shakeZ));
                _cameraGame._stateCurrent._shakeDuration = Math.Max(_cameraGame._stateCurrent._shakeDuration, duration);
            }
            else
            {
                _cameraGame._stateCurrent._shakeOffset = new Vector3(shakeX, shakeY, shakeZ);
                _cameraGame._stateCurrent._shakeDuration = Math.Max(0f, duration);
            }
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
        /// <param name="offsetX">optionaler horizontaler Offset</param>
        public void UpdateCameraPositionForFirstPersonView(Vector3 position, float offsetY = 0, float offsetX = 0)
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
        /// Rotiert die Kamera gemäß der gegebenen Mausbewegung
        /// </summary>
        public void AddCameraRotationFromMouseDelta()
        {
            if (_mouseCursorJustGrabbed)
                return;
            _cameraGame.YawAndPitch(new Vector2(KWEngine.Window._mouseDeltaToUse.X * Math.Abs(KWEngine.MouseSensitivity), KWEngine.Window._mouseDeltaToUse.Y * KWEngine.MouseSensitivity));
            //_cameraGame.YawAndPitch(new Vector2(KWEngine.Window.MouseState.Delta.X * Math.Abs(KWEngine.MouseSensitivity), KWEngine.Window.MouseState.Delta.Y * KWEngine.MouseSensitivity));
        }

        /// <summary>
        /// Rotiert die Kamera gemäß des gegebenen Bewegungsdeltas
        /// </summary>
        /// <param name="yawPitch">Bewegung in x-/y-Richtung (Delta)</param>
        public void AddCameraRotation(Vector2 yawPitch)
        {
            _cameraGame.YawAndPitch(yawPitch * KWEngine.MouseSensitivity);
        }

        /// <summary>
        /// Durchsucht alle TerrainObject-Instanzen und gibt das erste Suchergebnis zum angegebenen Namen zurück
        /// </summary>
        /// <param name="name">gesuchter Name</param>
        /// <returns>gefundenes Objekt (oder null)</returns>
        public TerrainObject GetTerrainObjectByName(string name)
        {
            TerrainObject t = _terrainObjects.Find(to => to.Name == name);
            return t;
        }

        /// <summary>
        /// Durchsucht alle TextObject-Instanzen und gibt das erste Suchergebnis zum angegebenen Namen zurück
        /// </summary>
        /// <param name="name">gesuchter Name</param>
        /// <returns>gefundenes Objekt (oder null)</returns>
        public TextObject GetTextObjectByName(string name)
        {
            name = name.Trim();
            TextObject t = _textObjects.Find(to => to.Name == name);
            return t;
        }

        /// <summary>
        /// Erstellt eine Liste aller GameObject-Instanzen mit einem bestimmten Namen
        /// </summary>
        /// <param name="name">gesuchter Name</param>
        /// <returns>Liste der gefundenen Instanzen</returns>
        public List<GameObject> GetGameObjectsByName(string name)
        {
            name = name.Trim();
            List<GameObject> os = _gameObjects.FindAll(go => go.Name == name).ToList();
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
            List<T> os = new();
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
            List<T> os = new();
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
        /// Durchsucht die Liste der RenderObject-Instanzen nach einem Objekt des gegebenen Typs mit dem gegebenen Namen
        /// </summary>
        /// <typeparam name="T">Klasse des gesuchten Objekts</typeparam>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public T GetRenderObjectByName<T>(string name) where T : class
        {
            name = name.Trim();
            RenderObject g = _renderObjects.FirstOrDefault(go => go is T && go.Name == name);
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
        /// Liefert eine Liste aller sich aktuell in der Welt befindenden RenderObject-Instanzen
        /// </summary>
        /// <returns>Liste aller RenderObject-Instanzen</returns>
        public IReadOnlyCollection<RenderObject> GetRenderObjects()
        {
            return _renderObjects.AsReadOnly();
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
        /// Durchsucht die Liste der HUDObject-Instanzen nach Objekten, die die angegebene Zeichenkette enthalten
        /// </summary>
        /// <param name="name">Zu suchende Zeichenkette</param>
        /// <returns>Liste der gefundenen Objekte</returns>
        public List<HUDObject> GetHUDObjectsByName(string name)
        {
            List<HUDObject> list = new();
            foreach(HUDObject h in _hudObjects)
            {
                if(h.Name != null && h.Name.Contains(name))
                {
                    list.Add(h);
                }
            }
            return list;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach einem Textobjekt mit dem gegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public HUDObjectText GetHUDObjectTextByName(string name)
        {
            name = name.Trim();
            HUDObject h = _hudObjects.FirstOrDefault(ho => ho is HUDObjectText && ho.Name == name);
            return h as HUDObjectText;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach HUDObjectText-Objekten, die die angegebene Zeichenkette enthalten
        /// </summary>
        /// <param name="name">Zu suchende Zeichenkette</param>
        /// <returns>Liste der gefundenen Objekte</returns>
        public List<HUDObjectText> GetHUDObjectTextsByName(string name)
        {
            List<HUDObjectText> list = new();
            foreach (HUDObject h in _hudObjects)
            {
                if (h is HUDObjectText && h.Name != null && h.Name.Contains(name))
                {
                    list.Add(h as HUDObjectText);
                }
            }
            return list;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach einem Bildobjekt mit dem gegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public HUDObjectImage GetHUDObjectImageByName(string name)
        {
            name = name.Trim();
            HUDObject h = _hudObjects.FirstOrDefault(ho => ho is HUDObjectImage && ho.Name == name);
            return h as HUDObjectImage;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach HUDObjectImage-Objekten, die die angegebene Zeichenkette enthalten
        /// </summary>
        /// <param name="name">Zu suchende Zeichenkette</param>
        /// <returns>Liste der gefundenen Objekte</returns>
        public List<HUDObjectImage> GetHUDObjectImagesByName(string name)
        {
            List<HUDObjectImage> list = new();
            foreach (HUDObject h in _hudObjects)
            {
                if (h is HUDObjectImage && h.Name != null && h.Name.Contains(name))
                {
                    list.Add(h as HUDObjectImage);
                }
            }
            return list;
        }

        /// <summary>
        /// Erfragt die HUDObjectTextInput-Instanz, die gerade den Eingabefokus hat
        /// </summary>
        /// <returns>Instanz (oder null, falls gerade kein Objekt den Eingabefokus hat)</returns>
        public HUDObjectTextInput GetHUDObjectTextInputWithFocus()
        {
            foreach (HUDObject h in _hudObjects)
            {
                if (h is HUDObjectTextInput && (h as HUDObjectTextInput).HasFocus)
                    return (h as HUDObjectTextInput);
            }
            return null;
        }

        /// <summary>
        /// Durchsucht die Liste der HUDObject-Instanzen nach einem Textobjekt mit Eingabefunktion und dem angegebenen Namen
        /// </summary>
        /// <param name="name">Name des gesuchten Objekts</param>
        /// <returns>Gesuchtes Objekt oder null (falls nicht gefunden)</returns>
        public HUDObjectTextInput GetHUDObjectTextInputByName(string name)
        {
            name = name.Trim();
            HUDObject h = _hudObjects.FirstOrDefault(ho => ho is HUDObjectTextInput && ho.Name == name);
            return h as HUDObjectTextInput;
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
        /// Setzt die vergangene Weltzeit zurück auf 0 Sekunden (ACHTUNG: Löscht alle vorhandenen Explosions- und Partikelobjekte sowie geplante Events in der Welt)
        /// </summary>
        /// <param name="initialTime">optional: setzt die Weltzeit auf die angegebene Sekundenzahl</param>
        public void ResetWorldTime(float initialTime = 0f)
        {
            _eventQueue.Clear();
            _particleAndExplosionObjects.Clear();
            KWEngine.WorldTime = Math.Max(0, initialTime);
        }

        /// <summary>
        /// Verweis auf Keyboardeingaben
        /// </summary>
        public static KeyboardExt Keyboard { get { return KWEngine.Window._keyboard; } }
        /// <summary>
        /// Verweis auf Mauseingaben
        /// </summary>
        public static MouseExt Mouse { get { return KWEngine.Window._mouse; } }
        /// <summary>
        /// Verweis auf das aktuelle Programmfenster
        /// </summary>
        public static GLWindow Window { get { return KWEngine.Window; } }

        /// <summary>
        /// Blickrichtung der Kamera
        /// </summary>
        public Vector3 CameraLookAtVector { get { return _cameraGame._stateCurrent.LookAtVector; } }
        /// <summary>
        /// Blickrichtung der Kamera ohne die Y-Achse zu berücksichtigen
        /// </summary>
        public Vector3 CameraLookAtVectorXZ { get { return Vector3.NormalizeFast(new Vector3(_cameraGame._stateCurrent.LookAtVector.X, 0.0000001f, _cameraGame._stateCurrent.LookAtVector.Z)); } }
        /// <summary>
        /// Blickrichtung der Kamera nach oben ohne die Y-Achse zu berücksichtigen
        /// </summary>
        public Vector3 CameraLookAtVectorLocalUpXZ { get { return Vector3.NormalizeFast(new Vector3(_cameraGame._stateCurrent.LookAtVectorLocalUp.X, 0.0000001f, _cameraGame._stateCurrent.LookAtVectorLocalUp.Z)); } }
        /// <summary>
        /// Blickrichtung der Kamera nach oben
        /// </summary>
        public Vector3 CameraLookAtVectorLocalUp { get { return _cameraGame._stateCurrent.LookAtVectorLocalUp; } }
        /// <summary>
        /// Blickrichtung der Kamera nach rechts ohne die Y-Achse zu berücksichtigen
        /// </summary>
        public Vector3 CameraLookAtVectorLocalRightXZ { get { return Vector3.NormalizeFast(new Vector3(_cameraGame._stateCurrent.LookAtVectorLocalRight.X, 0.0000001f, _cameraGame._stateCurrent.LookAtVectorLocalRight.Z)); } }
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
            unsafe
            {
                GLFW.SetCursorPos(Window.WindowPtr, Window.ClientSize.X / 2, Window.ClientSize.Y / 2);
            }
            Window.Mouse._mousePositionFromGLFW = Window.ClientSize / 2;
            Window.ResetMouseDeltas();
            _mouseCursorJustGrabbed = true;
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
            unsafe
            {
                GLFW.SetCursorPos(Window.WindowPtr, Window.ClientSize.X / 2, Window.ClientSize.Y / 2);
            }
            Window.Mouse._mousePositionFromGLFW = Window.ClientSize / 2;
            Window.ResetMouseDeltas();
            _startingFrameActive = true;
        }

        /// <summary>
        /// Setzt den Mauszeiger in seiner Position auf die angegebene Koordinate (relativ zum Fenster)
        /// </summary>
        /// <param name="x">X-Position im Fenster</param>
        /// <param name="y">Y-Position im Fenster</param>
        public void MouseCursorResetPosition(int x, int y)
        {
            x = Math.Clamp(x, 0, Window.ClientSize.X - 1);
            y = Math.Clamp(y, 0, Window.ClientSize.Y - 1);
            unsafe
            {
                GLFW.SetCursorPos(Window.WindowPtr, x, y);
            }
            Window.Mouse._mousePositionFromGLFW = new Vector2(x, y);
            Window.ResetMouseDeltas();
            _startingFrameActive = true;
            Keyboard.DeleteKeys();
            Mouse.DeleteButtons();
        }

        /// <summary>
        /// Setze den Blickwinkel der Kamera
        /// </summary>
        /// <param name="fov">Blickwinkel in Grad (zwischen 10 und 180)</param>
        public void SetCameraFOV(float fov)
        {
            _cameraGame.SetFOVForPerspectiveProjection(fov);
        }

        /// <summary>
        /// Erfragt die Liste der aktuellen GameObject-Instanzen der Welt
        /// </summary>
        /// <returns>Liste der aktuell aktiven Objekte</returns>
        public List<GameObject> GetGameObjects()
        {
            List<GameObject> gos = new();
            lock(_gameObjects)
            {
                foreach (GameObject gameObject in _gameObjects)
                {
                    gos.Add(gameObject);
                }
            }
            return gos;
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
        public void LoadJSON(string filename)
        {
            MethodBase caller = new StackTrace().GetFrame(1).GetMethod();
            string callerName = caller.Name;
            if (callerName != "Prepare")
            {
                KWEngine.LogWriteLine("[World] LoadJSON only allowed in Prepare()");
                return;
            }
            if (filename == null)
                filename = "";
            else
                filename = HelperGeneral.EqualizePathDividers(filename.Trim());

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

        /// <summary>
        /// Diese Methode wird im Falle eines geplanten Ereignisses aufgerufen, so dass das jeweilige Ereignis indidivuell verarbeitet werden kann.
        /// </summary>
        /// <param name="e">Ereignisinstanz</param>
        protected virtual void OnWorldEvent(WorldEvent e)
        {
            KWEngine.LogWriteLine("[World] WorldEvent ignoriert");
        }

        /// <summary>
        /// Fügt ein geplantes Weltereignis der weltinternen Ereignisliste hinzu
        /// </summary>
        /// <param name="e">Hinzufügendes Ereignisobjekt</param>
        public void AddWorldEvent(WorldEvent e)
        {
            if(e != null && e.Timestamp >= 0f)
            {
                e.Owner = this;
                _eventQueue.Add(e);
                _eventQueue.Sort();
            }
            else
            {
                KWEngine.LogWriteLine("[World] Ungültiges WorldEvent-Objekt");
            }
        }

        /// <summary>
        /// Gibt an, ob es gerade eine HUDObject-Instanz gibt, die den Eingabefokus hat
        /// </summary>
        public bool HasObjectWithActiveInputFocus
        {
            get { return _hudObjectInputWithFocus != null; }
        }
    }
}
