using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Basisklasse für Objekte, die im First-Person-Modus zu sehen sind (Waffen, etc.)
    /// </summary>
    public abstract class ViewSpaceGameObject
    {
        internal GameObject _gameObject;
        internal Vector3 _offset = Vector3.Zero;
        internal Vector3 _right = Vector3.UnitX;
        internal Vector3 _up = Vector3.UnitY;
        internal Quaternion _rotation = Quaternion.Identity;
        internal readonly static Quaternion ROT180 = Quaternion.FromEulerAngles(0, (float)Math.PI, 0);

        internal bool IsValid { get { return _gameObject != null; } }

        /// <summary>
        /// Standardkonstruktormethode (erzeugt einen Würfel)
        /// </summary>
        public ViewSpaceGameObject()
            :this("KWCube")
        {
        }

        /// <summary>
        /// Konstruktormethode, die ein ViewSpaceGameObject des angegebenen Modells erstellt
        /// </summary>
        /// <param name="modelName">Name des Modells</param>
        public ViewSpaceGameObject(string modelName)
        {
            if (modelName != null && KWEngine.Models.ContainsKey(modelName.Trim()))
            {
                _gameObject = new GameObjectFPV(modelName.Trim());
            }
            else
            { 
                _gameObject = null;
            }
        }

        /// <summary>
        /// Setzt die Textur, falls das Modell des Objekts KWCube, KWSphere oder KWQuad(2D) ist
        /// </summary>
        /// <param name="file">Texturdatei (inkl. relativem Pfad)</param>
        /// <param name="type">Texturtyp</param>
        public void SetTextureForPrimitiveModel(string file, TextureType type = TextureType.Albedo)
        {
            if(_gameObject._model.ModelOriginal.IsPrimitive)
            {
                _gameObject.SetTexture(file, type);
            }
            else
            {
                KWEngine.LogWriteLine("[ViewSpaceGameObject] Cannot set texture on non-primitive model");
            }
        }

        /// <summary>
        /// Aktivitätsmethode des Objekts
        /// </summary>
        public abstract void Act();

        /// <summary>
        /// Setzt das 3D-Modell
        /// </summary>
        /// <param name="modelName">Name des 3D-Modells</param>
        public void SetModel(string modelName)
        {
            if(IsValid)
            {
                if(!_gameObject.SetModel(modelName))
                {
                    KWEngine.LogWriteLine("[ViewSpaceGameObject] Invalid model");
                }
                else
                {
                    UpdatePosition();
                }
            }
        }

        /// <summary>
        /// Aktualisiert die Position des Objekts anhand der aktuellen Position und Rotation des Elternobjekts
        /// </summary>
        public void UpdatePosition()
        {
            if (IsValid)
            {
                Vector3 pos = KWEngine.CurrentWorld.CameraPosition;

                _right = Vector3.NormalizeFast(Vector3.Cross(KWEngine.CurrentWorld.CameraLookAtVector, KWEngine.WorldUp));
                _up = Vector3.NormalizeFast(Vector3.Cross(KWEngine.CurrentWorld.CameraLookAtVector, _right));
                Vector3 pos2 = pos + _right * _offset.X + _up * _offset.Y + KWEngine.CurrentWorld.CameraLookAtVector * _offset.Z;

                _gameObject.SetPosition(pos2);
                Quaternion invertedViewRotation = Quaternion.Invert(KWEngine.CurrentWorld._cameraGame._stateCurrent.ViewMatrix.ExtractRotation(false));
                _gameObject.SetRotation(invertedViewRotation * ROT180 * _rotation);
            }
        }

        /// <summary>
        /// Legt die lokale Rotation fest (Reihenfolge x->y->z), die zusätzlich zur Rotation des Elternobjekts angewendet wird
        /// </summary>
        /// <param name="x">Rotation in Grad um die lokale x-Achse</param>
        /// <param name="y">Rotation in Grad um die lokale y-Achse</param>
        /// <param name="z">Rotation in Grad um die lokale z-Achse</param>
        public void SetRotation(float x, float y, float z)
        {
            _rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z));
        }



        /// <summary>
        /// Lässt das Objekt auf Kollisionen mit anderen GameObject-Instanzen prüfen
        /// </summary>
        /// <returns>Liste mit Kollisionen (kann leer sein)</returns>
        public List<Intersection> GetIntersections()
        {
            List<Intersection> intersections = new List<Intersection>();
            if(IsValid)
            {
                intersections.AddRange(_gameObject.GetIntersections());
            }
            return intersections;
        }

        /// <summary>
        /// Verweis auf die aktuelle Welt
        /// </summary>
        public World CurrentWorld { get { return KWEngine.CurrentWorld; } }

        /// <summary>
        /// Setzt fest, ob das Objekt ein Kollisionsobjekt ist
        /// </summary>
        public bool IsCollisionObject { 
            get 
            { 
                return IsValid && _gameObject.IsCollisionObject; 
            }
            /*
            set
            {
                if(IsValid)
                {
                    _gameObject._colliderType = ColliderType.ConvexHull;
                }
            }
            */
        }

        /// <summary>
        /// Setzt fest, ob das Objekt Schatten werfen und empfangen kann
        /// </summary>
        public bool IsShadowCaster
        {
            get
            {
                return IsValid && _gameObject.IsShadowCaster;
            }
            set
            {
                if (IsValid)
                {
                    _gameObject.IsShadowCaster = value;
                }
            }
        }

        /// <summary>
        /// Setzt den Abstand des Objekts zur Kamera bzw. zum Elternobjekt
        /// </summary>
        /// <param name="horizontal">Abstand auf der lokalen x-Achse</param>
        /// <param name="vertical">Abstand auf der lokalen y-Achse</param>
        /// <param name="nearFar">Abstand auf der lokalen z-Achse</param>
        public void SetOffset(float horizontal, float vertical, float nearFar)
        {
            SetOffset(new Vector3(horizontal, -vertical, nearFar));
        }

        /// <summary>
        /// Setzt den Abstand des Objekts zur Kamera bzw. zum Elternobjekt
        /// </summary>
        /// <param name="offset">Abstand</param>
        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
        }

        /// <summary>
        /// Setzt die Größe des Objekts (muss > 0 sein)
        /// </summary>
        /// <param name="s">Skalierung (1 = Standardgröße)</param>
        public void SetScale(float s)
        {
            if (IsValid)
            {
                s = MathHelper.Max(0.000001f, s);
                _gameObject.SetScale(s);
            }
        }

        /// <summary>
        /// Setzt die Animation des Objekts (falls das Modell über Animationen verfügt)
        /// </summary>
        /// <param name="id">Animations-ID (>= 0)</param>
        public void SetAnimationID(int id)
        {
            if (IsValid && _gameObject._model.ModelOriginal.Animations != null)
                _gameObject._stateCurrent._animationID = MathHelper.Clamp(id, -1, _gameObject._model.ModelOriginal.Animations.Count - 1);
            else
            {
                if(IsValid)
                    _gameObject._stateCurrent._animationID = -1;
            }
        }

        /// <summary>
        /// Setzt den Animationsfortschritt (zwischen 0 und 1)
        /// </summary>
        /// <param name="p">Fortschritt (0 = 0%, 1 = 100%)</param>
        public void SetAnimationPercentage(float p)
        {
            if (IsValid)
                _gameObject.SetAnimationPercentage(p);// _stateCurrent._animationPercentage = MathHelper.Clamp(p, 0f, 1f);
        }

        /// <summary>
        /// Setzt den relativen Animationsfortschritt
        /// </summary>
        /// <param name="p">relativer Animationsfortschritt</param>
        public void SetAnimationPercentageAdvance(float p)
        {
            if (IsValid)
                _gameObject.SetAnimationPercentageAdvance(p);
        }

        /// <summary>
        /// Bindet eine andere GameObject-Instanz an den jeweiligen Knochen des aktuell verwendeten Modells
        /// </summary>
        /// <param name="g">Anzuhängende Instanz</param>
        /// <param name="boneName">Name des Knochens, an den die Instanz angehängt werden soll</param>
        public void AttachGameObjectToBone(GameObject g, string boneName)
        {
            if (IsValid)
                _gameObject.AttachGameObjectToBone(g, boneName);
        }

        /// <summary>
        /// Entfernt die Bindung (Attachment) einer GameObject-Instanz 
        /// </summary>
        /// <param name="boneName">Name des Knochens</param>
        public void DetachGameObjectFromBone(string boneName)
        {
            if (IsValid)
                _gameObject.DetachGameObjectFromBone(boneName);
        }

        /// <summary>
        /// Liefert true, wenn mind. eine GameObject-Instanz an einen Knochen des aufrufenden Objekts gebunden ist
        /// </summary>
        public bool HasAttachedGameObjects { get { return IsValid && _gameObject._gameObjectsAttached.Count > 0; } }

        /// <summary>
        /// Liefert eine Liste der Knochennamen, an die aktuell eine andere GameObject-Instanz gebunden ist
        /// </summary>
        /// <returns>Liste der verwendeten Knochennamen (für Attachments)</returns>
        public List<string> GetBoneNamesForAttachedGameObject()
        {
            List<string> values = new List<string>();
            if(IsValid)
            {
                values.AddRange(_gameObject.GetBoneNamesForAttachedGameObject());
            }
            return values;
        }

        /// <summary>
        /// Liefert die Referenz auf das Objekt, an das die aktuelle Instanz gebunden ist (kann null sein!)
        /// </summary>
        /// <returns>GameObject, an das die Instanz (via Knochen) gebunden ist</returns>
        public GameObject GetGameObjectThatIAmAttachedTo()
        {
            if (IsValid)
                return _gameObject._attachedTo;
            else
                return null;
        }

        /// <summary>
        /// Liefert die an einen Knochen gebundene GameObject-Instanz
        /// </summary>
        /// <param name="boneName">Knochenname</param>
        /// <returns>Gebundene GameObject-Instanz</returns>
        public GameObject GetAttachedGameObjectForBone(string boneName)
        {
            if(IsValid)
            {
                return _gameObject.GetAttachedGameObjectForBone(boneName);
            }
            return null;
        }
    }
}
