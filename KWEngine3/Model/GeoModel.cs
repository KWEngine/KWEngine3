using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using KWEngine3.Helper;

namespace KWEngine3.Model
{
    /// <summary>
    /// Modellklasse
    /// </summary>
    internal class GeoModel
    {
        /// <summary>
        /// Gibt Auskunft über den Modellnamen
        /// </summary>
        /// <returns>Modellname</returns>
        public override string ToString()
        {
            return Name;
        }

        internal bool HasTransparencyTexture { get; set; } = false;
        internal bool IsKWCube6 { get; set; } = false;

        internal bool IsPrimitive { get { return Name == "kwcube.obj" || Name == "kwsphere.fbx" || Name.StartsWith("kwquad"); } }

        /// <summary>
        /// Wurzelknoten
        /// </summary>
        internal GeoNode Root { get; set; } = null;
        /// <summary>
        /// Pfad zur Modelldatei
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// Absoluter Pfad zur Modelldatei
        /// </summary>
        public string PathAbsolute { get; internal set; }
        internal SceneImporter.AssemblyMode AssemblyMode { get; set; }
        /// <summary>
        /// Animationsliste
        /// </summary>
        public List<GeoAnimation> Animations { get; internal set; }
        /// <summary>
        /// Verfügt das Modell über Knochen für Animationen?
        /// </summary>
        public bool HasBones
        {
            get; internal set;

        } = false;

        public Dictionary<string, Dictionary<string, GeoBone>> BoneDictionary = new();

        
        /// <summary>
        /// Knoten des Skeletts
        /// </summary>
        internal GeoNode Armature { get; set; }

        /// <summary>
        /// Validität des Modells
        /// </summary>
        public bool IsValid { get; internal set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Dateiname
        /// </summary>
        public string Filename { get; internal set; }
        /// <summary>
        /// Globale Transformationsinvers-Matrix
        /// </summary>
        public Matrix4 TransformGlobalInverse { get; internal set; }
        internal SortedDictionary<string, GeoMesh> Meshes { get; set; }
        internal List<GeoMeshHitbox> MeshHitboxes { get; set; }
        /// <summary>
        /// Handelt es sich bei dem Modell um Terrain?
        /// </summary>
        public bool IsTerrain
        { 
            get
            {
                bool found = Meshes.TryGetValue("Terrain", out GeoMesh terrainMesh);
                if (found)
                {
                    return terrainMesh.Terrain != null;
                }
                else
                {
                    return false;
                }
            }
        }

        internal List<string> BoneNames { get; set; } = new List<string>();
        internal List<GeoNode> NodesWithoutHierarchy = new List<GeoNode>();
        internal Dictionary<string, GeoTexture> Textures { get; set; }

        internal void Dispose()
        {
            IsValid = false;
            if (Textures != null)
            {

                foreach (GeoTexture t in Textures.Values)
                {
                    GL.DeleteTextures(1, new int[] { t.OpenGLID });
                }
                Textures.Clear();

            }
            if (Meshes != null)
            {
                foreach (GeoMesh m in Meshes.Values)
                {
                    if (m.Terrain != null)
                    {
                        m.Terrain.Dispose();
                        m.Terrain = null;
                    }
                    m.Dispose();
                   
                }
            }

            // Delete all connected instances (c#):
            foreach(GeoMesh m in Meshes.Values)
            {
                m.BoneIndices = null;
                m.BoneNames = null;
                m.BoneOffset = null;
                m.BoneOffsetInverse = null;
                m.BoneTranslationMatrixCount = -1;
                m.Vertices = null;
            }

            this.NodesWithoutHierarchy = null;
            this.MeshHitboxes = null;
            this.Meshes = null;
            this.Animations = null;
            this.Armature = null;
            this.BoneNames = null;
            this.Root = null;
            this.Textures = null;
        }

        internal void CalculatePath()
        {
            if (AssemblyMode == SceneImporter.AssemblyMode.File)
            {
                FileInfo fi = new FileInfo(Filename);
                if (fi.Exists)
                {
                    Path = HelperGeneral.EqualizePathDividers(fi.DirectoryName);
                }
                else
                {
                    throw new Exception("File " + Filename + " does not exist.");
                }
            }
        }

        internal int GLBOffset { get; set; } = -1;

        internal bool IsGLTF { get; set; } = false;
    }
}
