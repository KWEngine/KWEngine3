using Assimp;
using KWEngine3.GameObjects;
using KWEngine3.Model;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Kollisionen und Schnittoperationen
    /// </summary>
    public static class HelperIntersection
    {
        /// <summary>
        /// Ermittelt für die angegebenen Bildschirmkoordinaten, wo diese Koordinaten in der 3D-Welt liegen
        /// </summary>
        /// <param name="screenposX">X-Bildschirmkoordinate (relativ zum Anwendungsfenster, in Pixeln)</param>
        /// <param name="screenposY">Y-Bildschirmkoordinate (relativ zum Anwendungsfenster, in Pixeln)</param>
        /// <param name="planeNormal">Gibt an, wie die Fläche ausgerichtet ist, auf die die Bildschirmkoordinate projeziert wird</param>
        /// <param name="planeHeight">Gibt die Höhe der Fläche an, auf die die Bildschirmkoordinate projeziert wird</param>
        /// <returns>Schnittpunkt in 3D-Weltkoordinaten</returns>
        public static Vector3 GetIntersectionPointOnPlaneFromScreenSpace(int screenposX, int screenposY, Plane planeNormal = Plane.Camera, float planeHeight = 0f)
        {
            Vector3 normal;
            if (planeNormal == Plane.XZ)
                normal = new Vector3(0, 1, 0.0000001f);
            else if (planeNormal == Plane.YZ)
                normal = new Vector3(1, 0, 0);
            else if (planeNormal == Plane.XY)
                normal = new Vector3(0, 0.0000001f, 1);
            else
            {
                if (KWEngine.CurrentWorld != null)
                {
                    normal = -KWEngine.CurrentWorld._cameraGame._stateCurrent.LookAtVector;
                }
                else
                {
                    normal = new Vector3(0, 1, 0.000001f);
                }
            }


            screenposX = MathHelper.Clamp(screenposX, 0, KWEngine.Window.Width - 1);
            screenposY = MathHelper.Clamp(screenposY, 0, KWEngine.Window.Height - 1);
            Vector3 worldRay = HelperGeneral.Get3DMouseCoords(new Vector2(screenposX, screenposY));
            bool result;
            Vector3 intersection;
            result = LinePlaneIntersection(out intersection, worldRay, KWEngine.CurrentWorld._cameraGame._stateCurrent._position, normal, normal * planeHeight);

            if (result)
            {
                return intersection;
            }
            else
                return normal * planeHeight;
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebene Position nach unten und prüft, ob die quaderförmige Hitbox eines Objekts des angegebenen Typs geschnitten wird
        /// </summary>
        /// <remarks>Diese Variante prüft nur grobe Hitbox-Werte und eignet sich, wenn ein ungenaues Ergebnis ausreicht oder die zu prüfenden Objekte allesamt achsenparallel ausgerichtet sind</remarks>
        /// <param name="position">Startposition des Strahls</param>
        /// <param name="rayLength">Positive Maximallänge des Teststrahls (0 = unendlich)</param>
        /// <param name="typelist">Typen für die getestet werden soll [z.B. typeof(GameObject) oder typeof(Floor)]</param>
        /// <returns>Ergebnis der Strahlenprüfung</returns>
        public static RayIntersection IsAnyObjectBelowPositionFast(Vector3 position, float rayLength, params Type[] typelist)
        {
            List<RayIntersection> list = RayTraceObjectsBelowPositionFast(position, rayLength, true, typelist);
            if(list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new RayIntersection();
            }
        }

        /// <summary>
        /// Schießt einen Strahl von der angegebene Position nach unten und prüft, ob irgendein anderes Objekt der Welt davon getroffen wird
        /// </summary>
        /// <param name="position">Startposition des Strahls</param>
        /// <param name="rayLength">Positive Maximallänge des Teststrahls (0 = unendlich)</param>
        /// <param name="typelist">Typen für die getestet werden soll [z.B. typeof(GameObject) oder typeof(Floor)]</param>
        /// <returns>Ergebnis des Raytracings</returns>
        public static RayIntersectionExt IsAnyObjectBelowPosition(Vector3 position, float rayLength, params Type[] typelist)
        {
            List<RayIntersectionExt> list = RayTraceObjectsBelowPosition(position, rayLength, true, typelist);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new RayIntersectionExt();
            }
        }

        /// <summary>
        /// Prüft auf Kollisionen des übergebenen Objekts mit anderen Objekten unter Berücksichtigung einer Verschiebung (offset)
        /// </summary>
        /// <param name="caller">Das Objekt für das auf Kollisionen geprüft werden soll</param>
        /// <param name="offsetX">Verschiebung entlang der globalen X-Achse</param>
        /// <param name="offsetY">Verschiebung entlang der globalen Y-Achse</param>
        /// <param name="offsetZ">Verschiebung entlang der globalen Z-Achse</param>
        /// <returns>Liste mit Kollisionsobjekten</returns>
        public static List<Intersection> GetIntersectionsForObjectWithOffset(GameObject caller, float offsetX, float offsetY, float offsetZ)
        {
            return GetIntersectionsForObjectWithOffset(caller, new Vector3(offsetX, offsetY, offsetZ));
        }

        /// <summary>
        /// Prüft auf Kollisionen des übergebenen Objekts mit anderen Objekten unter Berücksichtigung einer Verschiebung (offset)
        /// </summary>
        /// <param name="caller">Das Objekt für das auf Kollisionen geprüft werden soll</param>
        /// <param name="offset">Verschiebung entlang der globalen XYZ-Achse</param>
        /// <returns>Liste mit Kollisionsobjekten</returns>
        public static List<Intersection> GetIntersectionsForObjectWithOffset(GameObject caller, Vector3 offset)
        {
            List<Intersection> intersections = new List<Intersection>();
            foreach(GameObjectHitbox hbCaller in caller._colliderModel._hitboxes)
            {
                foreach(GameObjectHitbox hbOther in caller._collisionCandidates)
                {
                    Intersection i = TestIntersection(hbCaller, hbOther, offset);
                    if(i != null)
                    {
                        intersections.Add(i);
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Prüft die Position eines Terrain-Objekts unterhalb der angegebenen Position
        /// </summary>
        /// <param name="position">Position für die getestet werden soll</param>
        /// <param name="intersectionPoint">gemessener Schnittpunkt mit dem Terrain</param>
        /// <param name="distance">Distanz von der angegebenen Position zum Schnittpunkt</param>
        /// <returns>true, wenn gerade ein Terrain unter der angegebenen Position liegt, andernfalls false</returns>
        public static bool GetPositionOnTerrainBelow(Vector3 position, out Vector3 intersectionPoint, out float distance)
        {
            intersectionPoint = Vector3.Zero;
            distance = -1;
            foreach(TerrainObject t in KWEngine.CurrentWorld.GetTerrainObjects())
            {
                Vector3 untranslatedPosition = position - new Vector3(t._hitboxes[0]._center.X, 0, t._hitboxes[0]._center.Z);
                Sector s = t._gModel.ModelOriginal.Meshes.ElementAt(0).Value.Terrain.GetSectorForUntranslatedPosition(untranslatedPosition);
                if (s != null)
                {
                    GeoTerrainTriangle? tris = s.GetTriangle(ref untranslatedPosition);
                    if (tris.HasValue)
                    {
                        bool rayHasContact = RayTriangleIntersection(untranslatedPosition, -KWEngine.WorldUp, tris.Value.Vertices[0], tris.Value.Vertices[1], tris.Value.Vertices[2], out Vector3 contactPoint);
                        if(rayHasContact)
                        {
                            intersectionPoint = contactPoint;
                            distance = (intersectionPoint - position).LengthFast;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Prüft die Position eines Terrain-Objekts unterhalb der angegebenen Position
        /// </summary>
        /// <param name="positionX">X-Komponente der Position für die getestet werden soll</param>
        /// <param name="positionY">Y-Komponente der Position für die getestet werden soll</param>
        /// <param name="positionZ">Z-Komponente der Position für die getestet werden soll</param>
        /// <param name="intersectionPoint">gemessener Schnittpunkt mit dem Terrain</param>
        /// <param name="distance">Distanz von der angegebenen Position zum Schnittpunkt</param>
        /// <returns>true, wenn gerade ein Terrain unter der angegebenen Position liegt, andernfalls false</returns>
        public static bool GetPositionOnTerrainBelow(float positionX, float positionY, float positionZ, out Vector3 intersectionPoint, out float distance)
        {
            return GetPositionOnTerrainBelow(new Vector3(positionX, positionY, positionZ), out intersectionPoint, out distance);
        }

        /// <summary>
        /// Prüft auf Kollisionen für ein Objekt g in Kombinationen mit allen (auch geplanten) anderen GameObject-Instanzen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject">Instanz, für die auf Kollisionen geprüft werden soll</param>
        /// <returns>Liste der Kollisionen</returns>
        public static List<Intersection> GetIntersectionsForAllObjects<T>(GameObject gameObject) where T : GameObject
        {
            List<Intersection> intersections = new List<Intersection>();

            foreach(GameObject g in KWEngine.CurrentWorld._gameObjectsToBeAdded)
            {
                if(g == gameObject)
                    continue;
                
                foreach (GameObjectHitbox hbCaller in gameObject._colliderModel._hitboxes)
                {
                    foreach(GameObjectHitbox hbother in g._colliderModel._hitboxes)
                    {
                        Intersection i = TestIntersection(hbCaller, hbother, HelperVector.VectorZero);
                        if (i != null)
                            intersections.Add(i);
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Prüft auf Kollisionen für ein Objekt g in Kombinationen mit allen (auch geplanten) anderen GameObject-Instanzen
        /// </summary>
        /// <param name="gameObject">Instanz, für die auf Kollisionen geprüft werden soll</param>
        /// <param name="typelist">Auflistung der zu prüfenden Datentypen (müssen Unterklassen von GameObject sein)</param>
        /// <returns>Liste der Kollisionen</returns>
        public static List<Intersection> GetIntersectionsForAllObjects(GameObject gameObject, params Type[] typelist)
        {
            List<Intersection> intersections = new List<Intersection>();
            foreach (GameObject g in KWEngine.CurrentWorld._gameObjectsToBeAdded)
            {
                if (g == gameObject)
                    continue;
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    foreach (GameObjectHitbox hbCaller in gameObject._colliderModel._hitboxes)
                    {
                        foreach (GameObjectHitbox hbother in g._colliderModel._hitboxes)
                        {
                            Intersection i = TestIntersection(hbCaller, hbother, HelperVector.VectorZero);
                            if (i != null)
                                intersections.Add(i);
                        }
                    }
                }
            }

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (g == gameObject)
                    continue;
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    foreach (GameObjectHitbox hbCaller in gameObject._colliderModel._hitboxes)
                    {
                        foreach (GameObjectHitbox hbother in g._colliderModel._hitboxes)
                        {
                            Intersection i = TestIntersection(hbCaller, hbother, HelperVector.VectorZero);
                            if (i != null)
                                intersections.Add(i);
                        }
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Gibt die aktuelle Anzahl potenzieller Kollisionskandidaten an, die sich in der Nähe des Objekts befinden
        /// </summary>
        /// <param name="g">zu prüfendes Objekt</param>
        /// <returns>Anzahl potenzieller Kollisionskandidaten</returns>
        public static int GetCollisionCandidateCountFor(GameObject g)
        {
            int c = 0;
            lock (g._collisionCandidates)
            {
                c = g._collisionCandidates.Count;
            }
            return c;
        }

        /// <summary>
        /// Gibt zu Testzwecken eine Zeichenkette mit den Namen aller für die Kollsionsbehandlung 
        /// untersuchten GameObject-Instanzen zurück
        /// </summary>
        /// <param name="g">betroffenes GameObject</param>
        /// <returns>Liste der Instanznamen</returns>
        public static string GetCollisionCandidateNamesFor(GameObject g)
        {
            string result = "";
            lock (g._collisionCandidates)
            {
                foreach (GameObjectHitbox hb in g._collisionCandidates)
                {
                    result += hb.Owner.Name + ", ";
                }
            }

            return result.Substring(0, Math.Max(0, result.Length - 2));
        }

        /// <summary>
        /// Berechnet die Richtung von der Kameraposition zum Mauszeiger in 3D
        /// </summary>
        /// <returns>Blickrichtung der Kamera in Richtung des Mauszeigers</returns>
        public static Vector3 GetMouseRay()
        {
            Vector2 mc;
            if (KWEngine.Window.CursorState == OpenTK.Windowing.Common.CursorState.Grabbed)
            {
                mc = KWEngine.Window.ClientRectangle.HalfSize;
            }
            else
            {
                mc = KWEngine.Window.MousePosition;
            }

            return HelperGeneral.Get3DMouseCoords(mc);
        }

        /// <summary>
        /// Gibt die aktuellen Mauszeigerursprungskoordinaten zurück (entspricht immer der Kameraposition)
        /// </summary>
        /// <returns>Mauszeigerursprungskoordinaten</returns>
        public static Vector3 GetMouseOrigin()
        {
            Vector3 result = Vector3.Zero;
            if(KWEngine.CurrentWorld != null)
            {
                return KWEngine.CurrentWorld.CameraPosition;
            }
            return result;
        }

        /// <summary>
        /// Berechnet den ungefähren Schnittpunkt des Mauszeigers mit einem Terrain-Objekt.
        /// </summary>
        /// <param name="intersectionPoint">Ausgabe des Schnittpunkts (wenn der Mauszeiger über einem Terrain-Objekt liegt - sonst (0|0|0))</param>
        /// <returns>true, wenn der Mauszeiger über einem Terrain-Objekt liegt</returns>
        public static bool GetMouseIntersectionPointOnAnyTerrain(out Vector3 intersectionPoint)
        {
            foreach (TerrainObject to in KWEngine.CurrentWorld._terrainObjects)
            {
                if (to.IsCollisionObject)
                {
                    Vector3 miPointLow = GetMouseIntersectionPointOnPlane(Plane.XZ, to._stateCurrent._center.Y);
                    if (RayTerrainIntersection(to, miPointLow + new Vector3(0, to._hitboxes[0]._center.Y + to._hitboxes[0]._dimensions.Y, 0), -Vector3.UnitY, out intersectionPoint))
                    {
                        return true;
                    }
                }
            }
            intersectionPoint = Vector3.Zero;
            return false;
        }

        /// <summary>
        /// Konvertiert 2D-Mauskoordinaten in 3D-Koordinaten
        /// </summary>
        /// <param name="planeNormal">Kollisionsebene (Standard: Camera)</param>
        /// <param name="planeHeight">Höhe der Kollisionsebene</param>
        /// <returns>3D-Mauskoordinaten</returns>
        public static Vector3 GetMouseIntersectionPointOnPlane(Plane planeNormal = Plane.Camera, float planeHeight = 0f)
        {
            Vector3 normal;
            if (planeNormal == Plane.XZ)
                normal = new Vector3(0, 1, 0.000001f);
            else if (planeNormal == Plane.YZ)
                normal = new Vector3(1, 0, 0);
            else if (planeNormal == Plane.XY)
                normal = new Vector3(0, 0.000001f, 1);
            else
            {
                if (KWEngine.CurrentWorld != null)
                {
                    normal = -KWEngine.CurrentWorld._cameraGame._stateCurrent.LookAtVector;
                }
                else
                {
                    normal = new Vector3(0, 1, 0.000001f);
                }
            }

            Vector2 mc;
            if (KWEngine.Window.CursorState == OpenTK.Windowing.Common.CursorState.Grabbed)
            {
                mc = KWEngine.Window.ClientRectangle.HalfSize;
            }
            else
            {
                mc = KWEngine.Window.MousePosition;
            }

            Vector3 worldRay = HelperGeneral.Get3DMouseCoords(mc);
            bool result;
            Vector3 intersection;
            result = LinePlaneIntersection(out intersection, worldRay, KWEngine.CurrentWorld._cameraGame._stateCurrent._position, normal, normal * planeHeight);

            if (result)
            {
                return intersection;
            }
            else
                return normal * planeHeight;
        }

        /// <summary>
        /// Erfragt den Kollisionspunkt des Mauszeigers mit der 3D-Welt auf Höhe der angegebenen Position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="plane">Kollisionsebene (Standard: Camera)</param>
        /// <returns></returns>
        public static Vector3 GetMouseIntersectionPointOnPlane(Vector3 position, Plane plane = Plane.Camera)
        {
            Vector2 mc;
            if (KWEngine.Window.CursorState == OpenTK.Windowing.Common.CursorState.Grabbed)
            {
                mc = KWEngine.Window.ClientRectangle.HalfSize;
            }
            else
            {
                mc = KWEngine.Window.MousePosition;
            }
            Vector3 worldRay = HelperGeneral.Get3DMouseCoords(mc);
            Vector3 normal;
            if (plane == Plane.XZ)
            {
                normal = new Vector3(0, 1, 0.000001f);
            }
            else if (plane == Plane.YZ)
            {
                normal = new Vector3(1, 0, 0);
            }
            else if (plane == Plane.XY)
            {
                normal = new Vector3(0, 0.000001f, 1);
            }
            else
            {
                if (KWEngine.CurrentWorld != null)
                {
                    normal = -KWEngine.CurrentWorld._cameraGame._stateCurrent.LookAtVector;
                }
                else
                {
                    normal = new Vector3(0, 1, 0.000001f);
                }
            }
            bool contact;
            Vector3 intersection;
            contact = LinePlaneIntersection(out intersection, worldRay, KWEngine.CurrentWorld._cameraGame._stateCurrent._position, normal, position);
            if (contact)
            {
                return intersection;
            }
            else
                return position;
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger über einem Objekt des angegebenen Typs liegt und gibt ggf. diese Instanz über den out-Parameter zurück
        /// </summary>
        /// <remarks>
        /// Achtung: Methode ist sehr präzise aber daher auch sehr langsam! Für schnellere Ergebnisse sollte die Methode IsMouseCursorOnAnyFast() verwendet werden
        /// </remarks>
        /// <typeparam name="T">Beliebige Unterklasse von GameObject</typeparam>
        /// <param name="gameObject">Gefundene GameObject-Instanz (nur gefüllt, wenn der Rückgabewert true ist)</param>
        /// <param name="includeNonCollisionObjects">Sollen auch Objekte einbezogen werden, die nicht als Kollisionsobjekte markiert sind?</param>
        /// <returns>true, wenn der Mauscursor auf einem Objekt der angegebenen Art ist</returns>
        public static bool IsMouseCursorOnAny<T>(out T gameObject, bool includeNonCollisionObjects = true) where T : GameObject
        {
            gameObject = null;
            GameObject[] list;
            if(includeNonCollisionObjects)
                list = KWEngine.CurrentWorld._gameObjects.FindAll(go => go is T).ToArray();
            else
                list = KWEngine.CurrentWorld._gameObjects.FindAll(go => go is T && go.IsCollisionObject).ToArray();
            if (list.Length == 0)
                return false;


            Vector3 rayDirection = GetMouseRay();
            Vector3 rayOrigin = KWEngine.CurrentWorld._cameraGame._stateCurrent._position;

            float minDistance = float.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < list.Length; i++)
            {
                bool rayHitGameObject = IsMouseCursorOnGameObject(list[i], rayOrigin, rayDirection, true);
                if (rayHitGameObject)
                {
                    float currentDistance = (rayOrigin - list[i].Center).LengthSquared;
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        minIndex = i;
                    }
                }
            }
            if (minIndex >= 0)
            {
                gameObject = list[minIndex] as T;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob der Mauszeiger über der achsenparallelen Hitbox eines Objekts des angegebenen Typs liegt und gibt ggf. diese Instanz über den out-Parameter zurück
        /// </summary>
        /// <remarks>
        /// Achtung: Methode ist sehr performant aber sehr ungenau! Für pixelgenaue Ergebnisse sollte stattdessen die Methode IsMouseCursorOnAny() verwendet werden.
        /// </remarks>
        /// <typeparam name="T">Beliebige Unterklasse von GameObject</typeparam>
        /// <param name="gameObject">Gefundene GameObject-Instanz (nur gefüllt, wenn der Rückgabewert true ist)</param>
        /// <param name="includeNonCollisionObjects">Sollen auch Objekte einbezogen werden, die nicht als Kollisionsobjekte markiert sind?</param>
        /// <returns>true, wenn der Mauscursor auf einem Objekt der angegebenen Art ist</returns>
        public static bool IsMouseCursorOnAnyFast<T>(out T gameObject, bool includeNonCollisionObjects = true) where T : GameObject
        {
            gameObject = null;
            GameObject[] list;
            if (includeNonCollisionObjects)
                list = KWEngine.CurrentWorld._gameObjects.FindAll(go => go is T).ToArray();
            else
                list = KWEngine.CurrentWorld._gameObjects.FindAll(go => go is T && go.IsCollisionObject).ToArray();
            if (list.Length == 0)
                return false;


            Vector3 rayDirection = GetMouseRay();
            Vector3 rayOrigin = KWEngine.CurrentWorld._cameraGame._stateCurrent._position;
            float minDistance = float.MaxValue;
            int minIndex = -1;
            rayDirection.X = 1f / rayDirection.X;
            rayDirection.Y = 1f / rayDirection.Y;
            rayDirection.Z = 1f / rayDirection.Z;

            for (int i = 0; i < list.Length; i++)
            {
                bool rayHitGameObject = RayAABBIntersection(rayOrigin, rayDirection, list[i]._stateCurrent._center, list[i]._stateCurrent._dimensions, out float currentDistance);
                if (rayHitGameObject && currentDistance >= 0)
                {
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        minIndex = i;
                    }
                }
            }
            if (minIndex >= 0)
            {
                gameObject = list[minIndex] as T;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob der angegebene Strahl (origin, direction) auf die Hitbox von Objekten bestimmter Klassen trifft
        /// </summary>
        /// <param name="origin">Ausgangspunkt des Strahls</param>
        /// <param name="direction">Richtung des Strahls (MUSS normalisiert sein)</param>
        /// <param name="caller">Aufruferinstanz, damit die Instanz sich nicht selbst überprüft</param>
        /// <param name="maxDistance">maximale Länge des Strahls</param>
        /// <param name="typelist">Klassen, deren Objekte geprüft werden sollen (mehrere möglich)</param>
        /// <param name="sort">Wenn true, wird die Ergebnisliste aufsteigend nach Objektentfernung sortiert</param>
        /// <returns>Liste der Strahlentreffer</returns>
        public static List<RayIntersection> RayTraceObjectsForViewVectorFast(Vector3 origin, Vector3 direction, GameObject caller, float maxDistance, bool sort, params Type[] typelist)
        {
            List<RayIntersection> list = new List<RayIntersection>();
            if (maxDistance <= 0)
            {
                maxDistance = float.MaxValue;
            }

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (g != caller && HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    ConvertRayToMeshSpaceForAABBTest(ref origin, ref direction, ref g._stateCurrent._modelMatrixInverse, out Vector3 originTransformed, out Vector3 directionTransformed);
                    Vector3 directionTransformedInv = new Vector3(1f / directionTransformed.X, 1f / directionTransformed.Y, 1f / directionTransformed.Z);

                    foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
                    {
                        bool result = RayAABBIntersection(originTransformed, directionTransformedInv, hb._mesh.Center, new Vector3(hb._mesh.width, hb._mesh.height, hb._mesh.depth), out float currentDistance);
                        if (result == true)
                        {
                            ConvertRayToWorldSpaceAfterAABBTest(ref originTransformed, ref directionTransformed, currentDistance, ref g._stateCurrent._modelMatrix, ref origin, out Vector3 intersectionPoint, out float distanceWorldspace);
                            if (distanceWorldspace >= 0 && distanceWorldspace <= maxDistance)
                            {
                                RayIntersection gd = new RayIntersection()
                                {
                                    Distance = (intersectionPoint - origin).LengthFast,
                                    Object = g,
                                    IntersectionPoint = intersectionPoint
                                };
                                list.Add(gd);
                            }
                        }
                    }
                }
            }

            if (sort)
                list.Sort();

            return list;
        }

        /// <summary>
        /// Prüft, ob der angegebene Strahl (origin, direction) auf die achsenparallele Hitbox von Objekten bestimmter Klassen trifft
        /// </summary>
        /// <param name="origin">Ausgangspunkt des Strahls</param>
        /// <param name="direction">Richtung des Strahls (MUSS normalisiert sein)</param>
        /// <param name="caller">Aufruferinstanz, damit die Instanz sich nicht selbst überprüft</param>
        /// <param name="maxDistance">maximale Länge des Strahls</param>
        /// <param name="typelist">Klassen, deren Objekte geprüft werden sollen (mehrere möglich)</param>
        /// <param name="sort">Wenn true, wird die Ergebnisliste aufsteigend nach Objektentfernung sortiert</param>
        /// <returns>Liste der Strahlentreffer</returns>
        public static List<RayIntersection> RayTraceObjectsForViewVectorFastest(Vector3 origin, Vector3 direction, GameObject caller, float maxDistance, bool sort,  params Type[] typelist) 
        {
            direction.X = 1f / direction.X;
            direction.Y = 1f / direction.Y;
            direction.Z = 1f / direction.Z;

            List<RayIntersection> list = new List<RayIntersection>();
            if (maxDistance <= 0)
            {
                maxDistance = float.MaxValue;
            }

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (g == caller)
                    continue;
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {

                    bool result = RayAABBIntersection(origin, direction, g._stateCurrent._center, g._stateCurrent._dimensions, out float currentDistance);
                    if (result == true && currentDistance >= 0)
                    {
                        if (maxDistance > 0 && currentDistance <= maxDistance)
                        {
                            RayIntersection gd = new RayIntersection()
                            {
                                Distance = currentDistance,
                                Object = g
                            };
                            list.Add(gd);
                        }
                    }
                }
            }

            if(sort)
                list.Sort();

            return list;
        }

        /// <summary>
        /// Prüft, welche Objekte (bzw. deren Hitboxen) in der angegebenen Blickrichtung liegen und gibt diese als Liste zurück
        /// </summary>
        /// <param name="origin">Ursprung des Strahls</param>
        /// <param name="direction">Blickrichtung des Strahls (MUSS normalisiert sein!)</param>
        /// <param name="maxDistance">Objekte weiter weg als dieser Wert werden ignoriert (Standard: 0 = unendliche Entfernung)</param>
        /// <param name="sort">true, falls die Objekte ihrer Entfernung nach aufsteigend sortiert werden sollen</param>
        /// <param name="caller">Aufrufendes Objekt, das bei Nennung ignoriert wird</param>
        /// <param name="typelist">Liste der Klassen, die getestet werden sollen</param>
        /// <returns>Liste der GameObjectDistance-Instanzen</returns>
        public static List<RayIntersectionExt> RayTraceObjectsForViewVector(Vector3 origin, Vector3 direction, float maxDistance, bool sort, GameObject caller, params Type[] typelist)
        {
            List<RayIntersectionExt> list = new List<RayIntersectionExt>();
            if (maxDistance <= 0)
            {
                maxDistance = float.MaxValue;
            }

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {

                if (caller == g)
                    continue;
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    bool result = RaytraceObject(g, origin, direction, out Vector3 intersectionPoint, out Vector3 normal, out string hitboxname);
                    if (result)
                    {
                        float currentDistance = (intersectionPoint - origin).LengthFast;
                        if (maxDistance > 0 && currentDistance <= maxDistance)
                        {
                            RayIntersectionExt gd = new RayIntersectionExt()
                            {
                                Distance = currentDistance,
                                IntersectionPoint = intersectionPoint,
                                SurfaceNormal = normal,
                                HitboxName = hitboxname,
                                Object = g
                            };
                            list.Add(gd);
                        }
                    }
                }
            }

            if (sort)
            {
                list.Sort();
            }
            return list;
        }

        internal static bool GetRayIntersectionPointOnGameObject(GameObject g, Vector3 origin, Vector3 worldRay, out Vector3 intersectionPoint, out string hitboxname)
        {
            return RaytraceObject(g, origin, worldRay, out intersectionPoint, out Vector3 normal, out hitboxname, true);
        }

        internal static bool RaytraceHitbox(GameObjectHitbox hb, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersectionPoint, out Vector3 faceNormal)
        {
            intersectionPoint = Vector3.Zero;
            faceNormal = Vector3.Zero;
            for (int j = 0; j < hb._mesh.Faces.Length; j++)
            { 
                //if (hb.IsExtended)
                //{
                    Span<Vector3> faceVertices = stackalloc Vector3[hb._mesh.Faces[j].VertexCount];
                    if(hb.GetVerticesFromFaceAndCheckAngle(j, rayDirection, ref faceVertices, out HitboxFace currentFace))
                    {
                        bool hit = RayNGonIntersection(rayOrigin, rayDirection, currentFace.Normal, ref faceVertices, out Vector3 currentContact);
                        //bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out Vector3 currentContact);
                        if (hit)
                        {
                            faceNormal = currentFace.Normal;
                            intersectionPoint = currentContact;
                            return true;
                        }
                    }
                    //hb.GetVerticesForTriangleFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 currentFaceNormal);
                    //float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                    //if (dot < 0)
                    //{
                        
                    //}
                //}
                /*
                else
                {
                    hb.GetVerticesForCubeFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 currentFaceNormal);
                    float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                    if (dot < 0)
                    {
                        Vector3 currentContact;
                        bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out currentContact);
                        if (!hit)
                        {
                            hit = RayTriangleIntersection(rayOrigin, rayDirection, v4, v5, v6, out currentContact);
                        }
                        if (hit)
                        {
                            faceNormal = currentFaceNormal;
                            intersectionPoint = currentContact;
                            return true;
                        }
                    }
                }
                */
            }
            return false;
        }

        /// <summary>
        /// Berechnet den Schnittpunkt eines Strahls mit dem angegebenen GameObject
        /// </summary>
        /// <param name="g">zu prüfendes GameObject</param>
        /// <param name="rayOrigin">Ursprungsposition des Strahls</param>
        /// <param name="rayDirection">Richtung des Strahls (MUSS normalisiert sein!)</param>
        /// <param name="intersectionPoint">Schnittpunkt (Ausgabe)</param>
        /// <param name="faceNormal">Ebene des Schnittpunkts (Ausgabe)</param>
        /// <param name="hitboxname">Name der getroffenen Hitbox</param>
        /// <param name="includeNonCollisionObjects">Sollen Objekte berücksichtigt werden, die NICHT als Kollisionsobjekt markiert sind?</param>
        /// <returns>true, wenn der Strahl das GameObject getroffen hat</returns>
        public static bool RaytraceObject(GameObject g, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersectionPoint, out Vector3 faceNormal, out string hitboxname, bool includeNonCollisionObjects = true)
        {
            faceNormal = KWEngine.WorldUp;
            intersectionPoint = new Vector3();
            hitboxname = "";
            if (g == null || (includeNonCollisionObjects == false && !g.IsCollisionObject))
            {
                return false;
            }


            int resultSum = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < g._colliderModel._hitboxes.Count; i++)
            {
                GameObjectHitbox currentHitbox = g._colliderModel._hitboxes[i];
                for (int j = 0; j < currentHitbox._mesh.Faces.Length; j++)
                {
                    GeoMeshFace face = currentHitbox._mesh.Faces[j];
                    Span<Vector3> faceVertices = stackalloc Vector3[face.Vertices.Length];
                    currentHitbox.GetVerticesFromFace(j, ref faceVertices, out Vector3 currentFaceNormal);
                    float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                    if (dot < 0)
                    {
                        bool hit = RayNGonIntersection(rayOrigin, rayDirection, currentFaceNormal, ref faceVertices, out Vector3 currentContact);
                        if (hit)
                        {
                            float currentDistance = (rayOrigin - currentContact).LengthSquared;
                            if (currentDistance < minDistance)
                            {
                                resultSum++;
                                hitboxname = currentHitbox._mesh.Name;
                                faceNormal = currentFaceNormal;
                                intersectionPoint = currentContact;
                                minDistance = currentDistance;
                            }
                        }
                    }
                }
            }
            return resultSum > 0;
        }

        /// <summary>
        /// Prüft, ob ein Strahl die Hitbox der angegebenen GameObject-Instanz trifft
        /// </summary>
        /// <remarks>
        /// Diese Methode ist schnell aber unpräzise, da sie sich an der quaderförmigen Hitbox des Objekts orientiert.
        /// </remarks>
        /// <param name="g">zu prüfendes GameObject</param>
        /// <param name="rayOrigin">Ursprungsposition des Strahls</param>
        /// <param name="rayDirection">Richtung des Strahls (MUSS normalisiert sein!)</param>
        /// <returns>true, wenn der Strahl das GameObject trifft</returns>
        public static bool RaytraceObjectFast(GameObject g, Vector3 rayOrigin, Vector3 rayDirection)
        {
            ConvertRayToMeshSpaceForAABBTest(ref rayOrigin, ref rayDirection, ref g._stateCurrent._modelMatrixInverse, out Vector3 originTransformed, out Vector3 directionTransformed);
            Vector3 directionTransformedInv = new Vector3(1f / (directionTransformed.X == 0f ? KWEngine.RAYTRACE_EPSILON : directionTransformed.X), 1f / (directionTransformed.Y == 0f ? KWEngine.RAYTRACE_EPSILON : directionTransformed.Y), 1f / (directionTransformed.Z == 0f ? KWEngine.RAYTRACE_EPSILON : directionTransformed.Z));
            foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                bool result = RayAABBIntersection(originTransformed, directionTransformedInv, hb._mesh.Center, new Vector3(hb._mesh.width, hb._mesh.height, hb._mesh.depth), out float currentDistance);
                if(result == true)
                {
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob ein Strahl die achsenparallele Hitbox der angegebenen GameObject-Instanz trifft
        /// </summary>
        /// <remarks>
        /// Diese Methode ist schnell aber sehr unpräzise, da sie die Rotation von Objekten nur näherungsweise einbezieht.
        /// Es kann vermehrt zu falsch-positiven Rückmeldungen kommen.
        /// </remarks>
        /// <param name="g">zu prüfendes GameObject</param>
        /// <param name="rayOrigin">Ursprungsposition des Strahls</param>
        /// <param name="rayDirection">Richtung des Strahls (MUSS normalisiert sein!)</param>
        /// <returns>true, wenn der Strahl das GameObject trifft</returns>
        public static bool RaytraceObjectFastest(GameObject g, Vector3 rayOrigin, Vector3 rayDirection)
        {
            rayDirection.X = 1f / rayDirection.X;
            rayDirection.Y = 1f / rayDirection.Y;
            rayDirection.Z = 1f / rayDirection.Z;

            bool result = RayAABBIntersection(rayOrigin, rayDirection, g._stateCurrent._center, g._stateCurrent._dimensions, out float d);
            if (result == true && d >= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob ein Punkt innerhalb einer Box liegt.
        /// </summary>
        /// <param name="pos">Zu prüfender Punkt</param>
        /// <param name="center">Zentrum der Box</param>
        /// <param name="width">Breite der Box</param>
        /// <param name="height">Breite der Box</param>
        /// <param name="depth">Breite der Box</param>
        /// <returns>true, wenn der Punkt innerhalb der Box liegt</returns>
        public static bool IsPointInsideBox(Vector3 pos, Vector3 center, float width, float height, float depth)
        {
            return
                pos.X >= center.X - width / 2 &&
                pos.X <= center.X + width / 2 &&
                pos.Y >= center.Y - height / 2 &&
                pos.Y <= center.Y + height / 2 &&
                pos.Z >= center.Z - depth / 2 &&
                pos.Z <= center.Z + depth / 2;
        }

        /// <summary>
        /// Prüft, ob ein Punkt innerhalb eines Rechtecks auf XZ-Ebene liegt.
        /// </summary>
        /// <param name="pos">Zu prüfender Punkt</param>
        /// <param name="center">Zentrum des Rechtecks</param>
        /// <param name="width">Breite des Rechtecks</param>
        /// <param name="depth">Breite des Rechtecks</param>
        /// <returns>true, wenn der Punkt innerhalb des Rechtecks liegt</returns>
        public static bool IsPointInsideRectangle(Vector3 pos, Vector3 center, float width, float depth)
        {
            return
                pos.X >= center.X - width * 0.5f &&
                pos.X <= center.X + width * 0.5f &&
                pos.Z >= center.Z - depth * 0.5f &&
                pos.Z <= center.Z + depth * 0.5f;
        }

        /// <summary>
        /// Prüft, ob sich ein Punkt innerhalb einer Kugel befindet
        /// </summary>
        /// <param name="point">zu prüfender Punkt</param>
        /// <param name="sphereCenter">Zentrum der Kugel</param>
        /// <param name="diameter">Durchmesser der Kugel</param>
        /// <returns></returns>
        public static bool IsPointInsideSphere(Vector3 point, Vector3 sphereCenter, float diameter)
        {
            return (point - sphereCenter).LengthFast <= diameter / 2f;
        }

        /// <summary>
        /// Prüft, ob sich ein Punkt innerhalb der Hitbox einer GameObject-Instanz befindet
        /// </summary>
        /// <param name="point">Zu prüfender Punkt in der Welt</param>
        /// <param name="g">GameObject-Instanz für die die Prüfung durchgeführt werden soll</param>
        /// <returns>true, wenn sich der Punkt innerhalb der Hitbox befindet</returns>
        public static bool IsPointInsideGameObject(Vector3 point, GameObject g)
        {
            foreach (GameObjectHitbox h in g._colliderModel._hitboxes)
            {
                bool isInsideCurrentHitbox = true;
                for (int boxFaceIndex = 0; boxFaceIndex < h._mesh.Faces.Length; boxFaceIndex++)
                {
                    GeoMeshFace boxFace = h._mesh.Faces[boxFaceIndex];
                    Vector3 boxNormal = boxFace.Flip ? h._normals[boxFace.Normal] : -h._normals[boxFace.Normal];
                    Vector3 vertexOnFace = h._vertices[boxFace.Vertices[0]] - boxNormal * KWEngine.RayIntersectionErrorMarginFactor;
                    bool tmpResult = IsInFrontOfPlane(ref point, ref boxNormal, ref vertexOnFace);
                    if (tmpResult == false)
                    {
                        isInsideCurrentHitbox = false;
                        break;
                    }
                }
                if (isInsideCurrentHitbox)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger auf dem Objekt liegt (langsam, da pixelgenaue Prüfung)
        /// </summary>
        /// <param name="g">Zu untersuchendes GameObject</param>
        /// <param name="includeNonCollisionObjects">Sollen das Objekt auch dann untersucht werden, wenn es kein Kollisionsobjekt ist?</param>
        /// <returns>true, wenn der Mauszeiger auf dem Objekt liegt</returns>
        public static bool IsMouseCursorInsideHitbox(GameObject g, bool includeNonCollisionObjects = true)
        {
            if (g == null || (includeNonCollisionObjects == false && !g.IsCollisionObject))
            {
                return false;
            }

            Vector3 rayDirection = GetMouseRay();
            Vector3 rayOrigin = KWEngine.CurrentWorld._cameraGame._stateCurrent._position;

            for (int i = 0; i < g._colliderModel._hitboxes.Count; i++)
            {
                GameObjectHitbox currentHitbox = g._colliderModel._hitboxes[i];

                for (int j = 0; j < currentHitbox._mesh.Faces.Length; j++)
                {
                    Span<Vector3> faceVertices = stackalloc Vector3[currentHitbox._mesh.Faces[j].VertexCount];
                    if (currentHitbox.GetVerticesFromFaceAndCheckAngle(j, rayDirection, ref faceVertices, out HitboxFace currentFace))
                    {
                        bool hit = RayNGonIntersection(rayOrigin, rayDirection, currentFace.Normal, ref faceVertices, out Vector3 currentContact);
                        if (hit)
                        {
                            return true;
                        }
                    }


                    /*
                    if (currentHitbox.IsExtended)
                    {
                        currentHitbox.GetVerticesForTriangleFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 currentFaceNormal);
                        float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                        if (dot < 0)
                        {
                            bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out Vector3 currentContact);
                            if (hit)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        currentHitbox.GetVerticesForCubeFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 currentFaceNormal);
                        float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                        if (dot < 0)
                        {
                            bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out Vector3 currentContact);
                            if (!hit)
                            {
                                hit = RayTriangleIntersection(rayOrigin, rayDirection, v4, v5, v6, out currentContact);
                            }
                            if (hit)
                            {
                                return true;
                            }
                        }
                    }
                    */
                }
            }
            return false;
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger auf dem Objekt liegt (schnell, aber prüft dafür nur die achsenparallele Hitbox)
        /// </summary>
        /// <param name="g">Zu untersuchendes GameObject</param>
        /// <param name="includeNonCollisionObjects">Sollen das Objekt auch dann untersucht werden, wenn es kein Kollisionsobjekt ist?</param>
        /// <returns>true, wenn der Mauszeiger auf dem Objekt liegt</returns>
        public static bool IsMouseCursorInsideHitboxFast(GameObject g, bool includeNonCollisionObjects = true)
        {
            if (g == null || (includeNonCollisionObjects == false && !g.IsCollisionObject))
            {
                return false;
            }

            Vector3 rayDirection = GetMouseRay();
            Vector3 rayOrigin = KWEngine.CurrentWorld._cameraGame._stateCurrent._position;

            rayDirection.X = 1f / rayDirection.X;
            rayDirection.Y = 1f / rayDirection.Y;
            rayDirection.Z = 1f / rayDirection.Z;
            bool result = RayAABBIntersection(rayOrigin, rayDirection, g._stateCurrent._center, g._stateCurrent._dimensions, out float distance);
            return result && distance >= 0;
        }

        /// <summary>
        /// Prüft auf eine Strahlenkollision mit einem Terrain-Objekt direkt unterhalb der angegebenen Position
        /// </summary>
        /// <param name="position">Startposition des nach unten gerichteten Teststrahls</param>
        /// <returns>Ergebnis der Strahlenkollisionsmessung</returns>
        public static RayTerrainIntersection RaytraceTerrainBelowPosition(Vector3 position)
        {
            RayTerrainIntersection i = new RayTerrainIntersection();

            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                bool result = HelperIntersection.TestIntersectionTerrainBelowForRay(position, t, out Vector3 intersectionPoint, out Vector3 surfaceNormal);
                float distanceToIntersection = (position - intersectionPoint).Length;
                if (result == true)
                {
                    i.Object = t;
                    i.Distance = distanceToIntersection;
                    i.IntersectionPoint = intersectionPoint;
                    i.SurfaceNormal = surfaceNormal;

                    break;
                }
            }
            return i;
        }

        #region Internals

        internal static bool IsMouseCursorOnGameObject(GameObject g, Vector3 rayOrigin, Vector3 rayDirection, bool includeNonCollisionObjects)
        {
            if (g == null || (includeNonCollisionObjects == false && !g.IsCollisionObject))
            {
                return false;
            }

            for (int i = 0; i < g._colliderModel._hitboxes.Count; i++)
            {
                GameObjectHitbox currentHitbox = g._colliderModel._hitboxes[i];

                // test if object is far behind camera:
                Vector3 camToObject = Vector3.NormalizeFast(currentHitbox._center - rayOrigin);
                if (Vector3.Dot(camToObject, rayDirection) < 0)
                    continue;

                for (int j = 0; j < currentHitbox._mesh.Faces.Length; j++)
                {
                    Span<Vector3> faceVertices = stackalloc Vector3[currentHitbox._mesh.Faces[j].VertexCount];
                    if (currentHitbox.GetVerticesFromFaceAndCheckAngle(j, rayDirection, ref faceVertices, out HitboxFace currentFace))
                    {
                        bool hit = RayNGonIntersection(rayOrigin, rayDirection, currentFace.Normal, ref faceVertices, out Vector3 currentContact);
                        if (hit)
                        {
                            return true;
                        }
                    }

                    /*
                    if (currentHitbox.IsExtended)
                    {
                        currentHitbox.GetVerticesForTriangleFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 currentFaceNormal);
                        float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                        if (dot < 0)
                        {
                            bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out Vector3 currentContact);
                            if (hit)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        currentHitbox.GetVerticesForCubeFace(j, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 currentFaceNormal);
                        float dot = Vector3.Dot(rayDirection, currentFaceNormal);
                        if (dot < 0)
                        {
                            bool hit = RayTriangleIntersection(rayOrigin, rayDirection, v1, v2, v3, out Vector3 currentContact);
                            if (!hit)
                            {
                                hit = RayTriangleIntersection(rayOrigin, rayDirection, v4, v5, v6, out currentContact);
                            }
                            if (hit)
                            {
                                return true;
                            }
                        }
                    }
                    */
                }
            }
            return false;
        }

        private static readonly int[] pixelColor = new int[2];
        internal static int FramebufferPicking(Vector2 mousePosition)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int previousFBID);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderManager.FramebufferDeferred.ID);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
            GL.ReadPixels((int)mousePosition.X, KWEngine.Window.ClientSize.Y - (int)mousePosition.Y, 1, 1, PixelFormat.RgInteger, PixelType.Int, pixelColor);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBID);
            return pixelColor[0];
        }

        internal static void GetArmatureTransformForDefaultAnimation(GameObject g, GeoNode node, ref Matrix4 transform, ref bool found)
        {
            GeoNodeAnimationChannel nac;
            bool nacFound = g._model.ModelOriginal.Animations[0].AnimationChannels.TryGetValue(node.Name, out nac);
            transform = node.Transform * transform;

            if (!nacFound)
            {
                foreach(GeoNode child in node.Children)
                {
                    GetArmatureTransformForDefaultAnimation(g, child, ref transform, ref found);
                    if (found)
                        return;
                }
            }
            else
            {

                foreach (GeoMesh mesh in g._model.ModelOriginal.Meshes.Values)
                {
                    int index = mesh.BoneNames.IndexOf(node.Name);
                    if (index >= 0)
                    {
                        transform = mesh.BoneOffset[index] * transform * g._model.ModelOriginal.TransformGlobalInverse;
                        break;
                    }
                }

                found = true;
            }
        }

        internal static Matrix4 CalculateMeshTransformForGameObject(GameObject g, int meshIndex, CapsuleHitboxMode mode)
        {
            Matrix4 meshTransform = Matrix4.Identity;

            if (mode == CapsuleHitboxMode.Default || mode == CapsuleHitboxMode.AlwaysArmatureTransform)
            {

                if (g.HasArmatureAndAnimations)
                {
                    bool found = false;
                    GetArmatureTransformForDefaultAnimation(g, g._model.ModelOriginal.Armature, ref meshTransform, ref found);
                }
                else
                {
                    meshTransform = g._colliderModel._hitboxes[meshIndex]._mesh.Transform;
                }
            }
            else
            {
                meshTransform = g._colliderModel._hitboxes[meshIndex]._mesh.Transform;
            }

            return meshTransform;
        }

        internal static Vector3[] bounds = new Vector3[2];
        internal static bool RayBoxIntersection(ref Vector3 rayOrigin, ref Vector3 rayDirection, GameObjectHitbox hitbox)
        {
            Vector3 rayInverted = new Vector3(1f / rayDirection.X, 1f / rayDirection.Y, 1f / rayDirection.Z);
            int sign0 = rayInverted.X < 0 ? 1 : 0;
            int sign1 = rayInverted.Y < 0 ? 1 : 0;
            int sign2 = rayInverted.Z < 0 ? 1 : 0;

            bounds[0] = new Vector3(hitbox._left, hitbox._low, hitbox._back);
            bounds[1] = new Vector3(hitbox._right, hitbox._high, hitbox._front);

            float tmin, tmax, tymin, tymax, tzmin, tzmax;

            tmin = (bounds[sign0].X - rayOrigin.X) * rayInverted.X;
            tmax = (bounds[1 - sign0].X - rayOrigin.X) * rayInverted.X;
            tymin = (bounds[sign1].Y - rayOrigin.Y) * rayInverted.Y;
            tymax = (bounds[1 - sign1].Y - rayOrigin.Y) * rayInverted.Y;

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin)
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;

            tzmin = (bounds[sign2].Z - rayOrigin.Z) * rayInverted.Z;
            tzmax = (bounds[1 - sign2].Z - rayOrigin.Z) * rayInverted.Z;

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            if (tzmin > tmin)
                tmin = tzmin;
            if (tzmax < tmax)
                tmax = tzmax;

            return true;
        }

        internal static List<Vector3> ClipFaces(GameObjectHitbox caller, GameObjectHitbox collider)
        {
            List<Vector3> callerVertices = new List<Vector3>(caller._vertices);
            List<Vector3> collisionVolumeVertices = new List<Vector3>();

            // Clip caller against collider faces:
            for (int colliderFaceIndex = 0; colliderFaceIndex < collider._mesh.Faces.Length; colliderFaceIndex++)
            {
                GeoMeshFace colliderClippingFace = collider._mesh.Faces[colliderFaceIndex];
                Vector3 colliderClippingFaceVertex = collider._vertices[colliderClippingFace.Vertices[0]];
                Vector3 colliderClippingFaceNormal = colliderClippingFace.Flip ? collider._normals[colliderClippingFace.Normal] : -collider._normals[colliderClippingFace.Normal];
                for (int callerVertexIndex = 0; callerVertexIndex < callerVertices.Count; callerVertexIndex++)
                {
                    Vector3 callerVertex1 = callerVertices[callerVertexIndex];
                    Vector3 callerVertex2 = callerVertices[(callerVertexIndex + 1) % callerVertices.Count];
                    Vector3 lineDirection = Vector3.NormalizeFast(callerVertex2 - callerVertex1);

                    bool callerVertex1InsideRegion = HelperIntersection.IsInFrontOfPlane(ref callerVertex1, ref colliderClippingFaceNormal, ref colliderClippingFaceVertex);
                    bool callerVertex2InsideRegion = HelperIntersection.IsInFrontOfPlane(ref callerVertex2, ref colliderClippingFaceNormal, ref colliderClippingFaceVertex);

                    if (callerVertex1InsideRegion)
                    {
                        if (callerVertex2InsideRegion)
                        {
                            if (!collisionVolumeVertices.Contains(callerVertex2))
                                collisionVolumeVertices.Add(callerVertex2);
                        }
                        else
                        {
                            Vector3? clippedVertex = ClipLineToPlane(ref callerVertex2, ref lineDirection, ref colliderClippingFaceVertex, ref colliderClippingFaceNormal);
                            if (clippedVertex != null && !collisionVolumeVertices.Contains(clippedVertex.Value))
                                collisionVolumeVertices.Add(clippedVertex.Value);
                        }
                    }
                    else
                    {
                        if (callerVertex2InsideRegion)
                        {
                            Vector3? clippedVertex = ClipLineToPlane(ref callerVertex1, ref lineDirection, ref colliderClippingFaceVertex, ref colliderClippingFaceNormal);
                            if (clippedVertex != null && !collisionVolumeVertices.Contains(clippedVertex.Value))
                                collisionVolumeVertices.Add(clippedVertex.Value);
                            if (!collisionVolumeVertices.Contains(callerVertex2))
                                collisionVolumeVertices.Add(callerVertex2);
                        }
                    }
                }
                callerVertices.Clear();
                for (int i = 0; i < collisionVolumeVertices.Count; i++)
                {
                    callerVertices.Add(collisionVolumeVertices[i]);
                }
                collisionVolumeVertices.Clear();
            }
            return callerVertices;
        }

        private static Vector3? ClipLineToPlane(ref Vector3 vertexToBeClipped, ref Vector3 lineDirectionNormalized, ref Vector3 vertexOnPlane, ref Vector3 planeNormal)
        {
            if (Vector3.Dot(planeNormal, lineDirectionNormalized) == 0)
                return null;

            float t = (Vector3.Dot(planeNormal, vertexOnPlane) - Vector3.Dot(planeNormal, vertexToBeClipped)) / Vector3.Dot(planeNormal, lineDirectionNormalized);
            Vector3 clippedVertex = vertexToBeClipped + lineDirectionNormalized * t;
            return clippedVertex;
        }

        internal static bool TestIntersectionTerrainBelowForRay(Vector3 rayStart, TerrainObject collider, out Vector3 contactPoint, out Vector3 surfaceNormal)
        {
            if(RayTerrainIntersection(collider, rayStart, out contactPoint, out surfaceNormal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static IntersectionTerrain TestIntersectionTerrain(GameObjectHitbox caller, TerrainObjectHitbox collider)
        {
            GeoModel model = collider.Owner._gModel.ModelOriginal;
            Vector3 untranslatedPosition = caller._center - new Vector3(collider._center.X, 0, collider._center.Z);
            Sector s = model.Meshes.Values.ElementAt(0).Terrain.GetSectorForUntranslatedPosition(untranslatedPosition);

            if (s != null)
            {
                GeoTerrainTriangle? tris = s.GetTriangle(ref untranslatedPosition);
                if (tris.HasValue)
                {
                    IntersectionTerrain i = TestIntersectionForTerrain(tris.Value, caller, collider);
                    return i;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal static bool RayTerrainIntersection(TerrainObject t, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 contactPoint)
        {
            contactPoint = Vector3.Zero;
            GeoModel model = t._gModel.ModelOriginal;
            Vector3 untranslatedPosition = rayOrigin - new Vector3(t._stateCurrent._center.X, 0, t._stateCurrent._center.Z);
            Sector s = model.Meshes.Values.ElementAt(0).Terrain.GetSectorForUntranslatedPosition(untranslatedPosition);
            if (s != null)
            {
                GeoTerrainTriangle? tris = s.GetTriangle(ref untranslatedPosition);
                if(tris.HasValue)
                {
                    bool hit = RayTriangleIntersection(untranslatedPosition, rayDirection, tris.Value.Vertices[0], tris.Value.Vertices[1], tris.Value.Vertices[2], out contactPoint);
                    if (hit)
                    {
                        contactPoint += new Vector3(t._stateCurrent._center.X, 0, t._stateCurrent._center.Z);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }

        internal static bool RayTerrainIntersection(TerrainObject t, Vector3 rayOrigin, out Vector3 contactPoint, out Vector3 surfaceNormal)
        {
            contactPoint = Vector3.Zero;
            surfaceNormal = Vector3.UnitY;
            GeoModel model = t._gModel.ModelOriginal;
            Vector3 untranslatedPosition = rayOrigin - new Vector3(t._stateCurrent._center.X, 0, t._stateCurrent._center.Z);
            Sector s = model.Meshes.Values.ElementAt(0).Terrain.GetSectorForUntranslatedPosition(untranslatedPosition);
            if (s != null)
            {
                GeoTerrainTriangle? tris = s.GetTriangle(ref untranslatedPosition);
                if (tris.HasValue)
                {
                    bool hit = RayTriangleIntersection(untranslatedPosition, -Vector3.UnitY, tris.Value.Vertices[0], tris.Value.Vertices[1], tris.Value.Vertices[2], out contactPoint);
                    if (hit)
                    {
                        surfaceNormal = tris.Value.Normal;
                        contactPoint += new Vector3(t._stateCurrent._center.X, 0, t._stateCurrent._center.Z);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }


        private static IntersectionTerrain TestIntersectionForTerrain(GeoTerrainTriangle triangle, GameObjectHitbox caller, TerrainObjectHitbox collider)
        {
            Vector3 MTVTemp;
            Vector3 MTVTempUp = Vector3.Zero;

            IntersectionTerrain tempIntersection;
            Vector3 rayPos = caller.Owner.Center + KWEngine.WorldUp * caller.Owner.Dimensions.Y;
            bool rayHasContact = HelperIntersection.RayTriangleIntersection(rayPos, -KWEngine.WorldUp, triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2], out Vector3 contactPoint);
            if (rayHasContact)
            {
                MTVTempUp.Y = contactPoint.Y - caller._low;
                if (MTVTempUp.Y > 0)
                {
                    float dot = Vector3.Dot(MTVTempUp, triangle.Normal);
                    MTVTemp = triangle.Normal * dot;

                    tempIntersection = new IntersectionTerrain(collider.Owner, caller, MTVTemp, MTVTempUp, collider.Owner.Name, triangle.Normal);
                    return tempIntersection;
                }

            }
            return null;
        }

        internal static bool TestIntersection(FlowFieldHitbox ffhb, GameObjectHitbox collider)
        {
            for (int i = 0; i < ffhb._normals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref ffhb._normals[i], ref ffhb._vertices, out shape1Min, out shape1Max, ref HelperVector.VectorZero);
                SatTest(ref ffhb._normals[i], ref collider._vertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return false;
                }
            }

            for (int i = 0; i < collider._normals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref collider._normals[i], ref ffhb._vertices, out shape1Min, out shape1Max, ref HelperVector.VectorZero);
                SatTest(ref collider._normals[i], ref collider._vertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return false;
                }
            }
            return true;
        }

        internal static Vector3[] _planeVertices = new Vector3[3];
        internal static Vector3[] _planeNormals = new Vector3[1];
        internal const float ONETHIRD = 1f / 3f;
        internal static Intersection TestIntersectionForPlaneFace(GameObjectHitbox caller, Span<Vector3> vertices, Vector3 n, Vector3 center, Vector3 offset, GameObjectHitbox collider)
        {
            _planeNormals[0] = n;
            _planeVertices = vertices.ToArray();

            float mtvDistance = float.MaxValue;
            float mtvDirection = 1;
            float mtvDistanceUp = float.MaxValue;
            float mtvDirectionUp = 1;

            Vector3 MTVTemp = Vector3.Zero;
            Vector3 MTVTempUp = Vector3.Zero;
            int collisionNormalIndex = 0;
            bool collisionNormalIndexFlip = false;
            bool collisionNormalFromCaller = false;
            
            for (int i = 0; i < caller._normals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref caller._normals[i], ref caller._vertices, out shape1Min, out shape1Max, ref offset);
                SatTest(ref caller._normals[i], ref _planeVertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    OverlapResult m = CalculateOverlap(ref caller._normals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        ref mtvDistance, ref mtvDistanceUp, ref MTVTemp, ref MTVTempUp, ref mtvDirection, ref mtvDirectionUp, ref caller._center, ref center, ref offset, true, caller);
                    if (m.IsBetterResult)
                    {
                        collisionNormalIndex = i;
                        collisionNormalIndexFlip = m.FlipNormal;
                        collisionNormalFromCaller = true;
                    }
                }
            }
            
            for (int i = 0; i < _planeNormals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref _planeNormals[i], ref caller._vertices, out shape1Min, out shape1Max, ref offset);
                SatTest(ref _planeNormals[i], ref _planeVertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    OverlapResult m = CalculateOverlap(ref _planeNormals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        ref mtvDistance, ref mtvDistanceUp, ref MTVTemp, ref MTVTempUp, ref mtvDirection, ref mtvDirectionUp, ref caller._center, ref center, ref offset, false, collider);
                    if (m.IsBetterResult)
                    {
                        collisionNormalIndex = i;
                        collisionNormalIndexFlip = m.FlipNormal;
                        collisionNormalFromCaller = false;
                    }
                }
            }

            if (MTVTemp == Vector3.Zero)
                return null;

            Vector3 collisionSurfaceNormal;
            if (collisionNormalFromCaller)
            {
                collisionSurfaceNormal = caller._normals[collisionNormalIndex] * (collisionNormalIndexFlip ? -1 : 1);
            }
            else
            {
                collisionSurfaceNormal = collider._normals[collisionNormalIndex] * (collisionNormalIndexFlip ? -1 : 1);
            }

            Vector3 cross = Vector3.NormalizeFast(Vector3.Cross(Vector3.Cross(MTVTemp, KWEngine.WorldUp), MTVTemp));
            Vector3 MTVTempUpToTop = MTVTemp + cross * MTVTempUp.LengthFast;
            if (Vector3.Dot(MTVTempUpToTop, KWEngine.WorldUp) < 0)
            {
                MTVTempUpToTop = -MTVTempUpToTop;
            }

            // limit mtvup:
            if (cross != Vector3.Zero && (Vector3.Dot(cross, KWEngine.WorldUp) < 0.999f))
            {
                //MTVUp:
                float betaX = MTVTemp.X / cross.X;
                float betaZ = MTVTemp.Z / cross.Z;
                float betaXToTop = MTVTempUp.X / cross.X;
                float betaZToTop = MTVTempUp.Z / cross.Z;
                float beta = (float.IsNaN(betaX) || betaX == 0) ? betaZ : betaX;
                float betaToTop = (float.IsNaN(betaXToTop) || betaXToTop == 0) ? betaZToTop : betaXToTop;
                if (float.IsNaN(beta) == false && beta != 0 && float.IsInfinity(beta) == false)
                {
                    MTVTempUp = MTVTemp + new Vector3(-beta * cross.X, -beta * cross.Y, -beta * cross.Z);
                    if (MTVTempUp.LengthFast > caller.Owner.Dimensions.Y)
                    {
                        MTVTempUp = MTVTemp;
                    }
                }
                else
                {
                    MTVTempUp = MTVTemp + cross * MTVTemp.LengthFast;
                }

                if (float.IsNaN(betaToTop) == false && betaToTop != 0 && float.IsInfinity(betaToTop) == false)
                {
                    MTVTempUpToTop = MTVTempUp + new Vector3(-betaToTop * cross.X, -betaToTop * cross.Y, -betaToTop * cross.Z);
                    if (MTVTempUpToTop.LengthFast > caller.Owner.Dimensions.Y)
                    {
                        MTVTempUpToTop = MTVTemp;
                    }
                }
            }
            else
            {
                MTVTempUp = MTVTemp + cross * MTVTemp.LengthFast;
            }

            Intersection o = new Intersection(collider.Owner, caller, collider, MTVTemp, MTVTempUp, collider._mesh.Name, collisionSurfaceNormal, MTVTempUpToTop, collider._colliderType);
            return o;
        }

        internal static Vector3 CalculateSurfaceNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 u = b - a;
            Vector3 v = c - a;
            Vector3 n = new Vector3(
                u.Y * v.Z - u.Z * v.Y,
                u.Z * v.X - u.X * v.Z,
                u.X * v.Y - u.Y * v.X);
            n.NormalizeFast();
            return n;
        }

        internal static List<Intersection> TestIntersectionsWithPlaneCollider(GameObjectHitbox hbcaller, GameObjectHitbox hbother, Vector3 offset)
        {
            List<Intersection> intersections = new();
            foreach (GeoMeshFace face in hbother._mesh.Faces)
            {
                Vector3 n = hbother._normals[face.Normal] * (face.Flip ? -1f : 1f);
                Span<Vector3> faceVertices = stackalloc Vector3[face.VertexCount];
                Vector3 centerTemp = Vector3.Zero;
                int index = 0;
                foreach (int fvi in face.Vertices)
                {
                    faceVertices[index] = hbother._vertices[fvi];
                    centerTemp += faceVertices[index];
                    index++;
                }
                centerTemp /= face.VertexCount;

                Intersection i = TestIntersectionForPlaneFace(hbcaller, faceVertices, n, centerTemp, offset, hbother);
                if (i != null)
                {
                    intersections.Add(i);
                }
            }
            return intersections;
        }


        internal static Intersection TestIntersection(GameObjectHitbox caller, GameObjectHitbox collider, Vector3 offset)
        {
            float mtvDistance = float.MaxValue;
            float mtvDirection = 1;
            float mtvDistanceUp = float.MaxValue;
            float mtvDirectionUp = 1;

            Vector3 MTVTemp = Vector3.Zero;
            Vector3 MTVTempUp = Vector3.Zero;
            int collisionNormalIndex = 0;
            bool collisionNormalIndexFlip = false;
            bool collisionNormalFromCaller = false;

            for (int i = 0; i < caller._normals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref caller._normals[i], ref caller._vertices, out shape1Min, out shape1Max, ref offset);
                SatTest(ref caller._normals[i], ref collider._vertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    OverlapResult m = CalculateOverlap(ref caller._normals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        ref mtvDistance, ref mtvDistanceUp, ref MTVTemp, ref MTVTempUp, ref mtvDirection, ref mtvDirectionUp, ref caller._center, ref collider._center, ref offset, true, caller);
                    if (m.IsBetterResult)
                    {
                        collisionNormalIndex = i;
                        collisionNormalIndexFlip = m.FlipNormal;
                        collisionNormalFromCaller = true;
                    }
                }
            }

            for (int i = 0; i < collider._normals.Length; i++)
            {
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref collider._normals[i], ref caller._vertices, out shape1Min, out shape1Max, ref offset);
                SatTest(ref collider._normals[i], ref collider._vertices, out shape2Min, out shape2Max, ref HelperVector.VectorZero);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    OverlapResult m = CalculateOverlap(ref collider._normals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        ref mtvDistance, ref mtvDistanceUp, ref MTVTemp, ref MTVTempUp, ref mtvDirection, ref mtvDirectionUp, ref caller._center, ref collider._center, ref offset, false, collider);
                    if (m.IsBetterResult)
                    {
                        collisionNormalIndex = i;
                        collisionNormalIndexFlip = m.FlipNormal;
                        collisionNormalFromCaller = false;
                    }
                }
            }

            if (MTVTemp == Vector3.Zero)
                return null;

            Vector3 collisionSurfaceNormal;
            if (collisionNormalFromCaller)
            {
                collisionSurfaceNormal = caller._normals[collisionNormalIndex] * (collisionNormalIndexFlip ? -1 : 1);
            }
            else
            {
                collisionSurfaceNormal = collider._normals[collisionNormalIndex] * (collisionNormalIndexFlip ? -1 : 1);
            }

            Vector3 cross = Vector3.NormalizeFast(Vector3.Cross(Vector3.Cross(MTVTemp, KWEngine.WorldUp), MTVTemp));
            Vector3 MTVTempUpToTop = MTVTemp + cross * MTVTempUp.LengthFast;
            if (Vector3.Dot(MTVTempUpToTop, KWEngine.WorldUp) < 0)
            {
                MTVTempUpToTop = -MTVTempUpToTop;
            }

            // limit mtvup:
            if (cross != Vector3.Zero && (Vector3.Dot(cross, KWEngine.WorldUp) < 0.999f))
            {
                //MTVUp:
                float betaX = MTVTemp.X / cross.X;
                float betaZ = MTVTemp.Z / cross.Z;
                float betaXToTop = MTVTempUp.X / cross.X;
                float betaZToTop = MTVTempUp.Z / cross.Z;
                float beta = (float.IsNaN(betaX) || betaX == 0) ? betaZ : betaX;
                float betaToTop = (float.IsNaN(betaXToTop) || betaXToTop == 0) ? betaZToTop : betaXToTop;
                if (float.IsNaN(beta) == false && beta != 0 && float.IsInfinity(beta) == false)
                {
                    MTVTempUp = MTVTemp + new Vector3(-beta * cross.X, -beta * cross.Y, -beta * cross.Z);
                    if (MTVTempUp.LengthFast > caller.Owner.Dimensions.Y)
                    {
                        MTVTempUp = MTVTemp;
                    }
                }
                else
                {
                    MTVTempUp = MTVTemp + cross * MTVTemp.LengthFast;
                }

                if (float.IsNaN(betaToTop) == false && betaToTop != 0 && float.IsInfinity(betaToTop) == false)
                {
                    MTVTempUpToTop = MTVTempUp + new Vector3(-betaToTop * cross.X, -betaToTop * cross.Y, -betaToTop * cross.Z);
                    if(MTVTempUpToTop.LengthFast > caller.Owner.Dimensions.Y)
                    {
                        MTVTempUpToTop = MTVTemp;
                    }
                }
            }
            else
            {
                MTVTempUp = MTVTemp + cross * MTVTemp.LengthFast;
            }

            Intersection o = new Intersection(collider.Owner, caller, collider, MTVTemp, MTVTempUp, collider._mesh.Name, collisionSurfaceNormal, MTVTempUpToTop, collider._colliderType);
            return o;
        }

        private static OverlapResult CalculateOverlap(ref Vector3 axis, ref float shape1Min, ref float shape1Max, ref float shape2Min, ref float shape2Max,
            ref float mtvDistance, ref float mtvDistanceUp, ref Vector3 mtv, ref Vector3 mtvUp, ref float mtvDirection, ref float mtvDirectionUp, ref Vector3 posA, ref Vector3 posB, ref Vector3 offset, bool isCaller,  GameObjectHitbox hb)
        {
            float intersectionDepthScaled;
            if (shape1Min < shape2Min)
            {
                if (shape1Max > shape2Max)
                {
                    float diff1 = shape1Max - shape2Max;
                    float diff2 = shape2Min - shape1Min;
                    if (diff1 > diff2)
                    {
                        intersectionDepthScaled = shape2Max - shape1Min;
                    }
                    else
                    {
                        intersectionDepthScaled = shape2Min - shape1Max;
                    }

                }
                else
                {
                    intersectionDepthScaled = shape1Max - shape2Min; // default
                }

            }
            else
            {
                if (shape1Max < shape2Max)
                {
                    float diff1 = shape2Max - shape1Max;
                    float diff2 = shape1Min - shape2Min;
                    if (diff1 > diff2)
                    {
                        intersectionDepthScaled = shape1Max - shape2Min;
                    }
                    else
                    {
                        intersectionDepthScaled = shape1Min - shape2Max;
                    }
                }
                else
                {
                    intersectionDepthScaled = shape1Min - shape2Max; // default
                }
            }

            float axisLengthSquared = Vector3.Dot(axis, axis);
            float intersectionDepthSquared = (intersectionDepthScaled * intersectionDepthScaled) / axisLengthSquared;

            if (Math.Abs(axis.Y) > Math.Abs(axis.X) && Math.Abs(axis.Y) > Math.Abs(axis.Z) && intersectionDepthSquared < mtvDistanceUp)
            {
                mtvDistanceUp = intersectionDepthSquared;
                mtvUp = axis * (intersectionDepthScaled / axisLengthSquared);
                float notSameDirection = Vector3.Dot(posA + offset - posB, mtvUp);
                mtvDirectionUp = notSameDirection < 0 ? -1.0f : 1.0f;
                mtvUp = mtvUp * mtvDirectionUp;
            }
            if (intersectionDepthSquared < mtvDistance || mtvDistance < 0)
            {
                mtvDistance = intersectionDepthSquared;
                mtv = axis * (intersectionDepthScaled / axisLengthSquared);
                float notSameDirection = Vector3.Dot(posA + offset - posB, mtv);
                mtvDirection = notSameDirection < 0 ? -1.0f : 1.0f;
                mtv = mtv * mtvDirection;

                return new OverlapResult() { IsBetterResult = true, FlipNormal = Vector3.Dot(axis, mtv) < 0};
            }
            return new OverlapResult() { IsBetterResult = false, FlipNormal = false};
        }

        private static bool Overlaps(float min1, float max1, float min2, float max2)
        {
            return IsBetweenOrdered(min2, min1, max1) || IsBetweenOrdered(min1, min2, max2);
        }

        private static bool IsBetweenOrdered(float val, float lowerBound, float upperBound)
        {
            return lowerBound <= val && val <= upperBound;
        }

        private static void SatTest(ref Vector3 axis, ref Vector3[] ptSet, out float minAlong, out float maxAlong, ref Vector3 offset)
        {
            minAlong = float.MaxValue;
            maxAlong = float.MinValue;
            for (int i = 0; i < ptSet.Length; i++)
            {
                float dotVal = Vector3.Dot(ptSet[i] + offset, axis);
                if (dotVal < minAlong) minAlong = dotVal;
                if (dotVal > maxAlong) maxAlong = dotVal;
            }
        }
        private static bool LinePlaneIntersection(out Vector3 contact, Vector3 ray, Vector3 rayOrigin,
                                            Vector3 normal, Vector3 coord)
        {
            contact = Vector3.Zero;
            if (Vector3.Dot(normal, ray) == 0)
            {
                return false;
            }
            float d = Vector3.Dot(normal, coord);
            float x = (d - Vector3.Dot(normal, rayOrigin)) / Vector3.Dot(normal, ray);
            contact = rayOrigin + ray * x;
            return true;
        }
        internal static bool IsInFrontOfPlane(ref Vector3 vertex, ref Vector3 planeNormal, ref Vector3 vertexOnPlane)
        {
            float distancePointToPlane = Vector3.Dot(planeNormal, vertex - vertexOnPlane);
            return distancePointToPlane >= 0;
        }

        internal static bool IsPointInsideBox(ref Vector3 pos, float left, float right, float top, float bottom, float front, float back)
        {
            return (
               pos.X >= left &&
               pos.X <= right &&
               pos.Y >= bottom &&
               pos.Y <= top &&
               pos.Z >= back &&
               pos.Z <= front
               );
        }
        internal static bool IsPointOnTrianglePlane(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            a -= p;
            b -= p;
            c -= p;

            Vector3 u = Vector3.Cross(b, c);
            Vector3 v = Vector3.Cross(c, a);
            Vector3 w = Vector3.Cross(a, b);

            if (Vector3.Dot(u, v) < 0f)
            {
                return false;
            }
            if (Vector3.Dot(u, w) < 0f)
            {
                return false;
            }

            return true;
        }

        internal static bool RayNGonIntersection(Vector3 rayStart, Vector3 rayDirection, Vector3 normal, Vector3[] vertices, out Vector3 contactPoint)
        {
            bool planeWasHit = LinePlaneIntersection(out contactPoint, rayDirection, rayStart, normal, vertices[0]);
            if (planeWasHit)
            {
                // check for every edge of the n-gon if the contact point is perpendicular to it
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 b = vertices[(i + 1) % vertices.Length];
                    Vector3 a = vertices[i];
                    float mp = Vector3.Dot(Vector3.Cross(b - a, contactPoint - a), normal);
                    if (mp < 0)
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool RayNGonIntersection(Vector3 rayStart, Vector3 rayDirection, Vector3 normal, ref Span<Vector3> vertices, out Vector3 contactPoint)
        {
            bool planeWasHit = LinePlaneIntersection(out contactPoint, rayDirection, rayStart, normal, vertices[0]);
            if(planeWasHit)
            {
                // check for every edge of the n-gon if the contact point is perpendicular to it
                for(int i = 0; i < vertices.Length; i++)
                {
                    Vector3 b = vertices[(i + 1) % vertices.Length];
                    Vector3 a = vertices[i];
                    float mp = Vector3.Dot(Vector3.Cross(b - a, contactPoint - a), normal);
                    if (mp < 0)
                        return false;
                }
                return true;
            }
            else
            {
                contactPoint = Vector3.Zero;
                return false;
            }
        }

        internal static bool RayTriangleIntersection(Vector3 rayStart, Vector3 rayDirection, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, out Vector3 contactPoint)
        {
            
            contactPoint = Vector3.Zero;
            Vector3 edge1, edge2, h, s, q;
            float a, f, u, v;
            edge1 = vertex1 - vertex0;
            edge2 = vertex2 - vertex0;
            h = Vector3.Cross(rayDirection, edge2);
            a = Vector3.Dot(edge1, h);
            if (a > -KWEngine.RAYTRACE_EPSILON && a < KWEngine.RAYTRACE_EPSILON)
                return false;    // ray is parallel to triangle
            f = 1.0f / a;
            s = rayStart - vertex0;
            u = f * Vector3.Dot(s, h);
            if (u < 0.0 || u > 1.0)
                return false;

            q = Vector3.Cross(s, edge1);
            v = f * Vector3.Dot(rayDirection, q);
            if (v < 0.0 || u + v > 1.0)
                return false;
            float t = f * Vector3.Dot(edge2, q);
            if (t >= KWEngine.RAYTRACE_EPSILON) // ray intersection
            {
                contactPoint = rayStart + rayDirection * t;
                return true;
            }
            else
                return false;
        }

        internal static bool RayAABBIntersection(
            Vector3 origin, 
            Vector3 directionFrac, // =  1 / (direction normalized)
            Vector3 center,       // center
            Vector3 dimensions,      // aabb dimensions
            out float distance)
        {
            Vector3 aabbLow = new Vector3(center - dimensions * 0.5f);
            Vector3 aabbHigh = new Vector3(center + dimensions * 0.5f);

            float t1 = (aabbLow.X - origin.X) * directionFrac.X;
            float t2 = (aabbHigh.X - origin.X) * directionFrac.X;
            float t3 = (aabbLow.Y - origin.Y) * directionFrac.Y;
            float t4 = (aabbHigh.Y - origin.Y) * directionFrac.Y;
            float t5 = (aabbLow.Z - origin.Z) * directionFrac.Z;
            float t6 = (aabbHigh.Z - origin.Z) * directionFrac.Z;

            float tmin = Max(Max(Min(t1, t2), Min(t3, t4)), Min(t5, t6));
            float tmax = Min(Min(Max(t1, t2), Max(t3, t4)), Max(t5, t6));

            // if tmax < 0, schneidet der Strahl die AABB aber sie liegt hinter dem Ursprung des Strahls
            if (tmax < 0)
            {
                distance = tmax;
                return false;
            }

            // if tmin > tmax, schneidet der Strahl die AABB nicht
            if (tmin > tmax)
            {
                distance = tmax;
                return false;
            }

            distance = tmin; // Achtung: tmin kann negativ sein, wenn der Ursprung des Strahls innerhalb der AABB liegt
            return true;
        }

        internal static float Min(float x, float y)
        {
            return Math.Min(x, Math.Min(y, float.PositiveInfinity));
        }
        internal static float Max(float x, float y)
        {
            return Math.Max(x, Math.Max(y, float.NegativeInfinity));
        }

        internal static List<RayIntersection> RayTraceObjectsBelowPositionFast(Vector3 origin, float maxDistance, bool sort, params Type[] typelist)
        {
            Vector3 direction = -Vector3.UnitY;

            List<RayIntersection> list = new List<RayIntersection>();
            if (maxDistance <= 0)
            {
                maxDistance = float.MaxValue;
            }

            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    float left = g._stateCurrent._center.X - g._stateCurrent._dimensions.X * 0.5f;
                    float right = g._stateCurrent._center.X + g._stateCurrent._dimensions.X * 0.5f;
                    float back = g._stateCurrent._center.Z - g._stateCurrent._dimensions.Z * 0.5f;
                    float front = g._stateCurrent._center.Z + g._stateCurrent._dimensions.Z * 0.5f;

                    if (origin.X >= left && origin.X <= right && origin.Z >= back && origin.Z <= front)
                    {
                        foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
                        {
                            ConvertRayToMeshSpaceForAABBTest(ref origin, ref direction, ref g._stateCurrent._modelMatrixInverse, out Vector3 originTransformed, out Vector3 directionTransformed);
                            Vector3 directionTransformedInv = new Vector3(1f / directionTransformed.X, 1f / directionTransformed.Y, 1f / directionTransformed.Z);

                            bool result = RayAABBIntersection(originTransformed, directionTransformedInv, hb._mesh.Center, new Vector3(hb._mesh.width, hb._mesh.height, hb._mesh.depth), out float currentDistance);
                            if (result == true)
                            {
                                ConvertRayToWorldSpaceAfterAABBTest(ref originTransformed, ref directionTransformed, currentDistance, ref g._stateCurrent._modelMatrix, ref origin, out Vector3 intersectionPoint, out float distanceWorldspace);

                                if (distanceWorldspace <= maxDistance)
                                {
                                    RayIntersection gd = new RayIntersection()
                                    {
                                        Distance = Math.Max(0, currentDistance),
                                        Object = g,
                                        IntersectionPoint = intersectionPoint
                                    };
                                    list.Add(gd);
                                }
                            }
                        }
                    }
                }
            }

            if (sort)
                list.Sort();

            return list;
        }

        internal static List<RayIntersectionExt> RayTraceObjectsBelowPosition(Vector3 origin, float maxDistance, bool sort, params Type[] typelist)
        {
            Vector3 direction = new Vector3(0, -1, 0);

            List<RayIntersectionExt> list = new List<RayIntersectionExt>();
            if (maxDistance == 0)
                maxDistance = float.MaxValue;
            else
                maxDistance = maxDistance * maxDistance;


            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (HelperGeneral.IsObjectClassOrSubclassOfTypes(typelist, g))
                {
                    float left = g._stateCurrent._center.X - g._stateCurrent._dimensions.X * 0.5f;
                    float right = g._stateCurrent._center.X + g._stateCurrent._dimensions.X * 0.5f;
                    float back = g._stateCurrent._center.Z - g._stateCurrent._dimensions.Z * 0.5f;
                    float front = g._stateCurrent._center.Z + g._stateCurrent._dimensions.Z * 0.5f;

                    if (origin.X >= left && origin.X <= right && origin.Z >= back && origin.Z <= front)
                    {
                        bool result = RaytraceObject(g, origin + new Vector3(0, KWEngine.RAYTRACE_SAFETY, 0), direction, out Vector3 intersectionPoint, out Vector3 faceNormal, out string hitboxname, false);
                        
                        if (result == true)
                        {
                            if (maxDistance > 0 && (origin - intersectionPoint).LengthSquared - KWEngine.RAYTRACE_SAFETY_SQ <= maxDistance)
                            {
                                Vector3 delta = origin - intersectionPoint;
                                if (delta.Y < 0)
                                    delta.Y = 0;
                                RayIntersectionExt gd = new RayIntersectionExt()
                                {
                                    Distance = delta.LengthFast,
                                    IntersectionPoint = intersectionPoint,
                                    HitboxName = hitboxname,
                                    Object = g,
                                    SurfaceNormal = faceNormal
                                };
                                list.Add(gd);
                            }
                        }
                    }
                }
            }

            if (sort)
                list.Sort();

            return list;
        }

        internal static void ConvertRayToMeshSpaceForAABBTest(ref Vector3 origin, ref Vector3 direction, ref Matrix4 matInv, out Vector3 originTransformed, out Vector3 directionTransformed)
        {
            originTransformed = Vector4.TransformRow(new Vector4(origin, 1.0f), matInv).Xyz;
            directionTransformed = Vector3.NormalizeFast(Vector4.TransformRow(new Vector4(direction, 0.0f), matInv).Xyz);
        }
        internal static void ConvertRayToWorldSpaceAfterAABBTest(ref Vector3 originTransformed, ref Vector3 dirctnTransformedNormalized, float currentDistance, ref Matrix4 mat, ref Vector3 originWorldspace, out Vector3 intersectionPoint, out float distanceWorldspace)
        {
            intersectionPoint = Vector4.TransformRow(new Vector4(originTransformed + dirctnTransformedNormalized * currentDistance, 1.0f), mat).Xyz;
            distanceWorldspace = (originWorldspace - intersectionPoint).LengthFast;
        }
        /*
        internal static Intersection TestIntersectionWithPlaneCollider(GameObjectHitbox hbCaller, GameObjectHitbox hbCollider, Vector3 offset)
        {
            foreach (GeoMeshFace face in hbCollider._mesh.Faces)
            {
                Vector3 n = hbCollider._normals[face.Normal] * (face.Flip ? -1f : 1f);
                List<Vector3> faceVertices = new();
                Vector3 centerTemp = Vector3.Zero;
                foreach (int fvi in face.Vertices)
                {
                    faceVertices.Add(hbCollider._vertices[fvi]);
                    centerTemp += faceVertices[faceVertices.Count - 1];
                }
                centerTemp /= face.VertexCount;

                Intersection i = TestIntersectionForPlaneFace(hbCaller, faceVertices, n, centerTemp, offset, hbCollider);
                if (i != null)
                {
                    return i;
                }
            }
            return null;
        }
        */
        #endregion
    }
}
