namespace KWEngine3
{

    /// <summary>
    /// Himmelsrichtung
    /// </summary>
    public enum CardinalDirection
    {
        /// <summary>
        /// Nord
        /// </summary>
        North,
        /// <summary>
        /// Nordost
        /// </summary>
        NorthEast,
        /// <summary>
        /// Ost
        /// </summary>
        East,
        /// <summary>
        /// Südost
        /// </summary>
        SouthEast,
        /// <summary>
        /// Süd
        /// </summary>
        South,
        /// <summary>
        /// Südwest
        /// </summary>
        SouthWest,
        /// <summary>
        /// West
        /// </summary>
        West,
        /// <summary>
        /// Nordwest
        /// </summary>
        NorthWest,
        /// <summary>
        /// Keine
        /// </summary>
        None
    }

    /// <summary>
    /// Listet die Anzeigemöglichkeiten für Eingabecursor auf
    /// </summary>
    public enum KeyboardCursorType
    {
        /// <summary>
        /// Vertikaler Trennstrich (Standard)
        /// </summary>
        Pipe = 0,
        /// <summary>
        /// Unterstrich
        /// </summary>
        Underscore = 1,
        /// <summary>
        /// Vertikal zentrierter Punkt
        /// </summary>
        Dot = 2
    }

    /// <summary>
    /// Listet die Blinkmodi für Eingabecursor auf
    /// </summary>
    public enum KeyboardCursorBehaviour
    {
        /// <summary>
        /// Blendet den Cursor abwechselnd ein und aus
        /// </summary>
        Blink = -1,
        /// <summary>
        /// Blendet den Cursor langsam ein und aus
        /// </summary>
        Fade = 1
    }

    /// <summary>
    /// Gibt an, ob die für das Kamerazittern festgelegten Werte zusätzlich zum ggf. bestehenden Zittern addiert werden oder die bisherigen Werte ersetzen.
    /// </summary>
    public enum ShakeMode
    {
        /// <summary>
        /// Addiert die Werte zu ggf. gerade aktiven Werten
        /// </summary>
        Additive = 0,
        /// <summary>
        /// Ersetzt bisher ggf. festgelegte Werte
        /// </summary>
        Absolute = 1
    }

    /// <summary>
    /// Gibt an, ab welcher Entfernung Terrain-Objekte weniger detailliert gezeichnet werden
    /// </summary>
    public enum TerrainThresholdValue
    {
        /// <summary>
        /// Ab 32 Längeneinheiten
        /// </summary>
        T32 = 1024,
        /// <summary>
        /// Ab 64 Längeneinheiten
        /// </summary>
        T64 = 4096,
        /// <summary>
        /// Ab 128 Längeneinheiten
        /// </summary>
        T128 = 16384
    }

    /// <summary>
    /// Gibt die Projektionsrichtung für die Maperstellungshilfsmethode HelperVector.GetScreenCoordinatesNormalizedFor()
    /// </summary>
    public enum ProjectionDirection
    {
        /// <summary>
        /// Kamera blickt entlang der negativen Y-Achse
        /// </summary>
        NegativeY,
        /// <summary>
        /// Kamera blickt entlang der negativen Z-Achse
        /// </summary>
        NegativeZ
    }

    /// <summary>
    /// Debug-Modus für den Viewport
    /// </summary>
    public enum DebugMode
    {
        /// <summary>
        /// Keine Debugging-Darstellung (Standard)
        /// </summary>
        None = 0,
        /// <summary>
        /// Zeige Tiefenpufferwerte
        /// </summary>
        DepthBufferLinearized = 1,
        /// <summary>
        /// Zeige Farbwerte (vor der Lichtberechnung)
        /// </summary>
        Colors = 2,
        /// <summary>
        /// Zeige Oberflächenvektoren (farbkodiert)
        /// </summary>
        SurfaceNormals = 3,
        /// <summary>
        /// Zeige SSAO-Post-Processing-Effekt
        /// </summary>
        ScreenSpaceAmbientOcclusion = 4,
        /// <summary>
        /// Zeige Glow-Post-Processing-Effekt
        /// </summary>
        Glow = 5,
        /// <summary>
        /// Zeige Metallic/Roughness-Werte für die Lichtberechnung
        /// </summary>
        MetallicRoughness = 6,
        /// <summary>
        /// Zeige Tiefenpuffer für die erste Shadow-Map (falls vorhanden)
        /// </summary>
        DepthBufferShadowMap1 = 7,
        /// <summary>
        /// Zeige Tiefenpuffer für die zweite Shadow-Map (falls vorhanden)
        /// </summary>
        DepthBufferShadowMap2 = 8,
        /// <summary>
        /// Zeige Tiefenpuffer für die dritte Shadow-Map (falls vorhanden)
        /// </summary>
        DepthBufferShadowMap3 = 9,
        /// <summary>
        /// Zeige Kollisionsmodell aller TerrainObjekt-Instanzen (falls vorhanden)
        /// </summary>
        TerrainCollisionModel = 10
        
    }

    /// <summary>
    /// Fenstermodus der Anwendung
    /// </summary>
    public enum WindowMode
    {
        /// <summary>
        /// Standardfenster mit Dekoration (Standard, mit Titelleiste und Rahmen)
        /// </summary>
        Default,
        /// <summary>
        /// Rahmenloses Fenster
        /// </summary>
        BorderlessWindow
    }

    internal enum MatrixFBXType
    {
        PreTranslation,
        PreRotation,
        PreScale,
        PostTranslation,
        PostRotation,
        PostScale,
        Complete
    }

    /// <summary>
    /// Gibt an, welche Arten von Hitboxen für die Kollisionsmessung berücksichtigt werden sollen
    /// </summary>
    public enum IntersectionTestMode
    {
        /// <summary>
        /// Misst beide Arten: ConvexHull und Plane
        /// </summary>
        CheckAllHitboxTypes,
        /// <summary>
        /// Limitiert die Messung auf konvexe Hüllen (Convex Hull)
        /// </summary>
        CheckConvexHullsOnly,
        /// <summary>
        /// Limitiert die Messung auf Ebenen (Planes)
        /// </summary>
        CheckPlanesOnly
    }

    /// <summary>
    /// Bestimmt, wie viele Strahlen für die Messung der Bodennähe verwendet und in welche Richtung sie geschossen werden
    /// </summary>
    public enum RayMode
    {
        /// <summary>
        /// Verwendet einen Strahl aus der Mitte der Hitbox entlang der lokalen negativen Y-Achse
        /// </summary>
        SingleY,
        /// <summary>
        /// Verwendet einen Strahl aus der Mitte der Hitbox entlang der lokalen negativen Z-Achse
        /// </summary>
        SingleZ,
        /// <summary>
        /// Verwendet zwei Strahlen entlang der lokalen negativen Y-Achse der Instanz (für 2D-Platformer)
        /// </summary>
        TwoRays2DPlatformerY,
        /// <summary>
        /// Verwendet vier Strahlen entlang der lokalen negativen Z-Achse der Instanz (für Top-Down-Platformer)
        /// </summary>
        FourRaysZ,
        /// <summary>
        /// Verwendet vier Strahlen entlang der lokalen negativen Y-Achse der Instanz (für 3D-Platformer)
        /// </summary>
        FourRaysY,
        /// <summary>
        /// Verwendet fünf Strahlen entlang der lokalen negativen Y-Achse der Instanz (für 3D-Platformer)
        /// </summary>
        FiveRaysY
    }

    /// <summary>
    /// Art des Kollisionstyps
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// Hat eine konvexe Hülle (Standardtyp)
        /// </summary>
        ConvexHull,
        /// <summary>
        /// Eignet sich ausschließlich für raytraced Kollisionen
        /// </summary>
        PlaneCollider
    }

    /// <summary>
    /// Gibt an, auf welchen Fixpunkt sich die SetPosition()-Methode bezieht, wenn sie aufgerufen wird (Standardwert: Position)
    /// </summary>
    public enum PositionMode
    {
        /// <summary>
        /// Bezieht sich auf die tatsächliche Position des Objekts (Standard)
        /// </summary>
        Position,
        /// <summary>
        /// Bezieht sich auf den Mittelpunkt der Hitbox
        /// </summary>
        CenterOfHitbox,
        /// <summary>
        /// Bezieht sich auf die unterste Kante der achsenparallelen Hitbox (z.B. für 2D-Platformer)
        /// </summary>
        BottomOfAABBHitbox
    }

    internal enum AddRemoveHitboxMode
    {
        None,
        Add,
        Remove,
        AddCustomRemoveDefault,
        AddDefaultRemoveCustom
    }

    /// <summary>
    /// Gibt an, welche Richtungen die Flowfield-Instanz bei der Berechnung des kürzesten Wegs verwenden darf (Standard: CardinalAndIntercardinalDirections)
    /// </summary>
    public enum FlowFieldDirections
    {
        /// <summary>
        /// Es werden lediglich die Richtungen Nord, Ost, Süd und West verwendet
        /// </summary>
        CardinalDirections = 0,
        /// <summary>
        /// Es werden die Richtungen Nord, Nordost, Ost, Südost, Süd, Südwest, West, Nordwest verwendet
        /// </summary>
        CardinalAndIntercardinalDirections = 1
    }

    /// <summary>
    /// Modus, der bestimmt, wie genau das FlowField Hindernisse erkennt (Standard: Simple)
    /// </summary>
    public enum FlowFieldMode
    {
        /// <summary>
        /// Pro Zelle wird eine Kollisionsmessung der AABB-Hitboxen vorgenommen (kann ungenau sein)
        /// </summary>
        Simple = 1,
        /// <summary>
        /// Pro Zelle wird die OBB (oriented bounding-box) der Objekte geprüft
        /// </summary>
        Box = 2
    }

    /// <summary>
    /// Art des Bodengewächses
    /// </summary>
    public enum FoliageType
    {
        /// <summary>
        /// Gras (Variante 1)
        /// </summary>
        GrassFresh,
        /// <summary>
        /// Gras (Variante 2)
        /// </summary>
        GrassDry,
        /// <summary>
        /// Gras (Variante 3)
        /// </summary>
        GrassDesert,
        /// <summary>
        /// Gras (Variante 4)
        /// </summary>
        GrassMinecraft,
        /// <summary>
        /// Strauch
        /// </summary>
        Fern
    }

    /// <summary>
    /// Modus der Instanzplatzierung für RenderObject-Instanzen
    /// </summary>
    public enum InstanceMode
    {
        /// <summary>
        /// Jede zusätzliche Instanz hat eine unabhängige Position/Rotation/Skalierung
        /// </summary>
        Absolute,
        /// <summary>
        /// Jede zusätzliche hat eine vom Hauptinstanzelement abhängige Position/Rotation/Skalierung
        /// </summary>
        Relative
    }

    /// <summary>
    /// Modus der Textanordnung in einem HUDObject
    /// </summary>
    public enum TextAlignMode
    {
        /// <summary>
        /// Linksbündig
        /// </summary>
        Left = 0,
        /// <summary>
        /// Zentriert
        /// </summary>
        Center = 1,
        /// <summary>
        /// Rechtsbündig
        /// </summary>
        Right = 2
    }

    /// <summary>
    /// Modus der Skybox (Standard: CubeMap)
    /// </summary>
    public enum SkyboxType
    {
        /// <summary>
        /// Skybox liegt als 'unified cube map' vor
        /// </summary>
        CubeMap,
        /// <summary>
        /// Skybox liegt Gleichwinkelbild vor
        /// </summary>
        Equirectangular
    }

    /// <summary>
    /// Art der Animationsbewegung
    /// </summary>
    public enum ExplosionAnimation
    {
        /// <summary>
        /// Standard-Animationsalgorithmus
        /// </summary>
        Spread = 0,
        /// <summary>
        /// Partikel wandern entlang der positiven y-Achse nach oben
        /// </summary>
        WindUp = 1,
        /// <summary>
        /// Partikel wirbeln entlang der positiven y-Achse nach oben
        /// </summary>
        WhirlwindUp = 2

    }

    /// <summary>
    /// Form der Kapsel-Hitbox
    /// </summary>
    public enum CapsuleHitboxType
    {
        /// <summary>
        /// Standardmodus
        /// </summary>
        Default = 0,
        /// <summary>
        /// Kapsel ist in der unteren Hälfte stärker abgerundet 
        /// </summary>
        Sloped = 1
    }

    /// <summary>
    /// Modus für das Setzen der Kapsel-Hitbox bei 3D-Modellen
    /// </summary>
    public enum CapsuleHitboxMode
    {
        /// <summary>
        /// Standardmodus, der je nach 3D-Modell errät, welcher Modus geeignet sein könnte (Standard)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Benutze immer die Informationen aus dem 3D-Modell-Mesh ohne Knochenskelett
        /// </summary>
        AlwaysMeshTransform = 1,
        /// <summary>
        /// Benutze immer die Informationen aus dem Knochenskelett (Armature) des Modells (falls vorhanden)
        /// </summary>
        AlwaysArmatureTransform = 2
    }

    /// <summary>
    /// Art der Partikel
    /// </summary>
    public enum ParticleType
    {
        /// <summary>
        /// Feuer 1
        /// </summary>
        BurstFire1,
        /// <summary>
        /// Feuer 2
        /// </summary>
        BurstFire2,
        /// <summary>
        /// Feuer 3
        /// </summary>
        BurstFire3,
        /// <summary>
        /// Elektroschock
        /// </summary>
        BurstElectricity,
        /// <summary>
        /// Bälle
        /// </summary>
        BurstBubblesColored,
        /// <summary>
        /// Bälle (farblos)
        /// </summary>
        BurstBubblesMonochrome,
        /// <summary>
        /// Feuerwerk 1
        /// </summary>
        BurstFirework1,
        /// <summary>
        /// Feuerwerk 2
        /// </summary>
        BurstFirework2,
        /// <summary>
        /// Herzen
        /// </summary>
        BurstHearts,
        /// <summary>
        /// Pluszeichen
        /// </summary>
        BurstOneUps,
        /// <summary>
        /// Schild
        /// </summary>
        BurstShield,
        /// <summary>
        /// Teleport 1
        /// </summary>
        BurstTeleport1,
        /// <summary>
        /// Teleport 2
        /// </summary>
        BurstTeleport2,
        /// <summary>
        /// Teleport 3
        /// </summary>
        BurstTeleport3,
        /// <summary>
        /// Rauch 1 (Loop)
        /// </summary>
        LoopSmoke1,
        /// <summary>
        /// Rauch 2 (Loop)
        /// </summary>
        LoopSmoke2,
        /// <summary>
        /// Rauch 3 (Loop)
        /// </summary>
        LoopSmoke3
    }

    /// <summary>
    /// Art der Explosion
    /// </summary>
    public enum ExplosionType
    {
        /// <summary>
        /// Würfelpartikel in alle Richtungen
        /// </summary>
        Cube = 0,
        /// <summary>
        /// Würfelpartikel um die Y-Achse
        /// </summary>
        CubeRingY = 100,
        /// <summary>
        /// Würfelpartikel um die Z-Achse
        /// </summary>
        CubeRingZ = 1000,
        /// <summary>
        /// Kugelpartikel in alle Richtungen
        /// </summary>
        Sphere = 1,
        /// <summary>
        /// Kugelpartikel um die Y-Achse
        /// </summary>
        SphereRingY = 101,
        /// <summary>
        /// Kugelpartikel um die Z-Achse
        /// </summary>
        SphereRingZ = 1001,
        /// <summary>
        /// Sternenpartikel in alle Richtungen
        /// </summary>
        Star = 2,
        /// <summary>
        /// Sternenpartikel um die Y-Achse
        /// </summary>
        StarRingY = 102,
        /// <summary>
        /// Sternenpartikel um die Z-Achse
        /// </summary>
        StarRingZ = 1002,
        /// <summary>
        /// Herzpartikel in alle Richtungen
        /// </summary>
        Heart = 3,
        /// <summary>
        /// Herzpartikel um die Y-Achse
        /// </summary>
        HeartRingY = 103,
        /// <summary>
        /// Herzpartikel um die Z-Achse
        /// </summary>
        HeartRingZ = 1003,
        /// <summary>
        /// Schädelpartikel in alle Richtungen
        /// </summary>
        Skull = 4,
        /// <summary>
        /// Schädelpartikel um die Y-Achse
        /// </summary>
        SkullRingY = 104,
        /// <summary>
        /// Schädelpartikel um die Z-Achse
        /// </summary>
        SkullRingZ = 1004,
        /// <summary>
        /// Dollarpartikel in alle Richtungen
        /// </summary>
        Dollar = 5,
        /// <summary>
        /// Dollarpartikel um die Y-Achse
        /// </summary>
        DollarRingY = 105,
        /// <summary>
        /// Dollarpartikel um die Z-Achse
        /// </summary>
        DollarRingZ = 1005
    }
    internal enum BackgroundType
    {
        Skybox,
        Standard,
        None
    }

    /// <summary>
    /// Metalltyp
    /// </summary>
    public enum MetallicType
    {
        /// <summary>
        /// Standard
        /// </summary>
        Default = 0,
        /// <summary>
        /// Plastik/Glass
        /// </summary>
        PlasticOrGlassLow = 1,
        /// <summary>
        /// Plastik/Glass
        /// </summary>
        PlasticOrGlassHigh = 2,
        /// <summary>
        /// Diamant
        /// </summary>
        Diamond = 3,
        /// <summary>
        /// Eisen
        /// </summary>
        Iron = 4,
        /// <summary>
        /// Kupfer
        /// </summary>
        Copper = 5,
        /// <summary>
        /// Gold
        /// </summary>
        Gold = 6,
        /// <summary>
        /// Aluminium
        /// </summary>
        Aluminium = 7,
        /// <summary>
        /// Silber
        /// </summary>
        Silver = 8
    }

    /// <summary>
    /// Modus der Engine
    /// </summary>
    public enum EngineMode
    {
        /// <summary>
        /// Editor
        /// </summary>
        Edit,
        /// <summary>
        /// Game
        /// </summary>
        Play
    }

    /// <summary>
    /// Lichttyp
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// Punktlicht (in alle Richtungen scheinend)
        /// </summary>
        Point = 0,
        /// <summary>
        /// Gerichtetes Licht (in eine Richtung scheinend)
        /// </summary>
        Directional = 1,
        /// <summary>
        /// Sonnenlicht (in eine Richtung scheinend aber ohne Entfernungsbegrenzung)
        /// </summary>
        Sun = 2
    }

    internal enum EditorAddObjectType
    {
        None,
        GameObject,
        LightObject
    }

    /// <summary>
    /// Qualität der Lichtschatten
    /// </summary>
    public enum ShadowQuality
    {
        /// <summary>
        /// Kein Schattenwurf
        /// </summary>
        NoShadow = -1,
        /// <summary>
        /// Niedrige Qualität
        /// </summary>
        Low = 512,
        /// <summary>
        /// Mittlere Qualität (Standard)
        /// </summary>
        Medium = 1024,
        /// <summary>
        /// Hohe Qualität
        /// </summary>
        High = 4096
    }

    /// <summary>
    /// Steuert die 3D-Darstellung der Kamera
    /// </summary>
    public enum ProjectionType
    {
        /// <summary>
        /// Nahe Objekte werden größer gezeichnet als ferne Objekte (Standard)
        /// </summary>
        Perspective,
        /// <summary>
        /// Alle Objekte werden ungeachtet der Kameradistanz gleich groß gezeichnet (ideal für 2D)
        /// </summary>
        Orthographic
    }

    internal enum ClearColorMode
    {
        OneZeroZeroZero,
        ZeroZeroZeroOne,
        AllZero,
        AllOne,
        OneOneOneZero
    }

    /// <summary>
    /// Einheit zur Anzeige eines 10 Einheiten großen Gitternetzes
    /// </summary>
    public enum GridType
    {
        /// <summary>
        /// Kein Gitternetz
        /// </summary>
        None,
        /// <summary>
        /// Ein Gitternetz auf der durch die XZ-Achsen aufgespannten Ebene
        /// </summary>
        GridXZ,
        /// <summary>
        /// Ein Gitternetz auf der durch die XY-Achsen aufgespannten Ebene
        /// </summary>
        GridXY
    }

    /// <summary>
    /// Schriftart der HUD-Objekte
    /// </summary>
    public enum FontFace
    {
        /// <summary>
        /// "Anonymous" (Standardschriftart)
        /// </summary>
        Anonymous = 0,
        /// <summary>
        /// "Major Mono Display"
        /// </summary>
        MajorMonoDisplay = 1,
        /// <summary>
        /// "Nova Mono"
        /// </summary>
        NovaMono = 2,
        /// <summary>
        /// "Xanh Mono"
        /// </summary>
        XanhMono = 3,
        /// <summary>
        /// "Open Sans"
        /// </summary>
        OpenSans = 4
    }

    /// <summary>
    /// Aktivierung von Post-Processing-Effekten (Standard: hohe Qualität)
    /// </summary>
    public enum PostProcessingQuality
    {
        /// <summary>
        /// Hoch
        /// </summary>
        High = 2,
        /// <summary>
        /// Standard (vorausgewählt)
        /// </summary>
        Standard = 1
    };

    /// <summary>
    /// Seite des KWCube
    /// </summary>
    public enum CubeSide
    {
        /// <summary>
        /// Alle Würfelseiten
        /// </summary>
        All = 10,
        /// <summary>
        /// Frontseite (+Z)
        /// </summary>
        Front = 1,
        /// <summary>
        /// Rückseite (-Z)
        /// </summary>
        Back = 5,
        /// <summary>
        /// Links (-X)
        /// </summary>
        Left = 2,
        /// <summary>
        /// Rechts (+X)
        /// </summary>
        Right = 4,
        /// <summary>
        /// Oben (+Y)
        /// </summary>
        Top = 0,
        /// <summary>
        /// Unten (-Y)
        /// </summary>
        Bottom = 3
    }
    /// <summary>
    /// Art der Textur (Standard: Diffuse)
    /// </summary>
    public enum TextureType
    {
        /// <summary>
        /// Standardtextur
        /// </summary>
        Albedo,
        /// <summary>
        /// Normal Map
        /// </summary>
        Normal,
        /// <summary>
        /// Metallic Map (PBR Workflow)
        /// </summary>
        Metallic,
        /// <summary>
        /// Roughness Map (PBR Workflow)
        /// </summary>
        Roughness,
        /// <summary>
        /// Emissive Map
        /// </summary>
        Emissive,
        /// <summary>
        /// Transparency Map
        /// </summary>
        Transparency
    };

    /// <summary>
    /// Bezeichnet die beiden Achsen, die die gewünschte Ebene aufspannen.
    /// </summary>
    public enum Plane
    {
        /// <summary>
        /// YZ
        /// </summary>
        YZ,
        /// <summary>
        /// XZ
        /// </summary>
        XZ,
        /// <summary>
        /// XY
        /// </summary>
        XY,
        /// <summary>
        /// Kamerablickebene
        /// </summary>
        Camera
    }

    /// <summary>
    /// Bezeichnet die Achse
    /// </summary>
    public enum Axis
    {
        /// <summary>
        /// X
        /// </summary>
        X,
        /// <summary>
        /// Y
        /// </summary>
        Y,
        /// <summary>
        /// Z
        /// </summary>
        Z,
        /// <summary>
        /// Kamerablickebene
        /// </summary>
        Camera
    }

    internal enum FramebufferMode
    {
        Framebuffer,
        FramebufferRead,
        FramebufferDraw
    }

    internal enum FramebufferTextureMode
    {
        R32F,
        RGB8,
        RGBA8,
        RGBA16UI,
        RGB16F,
        RGBA16F,
        RGBA32F,
        RG8,
        RG32I,
        DEPTH32F,
        DEPTH16F,
        R8
    }

    internal enum HUDObjectType
    {
        Text,
        Image
    }

    internal enum FaceTestMode
    {
        Y,
        Z
    }
}
