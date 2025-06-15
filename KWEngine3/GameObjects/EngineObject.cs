using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Oberklasse aller GameObject- und RenderObject-Instanzen
    /// </summary>
    public abstract class EngineObject
    {
        /// <summary>
        /// Gibt an, ob sich das Objekt aktuell in der Welt befindet
        /// </summary>
        public bool IsInCurrentWorld
        {
            get
            {
                return _myWorld == KWEngine.CurrentWorld;
            }
        }

        /// <summary>
        /// Names des Objekts
        /// </summary>
        public string Name { get { return _name; } set { if (value != null && value.Length > 0) _name = value; } }

        /// <summary>
        /// Setzt bzw. gibt an, ob das Objekt von anderen Objekten aufgrund der Entfernung zur Kamera verdeckt werden kann (Standard: true)
        /// </summary>
        public bool IsDepthTesting { get; set; } = true;

        /// <summary>
        /// Gibt an, ob für das Objekt auch die der Kamera abgewandten Seiten gerendet werden sollen. Dies kann helfen, einseitige Meshes korrekt zu rendern.
        /// </summary>
        public bool DisableBackfaceCulling { get; set; } = false;

        /// <summary>
        /// Gibt an, ob das Objekt Schatten werfen und empfangen kann (Standard: false)
        /// </summary>
        public bool IsShadowCaster { get { return _isShadowCaster; } set { _isShadowCaster = value; } }

        /// <summary>
        /// Gibt an, ob das Objekt von Lichtquellen und dem Ambient Light beeinflusst wird (Standard: true)
        /// </summary>
        public bool IsAffectedByLight { get { return _isAffectedByLight; } set { _isAffectedByLight = value; } }

        /// <summary>
        /// Gibt an, ob sich das Objekt gerade auf dem Bildschirm befindet
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = true;
        internal bool IsInsideScreenSpaceForRenderPass { get; set; } = true;
        internal bool IsInsideScreenSpaceForShadowMap { get; set; } = true;


        /// <summary>
        /// Verweis auf die aktuelle Welt
        /// </summary>
        public static World CurrentWorld { get { return KWEngine.CurrentWorld; } }

        /// <summary>
        /// Verweis auf das Anwendungsfenster
        /// </summary>
        public static GLWindow Window { get { return KWEngine.Window; } }

        /// <summary>
        /// Gibt an, ob das Objekt nicht gerendert werden soll
        /// </summary>
        public bool SkipRender { get; set; } = false;

        /// <summary>
        /// Setzt manuell fest, ob das Objekt Texturen aufweist, die einen Alpha-Kanal besitzen
        /// </summary>
        public bool HasTransparencyTexture { get; set; } = false;

        /// <summary>
        /// Gibt an, ob das Objekt Transparenzanteile besitzt
        /// </summary>
        public bool IsTransparent
        {
            get
            {
                if (HasTransparencyTexture && _stateCurrent._opacity > 0f)
                    return true;
                if (_stateCurrent._opacity < 1f && _stateCurrent._opacity > 0f)
                    return true;
                foreach (GeoMaterial mat in _model.Material)
                {
                    if (mat.ColorAlbedo.W < 1f && mat.ColorAlbedo.W > 0f)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt gerade vollständig unsichtbar ist
        /// </summary>
        public bool IsInvisible
        {
            get
            {
                if (_stateCurrent._opacity <= 0f)
                    return true;
                else
                {
                    bool visible = false;
                    foreach (GeoMaterial mat in _model.Material)
                    {
                        if (mat.ColorAlbedo.W > 0f)
                        {
                            visible = true;
                        }
                    }
                    return !visible;
                }
            }
        }

        /// <summary>
        /// Mittelpunkt des Objekts
        /// </summary>
        public Vector3 Center { get { return _stateCurrent._center; } }
        /// <summary>
        /// Maße des Objekts (jeweils maximal)
        /// </summary>
        public Vector3 Dimensions { get { return _stateCurrent._dimensions; } }

        /// <summary>
        /// Erfragt die auf der Y-Achse niedrigste Position des Objekts
        /// </summary>
        public float AABBLow { get { return _stateCurrent._center.Y - _stateCurrent._dimensions.Y * 0.5f; } }

        /// <summary>
        /// Erfragt die auf der X-Achse linkste Position des Objekts
        /// </summary>
        public float AABBLeft { get { return _stateCurrent._center.X - _stateCurrent._dimensions.X * 0.5f; } }

        /// <summary>
        /// Erfragt die auf der X-Achse rechteste Position des Objekts
        /// </summary>
        public float AABBRight { get { return _stateCurrent._center.X + _stateCurrent._dimensions.X * 0.5f; } }

        /// <summary>
        /// Erfragt die auf der Y-Achse höchste Position des Objekts
        /// </summary>
        public float AABBHigh { get { return _stateCurrent._center.Y + _stateCurrent._dimensions.Y * 0.5f; } }

        /// <summary>
        /// Erfragt die auf der Z-Achse hinterste Position des Objekts
        /// </summary>
        public float AABBBack { get { return _stateCurrent._center.Z - _stateCurrent._dimensions.Z * 0.5f; } }

        /// <summary>
        /// Erfragt die auf der Z-Achse vorderste Position des Objekts
        /// </summary>
        public float AABBFront { get { return _stateCurrent._center.Z + _stateCurrent._dimensions.Z * 0.5f; } }

        /// <summary>
        /// (Normalisierter) Blickrichtungsvektor des Objekts
        /// </summary>
        public Vector3 LookAtVector
        {
            get { return _stateCurrent._lookAtVector; }
        }

        /// <summary>
        /// (Normalisierter) Blickrichtungsvektor des Objekts auf der XZ-Ebene
        /// </summary>
        public Vector3 LookAtVectorXZ
        {
            get
            {
                return Vector3.NormalizeFast(new Vector3(_stateCurrent._lookAtVector.X, 0f, _stateCurrent._lookAtVector.Z));
            }
        }

        /// <summary>
        /// (Normalisierter) Blickrichtungsvektor des Objekts auf der XY-Ebene
        /// </summary>
        public Vector3 LookAtVectorXY
        {
            get
            {
                return Vector3.NormalizeFast(new Vector3(_stateCurrent._lookAtVector.X, _stateCurrent._lookAtVector.Y, 0f));
            }
        }

        /// <summary>
        /// (Normalisierter) Lokaler Oben-Vektor des Objekts
        /// </summary>
        public Vector3 LookAtVectorLocalUp
        {
            get { return _stateCurrent._lookAtVectorUp; }
        }
        /// <summary>
        /// (Normalisierter) Lokaler Rechts-Vektor des Objekts
        /// </summary>
        public Vector3 LookAtVectorLocalRight
        {
            get { return _stateCurrent._lookAtVectorRight; }
        }

        /// <summary>
        /// Anzahl der Sekunden, die die Anwendung bereits läuft
        /// </summary>
        public float ApplicationTime { get { return KWEngine.ApplicationTime; } }
        /// <summary>
        /// Anzahl der Sekunden, die die aktuelle Welt bereits läuft
        /// </summary>
        public float WorldTime { get { return KWEngine.WorldTime; } }

        // =============================================================
        // =========================METHODS=============================
        // =============================================================

        /// <summary>
        /// Abstrakte Methode die von jeder erbenden Klasse implementiert werden muss
        /// </summary>
        public abstract void Act();

        /// <summary>
        /// Setzt das 3D-Modell des Objekts
        /// </summary>
        /// <param name="modelname">Name des 3D-Modells</param>
        /// <returns>true, wenn das Modell gesetzt werden konnte</returns>
        public abstract bool SetModel(string modelname);


        /// <summary>
        /// Gibt den Namen des Objekts zurück
        /// </summary>
        /// <returns>Informationen zum Objekt</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Erfragt den Namen des aktuell gesetzten 3D-Modells
        /// </summary>
        /// <returns>Modellname</returns>
        public string GetModelName()
        {
            return _modelNameInDB;
        }

        /// <summary>
        /// Setzt die Sichtbarkeit des Objekts (Standard: 1)
        /// </summary>
        /// <param name="o">Sichtbarkeit (0 bis 1)</param>
        public void SetOpacity(float o)
        {
            _stateCurrent._opacity = MathHelper.Clamp(o, 0f, 1f);
        }

        /// <summary>
        /// Enthält die aktuelle Farbtönung des Objekts
        /// </summary>
        /// <returns>Farbtönung (normalisierte RGB-Werte) als Vector3-Instanz</returns>
        public Vector3 Color
        {
            get
            {
                return _stateCurrent._colorTint;
            }
        }

        /// <summary>
        /// Enthält die aktuelle Leuchtfarbe und -intensität des Objekts
        /// </summary>
        /// <returns>Leuchtfarbe (normalisierte RGB-Werte) als Vector4-Instanz</returns>
        public Vector4 ColorEmissive
        {
            get
            {
                return _stateCurrent._colorEmissive;
            }
        }

        /// <summary>
        /// Setzt die Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        public void SetColor(float r, float g, float b)
        {
            _stateCurrent._colorTint = new Vector3(
                MathHelper.Clamp(r, 0, 1),
                MathHelper.Clamp(g, 0, 1),
                MathHelper.Clamp(b, 0, 1));
        }

        /// <summary>
        /// Setzt die selbstleuchtende Farbtönung des Objekts
        /// </summary>
        /// <param name="r">Rotanteil (zwischen 0 und 1)</param>
        /// <param name="g">Grünanteil (zwischen 0 und 1)</param>
        /// <param name="b">Blauanteil (zwischen 0 und 1)</param>
        /// <param name="intensity">Helligkeit (zwischen 0 und 2)</param>
        public void SetColorEmissive(float r, float g, float b, float intensity)
        {
            SetColorEmissive(new Vector3(r, g, b), intensity);
        }

        /// <summary>
        /// Setzt die selbstleuchtende Farbtönung des Objekts
        /// </summary>
        /// <param name="color">Rot-/Grün-/Blauanteil (zwischen 0 und 1)</param>
        /// <param name="intensity">Helligkeit (zwischen 0 und 2)</param>
        public void SetColorEmissive(Vector3 color, float intensity)
        {
            _stateCurrent._colorEmissive = new Vector4(
                MathHelper.Clamp(color.X, 0, 1),
                MathHelper.Clamp(color.Y, 0, 1),
                MathHelper.Clamp(color.Z, 0, 1),
                MathHelper.Clamp(intensity, 0, 2));
        }

        /// <summary>
        /// Setzt fest, wie metallisch das Objekt ist
        /// </summary>
        /// <param name="m">Metallwert (zwischen 0 und 1)</param>
        /// <param name="meshId">ID des zu ändernden Meshs/Materials (Standard: 0)</param>
        public void SetMetallic(float m, int meshId = 0)
        {
            if (meshId < _model.Material.Length)
                _model.Material[meshId].Metallic = MathHelper.Clamp(m, 0f, 1f);
        }

        /// <summary>
        /// Setzt die Art des Metalls
        /// </summary>
        /// <param name="type">Metalltyp</param>
        public void SetMetallicType(MetallicType type)
        {
            _model._metallicType = type;
        }

        /// <summary>
        /// Setzt die Rauheit der Objektoberfläche (Standard: 1)
        /// </summary>
        /// <param name="r">Rauheit (zwischen 0 und 1)</param>
        /// <param name="meshId">ID des zu ändernden Meshs/Materials (Standard: 0)</param>
        public void SetRoughness(float r, int meshId = 0)
        {
            if (meshId < _model.Material.Length)
                _model.Material[meshId].Roughness = MathHelper.Clamp(r, 0.00001f, 1f);
        }

        /// <summary>
        /// Setzt die Textur des Objekts
        /// </summary>
        /// <param name="filename">Dateiname der Textur (inkl. relativem Pfad)</param>
        /// <param name="type">Art der Textur (Standard: Albedo)</param>
        /// <param name="meshId">ID des 3D-Modellanteils (Standard: 0)</param>
        public void SetTexture(string filename, TextureType type = TextureType.Albedo, int meshId = 0)
        {
            filename = HelperGeneral.EqualizePathDividers(filename);
            _model.SetTexture(filename.Trim(), type, meshId);
        }

        /// <summary>
        /// Setzt die Texturverschiebung auf dem Objekt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureOffset(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(_stateCurrent._uvTransform.X, _stateCurrent._uvTransform.Y, x, y);
        }


        /// <summary>
        /// Setzt die Texturverschiebung auf dem Objekt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// /// <param name="meshId">ID des 3D-Modellanteils (Standard: 0)</param>
        public void SetTextureOffset(float x, float y, int meshId)
        {
            SetTextureOffsetForMaterial(x, y, meshId);
        }

        /// <summary>
        /// Beschneidet den durch SetTextureRepeat() und SetTextureOffset() gewählten Texturteil weiter (für den Fall, dass z.B. Spritesheets einen zu großen Rand pro Zelle haben) 
        /// </summary>
        /// <param name="x">Beschneidung auf x-Achse (positiver Wert = Beschnitt nach innen, negativer Wert = Beschnitt nach außen)</param>
        /// <param name="y">Beschneidung auf y-Achse (positiver Wert = Beschnitt nach innen, negativer Wert = Beschnitt nach außen)</param>
        public void SetTextureClip(float x, float y)
        {
            _stateCurrent._uvClip = new Vector2(MathHelper.Clamp(-x, -1.0f, 1.0f), MathHelper.Clamp(-y, -1.0f, 1.0f));
        }

        /// <summary>
        /// Setzt die Texturwiederholung auf dem Objekt (Standard: 1)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public void SetTextureRepeat(float x, float y)
        {
            _stateCurrent._uvTransform = new Vector4(x, y, _stateCurrent._uvTransform.Z, _stateCurrent._uvTransform.W);
        }

        /// <summary>
        /// Setzt die Texturwiederholung auf einem einzelnen Mesh eines Objekts (Standard: 1)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="meshIndex">Nullbasierter Index des zu ändernden Meshs/Materials</param>
        public void SetTextureRepeat(float x, float y, int meshIndex)
        {
            SetTextureRepeatForMaterial(x, y, meshIndex);
        }

        /// <summary>
        /// Erfragt die aktuellen Werte für die Texturwiederholung
        /// </summary>
        public Vector2 TextureRepeat
        {
            get
            {
                return this._stateCurrent._uvTransform.Xy;
            }
        }

        /// <summary>
        /// Erfragt die aktuellen Werte für die Texturverschiebung
        /// </summary>
        public Vector2 TextureOffset
        {
            get
            {
                return this._stateCurrent._uvTransform.Zw;
            }
        }

        /// <summary>
        /// Setzt die Größenskalierung des Objekts (muss > 0 sein)
        /// </summary>
        /// <param name="s">Skalierung</param>
        public void SetScale(float s)
        {
            SetScale(s, s, s);
        }

        /// <summary>
        /// Setzt die Größenskalierung des Objekts (muss > 0 sein)
        /// </summary>
        /// <param name="x">Skalierung entlang der x-Achse</param>
        /// <param name="y">Skalierung entlang der y-Achse</param>
        public void SetScale(float x, float y)
        {
            SetScale(x, y, Scale.Z);
        }

        /// <summary>
        /// Bewegt das Objekt in seiner Blickrichtung
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void Move(float units)
        {
            MoveOffset(LookAtVector * units);
        }

        /// <summary>
        /// Bewegt das Objekt in seiner Blickrichtung (ohne Höhenunterschied)
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveXZ(float units)
        {
            Vector3 lavXZ = LookAtVector;
            lavXZ.Y = 0;
            Vector3.NormalizeFast(lavXZ);
            MoveOffset(lavXZ * units);
        }

        /// <summary>
        /// Bewegt das Objekt entlang seines lokalen "Oben"-Vektors
        /// </summary>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveUp(float units)
        {
            MoveAlongVector(LookAtVectorLocalUp, units);
        }

        /// <summary>
        /// Bewegt das Objekt um die gegebenen Einheiten entlang eines Vektors
        /// </summary>
        /// <param name="v">Richtungsvektor</param>
        /// <param name="units">Bewegungseinheiten</param>
        public void MoveAlongVector(Vector3 v, float units)
        {
            if(float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z))
            {
                return;
            }
            MoveOffset(v * units);
        }

        /// <summary>
        /// Bewegt das Objekt entlang der drei Weltachsen
        /// </summary>
        /// <param name="x">Bewegungseinheiten in x-Richtung</param>
        /// <param name="y">Bewegungseinheiten in y-Richtung</param>
        /// <param name="z">Bewegungseinheiten in z-Richtung</param>
        public void MoveOffset(float x, float y, float z)
        {
            MoveOffset(new Vector3(x, y, z));
        }

        /// <summary>
        /// Bewegt das Objekt entlang der drei Weltachsen
        /// </summary>
        /// <param name="offset">Bewegungseinheiten in 3D</param>
        public void MoveOffset(Vector3 offset)
        {
            SetPosition(Position + offset);
        }

        /// <summary>
        /// Setzt die Skalierung der Instanz
        /// </summary>
        /// <param name="s">Skalierung in Reihenfolge X, Y, Z</param>
        public void SetScale(Vector3 s)
        {
            SetScale(s.X, s.Y, s.Z);
        }

        /// <summary>
        /// Position des Objekts
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _stateCurrent._position;
            }
        }

        /// <summary>
        /// Rotation/Orientierung des Objekts
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return _stateCurrent._rotation;
            }
        }

        /// <summary>
        /// Größe des Objekts
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return _stateCurrent._scale;
            }
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">Position auf x-Achse</param>
        /// <param name="y">Position auf y-Achse</param>
        /// <param name="z">Position auf z-Achse</param>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }
        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="position">Position in 3D</param>
        public void SetPosition(Vector3 position)
        {
            _stateCurrent._position = position;
            UpdateModelMatrixAndHitboxes();
        }
        /// <summary>
        /// Setzt die x-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="x">Positionswert</param>
        public void SetPositionX(float x)
        {
            SetPosition(new Vector3(x, Position.Y, Position.Z));
        }

        /// <summary>
        /// Setzt die y-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="y">Positionswert</param>
        public void SetPositionY(float y)
        {
            SetPosition(new Vector3(Position.X, y, Position.Z));
        }

        /// <summary>
        /// Setzt die z-Position der Instanz auf den gegebenen Wert
        /// </summary>
        /// <param name="z">Positionswert</param>
        public void SetPositionZ(float z)
        {
            SetPosition(new Vector3(Position.X, Position.Y, z));
        }

        /// <summary>
        /// Setzt die Orientierung/Rotation des Objekts
        /// </summary>
        /// <param name="x">Rotation um lokale x-Achse in Grad</param>
        /// <param name="y">Rotation um lokale y-Achse in Grad</param>
        /// <param name="z">Rotation um lokale z-Achse in Grad</param>
        public void SetRotation(float x, float y, float z)
        {
            SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z)));
        }

        /// <summary>
        /// Fügt die übergebene Rotation der bestehenden hinzu
        /// </summary>
        /// <param name="additionalRotation">hinzuzufügende Rotatation</param>
        public void AddRotation(Quaternion additionalRotation)
        {
            _stateCurrent._rotation *= additionalRotation;
        }

        /// <summary>
        /// Erhöht die Rotation um die x-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationX(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Erhöht die Rotation um die y-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationY(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Erhöht die Rotation um die z-Achse
        /// </summary>
        /// <param name="r">Grad</param>
        /// <param name="worldSpace">true, wenn um die Weltachse statt um die lokale Achse rotiert werden soll</param>
        public void AddRotationZ(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Setzt die Rotation mit Hilfe eines Quaternion-Objekts
        /// </summary>
        /// <param name="rotation">Rotation</param>
        public void SetRotation(Quaternion rotation)
        {
            _stateCurrent._rotation = rotation;
            UpdateModelMatrixAndHitboxes();
        }
        /// <summary>
        /// Setzt die Größenskalierung des Objekts entlang seiner lokalen drei Achsen
        /// </summary>
        /// <param name="x">Skalierung in x-Richtung</param>
        /// <param name="y">Skalierung in y-Richtung</param>
        /// <param name="z">Skalierung in z-Richtung</param>
        public void SetScale(float x, float y, float z)
        {
            _stateCurrent._scale = new Vector3(
                Math.Max(0.000001f, x),
                Math.Max(0.000001f, y),
                Math.Max(0.000001f, z));
            UpdateModelMatrixAndHitboxes();
        }

        /// <summary>
        /// Erfragt die Rotation, die zu einem bestimmten Ziel notwendig wäre
        /// </summary>
        /// <param name="target">Ziel</param>
        /// <returns>Rotation (als Quaternion)</returns>
        public Quaternion GetRotationToTarget(Vector3 target)
        {
            Matrix3 lookat;
            if (EngineCamera.Camera.IsCameraLookAtPossiblyNaN(Center, target, out Vector3 targetNew))
            {
                lookat = new(Matrix4.LookAt(targetNew, Center, KWEngine.WorldUp));
            }
            else
            {
                lookat = new(Matrix4.LookAt(target, Center, KWEngine.WorldUp));
            }

            
            lookat = Matrix3.Transpose(lookat);
            Quaternion q = Quaternion.FromMatrix(lookat);
            q.Invert();
            return q;
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="x">x-Zielkoordinate</param>
        /// <param name="y">y-Zielkoordinate</param>
        /// <param name="z">z-Zielkoordinate</param>
        public void TurnTowardsXYZ(float x, float y, float z)
        {
            TurnTowardsXYZ(new Vector3(x, y, z));
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="target">Zielkoordinate</param>
        public void TurnTowardsXYZ(Vector3 target)
        {
            if (target == _stateCurrent._position) return;
            SetRotation(GetRotationToTarget(target));
        }

        /// <summary>
        /// Gleicht die Rotation der Instanz an die der Kamera an
        /// </summary>
        public void AdjustRotationToCameraRotation()
        {
            SetRotation(HelperRotation.GetRotationTowardsCamera());
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="targetX">X-Koordinate</param>
        /// <param name="targetY">Y-Koordinate</param>
        public void TurnTowardsXY(float targetX, float targetY)
        {
            if (targetX == _stateCurrent._position.X && targetY == _stateCurrent._position.Y) return;
            Vector3 target = new(targetX, targetY, 0);
            TurnTowardsXY(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XY-Koordinaten blickt. Z-Unterschiede Unterschiede werden ignoriert.
        /// [Geeignet, wenn die Kamera entlang der z-Achse blickt (Standard)]
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXY(Vector3 target)
        {
            if (target == _stateCurrent._position) return;
            target.Z = Position.Z + 0.000001f;
            Matrix4 lookat = Matrix4.LookAt(target, Position, Vector3.UnitZ);
            lookat.Transpose();
            if (lookat.Determinant != 0)
            {
                lookat.Invert();
                SetRotation(Quaternion.FromMatrix(new Matrix3(lookat)));
            }
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="targetX">Zielkoordinate der x-Achse</param>
        /// <param name="targetZ">Zielkoordinate der z-Achse</param>
        public void TurnTowardsXZ(float targetX, float targetZ)
        {
            if (targetX == _stateCurrent._position.X && targetZ == _stateCurrent._position.Z) return;
            Vector3 target = new(targetX, 0, targetZ);
            TurnTowardsXZ(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXZ(Vector3 target)
        {
            if (target == _stateCurrent._position) return;
            target.Y = Position.Y + 0.000001f;
            Matrix4 lookat = Matrix4.LookAt(target, Position, Vector3.UnitY);
            lookat.Transpose();
            if (lookat.Determinant != 0)
            {
                lookat.Invert();
                SetRotation(Quaternion.FromMatrix(new Matrix3(lookat)));
            }
        }

        /// <summary>
        /// Konvertiert die aktuelle Rotation in Gradangaben für jede der drei Weltachsen
        /// </summary>
        /// <returns>Gradangaben für die aktuelle Rotation des Objekts um die x-, y- und z-Achse</returns>
        public Vector3 GetRotationEulerAngles()
        {
            return HelperRotation.ConvertQuaternionToEulerAngles(Rotation);
        }

        /// <summary>
        /// Setzt die Rotation passend zum übergebenen Ebenenvektor (surface normal), um z.B. das Objekt zu kippen, wenn es auf einer Schräge steht.
        /// </summary>
        /// <remarks>Hinweis: Kann bei KWQuad-Modellen zu fehlerhaften Rotationen führen. Hier muss ggf. manuell mit AddRotation...() nachgebessert werden.</remarks>
        /// <param name="surfaceNormal">Ebenenvektor</param>
        public void SetRotationToMatchSurfaceNormal(Vector3 surfaceNormal)
        {
            SetRotation(HelperVector.GetRotationToMatchSurfaceNormal(LookAtVector, surfaceNormal));
        }

        /// <summary>
        /// Gibt an, ob das Objekt gerade eine Animation ausgewählt hat
        /// </summary>
        public bool IsAnimated
        {
            get
            {
                return _model.ModelOriginal.HasBones && _statePrevious._animationID >= 0;
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt über Animationen verfügt
        /// </summary>
        public bool HasAnimations
        {
            get
            {
                return _model.ModelOriginal.HasBones && _model.ModelOriginal.Animations != null && _model.ModelOriginal.Animations.Count > 0;
            }
        }

        /// <summary>
        /// Setzt die Animationsnummer des Objekts (muss >= 0 sein)
        /// </summary>
        /// <param name="id">ID</param>
        public void SetAnimationID(int id)
        {
            if (_model.ModelOriginal.Animations != null)
                _stateCurrent._animationID = MathHelper.Clamp(id, -1, _model.ModelOriginal.Animations.Count - 1);
            else
                _stateCurrent._animationID = -1;
        }

        /// <summary>
        /// Setzt den Stand der Animation zwischen 0% und 100% (0 bis 1)
        /// </summary>
        /// <param name="p">Stand (Werte zwischen 0 und 1)</param>
        public void SetAnimationPercentage(float p)
        {
            p = MathHelper.Clamp(p, 0f, 1f);
            if (Math.Abs(p - _stateCurrent._animationPercentage) > 0.5f)
            {
                _stateCurrent._animationPercentage = p;
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
            else
            {
                _stateCurrent._animationPercentage = p;
            }
        }

        /// <summary>
        /// Führt die Animation um einen gegebenen Teil fort
        /// </summary>
        /// <param name="p">relativer Fortschritt der Animation</param>
        public void SetAnimationPercentageAdvance(float p)
        {
            _stateCurrent._animationPercentage += p;
            if (_stateCurrent._animationPercentage > 1f)
            {
                _stateCurrent._animationPercentage--;
                _statePrevious._animationPercentage = _stateCurrent._animationPercentage;
            }
        }

        /// <summary>
        /// Setzt die Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <param name="hue">Farbverschiebung (in Grad)</param>
        /// <param name="meshIndex">Index des Model-Meshs (Standard: 0)</param>
        public void SetHue(float hue, int meshIndex = 0)
        {
            if (_model.ModelOriginal.Meshes.Count > meshIndex)
                _hues[meshIndex] = MathHelper.DegreesToRadians(hue % 360f);
            else
                KWEngine.LogWriteLine("[EngineObject] Invalid meshIndex value");
        }

        /// <summary>
        /// Erfragt die aktuelle Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <param name="meshIndex">Index des Model-Meshs (Standard: 0)</param>
        /// <returns>Aktuelle Farbverschiebung (in Grad)</returns>
        public float GetHue(int meshIndex = 0)
        {
            if (_model.ModelOriginal.Meshes.Count > meshIndex)
            {
                return MathHelper.RadiansToDegrees(_hues[meshIndex]);
            }
            else
            {
                KWEngine.LogWriteLine("[EngineObject] Invalid meshIndex value - returning 0");
                return 0f;
            }
        }

        #region internals
        internal bool _isShadowCaster = false;
        internal bool _isAffectedByLight = true;
        internal EngineObjectState _statePrevious;
        internal EngineObjectState _stateCurrent;
        internal EngineObjectRenderState _stateRender;
        internal string _name = "(no name)";
        internal EngineObjectModel _model;
        internal ColliderModel _colliderModel;
        internal string _modelNameInDB = "KWCube";
        internal int _importedID = -1;
        internal float _positionCenterDelta = 0f;
        internal bool _positionLowerThanCenter = true;
        internal float[] _hues;

        internal void CheckPositionAndCenter()
        {
            Vector3 delta = Position - Center;
            _positionCenterDelta = delta.LengthFast;
            float dotPosLookAtUp = Vector3.Dot(delta, LookAtVectorLocalUp);
            _positionLowerThanCenter = dotPosLookAtUp < 0f;
        }

        internal void InitPreRotationQuaternions()
        {
            _statePrevious._rotationPre = new();
            _stateCurrent._rotationPre = new();
            _stateRender._rotationPre = new();
        }

        internal void InitRenderStateMatrices()
        {
            _stateRender._modelMatrices = new Matrix4[_model.ModelOriginal.Meshes.Count];
            _stateRender._normalMatrices = new Matrix4[_model.ModelOriginal.Meshes.Count];

            if (HasArmatureAndAnimations)
            {
                _stateRender._boneTranslationMatrices = new();
                foreach (GeoMesh mesh in _model.ModelOriginal.Meshes.Values)
                {
                    _stateRender._boneTranslationMatrices[mesh.Name] = new Matrix4[mesh.BoneIndices.Count];
                    for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        _stateRender._boneTranslationMatrices[mesh.Name][i] = Matrix4.Identity;
                }
            }
        }

        internal bool HasArmatureAndAnimations
        {
            get
            {
                return _model.ModelOriginal.HasBones && _model.ModelOriginal.Animations != null && _model.ModelOriginal.Animations.Count > 0;
            }
        }

        internal void SetTextureRepeatForMaterial(float x, float y, int materialIndex)
        {
            if (_model.Material.Length > materialIndex && _model.Material[materialIndex].TextureAlbedo.IsTextureSet)
            {
                float clipX = _model.Material[materialIndex].TextureAlbedo.UVTransform.Z;
                float clipY = _model.Material[materialIndex].TextureAlbedo.UVTransform.W;
                _model.Material[materialIndex].TextureAlbedo.UVTransform = new Vector4(x, y, clipX, clipY);
            }
            else
            {
                if (_model.Material.Length <= materialIndex)
                    KWEngine.LogWriteLine("[EngineObject] Texture repeat: invalid material");
            }
        }

        internal void SetTextureOffsetForMaterial(float x, float y, int materialIndex)
        {
            if (_model.Material.Length > materialIndex && _model.Material[materialIndex].TextureAlbedo.IsTextureSet)
            {
                float repeatX = _model.Material[materialIndex].TextureAlbedo.UVTransform.X;
                float repeatY = _model.Material[materialIndex].TextureAlbedo.UVTransform.Y;
                _model.Material[materialIndex].TextureAlbedo.UVTransform = new Vector4(repeatX, repeatY, x, y);
            }
            else
            {
                if (_model.Material.Length <= materialIndex)
                    KWEngine.LogWriteLine("[EngineObject] Texture offset: invalid material");
            }
        }

        internal void AddRotationXFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void AddRotationYFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void AddRotationZFromEditor(float r, bool worldSpace = false)
        {
            Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(r));
            if (worldSpace)
            {
                _stateCurrent._rotation = tmpRotate * _stateCurrent._rotation;
            }
            else
            {
                _stateCurrent._rotation *= tmpRotate;
            }
            UpdateModelMatrixAndHitboxes();
        }

        internal void SetMetallicType(int typeIndex)
        {
            SetMetallicType((MetallicType)typeIndex);
        }

        internal void InitStates()
        {
            _stateCurrent = new EngineObjectState(this);
            _stateRender = new EngineObjectRenderState(this);
            UpdateModelMatrixAndHitboxes();
            _statePrevious = _stateCurrent;
            CheckPositionAndCenter();
        }

        internal bool HasEmissiveTexture
        {
            get
            {
                foreach (GeoMaterial m in _model.Material)
                {
                    if (m.TextureEmissive.IsTextureSet)
                        return true;
                }
                return false;
            }
        }

        internal World _myWorld = null;

        internal abstract void InitHitboxes();

        internal abstract void UpdateModelMatrixAndHitboxes();
        
        #endregion
    }
}
