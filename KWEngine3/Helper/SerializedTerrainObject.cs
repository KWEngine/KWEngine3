using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal class SerializedTerrainObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsShadowCaster { get; set; }
        public bool IsCollisionObject { get; set; }
        public bool IsVisible { get; set; } = true;
        public string ModelName { get; set; }
        public string ModelPath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public float[] Position { get; set; }
        public float[] Color { get; set; }
        public float[] ColorEmissive { get; set; }
        public float Metallic { get; set; }
        public float Roughness { get; set; }
        public MetallicType MetallicType { get; set; }
        public float[] TextureOffset { get; set; }
        public float[] TextureRepeat { get; set; }
        public string TextureAlbedo { get; set; }
        public string TextureNormal { get; set; }
        public string TextureRoughness { get; set; }
        public string TextureMetallic { get; set; }
        public string TextureEmissive { get; set; }
        public float[] TextureTransform { get; set; }


        public static SerializedTerrainObject GenerateSerializedTerrainObject(TerrainObject t)
        {
            SerializedTerrainObject st = new SerializedTerrainObject();

            st.ID = t.ID;
            st.IsShadowCaster = t.IsShadowCaster;
            st.IsCollisionObject = t.IsCollisionObject;
            st.IsVisible = t.IsVisible;
            st.Name = t.Name;
            st.ModelName = t._gModel.ModelOriginal.Name;
            st.ModelPath = t._gModel.ModelOriginal.Meshes.Values.ElementAt(0).Terrain._heightMapName;
            st.Width = t._gModel.ModelOriginal.Meshes.Values.ElementAt(0).Terrain.GetWidth();
            st.Height = t._gModel.ModelOriginal.Meshes.Values.ElementAt(0).Terrain.GetHeight();
            st.Depth = t._gModel.ModelOriginal.Meshes.Values.ElementAt(0).Terrain.GetDepth();

            st.Position = new float[] { t._stateCurrent._position.X, t._stateCurrent._position.Y, t._stateCurrent._position.Z };


            st.Color = new float[] { t._stateCurrent._colorTint.X, t._stateCurrent._colorTint.Y, t._stateCurrent._colorTint.Z };
            st.ColorEmissive = new float[] { t._stateCurrent._colorEmissive.X, t._stateCurrent._colorEmissive.Y, t._stateCurrent._colorEmissive.Z, t._stateCurrent._colorEmissive.W };
            st.Metallic = t._gModel._metallicTerrain;
            st.Roughness = t._gModel._roughnessTerrain;
            st.MetallicType = t._gModel._metallicType;
            st.TextureOffset = new float[] { t._stateCurrent._uvTransform.Z, t._stateCurrent._uvTransform.W };
            st.TextureRepeat = new float[] { t._stateCurrent._uvTransform.X, t._stateCurrent._uvTransform.Y };
            st.TextureAlbedo = t._gModel.Material[0].TextureAlbedo.Filename;
            st.TextureNormal = t._gModel.Material[0].TextureNormal.Filename;
            st.TextureRoughness = t._gModel.Material[0].TextureRoughness.Filename;
            st.TextureMetallic = t._gModel.Material[0].TextureMetallic.Filename;
            st.TextureEmissive = t._gModel.Material[0].TextureEmissive.Filename;

            st.TextureTransform = new float[] { t._stateCurrent._uvTransform.X, t._stateCurrent._uvTransform.Y, t._stateCurrent._uvTransform.Z, t._stateCurrent._uvTransform.W };

            return st;
        }
    }
}
