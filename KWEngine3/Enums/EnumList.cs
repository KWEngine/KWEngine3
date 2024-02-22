namespace KWEngine3
{
    internal enum AddRemoveHitboxMode
    {
        None,
        Add,
        Remove
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
        Left,
        /// <summary>
        /// Zentriert
        /// </summary>
        Center,
        /// <summary>
        /// Rechtsbündig
        /// </summary>
        Right
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
        NoShadow = 0,
        /// <summary>
        /// Niedrige Qualität
        /// </summary>
        Low = 512,
        /// <summary>
        /// Mittlere Qualität
        /// </summary>
        Medium = 1024,
        /// <summary>
        /// Hohe Qualität
        /// </summary>
        High = 2048
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
        XanhMono = 3
    }

    /// <summary>
    /// Aktivierung von Post-Processing-Effekten (Standard: hohe Qualität)
    /// </summary>
    public enum PostProcessingQuality
    {
        /// <summary>
        /// Standard
        /// </summary>
        High,
        /// <summary>
        /// Niedrige Qualität (Artefaktbildung möglich)
        /// </summary>
        Low,
        /// <summary>
        /// Ausgeschaltet
        /// </summary>
        Disabled
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
        Emissive
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
        DEPTH16F
    }

    internal enum HUDObjectType
    {
        Text,
        Image
    }
}
