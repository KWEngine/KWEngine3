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
        //private string _type = "x5"; 

        public PowerUp(string type)
        {
            SetModel("PowerUp");
            _text = new PowerUpText();
            _text.SetText(type);
            KWEngine.CurrentWorld.AddTextObject(_text);
            SetScale(3.0f);
            UpdateTextPosition();
            SetColorEmissive(0.25f, 0.25f, 0.25f, 1f);
            IsCollisionObject = true;
            HasTransparencyTexture = true;
        }

        public override void Act()
        {
            UpdateTextPosition();
        }

        public void Destroy()
        {
            CurrentWorld.RemoveGameObject(this);
            CurrentWorld.RemoveTextObject(_text);

            ExplosionObject e = new ExplosionObject(512, 1.0f, 5.0f, 1.0f, ExplosionType.Star);
            e.SetAlgorithm(ExplosionAnimation.WindUp);
            e.SetPosition(Position);
            e.SetColorEmissive(1, 1, 1, 5);
            CurrentWorld.AddExplosionObject(e);
        }

        private void UpdateTextPosition()
        {
            if (_text != null)
            {
                _text.SetPosition(this.Center + new Vector3(0, 0, 0.125f));
            }
        }
    }
}
