using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class PowerUp : GameObject
    {
        private PowerUpText _text = null;
        private string _type = "x5"; 

        public PowerUp(string type)
        {
            SetModel("PowerUp");
            _text = new PowerUpText();
            _text.SetText(type);
            KWEngine.CurrentWorld.AddTextObject(_text);
            SetScale(2);
            UpdateTextPosition();
            IsCollisionObject = true;
            HasTransparencyTexture = true;
        }

        public override void Act()
        {
            UpdateTextPosition();
        }

        private void UpdateTextPosition()
        {
            if (_text != null)
            {
                _text.SetPosition(this.Center + new Vector3(0.5f, 0, 0.25f));
            }
        }
    }
}
