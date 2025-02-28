
using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedTextObject
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public float[] Color { get; set; }
        public float[] ColorEmissive { get; set; }
        public float Scale { get; set; }
        public float[] Rotation { get; set; }
        public string Text { get; set; }
        
        public float[] Position { get; set; }
        public FontFace Font { get; set; }
        public float Spread { get; set; } 
        public bool IsShadowCaster { get; set; }
        public bool IsAffectedByLight { get; set; }


        public static SerializedTextObject GenerateSerializedTextObject(TextObject t)
        {
            SerializedTextObject st = new SerializedTextObject();
            st.Type = t.GetType().FullName;
            st.Name = t.Name;
            st.IsShadowCaster = t.IsShadowReceiver;
            st.IsAffectedByLight = t.IsAffectedByLight;

            st.Color = new float[] { t.Color.X, t.Color.Y, t.Color.Z, t.Opacity };
            st.ColorEmissive = new float[] { t.ColorEmissive.X, t.ColorEmissive.Y, t.ColorEmissive.Z, t.ColorEmissive.W };

            st.Scale = t.Scale;
            st.Rotation = new float[] {t._stateCurrent._rotation.X, t._stateCurrent._rotation.Y, t._stateCurrent._rotation.Z, t._stateCurrent._rotation.W};
            st.Position = new float[] { t.Position.X, t.Position.Y, t.Position.Z };

            st.Text = t.Text;
            st.Font = FontFace.Anonymous; // TODO
            st.Spread = t._stateCurrent._spreadFactor;

            return st;
        }
    }
}
