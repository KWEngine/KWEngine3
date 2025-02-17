
using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class SerializedHUDObject
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public HUDObjectType Type { get; set; }
        public float[] Position { get; set; }
        public float[] Scale { get; set; }
        public float[] Color { get; set; }
        public float[] Glow { get; set; }

        public string Text { get; set; }
        public float CharacterDistanceFactor { get; set; }
        public TextAlignMode Alignment { get; set; }
        public FontFace Font { get; set; }
        public string Texture { get; set; }


        public static SerializedHUDObject GenerateSerializedHUDObject(HUDObject h)
        {
            SerializedHUDObject sh = new SerializedHUDObject();

            sh.Name = h.Name;
            sh.IsVisible = h.IsVisible;
            sh.Type = h is HUDObjectText ? HUDObjectType.Text : HUDObjectType.Image;
            sh.Position = new float[] { h.Position.X, h.Position.Y };
            sh.Scale = new float[] { h._scale.X, h._scale.Y };
            sh.Color = new float[] {h._tint.X, h._tint.Y, h._tint.Z, h._tint.W};
            sh.Glow = new float[] { h._glow.X, h._glow.Y, h._glow.Z, h._glow.W };

            if (sh.Type == HUDObjectType.Text)
            {
                HUDObjectText ht = h as HUDObjectText;
                sh.Text = ht._text;
                sh.CharacterDistanceFactor = ht._spread;
                sh.Alignment = ht.TextAlignment;
                sh.Font = FontFace.Anonymous; // TODO

                sh.Texture = "";
            }
            else
            {
                HUDObjectImage hi = h as HUDObjectImage;
                sh.Texture = hi._textureName;

                sh.Text = "";
                sh.CharacterDistanceFactor = 1;
                sh.Alignment = TextAlignMode.Left;
                sh.Font = FontFace.Anonymous;
            }
            return sh;
        }
    }
}
