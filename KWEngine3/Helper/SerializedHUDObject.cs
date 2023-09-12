
using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedHUDObject
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public HUDObjectType Type { get; set; }
        public float[] Scale { get; set; }
        public string Text { get; set; }
        public string Texture { get; set; }
        public int[] Position { get; set; }
        public FontFace Font { get; set; }


        public static SerializedHUDObject GenerateSerializedHUDObject(HUDObject h)
        {
            SerializedHUDObject sh = new SerializedHUDObject();

            sh.Name = h.Name;
            sh.IsVisible = h.IsVisible;
            sh.Type = h is HUDObjectText ? HUDObjectType.Text : HUDObjectType.Image;
            sh.Scale = new float[] { h._scale.X, h._scale.Y };
            //sh.Text = h._text;
            //sh.Texture = h._textureName;
            //sh.Position = new int[] { (int)h._absolute.X, (int)h._absolute.Y };
            //sh.Font = h.Font;

            return sh;
        }
    }
}
